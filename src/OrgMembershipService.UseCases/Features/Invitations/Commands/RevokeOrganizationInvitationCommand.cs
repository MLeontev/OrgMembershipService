using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OrgMembershipService.Application.Abstractions;
using OrgMembershipService.Application.Services;
using OrgMembershipService.Domain.Entities;
using OrgMembershipService.Domain.Exceptions;

namespace OrgMembershipService.Application.Features.Invitations.Commands;

/// <summary>
/// Команда отзыва приглашения в организацию
/// </summary>
/// <param name="OrganizationId">Идентификатор организации</param>
/// <param name="InvitationId">Идентификатор приглашения</param>
/// <param name="RevokedByIdentityId">Идентификатор пользователя в Keycloak (sub из access токена)</param>
public record RevokeOrganizationInvitationCommand(
    Guid OrganizationId,
    Guid InvitationId,
    string RevokedByIdentityId) : IRequest;

internal class RevokeOrganizationInvitationCommandHandler(
    IDbContext dbContext,
    IUserIdentityResolver identityResolver) : IRequestHandler<RevokeOrganizationInvitationCommand>
{
    public async Task Handle(RevokeOrganizationInvitationCommand request, CancellationToken cancellationToken)
    {
        var revokedByUserId = await identityResolver.ResolveUserIdAsync(request.RevokedByIdentityId, cancellationToken);

        var revokerMembershipExists = await dbContext.Memberships
            .AsNoTracking()
            .AnyAsync(
                x => x.OrganizationId == request.OrganizationId &&
                     x.UserId == revokedByUserId &&
                     x.Status == MembershipStatus.Active,
                cancellationToken);

        if (!revokerMembershipExists)
            throw new NotFoundException("MEMBERSHIP_NOT_FOUND", "Участник не найден в организации");

        var invitation = await dbContext.Invitations
            .SingleOrDefaultAsync(
                x => x.OrganizationId == request.OrganizationId && x.Id == request.InvitationId,
                cancellationToken);

        if (invitation is null)
            throw new NotFoundException("INVITATION_NOT_FOUND", "Приглашение не найдено");

        invitation.Revoke();

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
