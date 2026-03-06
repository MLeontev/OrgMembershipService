namespace OrgMembershipService.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Patronymic { get; set; }
    
    public string Email { get; set; } = string.Empty;
    public string IdentityId { get; set; } = string.Empty;

    public List<Membership> Memberships { get; set; } = [];
}