using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrgMembershipService.Api.Contracts;
using OrgMembershipService.Application.Features.Authorization.Queries;

namespace OrgMembershipService.Api.Controllers.Internal;

/// <summary>
/// Internal ручки для проверок авторизации
/// </summary>
[ApiController]
[Route("api/internal/authorization")]
[Produces("application/json")]
public class InternalAuthorizationController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Проверяет наличие права у пользователя в рамках организации
    /// </summary>
    /// <param name="request">Организация, identityId пользователя и код права</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Результат проверки права</returns>
    [HttpPost("check")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(CheckPermissionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CheckPermissionDto>> Check(
        [FromBody] CheckPermissionQuery request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(request, cancellationToken);
        return Ok(result);
    }
}
