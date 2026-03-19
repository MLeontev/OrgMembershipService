using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrgMembershipService.Api.Contracts;
using OrgMembershipService.Api.Extensions;
using OrgMembershipService.Application.Features.Memberships.Commands;
using OrgMembershipService.Application.Features.Memberships.Queries;
using OrgMembershipService.Application.Features.Roles.Commands;
using OrgMembershipService.Application.Features.Roles.Queries;

namespace OrgMembershipService.Api.Controllers.Public;

/// <summary>
/// Операции с организациями
/// </summary>
[ApiController]
[Route("api/[controller]/{organizationId:guid}")]
[Produces("application/json")]
public class OrganizationsController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Возвращает список ролей организации
    /// </summary>
    /// <param name="organizationId">Идентификатор организации</param>
    /// <param name="includeSystem">Включать ли системные роли (OrganizationId = null)</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Список ролей организации</returns>
    [Authorize]
    [HttpGet("roles")]
    [ProducesResponseType(typeof(OrganizationRolesDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OrganizationRolesDto>> GetRoles(
        [FromRoute] Guid organizationId,
        [FromQuery] bool includeSystem,
        CancellationToken cancellationToken)
    {
        await this.EnsurePermissionAsync(sender, organizationId, "ROLES_LIST", cancellationToken);
        var roles = await sender.Send(new GetOrganizationRolesQuery(organizationId, includeSystem), cancellationToken);
        return Ok(roles);
    }

    /// <summary>
    /// Возвращает каталог доступных прав
    /// </summary>
    /// <param name="organizationId">Идентификатор организации</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Список прав для настройки ролей</returns>
    [Authorize]
    [HttpGet("permissions")]
    [ProducesResponseType(typeof(PermissionsCatalogDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PermissionsCatalogDto>> GetPermissionsCatalog(
        [FromRoute] Guid organizationId,
        CancellationToken cancellationToken)
    {
        await this.EnsurePermissionAsync(sender, organizationId, "ROLES_LIST", cancellationToken);
        var permissions = await sender.Send(new GetPermissionsCatalogQuery(), cancellationToken);
        return Ok(permissions);
    }

    /// <summary>
    /// Создает кастомную роль организации
    /// </summary>
    /// <param name="organizationId">Идентификатор организации</param>
    /// <param name="request">Данные для создания кастомной роли</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Созданная кастомная роль</returns>
    [Authorize]
    [HttpPost("roles/custom")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(OrganizationRoleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OrganizationRoleDto>> CreateCustomRole(
        [FromRoute] Guid organizationId,
        [FromBody] CreateCustomRoleRequest request,
        CancellationToken cancellationToken)
    {
        await this.EnsurePermissionAsync(sender, organizationId, "ROLES_CREATE", cancellationToken);

        var role = await sender.Send(
            new CreateOrganizationCustomRoleCommand(
                organizationId,
                request.Code,
                request.Name,
                request.Description,
                request.Priority,
                request.PermissionCodes),
            cancellationToken);

        return Ok(role);
    }

    /// <summary>
    /// Обновляет кастомную роль организации
    /// </summary>
    /// <param name="organizationId">Идентификатор организации</param>
    /// <param name="roleId">Идентификатор роли</param>
    /// <param name="request">Данные для обновления кастомной роли</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Обновленная кастомная роль</returns>
    [Authorize]
    [HttpPut("roles/custom/{roleId:guid}")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(OrganizationRoleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OrganizationRoleDto>> UpdateCustomRole(
        [FromRoute] Guid organizationId,
        [FromRoute] Guid roleId,
        [FromBody] UpdateCustomRoleRequest request,
        CancellationToken cancellationToken)
    {
        await this.EnsurePermissionAsync(sender, organizationId, "ROLES_UPDATE", cancellationToken);

        var role = await sender.Send(
            new UpdateOrganizationCustomRoleCommand(
                organizationId,
                roleId,
                request.Code,
                request.Name,
                request.Description,
                request.PermissionCodes),
            cancellationToken);

        return Ok(role);
    }

    /// <summary>
    /// Удаляет кастомную роль организации
    /// </summary>
    /// <param name="organizationId">Идентификатор организации</param>
    /// <param name="roleId">Идентификатор роли</param>
    /// <param name="cancellationToken">Токен отмены</param>
    [Authorize]
    [HttpDelete("roles/custom/{roleId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteCustomRole(
        [FromRoute] Guid organizationId,
        [FromRoute] Guid roleId,
        CancellationToken cancellationToken)
    {
        await this.EnsurePermissionAsync(sender, organizationId, "ROLES_DELETE", cancellationToken);
        await sender.Send(new DeleteOrganizationCustomRoleCommand(organizationId, roleId), cancellationToken);
        return Ok();
    }

    /// <summary>
    /// Возвращает список участников организации
    /// </summary>
    /// <param name="organizationId">Идентификатор организации</param>
    /// <param name="status">Фильтр по статусу членства (Active, Deactivated, Removed)</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Список участников организации</returns>
    [Authorize]
    [HttpGet("members")]
    [ProducesResponseType(typeof(OrganizationMembersDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OrganizationMembersDto>> GetMembers(
        [FromRoute] Guid organizationId,
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        await this.EnsurePermissionAsync(sender, organizationId, "MEMBERS_LIST", cancellationToken);
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
    [HttpGet("members/{membershipId:guid}")]
    [ProducesResponseType(typeof(OrganizationMemberDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OrganizationMemberDto>> GetMemberById(
        [FromRoute] Guid organizationId,
        [FromRoute] Guid membershipId,
        CancellationToken cancellationToken)
    {
        await this.EnsurePermissionAsync(sender, organizationId, "MEMBERS_READ", cancellationToken);
        var member = await sender.Send(new GetOrganizationMemberByIdQuery(organizationId, membershipId), cancellationToken);
        return Ok(member);
    }

    /// <summary>
    /// Назначает роли участнику организации
    /// </summary>
    /// <param name="organizationId">Идентификатор организации</param>
    /// <param name="membershipId">Идентификатор членства</param>
    /// <param name="request">Коды ролей для назначения</param>
    /// <param name="cancellationToken">Токен отмены</param>
    [Authorize]
    [HttpPost("members/{membershipId:guid}/roles")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AssignRoles(
        [FromRoute] Guid organizationId,
        [FromRoute] Guid membershipId,
        [FromBody] AssignMemberRolesRequest request,
        CancellationToken cancellationToken)
    {
        var identityId = await this.EnsurePermissionAsync(sender, organizationId, "ROLES_ASSIGN", cancellationToken);

        await sender.Send(
            new AssignOrganizationMemberRolesCommand(
                organizationId,
                membershipId,
                identityId,
                request.RoleCodes),
            cancellationToken);

        return Ok();
    }

    /// <summary>
    /// Снимает роль с участника организации
    /// </summary>
    /// <param name="organizationId">Идентификатор организации</param>
    /// <param name="membershipId">Идентификатор членства</param>
    /// <param name="roleCode">Код роли для снятия</param>
    /// <param name="cancellationToken">Токен отмены</param>
    [Authorize]
    [HttpDelete("members/{membershipId:guid}/roles/{roleCode}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RevokeRole(
        [FromRoute] Guid organizationId,
        [FromRoute] Guid membershipId,
        [FromRoute] string roleCode,
        CancellationToken cancellationToken)
    {
        await this.EnsurePermissionAsync(sender, organizationId, "ROLES_REVOKE", cancellationToken);

        await sender.Send(
            new RevokeOrganizationMemberRoleCommand(
                organizationId,
                membershipId,
                roleCode),
            cancellationToken);

        return Ok();
    }

    /// <summary>
    /// Обновляет данные участника организации
    /// </summary>
    /// <param name="organizationId">Идентификатор организации</param>
    /// <param name="membershipId">Идентификатор членства</param>
    /// <param name="request">Данные для обновления участника</param>
    /// <param name="cancellationToken">Токен отмены</param>
    [Authorize]
    [HttpPatch("members/{membershipId:guid}")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateMember(
        [FromRoute] Guid organizationId,
        [FromRoute] Guid membershipId,
        [FromBody] UpdateOrganizationMemberRequest request,
        CancellationToken cancellationToken)
    {
        await this.EnsurePermissionAsync(sender, organizationId, "MEMBERS_UPDATE", cancellationToken);

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
    [HttpPost("members/{membershipId:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeactivateMember(
        [FromRoute] Guid organizationId,
        [FromRoute] Guid membershipId,
        CancellationToken cancellationToken)
    {
        await this.EnsurePermissionAsync(sender, organizationId, "MEMBERS_DEACTIVATE", cancellationToken);
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
    [HttpPost("members/{membershipId:guid}/activate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ActivateMember(
        [FromRoute] Guid organizationId,
        [FromRoute] Guid membershipId,
        CancellationToken cancellationToken)
    {
        await this.EnsurePermissionAsync(sender, organizationId, "MEMBERS_DEACTIVATE", cancellationToken);
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
    [HttpDelete("members/{membershipId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteMember(
        [FromRoute] Guid organizationId,
        [FromRoute] Guid membershipId,
        CancellationToken cancellationToken)
    {
        await this.EnsurePermissionAsync(sender, organizationId, "MEMBERS_REMOVE", cancellationToken);
        await sender.Send(new RemoveOrganizationMemberCommand(organizationId, membershipId), cancellationToken);
        return Ok();
    }
}
