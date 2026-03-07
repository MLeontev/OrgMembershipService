using Microsoft.EntityFrameworkCore;
using OrgMembershipService.Domain.Entities;

namespace OrgMembershipService.Infrastructure.Database.Seeding;

public class DbSeeder(AppDbContext dbContext)
{
    public async Task SeedAsync(CancellationToken ct = default)
    {
        await SeedPermissionsAsync(ct);
        await SeedDefaultRolesAsync(ct);
        await SeedRolePermissionsAsync(ct);
    }

    private async Task SeedPermissionsAsync(CancellationToken ct)
    {
        var existing = await dbContext.Permissions
            .Select(x => x.Code)
            .ToListAsync(ct);
        
        var existingSet = existing.ToHashSet(StringComparer.OrdinalIgnoreCase);

        var toAdd = SeedData.Permissions
            .Where(x => !existingSet.Contains(x.Code))
            .Select(x => new Permission
            {
                Id = Guid.NewGuid(),
                Code = x.Code,
                Name = x.Name,
                Description = x.Description
            })
            .ToList();

        if (toAdd.Count > 0)
        {
            await dbContext.Permissions.AddRangeAsync(toAdd, ct);
            await dbContext.SaveChangesAsync(ct);
        }
    }

    private async Task SeedDefaultRolesAsync(CancellationToken ct)
    {
        var existing = await dbContext.Roles
            .Where(x => x.OrganizationId == null)
            .Select(x => x.Code)
            .ToListAsync(ct);
        
        var existingSet = existing.ToHashSet(StringComparer.OrdinalIgnoreCase);

        var toAdd = SeedData.DefaultRoles
            .Where(x => !existingSet.Contains(x.Code))
            .Select(x => new Role
            {
                Id = Guid.NewGuid(),
                OrganizationId = null,
                Code = x.Code,
                Name = x.Name,
                Description = x.Description,
                Priority = x.Priority
            })
            .ToList();

        if (toAdd.Count > 0)
        {
            await dbContext.Roles.AddRangeAsync(toAdd, ct);
            await dbContext.SaveChangesAsync(ct);
        }
    }

    private async Task SeedRolePermissionsAsync(CancellationToken ct)
    {
        var roles = await dbContext.Roles
            .Where(r => r.OrganizationId == null)
            .ToDictionaryAsync(r => r.Code, StringComparer.OrdinalIgnoreCase, ct);
        
        var perms = await dbContext.Permissions
            .ToDictionaryAsync(p => p.Code, StringComparer.OrdinalIgnoreCase, ct);
        
        var existing = await dbContext.RolePermissions
            .Select(rp => new { rp.RoleId, rp.PermissionId })
            .ToListAsync(ct);
        
        var existingSet = existing.Select(x => (x.RoleId, x.PermissionId)).ToHashSet();
        
        var toAdd = new List<RolePermission>();

        foreach (var (roleCode, permCodes) in SeedData.RolePermissionMap)
        {
            if (!roles.TryGetValue(roleCode, out var role))
                continue;

            foreach (var permCode in permCodes)
            {
                if (!perms.TryGetValue(permCode, out var perm))
                    continue;

                var key = (role.Id, perm.Id);
                if (existingSet.Contains(key))
                    continue;
                
                toAdd.Add(new RolePermission
                {
                    RoleId = role.Id,
                    PermissionId = perm.Id
                });
                
                existingSet.Add(key);
            }
        }
        
        if (toAdd.Count > 0)
        {
            await dbContext.RolePermissions.AddRangeAsync(toAdd, ct);
            await dbContext.SaveChangesAsync(ct);
        }
    }
}