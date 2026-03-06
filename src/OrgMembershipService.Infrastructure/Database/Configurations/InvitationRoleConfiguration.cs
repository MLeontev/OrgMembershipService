using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrgMembershipService.Domain.Entities;

namespace OrgMembershipService.Infrastructure.Database.Configurations;

internal class InvitationRoleConfiguration : IEntityTypeConfiguration<InvitationRole>
{
    public void Configure(EntityTypeBuilder<InvitationRole> builder)
    {
        builder.HasKey(x => new { x.InvitationId, x.RoleId });
    }
}