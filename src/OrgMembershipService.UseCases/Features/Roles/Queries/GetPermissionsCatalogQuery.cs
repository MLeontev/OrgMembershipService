using MediatR;
using Microsoft.EntityFrameworkCore;
using OrgMembershipService.Application.Abstractions;

namespace OrgMembershipService.Application.Features.Roles.Queries;

/// <summary>
/// Запрос каталога прав
/// </summary>
public record GetPermissionsCatalogQuery : IRequest<PermissionsCatalogDto>;

/// <summary>
/// Каталог прав для UI
/// </summary>
/// <param name="Permissions">Список доступных прав</param>
public record PermissionsCatalogDto(IReadOnlyCollection<PermissionCatalogItemDto> Permissions);

/// <summary>
/// Данные права
/// </summary>
/// <param name="Code">Код права</param>
/// <param name="Name">Название права</param>
/// <param name="Description">Описание права</param>
public record PermissionCatalogItemDto(
    string Code,
    string Name,
    string? Description);

internal class GetPermissionsCatalogQueryHandler(IDbContext dbContext) : IRequestHandler<GetPermissionsCatalogQuery, PermissionsCatalogDto>
{
    public async Task<PermissionsCatalogDto> Handle(GetPermissionsCatalogQuery request, CancellationToken cancellationToken)
    {
        var permissions = await dbContext.Permissions
            .AsNoTracking()
            .OrderBy(x => x.Code)
            .Select(x => new PermissionCatalogItemDto(
                x.Code,
                x.Name,
                x.Description))
            .ToListAsync(cancellationToken);

        return new PermissionsCatalogDto(permissions);
    }
}
