namespace AuthService.Domain.Enums;

public enum AuditEventType
{
    UserRegistered = 0,
    LoginSuccessful = 1,
    LoginFailed = 2,
    LogoutSuccessful = 3,
    TokenRefreshed = 4,
    PasswordChanged = 5,
    SessionRevoked = 6,
    TokenReused = 7,
    AccountLocked = 8,
    AccountUnlocked = 9
}