using MediatR;
using Microsoft.EntityFrameworkCore;
using OrgMembershipService.Application.Abstractions;
using OrgMembershipService.Domain.Exceptions;

namespace OrgMembershipService.Application.Features.Memberships.Commands;

/// <summary>
/// Команда удаления участника из организации
/// </summary>
/// <param name="OrganizationId">Идентификатор организации</param>
/// <param name="MembershipId">Идентификатор членства</param>
public record RemoveOrganizationMemberCommand(Guid OrganizationId, Guid MembershipId) : IRequest;

internal class RemoveOrganizationMemberCommandHandler(IDbContext dbContext) : IRequestHandler<RemoveOrganizationMemberCommand>
{
    public async Task Handle(RemoveOrganizationMemberCommand request, CancellationToken cancellationToken)
    {
        var membership = await dbContext.Memberships
            .SingleOrDefaultAsync(
                x => x.OrganizationId == request.OrganizationId && x.Id == request.MembershipId,
                cancellationToken);

        if (membership is null)
            throw new NotFoundException("MEMBERSHIP_NOT_FOUND", "Участник не найден в организации");

        membership.Remove();

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
