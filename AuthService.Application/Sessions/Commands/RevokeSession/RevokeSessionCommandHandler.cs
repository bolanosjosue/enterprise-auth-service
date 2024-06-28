using AuthService.Application.Common.Interfaces;
using AuthService.Application.Common.Models;
using AuthService.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Sessions.Commands.RevokeSession;

public class RevokeSessionCommandHandler : IRequestHandler<RevokeSessionCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuditService _auditService;
    private readonly IUnitOfWork _unitOfWork;

    public RevokeSessionCommandHandler(
        IApplicationDbContext context,
        IAuditService auditService,
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _auditService = auditService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(RevokeSessionCommand request, CancellationToken cancellationToken)
    {
        var session = await _context.Sessions
            .FirstOrDefaultAsync(
                s => s.Id == request.SessionId && s.UserId == request.UserId,
                cancellationToken);

        if (session == null)
        {
            return Result.Failure("Session not found");
        }

        if (!session.IsActive())
        {
            return Result.Failure("Session is already inactive");
        }

        session.Revoke();

        var refreshTokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == request.UserId &&
                        rt.IpAddress == session.IpAddress &&
                        !rt.IsRevoked)
            .ToListAsync(cancellationToken);

        foreach (var token in refreshTokens)
        {
            token.Revoke();
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogEventAsync(
            AuditEventType.SessionRevoked,
            $"Session revoked manually from device: {session.DeviceName}",
            request.UserId,
            request.IpAddress,
            request.UserAgent,
            cancellationToken: cancellationToken);

        return Result.Success();
    }
}