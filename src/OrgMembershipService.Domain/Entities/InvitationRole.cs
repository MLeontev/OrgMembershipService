namespace OrgMembershipService.Domain.Entities;

public class InvitationRole
{
    public Guid InvitationId { get; set; }
    public Invitation Invitation { get; set; } = null!;

    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;
}