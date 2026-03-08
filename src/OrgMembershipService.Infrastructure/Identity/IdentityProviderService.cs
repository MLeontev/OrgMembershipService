using System.Net;
using OrgMembershipService.Application;
using OrgMembershipService.Application.Users.Services;
using OrgMembershipService.Domain.Exceptions;

namespace OrgMembershipService.Infrastructure.Identity;

internal class IdentityProviderService(KeycloakClient keycloakClient) : IIdentityProviderService
{
    public async Task<string> RegisterUserAsync(UserModel user, CancellationToken cancellationToken = default)
    {
        var userRepresentation = new UserRepresentation(
            user.Email,
            user.Email,
            user.FirstName,
            user.LastName,
            true,
            true,
            [new CredentialRepresentation("Password", user.Password, false)]);

        try
        {
            return await keycloakClient.RegisterUserAsync(userRepresentation, cancellationToken);
        }
        catch (HttpRequestException exception) when (exception.StatusCode == HttpStatusCode.Conflict)
        {
            throw new ConflictException(
                "IDENTITY_EMAIL_IS_NOT_UNIQUE", 
                "Пользователь с указанным email уже зарегистрирован");
        }
    }
}