namespace OrgMembershipService.Domain.Exceptions;

public class BusinessException : AppException
{
    public override int StatusCode => 422;

    public BusinessException(string code, string description) : base(code, description) { }
}