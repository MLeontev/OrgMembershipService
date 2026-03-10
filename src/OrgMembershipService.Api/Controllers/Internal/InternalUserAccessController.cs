using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrgMembershipService.Application.Features.Roles.Queries;

namespace OrgMembershipService.Api.Controllers.Internal;

[ApiController]
[Route("api/internal/organizations/{organizationId:guid}/users/{identityId}")]
public class InternalUserAccessController(ISender sender) : ControllerBase
{
    [HttpGet("permissions")]
    public async Task<IActionResult> GetPermissions(Guid organizationId, string identityId, CancellationToken cancellationToken)
    {
        var permissions = await sender.Send(
            new GetUserPermissionsQuery(organizationId, identityId),
            cancellationToken);

        return Ok(permissions);
    }

    [HttpGet("roles")]
    public async Task<IActionResult> GetRoles(Guid organizationId, string identityId, CancellationToken cancellationToken)
    {
        var roles = await sender.Send(
            new GetUserRolesQuery(organizationId, identityId),
            cancellationToken);

        return Ok(roles);
    }
}