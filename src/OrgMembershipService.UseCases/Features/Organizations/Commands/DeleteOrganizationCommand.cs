using MediatR;
using Microsoft.EntityFrameworkCore;
using OrgMembershipService.Application.Abstractions;

namespace OrgMembershipService.Application.Features.Organizations.Commands;

/// <summary>
/// Команда удаления данных организации в OrgMembershipService
/// </summary>
/// <param name="OrganizationId">Идентификатор организации</param>
public record DeleteOrganizationCommand(Guid OrganizationId) : IRequest;

internal class DeleteOrganizationCommandHandler(IDbContext dbContext) : IRequestHandler<DeleteOrganizationCommand>
{
    public async Task Handle(DeleteOrganizationCommand request, CancellationToken cancellationToken)
    {
        await dbContext.Memberships
            .Where(x => x.OrganizationId == request.OrganizationId)
            .ExecuteDeleteAsync(cancellationToken);

        await dbContext.Invitations
            .Where(x => x.OrganizationId == request.OrganizationId)
            .ExecuteDeleteAsync(cancellationToken);

        await dbContext.Roles
            .Where(x => x.OrganizationId == request.OrganizationId)
            .ExecuteDeleteAsync(cancellationToken);
    }
}
