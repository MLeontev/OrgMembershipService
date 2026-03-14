namespace OrgMembershipService.Api.Contracts;

/// <summary>
/// Данные для создания приглашения в организацию
/// </summary>
/// <param name="RoleCodes">Коды ролей для назначения при принятии приглашения</param>
/// <param name="ExpiresAt">Дата и время окончания действия приглашения</param>
public record CreateOrganizationInvitationRequest(
    IReadOnlyCollection<string> RoleCodes,
    DateTimeOffset ExpiresAt);
