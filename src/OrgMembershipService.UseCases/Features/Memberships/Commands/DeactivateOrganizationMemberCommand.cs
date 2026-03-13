using MediatR;
using Microsoft.EntityFrameworkCore;
using OrgMembershipService.Application.Abstractions;
using OrgMembershipService.Domain.Exceptions;

namespace OrgMembershipService.Application.Features.Memberships.Commands;

/// <summary>
/// Команда деактивации участника организации
/// </summary>
/// <param name="OrganizationId">Идентификатор организации</param>
/// <param name="MembershipId">Идентификатор членства</param>
public record DeactivateOrganizationMemberCommand(Guid OrganizationId, Guid MembershipId) : IRequest;

internal class DeactivateOrganizationMemberCommandHandler(IDbContext dbContext) : IRequestHandler<DeactivateOrganizationMemberCommand>
{
    public async Task Handle(DeactivateOrganizationMemberCommand request, CancellationToken cancellationToken)
    {
        var membership = await dbContext.Memberships
            .SingleOrDefaultAsync(
                x => x.OrganizationId == request.OrganizationId && x.Id == request.MembershipId,
                cancellationToken);

        if (membership is null)
            throw new NotFoundException("MEMBERSHIP_NOT_FOUND", "Участник не найден в организации");

        membership.Deactivate();

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
