using AuthService.Domain.Entities.Common;
using AuthService.Domain.Enums;

namespace AuthService.Domain.Entities;

public class User : AuditableEntity, ISoftDeletable
{
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string FullName { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public int FailedLoginAttempts { get; private set; }
    public DateTime? LockoutEndDate { get; private set; }

    // Navigation properties
    public ICollection<RefreshToken> RefreshTokens { get; private set; } = new List<RefreshToken>();
    public ICollection<Session> Sessions { get; private set; } = new List<Session>();

    private User() { }

    public static User Create(string email, string passwordHash, string fullName, UserRole role = UserRole.User)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required", nameof(email));

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash is required", nameof(passwordHash));

        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("Full name is required", nameof(fullName));

        return new User
        {
            Email = email.ToLowerInvariant().Trim(),
            PasswordHash = passwordHash,
            FullName = fullName.Trim(),
            Role = role,
            IsActive = true,
            FailedLoginAttempts = 0
        };
    }

    public void UpdatePassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw new ArgumentException("Password hash is required", nameof(newPasswordHash));

        PasswordHash = newPasswordHash;
        MarkAsUpdated();
    }

    public void UpdateRole(UserRole newRole)
    {
        Role = newRole;
        MarkAsUpdated();
    }

    public void Activate()
    {
        IsActive = true;
        MarkAsUpdated();
    }

    public void Deactivate()
    {
        IsActive = false;
        MarkAsUpdated();
    }

    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        FailedLoginAttempts = 0;
        LockoutEndDate = null;
        MarkAsUpdated();
    }

    public void RecordFailedLogin()
    {
        FailedLoginAttempts++;
        MarkAsUpdated();
    }

    public void LockAccount(int lockoutMinutes = 15)
    {
        LockoutEndDate = DateTime.UtcNow.AddMinutes(lockoutMinutes);
        MarkAsUpdated();
    }

    public void UnlockAccount()
    {
        FailedLoginAttempts = 0;
        LockoutEndDate = null;
        MarkAsUpdated();
    }

    public bool IsLockedOut()
    {
        return LockoutEndDate.HasValue && LockoutEndDate.Value > DateTime.UtcNow;
    }

    public void SoftDelete()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        IsActive = false;
        MarkAsUpdated();
    }

    public void Restore()
    {
        IsDeleted = false;
        DeletedAt = null;
        MarkAsUpdated();
    }
}