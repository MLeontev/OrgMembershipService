using System.Security.Cryptography;
using System.Text;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OrgMembershipService.Application.Abstractions;
using OrgMembershipService.Application.Services;
using OrgMembershipService.Domain.Entities;
using OrgMembershipService.Domain.Exceptions;

namespace OrgMembershipService.Application.Features.Invitations.Commands;

/// <summary>
/// Команда создания приглашения в организацию
/// </summary>
/// <param name="OrganizationId">Идентификатор организации</param>
/// <param name="CreatedByIdentityId">Идентификатор пользователя в Keycloak (sub из access токена)</param>
/// <param name="RoleCodes">Коды ролей для назначения при принятии приглашения</param>
/// <param name="ExpiresAt">Дата и время окончания действия приглашения</param>
public record CreateOrganizationInvitationCommand(
    Guid OrganizationId,
    string CreatedByIdentityId,
    IReadOnlyCollection<string> RoleCodes,
    DateTimeOffset ExpiresAt) : IRequest<CreateOrganizationInvitationDto>;

/// <summary>
/// Результат создания приглашения
/// </summary>
/// <param name="InvitationId">Идентификатор приглашения</param>
/// <param name="InvitationToken">Токен приглашения в открытом виде</param>
/// <param name="ExpiresAt">Дата и время окончания действия приглашения</param>
public record CreateOrganizationInvitationDto(
    Guid InvitationId,
    string InvitationToken,
    DateTimeOffset ExpiresAt);

internal class CreateOrganizationInvitationCommandValidator : AbstractValidator<CreateOrganizationInvitationCommand>
{
    public CreateOrganizationInvitationCommandValidator()
    {
        RuleFor(x => x.RoleCodes)
            .NotNull()
            .WithMessage("Список ролей обязателен")
            .NotEmpty()
            .WithMessage("Нужно указать хотя бы одну роль");

        RuleForEach(x => x.RoleCodes)
            .NotEmpty()
            .WithMessage("Код роли обязателен");

        RuleFor(x => x.ExpiresAt)
            .Must(expiresAt => expiresAt > DateTimeOffset.UtcNow)
            .WithMessage("Дата окончания должна быть в будущем");
    }
}

internal class CreateOrganizationInvitationCommandHandler(
    IDbContext dbContext,
    IUserIdentityResolver identityResolver) : IRequestHandler<CreateOrganizationInvitationCommand, CreateOrganizationInvitationDto>
{
    public async Task<CreateOrganizationInvitationDto> Handle(CreateOrganizationInvitationCommand request, CancellationToken cancellationToken)
    {
        var createdByUserId = await identityResolver.ResolveUserIdAsync(request.CreatedByIdentityId, cancellationToken);

        var creatorMembershipExists = await dbContext.Memberships
            .AsNoTracking()
            .AnyAsync(
                x => x.OrganizationId == request.OrganizationId &&
                     x.UserId == createdByUserId &&
                     x.Status == MembershipStatus.Active,
                cancellationToken);

        if (!creatorMembershipExists)
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
            .Select(x => new { x.Id, x.Code, x.OrganizationId })
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

        var token = GenerateToken();
        var tokenHash = ComputeSha256(token);

        var invitation = Invitation.Create(
            request.OrganizationId,
            tokenHash,
            request.ExpiresAt,
            createdByUserId,
            selectedRoles.Select(x => x.Id).ToList());

        dbContext.Invitations.Add(invitation);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new CreateOrganizationInvitationDto(
            invitation.Id,
            token,
            invitation.ExpiresAt);
    }

    private static string GenerateToken() =>
        Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();

    private static string ComputeSha256(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
