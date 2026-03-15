using MediatR;
using Microsoft.EntityFrameworkCore;
using OrgMembershipService.Application.Abstractions;

namespace OrgMembershipService.Application.Features.Roles.Queries;

/// <summary>
/// Запрос списка ролей организации
/// </summary>
/// <param name="OrganizationId">Идентификатор организации</param>
/// <param name="IncludeSystem">Включать ли системные роли (OrganizationId = null)</param>
public record GetOrganizationRolesQuery(Guid OrganizationId, bool IncludeSystem = true) : IRequest<OrganizationRolesDto>;

/// <summary>
/// Список ролей организации
/// </summary>
/// <param name="Roles">Роли организации</param>
public record OrganizationRolesDto(IReadOnlyCollection<OrganizationRoleDto> Roles);

/// <summary>
/// Данные роли организации
/// </summary>
/// <param name="RoleId">Идентификатор роли</param>
/// <param name="RoleCode">Код роли</param>
/// <param name="Name">Название роли</param>
/// <param name="Description">Описание роли</param>
/// <param name="Priority">Приоритет роли</param>
/// <param name="IsSystem">Признак системной роли</param>
/// <param name="PermissionCodes">Коды прав роли</param>
public record OrganizationRoleDto(
    Guid RoleId,
    string RoleCode,
    string Name,
    string? Description,
    int Priority,
    bool IsSystem,
    IReadOnlyCollection<string> PermissionCodes);

internal class GetOrganizationRolesQueryHandler(IDbContext dbContext) : IRequestHandler<GetOrganizationRolesQuery, OrganizationRolesDto>
{
    public async Task<OrganizationRolesDto> Handle(GetOrganizationRolesQuery request, CancellationToken cancellationToken)
    {
        var rolesQuery = dbContext.Roles.AsNoTracking();

        rolesQuery = request.IncludeSystem
            ? rolesQuery.Where(x => x.OrganizationId == request.OrganizationId || x.OrganizationId == null)
            : rolesQuery.Where(x => x.OrganizationId == request.OrganizationId);

        var roles = await rolesQuery
            .Select(x => new
            {
                x.Id,
                x.Code,
                x.Name,
                x.Description,
                x.OrganizationId,
                x.Priority
            })
            .OrderByDescending(x => x.OrganizationId == null)
            .ThenByDescending(x => x.Priority)
            .ThenBy(x => x.Code)
            .ToListAsync(cancellationToken);

        if (roles.Count == 0)
            return new OrganizationRolesDto([]);

        var roleIds = roles.Select(x => x.Id).ToList();

        var permissionAssignments = await dbContext.RolePermissions
            .AsNoTracking()
            .Where(x => roleIds.Contains(x.RoleId))
            .Select(x => new { x.RoleId, PermissionCode = x.Permission.Code })
            .ToListAsync(cancellationToken);

        var permissionCodesByRoleId = permissionAssignments
            .GroupBy(x => x.RoleId)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyCollection<string>)g
                    .Select(x => x.PermissionCode)
                    .Distinct()
                    .OrderBy(x => x)
                    .ToList());

        var result = roles
            .Select(x =>
            {
                var permissionCodes = permissionCodesByRoleId.TryGetValue(x.Id, out var codes)
                    ? codes
                    : [];

                return new OrganizationRoleDto(
                    x.Id,
                    x.Code,
                    x.Name,
                    x.Description,
                    x.Priority,
                    x.OrganizationId is null,
                    permissionCodes);
            })
            .ToList();

        return new OrganizationRolesDto(result);
    }
}
