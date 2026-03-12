using System.Security.Claims;
using OrgMembershipService.Domain.Exceptions;

namespace OrgMembershipService.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static string GetRequiredIdentityId(this ClaimsPrincipal user)
    {
        var identityId = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(identityId))
        {
            throw new UnauthorizedException(
                "UNAUTHORIZED",
                "В access токене отсутствует claim sub");
        }
        
        return identityId;
    }
}