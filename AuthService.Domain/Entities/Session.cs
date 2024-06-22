using AuthService.Domain.Entities.Common;
using AuthService.Domain.Enums;

namespace AuthService.Domain.Entities;

public class Session : BaseEntity
{
    public string DeviceName { get; private set; } = string.Empty;
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public DateTime LastActivityAt { get; private set; }
    public SessionStatus Status { get; private set; }
    public DateTime? RevokedAt { get; private set; }

    // Foreign key
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;

    private Session() { }

    public static Session Create(
        Guid userId,
        string deviceName,
        string? ipAddress = null,
        string? userAgent = null)
    {
        if (string.IsNullOrWhiteSpace(deviceName))
            throw new ArgumentException("Device name is required", nameof(deviceName));

        return new Session
        {
            UserId = userId,
            DeviceName = deviceName,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            LastActivityAt = DateTime.UtcNow,
            Status = SessionStatus.Active
        };
    }

    public void UpdateActivity()
    {
        LastActivityAt = DateTime.UtcNow;
        MarkAsUpdated();
    }

    public void Revoke()
    {
        Status = SessionStatus.Revoked;
        RevokedAt = DateTime.UtcNow;
        MarkAsUpdated();
    }

    public void MarkAsExpired()
    {
        Status = SessionStatus.Expired;
        MarkAsUpdated();
    }

    public void MarkAsCompromised()
    {
        Status = SessionStatus.Compromised;
        RevokedAt = DateTime.UtcNow;
        MarkAsUpdated();
    }

    public bool IsActive() => Status == SessionStatus.Active;
}