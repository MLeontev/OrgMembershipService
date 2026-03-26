using System.Security.Cryptography;
using System.Text;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OrgMembershipService.Application.Abstractions;
using OrgMembershipService.Domain.Entities;
using OrgMembershipService.Domain.Exceptions;

namespace OrgMembershipService.Application.Features.Invitations.Queries;

/// <summary>
/// Запрос приглашения по токену
/// </summary>
/// <param name="Token">Токен приглашения из ссылки</param>
public record GetInvitationByTokenQuery(string Token) : IRequest<InvitationByTokenDto>;

/// <summary>
/// Данные приглашения по токену
/// </summary>
/// <param name="InvitationId">Идентификатор приглашения</param>
/// <param name="OrganizationId">Идентификатор организации</param>
/// <param name="Status">Статус приглашения строкой (Pending, Accepted, Revoked, Expired)</param>
/// <param name="ExpiresAt">Дата и время окончания действия приглашения</param>
/// <param name="CanAccept">Можно ли принять приглашение</param>
/// <param name="RoleCodes">Коды ролей, назначаемых при принятии приглашения</param>
/// <param name="RoleNames">Названия ролей, назначаемых при принятии приглашения</param>
public record InvitationByTokenDto(
    Guid InvitationId,
    Guid OrganizationId,
    string Status,
    DateTimeOffset ExpiresAt,
    bool CanAccept,
    IReadOnlyCollection<string> RoleCodes,
    IReadOnlyCollection<string> RoleNames);

internal class GetInvitationByTokenQueryHandler(IDbContext dbContext) : IRequestHandler<GetInvitationByTokenQuery, InvitationByTokenDto>
{
    public async Task<InvitationByTokenDto> Handle(GetInvitationByTokenQuery request, CancellationToken cancellationToken)
    {
        var tokenHash = ComputeSha256(request.Token);

        var invitation = await dbContext.Invitations
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken);

        if (invitation is null)
            throw new NotFoundException("INVITATION_NOT_FOUND", "Приглашение не найдено");

        var roles = await dbContext.InvitationRoles
            .AsNoTracking()
            .Where(x => x.InvitationId == invitation.Id)
            .Select(x => new { x.Role.Code, x.Role.Name })
            .Distinct()
            .OrderBy(x => x.Code)
            .ToListAsync(cancellationToken);

        var roleCodes = roles
            .Select(x => x.Code)
            .ToList();

        var roleNames = roles
            .Select(x => x.Name)
            .ToList();

        var now = DateTimeOffset.UtcNow;
        var effectiveStatus = invitation.IsExpired(now)
            ? InvitationStatus.Expired
            : invitation.Status;

        return new InvitationByTokenDto(
            invitation.Id,
            invitation.OrganizationId,
            effectiveStatus.ToString(),
            invitation.ExpiresAt,
            effectiveStatus == InvitationStatus.Pending,
            roleCodes,
            roleNames);
    }

    private static string ComputeSha256(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
