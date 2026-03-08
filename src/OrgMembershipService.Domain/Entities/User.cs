namespace OrgMembershipService.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string? Patronymic { get; private set; }
    
    public string Email { get; private set; }
    public string IdentityId { get; private set; }

    private readonly List<Membership> _memberships = [];
    public IReadOnlyList<Membership> Memberships => _memberships;

    private User() { }

    public static User Create(
        string firstName,
        string lastName,
        string? patronymic,
        string email,
        string identityId)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = firstName,
            LastName = lastName,
            Patronymic = patronymic,
            Email = email,
            IdentityId = identityId
        };

        return user;
    }
}