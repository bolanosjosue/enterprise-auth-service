namespace AuthService.Domain.Exceptions;

public class TokenReusedException : DomainException
{
    public TokenReusedException()
        : base("Token has been reused - possible security breach detected")
    {
    }
}