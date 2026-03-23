using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrgMembershipService.Application.Features.Authorization.Queries;
using OrgMembershipService.Domain.Exceptions;

namespace OrgMembershipService.Api.Extensions;

public static class ControllerPermissionExtensions
{
    public static async Task<string> EnsurePermissionAsync(
        this ControllerBase controller,
        ISender sender,
        Guid organizationId,
        string permissionCode,
        CancellationToken cancellationToken)
    {
        var identityId = controller.User.GetRequiredIdentityId();

        var result = await sender.Send(
            new CheckPermissionQuery(organizationId, identityId, permissionCode),
            cancellationToken);

        if (!result.Allowed)
            throw new ForbiddenException("FORBIDDEN", "Недостаточно прав для выполнения этого действия");

        return identityId;
    }
}
