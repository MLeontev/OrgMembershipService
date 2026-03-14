using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrgMembershipService.Api.Contracts;
using OrgMembershipService.Api.Extensions;
using OrgMembershipService.Application.Features.Invitations.Commands;
using OrgMembershipService.Application.Features.Invitations.Queries;

namespace OrgMembershipService.Api.Controllers.Public;

/// <summary>
/// Операции с приглашениями
/// </summary>
[ApiController]
[Route("api/organizations/{organizationId:guid}/invitations")]
[Produces("application/json")]
public class OrganizationInvitationsController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Возвращает список приглашений организации
    /// </summary>
    /// <param name="organizationId">Идентификатор организации</param>
    /// <param name="status">Фильтр по статусу приглашения (Pending, Accepted, Revoked, Expired)</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Список приглашений организации</returns>
    [Authorize]
    [HttpGet]
    [ProducesResponseType(typeof(OrganizationInvitationsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OrganizationInvitationsDto>> GetList(
        [FromRoute] Guid organizationId,
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        var invitations = await sender.Send(new GetOrganizationInvitationsQuery(organizationId, status), cancellationToken);
        return Ok(invitations);
    }

    /// <summary>
    /// Отзывает приглашение в организацию
    /// </summary>
    /// <param name="organizationId">Идентификатор организации</param>
    /// <param name="invitationId">Идентификатор приглашения</param>
    /// <param name="cancellationToken">Токен отмены</param>
    [Authorize]
    [HttpPost("{invitationId:guid}/revoke")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Revoke(
        [FromRoute] Guid organizationId,
        [FromRoute] Guid invitationId,
        CancellationToken cancellationToken)
    {
        var identityId = User.GetRequiredIdentityId();

        await sender.Send(
            new RevokeOrganizationInvitationCommand(
                organizationId,
                invitationId,
                identityId),
            cancellationToken);

        return Ok();
    }

    /// <summary>
    /// Создает приглашение в организацию
    /// </summary>
    /// <param name="organizationId">Идентификатор организации</param>
    /// <param name="request">Данные для создания приглашения</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Идентификатор и токен приглашения</returns>
    [Authorize]
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(CreateOrganizationInvitationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CreateOrganizationInvitationDto>> Create(
        [FromRoute] Guid organizationId,
        [FromBody] CreateOrganizationInvitationRequest request,
        CancellationToken cancellationToken)
    {
        var identityId = User.GetRequiredIdentityId();

        var invitation = await sender.Send(
            new CreateOrganizationInvitationCommand(
                organizationId,
                identityId,
                request.RoleCodes,
                request.ExpiresAt),
            cancellationToken);

        return Ok(invitation);
    }
}
