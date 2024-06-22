namespace AuthService.Domain.Enums;

public enum SessionStatus
{
    Active = 0,
    Expired = 1,
    Revoked = 2,
    Compromised = 3
}