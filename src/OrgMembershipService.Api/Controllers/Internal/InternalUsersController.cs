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

    /// <summary>
    /// Возвращает список организаций пользователя
    /// </summary>
    /// <param name="identityId">Идентификатор пользователя в Keycloak (sub из access токена)</param>
    /// <param name="status">Фильтр по статусу членства (Active, Deactivated, Removed)</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Список организаций пользователя</returns>
    [HttpGet("{identityId}/organizations")]
    [ProducesResponseType(typeof(UserOrganizationsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserOrganizationsDto>> GetOrganizations(
        [FromRoute] string identityId,
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        var organizations = await sender.Send(
            new GetUserOrganizationsQuery(identityId, status),
            cancellationToken);
        
        return Ok(organizations);
    }
}
