namespace OrgMembershipService.Domain.Entities;

public class MembershipRole
{
    public Guid Id { get; set; }
    
    public Guid MembershipId { get; set; }
    public Membership Membership { get; set; } = null!;
    
    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;
    
    public Guid? AssignedByUserId { get; set; }
    public User? AssignedByUser { get; set; }
    public DateTimeOffset AssignedAt { get; set; } = DateTimeOffset.UtcNow;
}