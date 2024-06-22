using AuthService.Domain.Enums;

namespace AuthService.Application.Common.Interfaces;

public interface IAuditService
{
    Task LogEventAsync(
        AuditEventType eventType,
        string description,
        Guid? userId = null,
        string? ipAddress = null,
        string? userAgent = null,
        string? additionalData = null,
        CancellationToken cancellationToken = default);
}