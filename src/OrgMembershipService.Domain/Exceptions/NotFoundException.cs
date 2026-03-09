namespace OrgMembershipService.Domain.Exceptions;

public class NotFoundException : AppException
{
    public override int StatusCode => 404;
    
    public NotFoundException(string code, string description) : base(code, description) { }
}