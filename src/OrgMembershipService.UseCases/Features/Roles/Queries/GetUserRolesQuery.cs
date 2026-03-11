using MediatR;
using Microsoft.EntityFrameworkCore;
using OrgMembershipService.Application.Abstractions;
using OrgMembershipService.Application.Services;

namespace OrgMembershipService.Application.Features.Roles.Queries;

/// <summary>
/// Запрос списка role code пользователя в рамках организации
/// </summary>
/// <param name="OrganizationId">Идентификатор организации</param>
/// <param name="IdentityId">Внешний идентификатор пользователя в Keycloak (sub из access токена)</param>
public record GetUserRolesQuery(Guid OrganizationId, string IdentityId) : IRequest<UserRolesDto>;

/// <summary>
/// Список role code пользователя
/// </summary>
/// <param name="Roles">Уникальные коды ролей</param>
public record UserRolesDto(IReadOnlyCollection<string> Roles);

internal class GetUserRolesQueryHandler(
    IDbContext dbContext,
    IUserIdentityResolver identityResolver) : IRequestHandler<GetUserRolesQuery, UserRolesDto>
{
    public async Task<UserRolesDto> Handle(GetUserRolesQuery request, CancellationToken cancellationToken)
    {
        var userId = await identityResolver.ResolveUserIdAsync(request.IdentityId, cancellationToken);
        
        var roles = await dbContext.MembershipRoles
            .AsNoTracking()
            .Where(x =>
                x.Membership.OrganizationId == request.OrganizationId &&
                x.Membership.UserId == userId &&
                x.Membership.Status == Domain.Entities.MembershipStatus.Active)
            .Select(x => x.Role.Code)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync(cancellationToken);

        return new UserRolesDto(roles);
    }
}
