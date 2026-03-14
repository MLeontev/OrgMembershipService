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
/// Команда принятия приглашения по токену
/// </summary>
/// <param name="Token">Токен приглашения из ссылки</param>
/// <param name="IdentityId">Идентификатор пользователя в Keycloak (sub из access токена)</param>
public record AcceptInvitationByTokenCommand(string Token, string IdentityId) : IRequest;

internal class AcceptInvitationByTokenCommandHandler(
    IDbContext dbContext,
    IUserIdentityResolver identityResolver) : IRequestHandler<AcceptInvitationByTokenCommand>
{
    public async Task Handle(AcceptInvitationByTokenCommand request, CancellationToken cancellationToken)
    {
        var userId = await identityResolver.ResolveUserIdAsync(request.IdentityId, cancellationToken);

        var tokenHash = ComputeSha256(request.Token);

        var invitation = await dbContext.Invitations
            .SingleOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken);

        if (invitation is null)
            throw new NotFoundException("INVITATION_NOT_FOUND", "Приглашение не найдено");

        var roleIds = await dbContext.InvitationRoles
            .AsNoTracking()
            .Where(x => x.InvitationId == invitation.Id)
            .Select(x => x.RoleId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var membership = await dbContext.Memberships
            .SingleOrDefaultAsync(
                x => x.OrganizationId == invitation.OrganizationId && x.UserId == userId,
                cancellationToken);

        if (membership is null)
        {
            membership = Membership.Create(invitation.OrganizationId, userId);
            dbContext.Memberships.Add(membership);
        }
        else
        {
            membership.Activate();
        }

        foreach (var roleId in roleIds)
            membership.AssignRole(roleId, invitation.CreatedByUserId);

        invitation.Accept(userId);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static string ComputeSha256(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
