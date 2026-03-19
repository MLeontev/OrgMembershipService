using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OrgMembershipService.Application.Abstractions;
using OrgMembershipService.Application.Services;
using OrgMembershipService.Domain.Exceptions;

namespace OrgMembershipService.Application.Features.Memberships.Commands;

/// <summary>
/// Команда снятия роли с участника организации
/// </summary>
/// <param name="OrganizationId">Идентификатор организации</param>
/// <param name="MembershipId">Идентификатор членства</param>
/// <param name="ActorIdentityId">Идентификатор пользователя в Keycloak (sub из access токена)</param>
/// <param name="RoleCode">Код роли для снятия</param>
public record RevokeOrganizationMemberRoleCommand(
    Guid OrganizationId,
    Guid MembershipId,
    string ActorIdentityId,
    string RoleCode) : IRequest;

internal class RevokeOrganizationMemberRoleCommandValidator : AbstractValidator<RevokeOrganizationMemberRoleCommand>
{
    public RevokeOrganizationMemberRoleCommandValidator()
    {
        RuleFor(x => x.ActorIdentityId)
            .NotEmpty()
            .WithMessage("Идентификатор пользователя обязателен");

        RuleFor(x => x.RoleCode)
            .Must(x => !string.IsNullOrWhiteSpace(x))
            .WithMessage("Код роли обязателен");
    }
}

internal class RevokeOrganizationMemberRoleCommandHandler(
    IDbContext dbContext,
    IUserIdentityResolver identityResolver,
    IAccessPolicyService accessPolicy) : IRequestHandler<RevokeOrganizationMemberRoleCommand>
{
    public async Task Handle(RevokeOrganizationMemberRoleCommand request, CancellationToken cancellationToken)
    {
        var actorUserId = await identityResolver.ResolveUserIdAsync(request.ActorIdentityId, cancellationToken);

        var membership = await dbContext.Memberships
            .SingleOrDefaultAsync(
                x => x.OrganizationId == request.OrganizationId && x.Id == request.MembershipId,
                cancellationToken);

        if (membership is null)
            throw new NotFoundException("MEMBERSHIP_NOT_FOUND", "Участник не найден в организации");

        await accessPolicy.EnsureCanManageMembershipAsync(
            request.OrganizationId,
            actorUserId,
            membership,
            "ROLES_REVOKE",
            cancellationToken);

        var normalizedRoleCode = request.RoleCode.Trim().ToUpperInvariant();

        var candidateRoleIds = await dbContext.Roles
            .AsNoTracking()
            .Where(x =>
                x.Code == normalizedRoleCode &&
                (x.OrganizationId == null || x.OrganizationId == request.OrganizationId))
            .OrderByDescending(x => x.OrganizationId == request.OrganizationId)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        if (candidateRoleIds.Count == 0)
            throw new NotFoundException("ROLE_NOT_FOUND", "Роль не найдена");

        var membershipRole = await dbContext.MembershipRoles
            .Where(x => x.MembershipId == request.MembershipId && candidateRoleIds.Contains(x.RoleId))
            .OrderByDescending(x => x.Role.OrganizationId == request.OrganizationId)
            .FirstOrDefaultAsync(cancellationToken);

        if (membershipRole is null)
            throw new NotFoundException("MEMBERSHIP_ROLE_NOT_FOUND", "У участника нет этой роли");

        dbContext.MembershipRoles.Remove(membershipRole);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
