using AuthService.Domain.Entities.Common;
using AuthService.Domain.Enums;

namespace AuthService.Domain.Entities;

public class AuditLog : BaseEntity
{
    public AuditEventType EventType { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public string? AdditionalData { get; private set; }

    // Foreign key (nullable - some events may not have a user)
    public Guid? UserId { get; private set; }
    public User? User { get; private set; }

    private AuditLog() { }

    public static AuditLog Create(
        AuditEventType eventType,
        string description,
        Guid? userId = null,
        string? ipAddress = null,
        string? userAgent = null,
        string? additionalData = null)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description is required", nameof(description));

        return new AuditLog
        {
            EventType = eventType,
            Description = description,
            UserId = userId,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            AdditionalData = additionalData
        };
    }
}