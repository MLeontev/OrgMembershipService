using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OrgMembershipService.Application.Abstractions;
using OrgMembershipService.Domain.Exceptions;

namespace OrgMembershipService.Application.Features.Memberships.Queries;

/// <summary>
/// Запрос участника организации по идентификатору членства
/// </summary>
/// <param name="OrganizationId">Идентификатор организации</param>
/// <param name="MembershipId">Идентификатор членства</param>
public record GetOrganizationMemberByIdQuery(Guid OrganizationId, Guid MembershipId) : IRequest<OrganizationMemberDto>;

internal class GetOrganizationMemberByIdQueryHandler(IDbContext dbContext) : IRequestHandler<GetOrganizationMemberByIdQuery, OrganizationMemberDto>
{
    public async Task<OrganizationMemberDto> Handle(GetOrganizationMemberByIdQuery request, CancellationToken cancellationToken)
    {
        var member = await dbContext.Memberships
            .AsNoTracking()
            .Where(x => x.OrganizationId == request.OrganizationId && x.Id == request.MembershipId)
            .Select(x => new
            {
                MembershipId = x.Id,
                x.UserId,
                x.User.IdentityId,
                x.User.Email,
                x.User.FirstName,
                x.User.LastName,
                x.User.Patronymic,
                x.Status,
                x.Department,
                x.Title,
                x.JoinedAt,
                x.RemovedAt
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (member is null)
            throw new NotFoundException("MEMBERSHIP_NOT_FOUND", "Участник не найден в организации");

        var roles = await dbContext.MembershipRoles
            .AsNoTracking()
            .Where(x => x.MembershipId == request.MembershipId)
            .Select(x => x.Role.Code)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync(cancellationToken);

        return new OrganizationMemberDto(
            member.MembershipId,
            member.UserId,
            member.IdentityId,
            member.Email,
            member.FirstName,
            member.LastName,
            member.Patronymic,
            member.Status.ToString(),
            member.Department,
            member.Title,
            member.JoinedAt,
            member.RemovedAt,
            roles);
    }
}
