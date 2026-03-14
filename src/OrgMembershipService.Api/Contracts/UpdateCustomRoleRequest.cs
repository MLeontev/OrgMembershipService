namespace OrgMembershipService.Api.Contracts;

/// <summary>
/// Данные для обновления кастомной роли организации
/// </summary>
/// <param name="Code">Код роли</param>
/// <param name="Name">Название роли</param>
/// <param name="Description">Описание роли</param>
/// <param name="PermissionCodes">Коды прав роли</param>
public record UpdateCustomRoleRequest(
    string Code,
    string Name,
    string? Description,
    IReadOnlyCollection<string> PermissionCodes);
