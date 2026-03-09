using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrgMembershipService.Application.Features.Users.Queries;

namespace OrgMembershipService.Api.Controllers.Internal;

[ApiController]
[Route("api/internal/users")]
public class InternalUsersController(ISender sender) : ControllerBase
{
    [HttpGet("by-identity/{identityId}")]
    public async Task<IActionResult> GetByIdentity(string identityId, CancellationToken cancellationToken)
    {
        var user = await sender.Send(new GetUserByIdentityQuery(identityId), cancellationToken);
        return Ok(user);
    }
}