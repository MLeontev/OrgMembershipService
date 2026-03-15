namespace OrgMembershipService.Api.Contracts;

/// <summary>
/// Данные для создания кастомной роли организации
/// </summary>
/// <param name="Code">Код роли</param>
/// <param name="Name">Название роли</param>
/// <param name="Description">Описание роли</param>
/// <param name="Priority">Приоритет роли</param>
/// <param name="PermissionCodes">Коды прав роли</param>
public record CreateCustomRoleRequest(
    string Code,
    string Name,
    string? Description,
    int Priority,
    IReadOnlyCollection<string> PermissionCodes);
