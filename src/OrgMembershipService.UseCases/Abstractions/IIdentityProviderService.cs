namespace OrgMembershipService.Application.Abstractions;

public interface IIdentityProviderService
{
    Task<string> RegisterUserAsync(UserModel user, CancellationToken cancellationToken = default);
}

public record UserModel(string Email, string Password, string FirstName, string LastName);