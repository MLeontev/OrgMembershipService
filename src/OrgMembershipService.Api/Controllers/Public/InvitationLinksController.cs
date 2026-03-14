using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrgMembershipService.Api.Contracts;
using OrgMembershipService.Api.Extensions;
using OrgMembershipService.Application.Features.Invitations.Commands;
using OrgMembershipService.Application.Features.Invitations.Queries;

namespace OrgMembershipService.Api.Controllers.Public;

/// <summary>
/// Операции с приглашениями по токену
/// </summary>
[ApiController]
[Route("api/invitations/{token}")]
[Produces("application/json")]
public class InvitationLinksController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Возвращает приглашение по токену
    /// </summary>
    /// <param name="token">Токен приглашения из ссылки</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Данные приглашения</returns>
    [AllowAnonymous]
    [HttpGet("")]
    [ProducesResponseType(typeof(InvitationByTokenDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<InvitationByTokenDto>> GetByToken(
        [FromRoute] string token,
        CancellationToken cancellationToken)
    {
        var invitation = await sender.Send(new GetInvitationByTokenQuery(token), cancellationToken);
        return Ok(invitation);
    }

    /// <summary>
    /// Принимает приглашение по токену
    /// </summary>
    /// <param name="token">Токен приглашения из ссылки</param>
    /// <param name="cancellationToken">Токен отмены</param>
    [Authorize]
    [HttpPost("accept")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Accept(
        [FromRoute] string token,
        CancellationToken cancellationToken)
    {
        var identityId = User.GetRequiredIdentityId();
        await sender.Send(new AcceptInvitationByTokenCommand(token, identityId), cancellationToken);
        return Ok();
    }
}
