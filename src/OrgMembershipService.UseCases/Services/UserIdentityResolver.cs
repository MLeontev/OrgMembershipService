using Microsoft.EntityFrameworkCore;
using OrgMembershipService.Application.Abstractions;
using OrgMembershipService.Domain.Exceptions;

namespace OrgMembershipService.Application.Services;

public interface IUserIdentityResolver
{
    Task<Guid> ResolveUserIdAsync(string identityId, CancellationToken ct = default);
}

internal class UserIdentityResolver(IDbContext dbContext) : IUserIdentityResolver
{
    public async Task<Guid> ResolveUserIdAsync(string identityId, CancellationToken ct = default)
    {
        var userId = await dbContext.Users
            .AsNoTracking()
            .Where(x => x.IdentityId == identityId)
            .Select(x => (Guid?)x.Id)
            .SingleOrDefaultAsync(ct);

        return userId ?? throw new NotFoundException("USER_NOT_FOUND_BY_IDENTITY", "Пользователь не найден");
    }
}
