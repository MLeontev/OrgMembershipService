using MediatR;
using Microsoft.EntityFrameworkCore;
using OrgMembershipService.Application.Abstractions;
using OrgMembershipService.Application.Services;
using OrgMembershipService.Domain.Entities;

namespace OrgMembershipService.Application.Features.Authorization.Queries;

public record CheckPermissionQuery(
    Guid OrganizationId,
    string IdentityId,
    string PermissionCode) : IRequest<CheckPermissionDto>;
    
public record CheckPermissionDto(bool Allowed);

internal class CheckPermissionQueryHandler(
    IDbContext dbContext,
    IUserIdentityResolver identityResolver) : IRequestHandler<CheckPermissionQuery, CheckPermissionDto>
{
    public async Task<CheckPermissionDto> Handle(CheckPermissionQuery request, CancellationToken cancellationToken)
    {
        var userId = await identityResolver.ResolveUserIdAsync(request.IdentityId, cancellationToken);
        
        var allowed = await dbContext.MembershipRoles
            .AsNoTracking()
            .Where(x =>
                x.Membership.OrganizationId == request.OrganizationId &&
                x.Membership.UserId == userId &&
                x.Membership.Status == MembershipStatus.Active)
            .SelectMany(x => x.Role.RolePermissions)
            .AnyAsync(
                x => x.Permission.Code == request.PermissionCode,
                cancellationToken);
        
        return new CheckPermissionDto(allowed);
    }
}