using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrgMembershipService.Api.Contracts;
using OrgMembershipService.Api.Extensions;
using OrgMembershipService.Application.Features.Users.Commands;
using OrgMembershipService.Application.Features.Users.Queries;

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
    
    /// <summary>
    /// Возвращает профиль текущего пользователя
    /// </summary>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Профиль текущего пользователя</returns>
    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserByIdentityDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserByIdentityDto>> Me(CancellationToken cancellationToken)
    {
        var identityId = User.GetRequiredIdentityId();
        var user = await sender.Send(new GetUserByIdentityQuery(identityId), cancellationToken);
        return Ok(user);
    }
    
    /// <summary>
    /// Возвращает список организаций текущего пользователя
    /// </summary>
    /// <param name="status">Фильтр по статусу членства (Active, Deactivated, Removed)</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Список организаций пользователя</returns>
    [Authorize]
    [HttpGet("me/organizations")]
    [ProducesResponseType(typeof(UserOrganizationsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserOrganizationsDto>> MyOrganizations(
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        var identityId = User.GetRequiredIdentityId();
        var organizations = await sender.Send(new GetUserOrganizationsQuery(identityId, status), cancellationToken);
        return Ok(organizations);
    }
    
    /// <summary>
    /// Возвращает роли, права и статус членства текущего пользователя в организации
    /// </summary>
    /// <param name="organizationId">Идентификатор организации</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Доступ текущего пользователя в выбранной организации</returns>
    [Authorize]
    [HttpGet("me/access")]
    [ProducesResponseType(typeof(MyAccessDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<MyAccessDto>> MyAccess(
        [FromQuery] Guid organizationId,
        CancellationToken cancellationToken)
    {
        var identityId = User.GetRequiredIdentityId();
        var access = await sender.Send(new GetMyAccessQuery(organizationId, identityId), cancellationToken);
        return Ok(access);
    }
}
