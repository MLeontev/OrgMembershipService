using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OrgMembershipService.Application.Abstractions;
using OrgMembershipService.Application.Features.Roles.Queries;
using OrgMembershipService.Domain.Entities;
using OrgMembershipService.Domain.Exceptions;

namespace OrgMembershipService.Application.Features.Roles.Commands;

/// <summary>
/// Команда создания кастомной роли организации
/// </summary>
/// <param name="OrganizationId">Идентификатор организации</param>
/// <param name="Code">Код роли</param>
/// <param name="Name">Название роли</param>
/// <param name="Description">Описание роли</param>
/// <param name="PermissionCodes">Коды прав роли</param>
public record CreateOrganizationCustomRoleCommand(
    Guid OrganizationId,
    string Code,
    string Name,
    string? Description,
    IReadOnlyCollection<string> PermissionCodes) : IRequest<OrganizationRoleDto>;

internal class CreateOrganizationCustomRoleCommandValidator : AbstractValidator<CreateOrganizationCustomRoleCommand>
{
    public CreateOrganizationCustomRoleCommandValidator()
    {
        RuleFor(x => x.Code)
            .Must(x => !string.IsNullOrWhiteSpace(x))
            .WithMessage("Код роли обязателен");

        RuleFor(x => x.Name)
            .Must(x => !string.IsNullOrWhiteSpace(x))
            .WithMessage("Название роли обязательно");

        RuleFor(x => x.PermissionCodes)
            .NotNull()
            .WithMessage("Список прав обязателен")
            .NotEmpty()
            .WithMessage("Нужно указать хотя бы одно право");

        RuleForEach(x => x.PermissionCodes)
            .Must(x => !string.IsNullOrWhiteSpace(x))
            .WithMessage("Код права обязателен");
    }
}

internal class CreateOrganizationCustomRoleCommandHandler(IDbContext dbContext) : IRequestHandler<CreateOrganizationCustomRoleCommand, OrganizationRoleDto>
{
    public async Task<OrganizationRoleDto> Handle(CreateOrganizationCustomRoleCommand request, CancellationToken cancellationToken)
    {
        var roleCode = request.Code.Trim().ToUpperInvariant();
        var roleName = request.Name.Trim();
        var description = string.IsNullOrWhiteSpace(request.Description)
            ? null
            : request.Description.Trim();

        var roleCodeExists = await dbContext.Roles
            .AsNoTracking()
            .AnyAsync(
                x => x.OrganizationId == request.OrganizationId && x.Code == roleCode,
                cancellationToken);

        if (roleCodeExists)
            throw new ConflictException("ROLE_CODE_ALREADY_EXISTS", "Роль с таким кодом уже существует в организации");

        var requestedPermissionCodes = request.PermissionCodes
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim().ToUpperInvariant())
            .Distinct()
            .ToList();

        var permissions = await dbContext.Permissions
            .AsNoTracking()
            .Where(x => requestedPermissionCodes.Contains(x.Code))
            .Select(x => new { x.Id, x.Code })
            .ToListAsync(cancellationToken);

        var foundPermissionCodes = permissions
            .Select(x => x.Code)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var missingPermissionCodes = requestedPermissionCodes
            .Where(x => !foundPermissionCodes.Contains(x))
            .ToList();

        if (missingPermissionCodes.Count > 0)
            throw new NotFoundException("PERMISSION_NOT_FOUND", $"Права не найдены: {string.Join(", ", missingPermissionCodes)}");

        var role = new Role
        {
            Id = Guid.NewGuid(),
            Code = roleCode,
            Name = roleName,
            Description = description,
            OrganizationId = request.OrganizationId,
            Priority = 0
        };

        dbContext.Roles.Add(role);

        foreach (var permissionId in permissions.Select(x => x.Id))
        {
            dbContext.RolePermissions.Add(new RolePermission
            {
                RoleId = role.Id,
                PermissionId = permissionId
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        var sortedPermissionCodes = permissions
            .Select(x => x.Code)
            .Distinct()
            .OrderBy(x => x)
            .ToList();

        return new OrganizationRoleDto(
            role.Id,
            role.Code,
            role.Name,
            role.Description,
            false,
            sortedPermissionCodes);
    }
}
