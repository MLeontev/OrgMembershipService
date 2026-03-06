using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrgMembershipService.Domain.Entities;

namespace OrgMembershipService.Infrastructure.Database.Configurations;

internal class MembershipRoleConfiguration : IEntityTypeConfiguration<MembershipRole>
{
    public void Configure(EntityTypeBuilder<MembershipRole> builder)
    {
        builder.HasIndex(x => new { x.MembershipId, x.RoleId }).IsUnique();
    }
}