using AuthService.Application.Common.Interfaces;
using AuthService.Application.Common.Models;
using AuthService.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Auth.Commands.Logout;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuditService _auditService;
    private readonly IUnitOfWork _unitOfWork;

    public LogoutCommandHandler(
        IApplicationDbContext context,
        IAuditService auditService,
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _auditService = auditService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var refreshToken = await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken, cancellationToken);

        if (refreshToken == null)
        {
            return Result.Failure("Invalid refresh token");
        }

        var user = refreshToken.User;

        if (!refreshToken.IsRevoked)
        {
            refreshToken.Revoke();
        }

        var session = await _context.Sessions
            .Where(s => s.UserId == user.Id &&
                       s.Status == SessionStatus.Active &&
                       s.IpAddress == request.IpAddress)
            .OrderByDescending(s => s.LastActivityAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (session != null)
        {
            session.Revoke();
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogEventAsync(
            AuditEventType.LogoutSuccessful,
            $"User logged out from {request.IpAddress}",
            user.Id,
            request.IpAddress,
            request.UserAgent,
            cancellationToken: cancellationToken);

        return Result.Success();
    }
}