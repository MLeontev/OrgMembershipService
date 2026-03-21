using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OrgMembershipService.Application.Abstractions;
using OrgMembershipService.Application.Services;
using OrgMembershipService.Domain.Entities;
using OrgMembershipService.Domain.Exceptions;

namespace OrgMembershipService.Application.Features.Organizations.Commands;

/// <summary>
/// Команда смены владельца организации
/// </summary>
/// <param name="OrganizationId">Идентификатор организации</param>
/// <param name="NewOwnerIdentityId">Идентификатор нового владельца в Keycloak (sub из access токена)</param>
public record ChangeOrganizationOwnerCommand(
    Guid OrganizationId,
    string NewOwnerIdentityId) : IRequest;

internal class ChangeOrganizationOwnerCommandHandler(
    IDbContext dbContext,
    IUserIdentityResolver identityResolver) : IRequestHandler<ChangeOrganizationOwnerCommand>
{
    public async Task Handle(ChangeOrganizationOwnerCommand request, CancellationToken cancellationToken)
    {
        var ownerRole = await dbContext.Roles
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.Code == "ORG_OWNER" && x.OrganizationId == null,
                cancellationToken);

        if (ownerRole is null)
            throw new NotFoundException("ORG_OWNER_ROLE_NOT_FOUND", "Роль владельца не найдена");

        var currentOwnerAssignments = await dbContext.MembershipRoles
            .Include(x => x.Membership)
            .Where(x =>
                x.RoleId == ownerRole.Id &&
                x.Membership.OrganizationId == request.OrganizationId)
            .ToListAsync(cancellationToken);

        if (currentOwnerAssignments.Count == 0)
            throw new NotFoundException("ORGANIZATION_OWNER_NOT_FOUND", "Текущий владелец организации не найден");

        var newOwnerUserId = await identityResolver.ResolveUserIdAsync(request.NewOwnerIdentityId, cancellationToken);

        var newOwnerMembership = await dbContext.Memberships
            .Include(x => x.RoleAssignments)
            .SingleOrDefaultAsync(
                x => x.OrganizationId == request.OrganizationId && x.UserId == newOwnerUserId,
                cancellationToken);

        if (newOwnerMembership is null)
        {
            newOwnerMembership = Membership.Create(request.OrganizationId, newOwnerUserId);
            dbContext.Memberships.Add(newOwnerMembership);
        }
        else
        {
            newOwnerMembership.Reactivate();
        }

        newOwnerMembership.AssignRole(ownerRole.Id, null);

        var assignmentsToRemove = currentOwnerAssignments
            .Where(x => x.Membership.UserId != newOwnerUserId)
            .ToList();

        if (assignmentsToRemove.Count > 0)
            dbContext.MembershipRoles.RemoveRange(assignmentsToRemove);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
