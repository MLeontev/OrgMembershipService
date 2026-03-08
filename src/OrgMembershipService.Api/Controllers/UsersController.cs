using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrgMembershipService.Application.Users.Commands;

namespace OrgMembershipService.Api.Controllers;

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