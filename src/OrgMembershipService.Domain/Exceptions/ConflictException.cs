namespace OrgMembershipService.Domain.Exceptions;

public class ConflictException : AppException
{
    public override int StatusCode => 409;

    public ConflictException(string code, string description) : base(code, description) { }
}