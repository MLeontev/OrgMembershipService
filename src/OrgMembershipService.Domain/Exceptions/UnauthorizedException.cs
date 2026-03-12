namespace OrgMembershipService.Domain.Exceptions;

public class UnauthorizedException : AppException
{
    public override int StatusCode => 401;

    public UnauthorizedException(string code, string description) : base(code, description) { }
}