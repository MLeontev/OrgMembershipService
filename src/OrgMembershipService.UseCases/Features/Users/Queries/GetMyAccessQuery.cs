using MediatR;
using Microsoft.EntityFrameworkCore;
using OrgMembershipService.Application.Abstractions;
using OrgMembershipService.Application.Services;
using OrgMembershipService.Domain.Entities;
using OrgMembershipService.Domain.Exceptions;

namespace OrgMembershipService.Application.Features.Users.Queries;

/// <summary>
/// Запрос доступа текущего пользователя в организации
/// </summary>
/// <param name="OrganizationId">Идентификатор организации</param>
/// <param name="IdentityId">Идентификатор пользователя в Keycloak (sub из access токена)</param>
public record GetMyAccessQuery(Guid OrganizationId, string IdentityId) : IRequest<MyAccessDto>;

/// <summary>
/// Доступ пользователя в организации
/// </summary>
/// <param name="OrganizationId">Идентификатор организации</param>
/// <param name="MembershipStatus">Статус членства строкой (Active, Deactivated, Removed)</param>
/// <param name="Roles">Список кодов ролей пользователя</param>
/// <param name="Permissions">Список кодов прав пользователя</param>
public record MyAccessDto(
    Guid OrganizationId,
    string MembershipStatus,
    IReadOnlyCollection<string> Roles,
    IReadOnlyCollection<string> Permissions);
    
internal class GetMyAccessQueryHandler(
    IDbContext dbContext,
    IUserIdentityResolver identityResolver) : IRequestHandler<GetMyAccessQuery, MyAccessDto>
{
    public async Task<MyAccessDto> Handle(GetMyAccessQuery request, CancellationToken cancellationToken)
    {
        var userId = await identityResolver.ResolveUserIdAsync(request.IdentityId, cancellationToken);

        var membership = await dbContext.Memberships
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.OrganizationId == request.OrganizationId)
            .Select(x => new { x.Id, x.Status })
            .FirstOrDefaultAsync(cancellationToken);
        
        if (membership is null)
            throw new NotFoundException("MEMBERSHIP_NOT_FOUND", "Участник не найден в организации");

        var roles = await dbContext.MembershipRoles
            .AsNoTracking()
            .Where(x => x.MembershipId == membership.Id)
            .Select(x => x.Role.Code)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync(cancellationToken);

        List<string> permissions = [];
        if (membership.Status == MembershipStatus.Active)
        {
            permissions = await dbContext.MembershipRoles
                .AsNoTracking()
                .Where(x => x.MembershipId == membership.Id)
                .SelectMany(x => x.Role.RolePermissions)
                .Select(x => x.Permission.Code)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync(cancellationToken);
        }
        
        return new MyAccessDto(
            request.OrganizationId,
            membership.Status.ToString(),
            roles,
            permissions);
    }
}
