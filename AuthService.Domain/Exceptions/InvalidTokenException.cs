namespace AuthService.Domain.Exceptions;

public class InvalidTokenException : DomainException
{
    public InvalidTokenException()
        : base("Invalid or expired token")
    {
    }

    public InvalidTokenException(string message)
        : base(message)
    {
    }
}