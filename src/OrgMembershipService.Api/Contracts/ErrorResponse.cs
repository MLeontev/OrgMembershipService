namespace OrgMembershipService.Api.Contracts;

/// <summary>
/// Ошибка API
/// </summary>
/// <param name="Code">Код ошибки (машиночитаемый)</param>
/// <param name="Description">Описание ошибки (на русском, может отображаться на UI)</param>
/// <param name="Errors">Ошибки валидации по полям</param>
public record ErrorResponse(
    string Code,
    string Description,
    IReadOnlyDictionary<string, string[]>? Errors = null);
