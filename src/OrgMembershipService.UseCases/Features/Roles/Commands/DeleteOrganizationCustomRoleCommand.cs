using MediatR;
using Microsoft.EntityFrameworkCore;
using OrgMembershipService.Application.Abstractions;
using OrgMembershipService.Domain.Exceptions;

namespace OrgMembershipService.Application.Features.Roles.Commands;

/// <summary>
/// Команда удаления кастомной роли организации
/// </summary>
/// <param name="OrganizationId">Идентификатор организации</param>
/// <param name="RoleId">Идентификатор роли</param>
public record DeleteOrganizationCustomRoleCommand(
    Guid OrganizationId,
    Guid RoleId) : IRequest;

internal class DeleteOrganizationCustomRoleCommandHandler(IDbContext dbContext) : IRequestHandler<DeleteOrganizationCustomRoleCommand>
{
    public async Task Handle(DeleteOrganizationCustomRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await dbContext.Roles
            .SingleOrDefaultAsync(
                x => x.OrganizationId == request.OrganizationId && x.Id == request.RoleId,
                cancellationToken);

        if (role is null)
            throw new NotFoundException("ROLE_NOT_FOUND", "Роль не найдена");

        var assignedToMembers = await dbContext.MembershipRoles
            .AsNoTracking()
            .AnyAsync(x => x.RoleId == role.Id, cancellationToken);

        if (assignedToMembers)
            throw new ConflictException("ROLE_IN_USE", "Роль назначена участникам");

        var usedInInvitations = await dbContext.InvitationRoles
            .AsNoTracking()
            .AnyAsync(x => x.RoleId == role.Id, cancellationToken);

        if (usedInInvitations)
            throw new ConflictException("ROLE_IN_USE", "Роль используется в приглашениях");

        dbContext.Roles.Remove(role);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
