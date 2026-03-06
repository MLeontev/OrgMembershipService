using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrgMembershipService.Domain.Entities;

namespace OrgMembershipService.Infrastructure.Database.Configurations;

internal class MembershipConfiguration : IEntityTypeConfiguration<Membership>
{
    public void Configure(EntityTypeBuilder<Membership> builder)
    {
        builder.HasIndex(x => new { x.OrganizationId, x.UserId }).IsUnique();
        builder.Property(x => x.Status).HasConversion<string>();
    }
}