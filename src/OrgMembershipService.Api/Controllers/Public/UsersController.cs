using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrgMembershipService.Api.Contracts;
using OrgMembershipService.Application.Features.Users.Commands;

namespace OrgMembershipService.Api.Controllers.Public;

/// <summary>
/// Операции с пользователями
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class UsersController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Регистрация пользователя в приложении и Keycloak
    /// </summary>
    /// <param name="request">Данные для регистрации пользователя</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Идентификаторы пользователя в сервисе и в Keycloak</returns>
    [HttpPost("register")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(RegisterUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<RegisterUserDto>> Register(
        [FromBody] RegisterUserCommand request,
        CancellationToken cancellationToken)
    {
        var response = await sender.Send(request, cancellationToken);
        return Ok(response);
    }
}
