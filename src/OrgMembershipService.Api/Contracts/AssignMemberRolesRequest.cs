namespace OrgMembershipService.Api.Contracts;

/// <summary>
/// Данные для назначения ролей участнику организации
/// </summary>
/// <param name="RoleCodes">Коды ролей для назначения</param>
public record AssignMemberRolesRequest(IReadOnlyCollection<string> RoleCodes);
