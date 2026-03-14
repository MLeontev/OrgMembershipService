using OrgMembershipService.Domain.Exceptions;

namespace OrgMembershipService.Domain.Entities;

public class Invitation
{
    public Guid Id { get; private set; }

    public Guid OrganizationId { get; private set; }

    public string TokenHash { get; private set; } = string.Empty;
    public InvitationStatus Status { get; private set; } = InvitationStatus.Pending;

    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;

    public Guid CreatedByUserId { get; private set; }
    public User CreatedByUser { get; private set; } = null!;

    public Guid? AcceptedByUserId { get; private set; }
    public User? AcceptedByUser { get; private set; }
    public DateTimeOffset? AcceptedAt { get; private set; }

    public DateTimeOffset? RevokedAt { get; private set; }
    
    private readonly List<InvitationRole> _roles = [];
    public IReadOnlyList<InvitationRole> Roles => _roles;
    
    private Invitation() { }

    public static Invitation Create(
        Guid organizationId,
        string tokenHash,
        DateTimeOffset expiresAt,
        Guid createdByUserId,
        IReadOnlyCollection<Guid> roleIds)
    {
        var validRoleIds = roleIds
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToList();

        if (validRoleIds.Count == 0)
            throw new BusinessException("INVITATION_ROLES_REQUIRED", "У приглашения должна быть хотя бы одна роль");

        var invitation = new Invitation
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            TokenHash = tokenHash,
            Status = InvitationStatus.Pending,
            ExpiresAt = expiresAt,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedByUserId = createdByUserId
        };

        foreach (var roleId in validRoleIds)
            invitation.AddRole(roleId);

        return invitation;
    }

    public void AddRole(Guid roleId)
    {
        if (_roles.Any(x => x.RoleId == roleId))
            return;

        _roles.Add(new InvitationRole { RoleId = roleId, InvitationId = Id });
    }

    public void Revoke()
    {
        TryExpire();

        switch (Status)
        {
            case InvitationStatus.Accepted:
                throw new ConflictException("INVITATION_ALREADY_ACCEPTED", "Приглашение уже принято");

            case InvitationStatus.Revoked or InvitationStatus.Expired:
                return;

            default:
                Status = InvitationStatus.Revoked;
                RevokedAt = DateTimeOffset.UtcNow;
                break;
        }
    }

    public void Accept(Guid acceptedByUserId)
    {
        TryExpire();

        switch (Status)
        {
            case InvitationStatus.Revoked:
                throw new ConflictException("INVITATION_REVOKED", "Приглашение отозвано");
            case InvitationStatus.Expired:
                throw new ConflictException("INVITATION_EXPIRED", "Срок действия приглашения истек");
            case InvitationStatus.Accepted:
                throw new ConflictException("INVITATION_ALREADY_ACCEPTED", "Приглашение уже принято");
        }

        Status = InvitationStatus.Accepted;
        AcceptedByUserId = acceptedByUserId;
        AcceptedAt = DateTimeOffset.UtcNow;
    }

    public bool IsExpired(DateTimeOffset now) =>
        Status == InvitationStatus.Expired ||
        (Status == InvitationStatus.Pending && ExpiresAt <= now);

    public bool TryExpire()
    {
        if (Status != InvitationStatus.Pending)
            return false;

        if (!IsExpired(DateTimeOffset.UtcNow))
            return false;

        Status = InvitationStatus.Expired;
        return true;
    }
}
