using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrgMembershipService.Application.Features.Authorization.Queries;

namespace OrgMembershipService.Api.Controllers.Internal;

[ApiController]
[Route("api/internal/authorization")]
public class InternalAuthorizationController(ISender sender) : ControllerBase
{
    [HttpPost("check")]
    public async Task<IActionResult> Check([FromBody] CheckPermissionQuery request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(request, cancellationToken);
        return Ok(result);
    }
}