namespace OrgMembershipService.Domain.Exceptions;

public class ForbiddenException : AppException
{
    public override int StatusCode => 403;

    public ForbiddenException(string code, string description) : base(code, description) { }
}
