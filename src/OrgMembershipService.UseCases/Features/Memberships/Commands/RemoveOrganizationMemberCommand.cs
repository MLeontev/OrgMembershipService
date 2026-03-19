using MediatR;
using Microsoft.EntityFrameworkCore;
using OrgMembershipService.Application.Abstractions;
using OrgMembershipService.Application.Services;
using OrgMembershipService.Domain.Exceptions;

namespace OrgMembershipService.Application.Features.Memberships.Commands;

/// <summary>
/// Команда удаления участника из организации
/// </summary>
/// <param name="OrganizationId">Идентификатор организации</param>
/// <param name="MembershipId">Идентификатор членства</param>
/// <param name="ActorIdentityId">Идентификатор пользователя в Keycloak (sub из access токена)</param>
public record RemoveOrganizationMemberCommand(Guid OrganizationId, Guid MembershipId, string ActorIdentityId) : IRequest;

internal class RemoveOrganizationMemberCommandHandler(
    IDbContext dbContext,
    IUserIdentityResolver identityResolver,
    IAccessPolicyService accessPolicy) : IRequestHandler<RemoveOrganizationMemberCommand>
{
    public async Task Handle(RemoveOrganizationMemberCommand request, CancellationToken cancellationToken)
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
            "MEMBERS_REMOVE",
            cancellationToken);

        membership.Remove();

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
