using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrgMembershipService.Application.Features.Organizations.Commands;

namespace OrgMembershipService.Api.Controllers.Internal;

[ApiController]
[Route("api/internal/organizations")]
public class InternalOrganizationsController(ISender sender) : ControllerBase
{
    [HttpPost("owner")]
    public async Task<IActionResult> CreateOwner(
        [FromBody] CreateOrganizationOwnerCommand command,
        CancellationToken cancellationToken)
    {
        await sender.Send(command, cancellationToken);
        return Ok();
    }
}