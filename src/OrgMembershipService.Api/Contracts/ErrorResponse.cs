namespace OrgMembershipService.Api.Contracts;

public record ErrorResponse(
    string Code,
    string Description,
    IReadOnlyDictionary<string, string[]>? Errors = null);