using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrgMembershipService.Api.Contracts;
using OrgMembershipService.Application.Features.Memberships.Queries;
using OrgMembershipService.Application.Features.Roles.Queries;

namespace OrgMembershipService.Api.Controllers.Internal;

/// <summary>
/// Internal ручки для чтения ролей и прав пользователя в организации
/// </summary>
[ApiController]
[Route("api/internal/organizations/{organizationId:guid}/users/{identityId}")]
[Produces("application/json")]
public class InternalUserAccessController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Возвращает агрегированный список кодов прав пользователя в организации
    /// </summary>
    /// <param name="organizationId">Идентификатор организации</param>
    /// <param name="identityId">Идентификатор пользователя в Keycloak (sub из access токена)</param>
    /// <param name="cancellationToken">Токен отмены</param>
    [HttpGet("permissions")]
    [ProducesResponseType(typeof(UserPermissionsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserPermissionsDto>> GetPermissions(
        [FromRoute] Guid organizationId,
        [FromRoute] string identityId,
        CancellationToken cancellationToken)
    {
        var permissions = await sender.Send(new GetUserPermissionsQuery(organizationId, identityId), cancellationToken);
        return Ok(permissions);
    }

    /// <summary>
    /// Возвращает агрегированный список кодов ролей пользователя в организации
    /// </summary>
    /// <param name="organizationId">Идентификатор организации</param>
    /// <param name="identityId">Идентификатор пользователя в Keycloak (sub из access токена)</param>
    /// <param name="cancellationToken">Токен отмены</param>
    [HttpGet("roles")]
    [ProducesResponseType(typeof(UserRolesDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserRolesDto>> GetRoles(
        [FromRoute] Guid organizationId,
        [FromRoute] string identityId,
        CancellationToken cancellationToken)
    {
        var roles = await sender.Send(new GetUserRolesQuery(organizationId, identityId), cancellationToken);
        return Ok(roles);
    }
    
    /// <summary>
    /// Возвращает членство пользователя в организации
    /// </summary>
    /// <param name="organizationId">Идентификатор организации</param>
    /// <param name="identityId">Идентификатор пользователя в Keycloak (sub из access токена)</param>
    /// <param name="cancellationToken">Токен отмены</param>
    [HttpGet("membership")]
    [ProducesResponseType(typeof(UserMembershipDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserMembershipDto>> GetMembership(
        [FromRoute] Guid organizationId,
        [FromRoute] string identityId,
        CancellationToken cancellationToken)
    {
        var membership = await sender.Send(new GetUserMembershipQuery(organizationId, identityId), cancellationToken);
        return Ok(membership);
    }
}
