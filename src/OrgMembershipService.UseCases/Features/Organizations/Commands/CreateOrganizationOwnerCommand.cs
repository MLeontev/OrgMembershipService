using MediatR;
using Microsoft.EntityFrameworkCore;
using OrgMembershipService.Application.Abstractions;
using OrgMembershipService.Application.Services;
using OrgMembershipService.Domain.Entities;
using OrgMembershipService.Domain.Exceptions;

namespace OrgMembershipService.Application.Features.Organizations.Commands;

/// <summary>
/// Команда назначения владельца организации
/// </summary>
/// <param name="OrganizationId">Идентификатор организации</param>
/// <param name="OwnerIdentityId">Внешний идентификатор пользователя в Keycloak (sub из access токена)</param>
public record CreateOrganizationOwnerCommand(Guid OrganizationId, string OwnerIdentityId) : IRequest;

internal class CreateOrganizationOwnerCommandHandler(
    IDbContext dbContext,
    IUserIdentityResolver identityResolver) : IRequestHandler<CreateOrganizationOwnerCommand>
{
    public async Task Handle(CreateOrganizationOwnerCommand request, CancellationToken cancellationToken)
    {
        var userId = await identityResolver.ResolveUserIdAsync(request.OwnerIdentityId, cancellationToken);
        
        var userExists = await dbContext.Users
            .AsNoTracking()
            .AnyAsync(x => x.Id == userId, cancellationToken);
        
        if (!userExists)
            throw new NotFoundException("USER_NOT_FOUND", "Пользователь не найден");
        
        var membershipExists = await dbContext.Memberships
            .AsNoTracking()
            .AnyAsync(
                x => x.OrganizationId == request.OrganizationId && x.UserId == userId,
                cancellationToken);

        if (membershipExists)
            throw new ConflictException("MEMBERSHIP_ALREADY_EXISTS", "Участник уже существует в организации");

        var ownerRole = await dbContext.Roles
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.Code == "ORG_OWNER" && x.OrganizationId == null,
                cancellationToken);
        
        if (ownerRole is null)
            throw new NotFoundException("ORG_OWNER_ROLE_NOT_FOUND", "Роль владельца не найдена");
        
        var membership = Membership.Create(request.OrganizationId, userId);
        membership.AssignRole(ownerRole.Id, null);
        
        dbContext.Memberships.Add(membership);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
