using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OrgMembershipService.Application.Abstractions;
using OrgMembershipService.Application.Services;
using OrgMembershipService.Domain.Entities;
using OrgMembershipService.Domain.Exceptions;

namespace OrgMembershipService.Application.Features.Invitations.Queries;

/// <summary>
/// Запрос списка приглашений организации
/// </summary>
/// <param name="OrganizationId">Идентификатор организации</param>
/// <param name="IdentityId">Идентификатор пользователя в Keycloak (sub из access токена)</param>
/// <param name="Status">Фильтр по статусу приглашения (Pending, Accepted, Revoked, Expired)</param>
public record GetOrganizationInvitationsQuery(Guid OrganizationId, string IdentityId, string? Status) : IRequest<OrganizationInvitationsDto>;

/// <summary>
/// Список приглашений организации
/// </summary>
/// <param name="Invitations">Приглашения организации</param>
public record OrganizationInvitationsDto(IReadOnlyCollection<OrganizationInvitationDto> Invitations);

/// <summary>
/// Данные приглашения
/// </summary>
/// <param name="InvitationId">Идентификатор приглашения</param>
/// <param name="Status">Статус приглашения строкой (Pending, Accepted, Revoked, Expired)</param>
/// <param name="ExpiresAt">Дата и время окончания действия приглашения</param>
/// <param name="CreatedAt">Дата и время создания приглашения</param>
/// <param name="CreatedByUserId">Внутренний идентификатор создателя приглашения</param>
/// <param name="AcceptedByUserId">Внутренний идентификатор пользователя, принявшего приглашение</param>
/// <param name="AcceptedAt">Дата и время принятия приглашения</param>
/// <param name="RevokedAt">Дата и время отзыва приглашения</param>
/// <param name="RoleCodes">Коды ролей, назначаемых при принятии приглашения</param>
public record OrganizationInvitationDto(
    Guid InvitationId,
    string Status,
    DateTimeOffset ExpiresAt,
    DateTimeOffset CreatedAt,
    Guid CreatedByUserId,
    Guid? AcceptedByUserId,
    DateTimeOffset? AcceptedAt,
    DateTimeOffset? RevokedAt,
    IReadOnlyCollection<string> RoleCodes);

internal class GetOrganizationInvitationsQueryValidator : AbstractValidator<GetOrganizationInvitationsQuery>
{
    public GetOrganizationInvitationsQueryValidator()
    {
        RuleFor(x => x.Status)
            .Must(BeValidStatus)
            .When(x => !string.IsNullOrWhiteSpace(x.Status))
            .WithMessage("Некорректный статус приглашения");
    }

    private static bool BeValidStatus(string? status) =>
        Enum.TryParse<InvitationStatus>(status, ignoreCase: true, out _);
}

internal class GetOrganizationInvitationsQueryHandler(
    IDbContext dbContext,
    IUserIdentityResolver identityResolver) : IRequestHandler<GetOrganizationInvitationsQuery, OrganizationInvitationsDto>
{
    public async Task<OrganizationInvitationsDto> Handle(
        GetOrganizationInvitationsQuery request,
        CancellationToken cancellationToken)
    {
        var userId = await identityResolver.ResolveUserIdAsync(request.IdentityId, cancellationToken);

        var membershipExists = await dbContext.Memberships
            .AsNoTracking()
            .AnyAsync(
                x => x.OrganizationId == request.OrganizationId &&
                     x.UserId == userId &&
                     x.Status == MembershipStatus.Active,
                cancellationToken);

        if (!membershipExists)
            throw new NotFoundException("MEMBERSHIP_NOT_FOUND", "Участник не найден в организации");

        InvitationStatus? statusFilter = null;
        if (!string.IsNullOrWhiteSpace(request.Status))
            statusFilter = Enum.Parse<InvitationStatus>(request.Status, ignoreCase: true);

        var now = DateTimeOffset.UtcNow;

        var query = dbContext.Invitations
            .AsNoTracking()
            .Where(x => x.OrganizationId == request.OrganizationId);

        if (statusFilter.HasValue)
        {
            switch (statusFilter.Value)
            {
                case InvitationStatus.Pending:
                    query = query.Where(x => x.Status == InvitationStatus.Pending && x.ExpiresAt > now);
                    break;
                
                case InvitationStatus.Expired:
                    query = query.Where(
                        x => x.Status == InvitationStatus.Expired ||
                             (x.Status == InvitationStatus.Pending && x.ExpiresAt <= now));
                    break;
                
                default:
                    query = query.Where(x => x.Status == statusFilter.Value);
                    break;
            }
        }

        var invitations = await query
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        if (invitations.Count == 0)
            return new OrganizationInvitationsDto([]);

        var invitationIds = invitations.Select(x => x.Id).ToList();

        var roleAssignments = await dbContext.InvitationRoles
            .AsNoTracking()
            .Where(x => invitationIds.Contains(x.InvitationId))
            .Select(x => new { x.InvitationId, RoleCode = x.Role.Code })
            .ToListAsync(cancellationToken);

        var roleCodesByInvitationId = roleAssignments
            .GroupBy(x => x.InvitationId)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyCollection<string>)g
                    .Select(x => x.RoleCode)
                    .Distinct()
                    .OrderBy(x => x)
                    .ToList());

        var result = invitations
            .Select(x =>
            {
                var roles = roleCodesByInvitationId.TryGetValue(x.Id, out var assignedRoles)
                    ? assignedRoles
                    : [];

                var effectiveStatus = x.IsExpired(now)
                    ? InvitationStatus.Expired
                    : x.Status;

                return new OrganizationInvitationDto(
                    x.Id,
                    effectiveStatus.ToString(),
                    x.ExpiresAt,
                    x.CreatedAt,
                    x.CreatedByUserId,
                    x.AcceptedByUserId,
                    x.AcceptedAt,
                    x.RevokedAt,
                    roles);
            })
            .ToList();

        return new OrganizationInvitationsDto(result);
    }
}
