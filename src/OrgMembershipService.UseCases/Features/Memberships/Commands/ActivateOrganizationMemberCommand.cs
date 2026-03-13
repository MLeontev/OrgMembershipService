using MediatR;
using Microsoft.EntityFrameworkCore;
using OrgMembershipService.Application.Abstractions;
using OrgMembershipService.Domain.Exceptions;

namespace OrgMembershipService.Application.Features.Memberships.Commands;

/// <summary>
/// Команда активации участника организации
/// </summary>
/// <param name="OrganizationId">Идентификатор организации</param>
/// <param name="MembershipId">Идентификатор членства</param>
public record ActivateOrganizationMemberCommand(Guid OrganizationId, Guid MembershipId) : IRequest;

internal class ActivateOrganizationMemberCommandHandler(IDbContext dbContext) : IRequestHandler<ActivateOrganizationMemberCommand>
{
    public async Task Handle(ActivateOrganizationMemberCommand request, CancellationToken cancellationToken)
    {
        var membership = await dbContext.Memberships
            .SingleOrDefaultAsync(
                x => x.OrganizationId == request.OrganizationId && x.Id == request.MembershipId,
                cancellationToken);

        if (membership is null)
            throw new NotFoundException("MEMBERSHIP_NOT_FOUND", "Участник не найден в организации");

        membership.Activate();

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
