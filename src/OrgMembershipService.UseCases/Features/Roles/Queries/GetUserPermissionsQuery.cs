using MediatR;
using Microsoft.EntityFrameworkCore;
using OrgMembershipService.Application.Abstractions;
using OrgMembershipService.Application.Services;
using OrgMembershipService.Domain.Entities;

namespace OrgMembershipService.Application.Features.Roles.Queries;

/// <summary>
/// Запрос списка permission code пользователя в рамках организации
/// </summary>
/// <param name="OrganizationId">Идентификатор организации</param>
/// <param name="IdentityId">Внешний идентификатор пользователя в Keycloak (sub из access токена)</param>
public record GetUserPermissionsQuery(Guid OrganizationId, string IdentityId) : IRequest<UserPermissionsDto>;

/// <summary>
/// Список permission code пользователя
/// </summary>
/// <param name="Permissions">Уникальные коды прав</param>
public record UserPermissionsDto(IReadOnlyCollection<string> Permissions);

internal class GetUserPermissionsQueryHandler(
    IDbContext dbContext,
    IUserIdentityResolver identityResolver) : IRequestHandler<GetUserPermissionsQuery, UserPermissionsDto>
{
    public async Task<UserPermissionsDto> Handle(GetUserPermissionsQuery request, CancellationToken cancellationToken)
    {
        var userId = await identityResolver.ResolveUserIdAsync(request.IdentityId, cancellationToken);
        
        var permissions = await dbContext.MembershipRoles
            .AsNoTracking()
            .Where(x =>
                x.Membership.OrganizationId == request.OrganizationId &&
                x.Membership.UserId == userId &&
                x.Membership.Status == MembershipStatus.Active)
            .SelectMany(x => x.Role.RolePermissions)
            .Select(x => x.Permission.Code)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync(cancellationToken);
        
        return new UserPermissionsDto(permissions);
    }
}
