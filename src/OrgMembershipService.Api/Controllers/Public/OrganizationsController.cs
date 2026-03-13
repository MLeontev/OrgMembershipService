using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrgMembershipService.Api.Contracts;
using OrgMembershipService.Application.Features.Memberships.Commands;
using OrgMembershipService.Application.Features.Memberships.Queries;

namespace OrgMembershipService.Api.Controllers.Public;

/// <summary>
/// Операции с организациями
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class OrganizationsController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Возвращает список участников организации
    /// </summary>
    /// <param name="organizationId">Идентификатор организации</param>
    /// <param name="status">Фильтр по статусу членства (Active, Deactivated, Removed)</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Список участников организации</returns>
    [Authorize]
    [HttpGet("{organizationId:guid}/members")]
    [ProducesResponseType(typeof(OrganizationMembersDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OrganizationMembersDto>> GetMembers(
        [FromRoute] Guid organizationId,
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        var members = await sender.Send(new GetOrganizationMembersQuery(organizationId, status), cancellationToken);
        return Ok(members);
    }

    /// <summary>
    /// Возвращает участника организации по идентификатору членства
    /// </summary>
    /// <param name="organizationId">Идентификатор организации</param>
    /// <param name="membershipId">Идентификатор членства</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Данные участника организации</returns>
    [Authorize]
    [HttpGet("{organizationId:guid}/members/{membershipId:guid}")]
    [ProducesResponseType(typeof(OrganizationMemberDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OrganizationMemberDto>> GetMemberById(
        [FromRoute] Guid organizationId,
        [FromRoute] Guid membershipId,
        CancellationToken cancellationToken)
    {
        var member = await sender.Send(new GetOrganizationMemberByIdQuery(organizationId, membershipId), cancellationToken);
        return Ok(member);
    }

    /// <summary>
    /// Обновляет данные участника организации
    /// </summary>
    /// <param name="organizationId">Идентификатор организации</param>
    /// <param name="membershipId">Идентификатор членства</param>
    /// <param name="request">Данные для обновления участника</param>
    /// <param name="cancellationToken">Токен отмены</param>
    [Authorize]
    [HttpPatch("{organizationId:guid}/members/{membershipId:guid}")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateMember(
        [FromRoute] Guid organizationId,
        [FromRoute] Guid membershipId,
        [FromBody] UpdateOrganizationMemberRequest request,
        CancellationToken cancellationToken)
    {
        await sender.Send(
            new UpdateOrganizationMemberCommand(
                organizationId,
                membershipId,
                request.Department,
                request.Title),
            cancellationToken);
        
        return Ok();
    }

    /// <summary>
    /// Деактивирует участника организации
    /// </summary>
    /// <param name="organizationId">Идентификатор организации</param>
    /// <param name="membershipId">Идентификатор членства</param>
    /// <param name="cancellationToken">Токен отмены</param>
    [Authorize]
    [HttpPost("{organizationId:guid}/members/{membershipId:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeactivateMember(
        [FromRoute] Guid organizationId,
        [FromRoute] Guid membershipId,
        CancellationToken cancellationToken)
    {
        await sender.Send(new DeactivateOrganizationMemberCommand(organizationId, membershipId), cancellationToken);
        return Ok();
    }

    /// <summary>
    /// Активирует участника организации
    /// </summary>
    /// <param name="organizationId">Идентификатор организации</param>
    /// <param name="membershipId">Идентификатор членства</param>
    /// <param name="cancellationToken">Токен отмены</param>
    [Authorize]
    [HttpPost("{organizationId:guid}/members/{membershipId:guid}/activate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ActivateMember(
        [FromRoute] Guid organizationId,
        [FromRoute] Guid membershipId,
        CancellationToken cancellationToken)
    {
        await sender.Send(new ActivateOrganizationMemberCommand(organizationId, membershipId), cancellationToken);
        return Ok();
    }

    /// <summary>
    /// Удаляет участника из организации
    /// </summary>
    /// <param name="organizationId">Идентификатор организации</param>
    /// <param name="membershipId">Идентификатор членства</param>
    /// <param name="cancellationToken">Токен отмены</param>
    [Authorize]
    [HttpDelete("{organizationId:guid}/members/{membershipId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteMember(
        [FromRoute] Guid organizationId,
        [FromRoute] Guid membershipId,
        CancellationToken cancellationToken)
    {
        await sender.Send(new RemoveOrganizationMemberCommand(organizationId, membershipId), cancellationToken);
        return Ok();
    }
}
