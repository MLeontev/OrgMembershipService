using MediatR;
using Microsoft.EntityFrameworkCore;
using OrgMembershipService.Application.Abstractions;
using OrgMembershipService.Application.Services;
using OrgMembershipService.Domain.Exceptions;

namespace OrgMembershipService.Application.Features.Memberships.Commands;

/// <summary>
/// Команда обновления данных участника организации
/// </summary>
/// <param name="OrganizationId">Идентификатор организации</param>
/// <param name="MembershipId">Идентификатор членства</param>
/// <param name="ActorIdentityId">Идентификатор пользователя в Keycloak (sub из access токена)</param>
/// <param name="Department">Подразделение пользователя</param>
/// <param name="Title">Должность пользователя</param>
public record UpdateOrganizationMemberCommand(
    Guid OrganizationId,
    Guid MembershipId,
    string ActorIdentityId,
    string? Department,
    string? Title) : IRequest;

internal class UpdateOrganizationMemberCommandHandler(
    IDbContext dbContext,
    IUserIdentityResolver identityResolver,
    IAccessPolicyService accessPolicy) : IRequestHandler<UpdateOrganizationMemberCommand>
{
    public async Task Handle(UpdateOrganizationMemberCommand request, CancellationToken cancellationToken)
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
            "MEMBERS_UPDATE",
            cancellationToken);

        membership.UpdateProfile(request.Department, request.Title);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
