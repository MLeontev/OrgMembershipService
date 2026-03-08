namespace OrgMembershipService.Infrastructure.Identity;

internal record UserRepresentation(
    string Username,
    string Email,
    string FirstName,
    string LastName,
    bool EmailVerified,
    bool Enabled,
    CredentialRepresentation[] Credentials);
    
internal record CredentialRepresentation(string Type, string Value, bool Temporary);