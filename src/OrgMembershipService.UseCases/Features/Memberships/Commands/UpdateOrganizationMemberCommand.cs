using MediatR;
using Microsoft.EntityFrameworkCore;
using OrgMembershipService.Application.Abstractions;
using OrgMembershipService.Domain.Exceptions;

namespace OrgMembershipService.Application.Features.Memberships.Commands;

/// <summary>
/// Команда обновления данных участника организации
/// </summary>
/// <param name="OrganizationId">Идентификатор организации</param>
/// <param name="MembershipId">Идентификатор членства</param>
/// <param name="Department">Подразделение пользователя</param>
/// <param name="Title">Должность пользователя</param>
public record UpdateOrganizationMemberCommand(
    Guid OrganizationId,
    Guid MembershipId,
    string? Department,
    string? Title) : IRequest;

internal class UpdateOrganizationMemberCommandHandler(IDbContext dbContext) : IRequestHandler<UpdateOrganizationMemberCommand>
{
    public async Task Handle(UpdateOrganizationMemberCommand request, CancellationToken cancellationToken)
    {
        var membership = await dbContext.Memberships
            .SingleOrDefaultAsync(
                x => x.OrganizationId == request.OrganizationId && x.Id == request.MembershipId,
                cancellationToken);

        if (membership is null)
            throw new NotFoundException("MEMBERSHIP_NOT_FOUND", "Участник не найден в организации");

        membership.UpdateProfile(request.Department, request.Title);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
