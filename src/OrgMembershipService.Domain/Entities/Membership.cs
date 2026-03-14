using OrgMembershipService.Domain.Exceptions;

namespace OrgMembershipService.Domain.Entities;

public class Membership
{
    public Guid Id { get; private set; }
    
    public Guid OrganizationId { get; private set; }
    
    public Guid UserId { get; private set; }
    public User User { get; private set; }
    
    public MembershipStatus Status { get; private set; }
    
    public DateTimeOffset? JoinedAt { get; private set; }
    public DateTimeOffset? RemovedAt { get; private set; }
    
    public string? Department { get; private set; }
    public string? Title { get; private set; }
    
    public Guid? DefaultOfficeId { get; private set; }
    public Guid? DefaultFloorId { get; private set; }
    public string? MetadataJson { get; private set; }
    
    private readonly List<MembershipRole> _roleAssignments = [];
    public IReadOnlyList<MembershipRole> RoleAssignments => _roleAssignments;
    
    private Membership() { }

    public static Membership Create(Guid organizationId, Guid userId)
    {
        return new Membership
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            UserId = userId,
            Status = MembershipStatus.Active,
            JoinedAt = DateTimeOffset.UtcNow
        };
    }

    public void AssignRole(Guid roleId, Guid? assignedByUserId)
    {
        if (_roleAssignments.Any(x => x.RoleId == roleId))
            return;

        _roleAssignments.Add(new MembershipRole
        {
            Membership = this,
            RoleId = roleId,
            AssignedByUserId = assignedByUserId,
            AssignedAt = DateTimeOffset.UtcNow
        });
    }

    public void UpdateProfile(string? department, string? title)
    {
        Department = department;
        Title = title;
    }

    public void Deactivate()
    {
        if (Status == MembershipStatus.Removed)
            throw new ConflictException("MEMBERSHIP_REMOVED", "Нельзя деактивировать удаленного участника");

        if (Status == MembershipStatus.Deactivated)
            return;

        Status = MembershipStatus.Deactivated;
    }

    public void Activate()
    {
        if (Status == MembershipStatus.Removed)
            throw new ConflictException("MEMBERSHIP_REMOVED", "Нельзя активировать удаленного участника");

        if (Status == MembershipStatus.Active)
            return;

        Status = MembershipStatus.Active;
    }

    public void Remove()
    {
        if (Status == MembershipStatus.Removed)
        {
            RemovedAt ??= DateTimeOffset.UtcNow;
            return;
        }

        Status = MembershipStatus.Removed;
        RemovedAt = DateTimeOffset.UtcNow;
    }
}
