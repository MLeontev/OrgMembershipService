namespace OrgMembershipService.Domain.Entities;

public class Role
{
    public Guid Id { get; set; }
    
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    public Guid? OrganizationId { get; set; }
    
    public int Priority { get; set; }

    public List<RolePermission> RolePermissions { get; set; } = [];
}