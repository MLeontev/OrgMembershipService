using Microsoft.EntityFrameworkCore;
using OrgMembershipService.Application.Abstractions;
using OrgMembershipService.Domain.Entities;
using OrgMembershipService.Domain.Exceptions;

namespace OrgMembershipService.Application.Services;

public record AccessRoleCandidate(
    string RoleCode,
    int Priority);

public interface IAccessPolicyService
{
    Task EnsureCanManageMembershipAsync(
        Guid organizationId,
        Guid actorUserId,
        Membership targetMembership,
        string actionCode,
        CancellationToken ct = default);

    Task EnsureCanAssignRolesAsync(
        Guid organizationId,
        Guid actorUserId,
        Membership targetMembership,
        IReadOnlyCollection<AccessRoleCandidate> roleCandidates,
        CancellationToken ct = default);

    Task EnsureCanCreateInvitationAsync(
        Guid organizationId,
        Guid actorUserId,
        IReadOnlyCollection<AccessRoleCandidate> roleCandidates,
        CancellationToken ct = default);
}

internal class AccessPolicyService(IDbContext dbContext) : IAccessPolicyService
{
    public async Task EnsureCanManageMembershipAsync(
        Guid organizationId,
        Guid actorUserId,
        Membership targetMembership,
        string actionCode,
        CancellationToken ct = default)
    {
        if (targetMembership.UserId == actorUserId)
            throw new ForbiddenException("SELF_OPERATION_FORBIDDEN", "Нельзя выполнять это действие для себя");

        var actorPriority = await GetActorMaxPriorityOrThrowAsync(organizationId, actorUserId, ct);
        var targetPriority = await GetMembershipMaxPriorityAsync(targetMembership.Id, ct) ?? int.MinValue;

        if (actorPriority <= targetPriority)
            throw new ForbiddenException("INSUFFICIENT_ROLE_PRIORITY", "Недостаточно прав для управления этим участником");
    }

    public async Task EnsureCanAssignRolesAsync(
        Guid organizationId,
        Guid actorUserId,
        Membership targetMembership,
        IReadOnlyCollection<AccessRoleCandidate> roleCandidates,
        CancellationToken ct = default)
    {
        if (targetMembership.UserId == actorUserId)
            throw new ForbiddenException("SELF_ROLE_MANAGEMENT_FORBIDDEN", "Нельзя менять свои роли");

        var actorPriority = await GetActorMaxPriorityOrThrowAsync(organizationId, actorUserId, ct);
        var targetPriority = await GetMembershipMaxPriorityAsync(targetMembership.Id, ct) ?? int.MinValue;

        if (actorPriority <= targetPriority)
            throw new ForbiddenException("INSUFFICIENT_ROLE_PRIORITY", "Недостаточно прав для управления ролями этого участника");

        EnsureRolePrioritiesAreLower(actorPriority, roleCandidates);
    }

    public async Task EnsureCanCreateInvitationAsync(
        Guid organizationId,
        Guid actorUserId,
        IReadOnlyCollection<AccessRoleCandidate> roleCandidates,
        CancellationToken ct = default)
    {
        var actorPriority = await GetActorMaxPriorityOrThrowAsync(organizationId, actorUserId, ct);
        EnsureRolePrioritiesAreLower(actorPriority, roleCandidates);
    }

    private async Task<int> GetActorMaxPriorityOrThrowAsync(
        Guid organizationId,
        Guid actorUserId,
        CancellationToken ct)
    {
        var actorPriority = await dbContext.MembershipRoles
            .AsNoTracking()
            .Where(x =>
                x.Membership.OrganizationId == organizationId &&
                x.Membership.UserId == actorUserId &&
                x.Membership.Status == MembershipStatus.Active)
            .Select(x => (int?)x.Role.Priority)
            .MaxAsync(ct);

        if (!actorPriority.HasValue)
            throw new ForbiddenException("ACTOR_MEMBERSHIP_NOT_ACTIVE", "Недостаточно прав для действия в организации");

        return actorPriority.Value;
    }

    private async Task<int?> GetMembershipMaxPriorityAsync(Guid membershipId, CancellationToken ct)
    {
        return await dbContext.MembershipRoles
            .AsNoTracking()
            .Where(x => x.MembershipId == membershipId)
            .Select(x => (int?)x.Role.Priority)
            .MaxAsync(ct);
    }

    private static void EnsureRolePrioritiesAreLower(int actorPriority, IReadOnlyCollection<AccessRoleCandidate> roleCandidates)
    {
        var forbiddenRoleCodes = roleCandidates
            .Where(x => x.Priority >= actorPriority)
            .Select(x => x.RoleCode)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x)
            .ToList();

        if (forbiddenRoleCodes.Count == 0)
            return;

        throw new ForbiddenException(
            "ROLE_PRIORITY_FORBIDDEN",
            $"Нельзя назначать роли с приоритетом не ниже вашего: {string.Join(", ", forbiddenRoleCodes)}");
    }
}
