using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OrgMembershipService.Application.Abstractions;
using OrgMembershipService.Application.Services;
using OrgMembershipService.Domain.Exceptions;

namespace OrgMembershipService.Application.Features.Memberships.Commands;

/// <summary>
/// Команда назначения ролей участнику организации
/// </summary>
/// <param name="OrganizationId">Идентификатор организации</param>
/// <param name="MembershipId">Идентификатор членства</param>
/// <param name="AssignedByIdentityId">Идентификатор пользователя в Keycloak (sub из access токена)</param>
/// <param name="RoleCodes">Коды ролей для назначения</param>
public record AssignOrganizationMemberRolesCommand(
    Guid OrganizationId,
    Guid MembershipId,
    string AssignedByIdentityId,
    IReadOnlyCollection<string> RoleCodes) : IRequest;

internal class AssignOrganizationMemberRolesCommandValidator : AbstractValidator<AssignOrganizationMemberRolesCommand>
{
    public AssignOrganizationMemberRolesCommandValidator()
    {
        RuleFor(x => x.AssignedByIdentityId)
            .NotEmpty()
            .WithMessage("Идентификатор назначающего пользователя обязателен");

        RuleFor(x => x.RoleCodes)
            .NotNull()
            .WithMessage("Список ролей обязателен")
            .NotEmpty()
            .WithMessage("Нужно указать хотя бы одну роль");

        RuleForEach(x => x.RoleCodes)
            .Must(x => !string.IsNullOrWhiteSpace(x))
            .WithMessage("Код роли обязателен");
    }
}

internal class AssignOrganizationMemberRolesCommandHandler(
    IDbContext dbContext,
    IUserIdentityResolver identityResolver,
    IAccessPolicyService accessPolicy) : IRequestHandler<AssignOrganizationMemberRolesCommand>
{
    public async Task Handle(AssignOrganizationMemberRolesCommand request, CancellationToken cancellationToken)
    {
        var assignedByUserId = await identityResolver.ResolveUserIdAsync(request.AssignedByIdentityId, cancellationToken);

        var membership = await dbContext.Memberships
            .SingleOrDefaultAsync(
                x => x.OrganizationId == request.OrganizationId && x.Id == request.MembershipId,
                cancellationToken);

        if (membership is null)
            throw new NotFoundException("MEMBERSHIP_NOT_FOUND", "Участник не найден в организации");

        var requestedRoleCodes = request.RoleCodes
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim().ToUpperInvariant())
            .Distinct()
            .ToList();

        var roleCandidates = await dbContext.Roles
            .AsNoTracking()
            .Where(x =>
                requestedRoleCodes.Contains(x.Code) &&
                (x.OrganizationId == null || x.OrganizationId == request.OrganizationId))
            .Select(x => new { x.Id, x.Code, x.OrganizationId, x.Priority })
            .ToListAsync(cancellationToken);

        var selectedRoles = roleCandidates
            .GroupBy(x => x.Code)
            .Select(g => g
                .OrderByDescending(x => x.OrganizationId == request.OrganizationId)
                .First())
            .ToList();

        var foundRoleCodes = selectedRoles
            .Select(x => x.Code)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var missingRoleCodes = requestedRoleCodes
            .Where(x => !foundRoleCodes.Contains(x))
            .ToList();

        if (missingRoleCodes.Count > 0)
            throw new NotFoundException("ROLE_NOT_FOUND", $"Роли не найдены: {string.Join(", ", missingRoleCodes)}");

        await accessPolicy.EnsureCanAssignRolesAsync(
            request.OrganizationId,
            assignedByUserId,
            membership,
            selectedRoles
                .Select(x => new AccessRoleCandidate(x.Code, x.Priority))
                .ToList(),
            cancellationToken);

        var existingRoleIds = await dbContext.MembershipRoles
            .AsNoTracking()
            .Where(x => x.MembershipId == membership.Id)
            .Select(x => x.RoleId)
            .ToHashSetAsync(cancellationToken);

        foreach (var roleId in selectedRoles.Select(x => x.Id).Where(x => !existingRoleIds.Contains(x)))
            membership.AssignRole(roleId, assignedByUserId);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
