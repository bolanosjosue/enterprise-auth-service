using AuthService.Application.Common.Interfaces;
using AuthService.Application.Common.Models;
using AuthService.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Sessions.Commands.RevokeAllSessions;

public class RevokeAllSessionsCommandHandler : IRequestHandler<RevokeAllSessionsCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuditService _auditService;
    private readonly IUnitOfWork _unitOfWork;

    public RevokeAllSessionsCommandHandler(
        IApplicationDbContext context,
        IAuditService auditService,
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _auditService = auditService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(RevokeAllSessionsCommand request, CancellationToken cancellationToken)
    {
        var activeSessions = await _context.Sessions
            .Where(s => s.UserId == request.UserId && s.Status == SessionStatus.Active)
            .ToListAsync(cancellationToken);

        if (!activeSessions.Any())
        {
            return Result.Success();
        }

        foreach (var userSession in activeSessions)
        {
            userSession.Revoke();
        }

        var activeTokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == request.UserId && !rt.IsRevoked)
            .ToListAsync(cancellationToken);

        foreach (var token in activeTokens)
        {
            token.Revoke();
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogEventAsync(
            AuditEventType.SessionRevoked,
            $"All sessions revoked - Total: {activeSessions.Count}",
            request.UserId,
            request.IpAddress,
            request.UserAgent,
            cancellationToken: cancellationToken);

        return Result.Success();
    }
}