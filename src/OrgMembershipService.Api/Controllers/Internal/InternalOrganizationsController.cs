using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrgMembershipService.Api.Contracts;
using OrgMembershipService.Application.Features.Organizations.Commands;

namespace OrgMembershipService.Api.Controllers.Internal;

/// <summary>
/// Internal ручки для управления организационным членством
/// </summary>
[ApiController]
[Route("api/internal/organizations")]
[Produces("application/json")]
public class InternalOrganizationsController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Создаёт владельца организации (создание членства + роль <c>ORG_OWNER</c>) по identityId пользователя
    /// </summary>
    /// <param name="command">Идентификатор организации и ownerIdentityId</param>
    /// <param name="cancellationToken">Токен отмены</param>
    [HttpPost("owner")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateOwner(
        [FromBody] CreateOrganizationOwnerCommand command,
        CancellationToken cancellationToken)
    {
        await sender.Send(command, cancellationToken);
        return Ok();
    }

    /// <summary>
    /// Удаляет данные организации в OrgMembershipService (членства, приглашения, кастомные роли)
    /// </summary>
    /// <param name="organizationId">Идентификатор организации</param>
    /// <param name="cancellationToken">Токен отмены</param>
    [HttpPost("{organizationId:guid}/delete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteOrganization(
        [FromRoute] Guid organizationId,
        CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteOrganizationCommand(organizationId), cancellationToken);
        return Ok();
    }
}
