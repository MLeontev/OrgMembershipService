using MediatR;
using Microsoft.EntityFrameworkCore;
using OrgMembershipService.Application.Abstractions;
using OrgMembershipService.Application.Services;
using OrgMembershipService.Domain.Exceptions;

namespace OrgMembershipService.Application.Features.Memberships.Commands;

/// <summary>
/// Команда деактивации участника организации
/// </summary>
/// <param name="OrganizationId">Идентификатор организации</param>
/// <param name="MembershipId">Идентификатор членства</param>
/// <param name="ActorIdentityId">Идентификатор пользователя в Keycloak (sub из access токена)</param>
public record DeactivateOrganizationMemberCommand(Guid OrganizationId, Guid MembershipId, string ActorIdentityId) : IRequest;

internal class DeactivateOrganizationMemberCommandHandler(
    IDbContext dbContext,
    IUserIdentityResolver identityResolver,
    IAccessPolicyService accessPolicy) : IRequestHandler<DeactivateOrganizationMemberCommand>
{
    public async Task Handle(DeactivateOrganizationMemberCommand request, CancellationToken cancellationToken)
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
            "MEMBERS_DEACTIVATE",
            cancellationToken);

        membership.Deactivate();

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
