namespace OrgMembershipService.Api.Contracts;

/// <summary>
/// Запрос обновления данных участника организации
/// </summary>
/// <param name="Department">Подразделение пользователя</param>
/// <param name="Title">Должность пользователя</param>
public record UpdateOrganizationMemberRequest(
    string? Department,
    string? Title);
