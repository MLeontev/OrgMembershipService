namespace OrgMembershipService.Domain.Entities;

public class Membership
{
    public Guid Id { get; set; }
    
    public Guid OrganizationId { get; set; }
    
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    
    public MembershipStatus Status { get; set; } = MembershipStatus.Active;
    
    public DateTimeOffset? JoinedAt { get; set; }
    public DateTimeOffset? RemovedAt { get; set; }
    
    public string? Department { get; set; }
    public string? Title { get; set; }
    
    public Guid? DefaultOfficeId { get; set; }
    public Guid? DefaultFloorId { get; set; }
    public string? MetadataJson { get; set; }
    
    public List<MembershipRole> RoleAssignments { get; set; } = [];
}