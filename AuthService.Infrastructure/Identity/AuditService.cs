using AuthService.Application.Common.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Domain.Enums;

namespace AuthService.Infrastructure.Identity;

public class AuditService : IAuditService
{
    private readonly IApplicationDbContext _context;

    public AuditService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task LogEventAsync(
        AuditEventType eventType,
        string description,
        Guid? userId = null,
        string? ipAddress = null,
        string? userAgent = null,
        string? additionalData = null,
        CancellationToken cancellationToken = default)
    {
        var auditLog = AuditLog.Create(
            eventType,
            description,
            userId,
            ipAddress,
            userAgent,
            additionalData
        );

        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync(cancellationToken);
    }
}