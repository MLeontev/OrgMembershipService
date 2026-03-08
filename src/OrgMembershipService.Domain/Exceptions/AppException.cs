namespace OrgMembershipService.Domain.Exceptions;

public abstract class AppException : Exception
{
    public abstract int StatusCode { get; }
    
    public string Code { get; }
    
    protected AppException(string code, string description) : base(description)
    {
        Code = code;
    }
}