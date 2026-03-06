namespace OrgMembershipService.Domain.Entities;

public class Invitation
{
    public Guid Id { get; set; }
    
    public Guid OrganizationId { get; set; }
    
    public string TokenHash { get; set; } = string.Empty;
    public InvitationStatus Status { get; set; } = InvitationStatus.Pending;
    
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    
    public Guid CreatedByUserId { get; set; }
    public User CreatedByUser { get; set; } = null!;
    
    public Guid? AcceptedByUserId { get; set; }
    public User? AcceptedByUser { get; set; }
    public DateTimeOffset? AcceptedAt { get; set; }

    public DateTimeOffset? RevokedAt { get; set; }

    public List<InvitationRole> Roles { get; set; } = [];
}