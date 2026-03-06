using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrgMembershipService.Domain.Entities;

namespace OrgMembershipService.Infrastructure.Database.Configurations;

internal class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.HasIndex(x => x.Code)
            .IsUnique()
            .HasFilter("\"OrganizationId\" IS NULL");
        
        builder.HasIndex(x => new { x.OrganizationId, x.Code })
            .IsUnique()
            .HasFilter("\"OrganizationId\" IS NOT NULL");
    }
}