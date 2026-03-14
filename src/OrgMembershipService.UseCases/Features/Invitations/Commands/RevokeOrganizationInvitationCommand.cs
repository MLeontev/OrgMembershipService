using MediatR;
using Microsoft.EntityFrameworkCore;
using OrgMembershipService.Application.Abstractions;
using OrgMembershipService.Domain.Exceptions;

namespace OrgMembershipService.Application.Features.Invitations.Commands;

/// <summary>
/// Команда отзыва приглашения в организацию
/// </summary>
/// <param name="OrganizationId">Идентификатор организации</param>
/// <param name="InvitationId">Идентификатор приглашения</param>
public record RevokeOrganizationInvitationCommand(
    Guid OrganizationId,
    Guid InvitationId) : IRequest;

internal class RevokeOrganizationInvitationCommandHandler(IDbContext dbContext) : IRequestHandler<RevokeOrganizationInvitationCommand>
{
    public async Task Handle(RevokeOrganizationInvitationCommand request, CancellationToken cancellationToken)
    {
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
