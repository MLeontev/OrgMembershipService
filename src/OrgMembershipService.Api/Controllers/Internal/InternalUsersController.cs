using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrgMembershipService.Api.Contracts;
using OrgMembershipService.Application.Features.Users.Queries;

namespace OrgMembershipService.Api.Controllers.Internal;

/// <summary>
/// Internal ручки для получения пользовательских данных
/// </summary>
[ApiController]
[Route("api/internal/users")]
[Produces("application/json")]
public class InternalUsersController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Возвращает пользователя по identityId (sub из access токена Keycloak)
    /// </summary>
    /// <param name="identityId">Идентификатор пользователя в Keycloak (sub из access токена)</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Профиль пользователя</returns>
    [HttpGet("by-identity/{identityId}")]
    [ProducesResponseType(typeof(UserByIdentityDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserByIdentityDto>> GetByIdentity(
        [FromRoute] string identityId,
        CancellationToken cancellationToken)
    {
        var user = await sender.Send(new GetUserByIdentityQuery(identityId), cancellationToken);
        return Ok(user);
    }
}
