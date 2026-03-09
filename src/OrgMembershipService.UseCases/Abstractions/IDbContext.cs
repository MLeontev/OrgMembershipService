using Microsoft.EntityFrameworkCore;
using OrgMembershipService.Domain.Entities;

namespace OrgMembershipService.Application.Abstractions;

public interface IDbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Membership> Memberships { get; set; }
    public DbSet<MembershipRole> MembershipRoles { get; set; }

    public DbSet<Role> Roles { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }

    public DbSet<Invitation> Invitations { get; set; }
    public DbSet<InvitationRole> InvitationRoles { get; set; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}