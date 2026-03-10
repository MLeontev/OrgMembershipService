using MediatR;
using Microsoft.EntityFrameworkCore;
using OrgMembershipService.Application.Abstractions;
using OrgMembershipService.Domain.Entities;

namespace OrgMembershipService.Application.Features.Roles.Queries;

public record GetUserPermissionsQuery(Guid OrganizationId, Guid UserId) : IRequest<UserPermissionsDto>;

public record UserPermissionsDto(IReadOnlyCollection<string> Permissions);

internal class GetUserPermissionsQueryHandler(IDbContext dbContext) : IRequestHandler<GetUserPermissionsQuery, UserPermissionsDto>
{
    public async Task<UserPermissionsDto> Handle(GetUserPermissionsQuery request, CancellationToken cancellationToken)
    {
        var permissions = await dbContext.MembershipRoles
            .AsNoTracking()
            .Where(x =>
                x.Membership.OrganizationId == request.OrganizationId &&
                x.Membership.UserId == request.UserId &&
                x.Membership.Status == MembershipStatus.Active)
            .SelectMany(x => x.Role.RolePermissions)
            .Select(x => x.Permission.Code)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync(cancellationToken);
        
        return new UserPermissionsDto(permissions);
    }
}