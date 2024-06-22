namespace AuthService.Domain.Exceptions;

public class AccountLockedException : DomainException
{
    public DateTime LockoutEndDate { get; }

    public AccountLockedException(DateTime lockoutEndDate)
        : base($"Account is locked until {lockoutEndDate:yyyy-MM-dd HH:mm:ss} UTC")
    {
        LockoutEndDate = lockoutEndDate;
    }
}