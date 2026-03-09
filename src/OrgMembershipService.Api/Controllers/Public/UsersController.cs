using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrgMembershipService.Application.Features.Users.Commands;

namespace OrgMembershipService.Api.Controllers.Public;

[ApiController]
[Route("api/[controller]")]
public class UsersController(ISender sender) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var userId = await sender.Send(request, cancellationToken);
        return Ok(userId);
    }
}