using AuthService.Application.Common.Interfaces;
using AuthService.Application.Common.Models;
using AuthService.Domain.Enums;
using AuthService.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<AuthResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly IAuditService _auditService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenConfiguration _tokenConfig;

    public RefreshTokenCommandHandler(
        IApplicationDbContext context,
        ITokenService tokenService,
        IAuditService auditService,
        IUnitOfWork unitOfWork,
        ITokenConfiguration tokenConfig)
    {
        _context = context;
        _tokenService = tokenService;
        _auditService = auditService;
        _unitOfWork = unitOfWork;
        _tokenConfig = tokenConfig;
    }

    public async Task<Result<AuthResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var refreshToken = await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken, cancellationToken);

        if (refreshToken == null)
        {
            throw new InvalidTokenException("Invalid refresh token");
        }

        var user = refreshToken.User;

        if (refreshToken.IsRevoked)
        {
            var userSessions = await _context.Sessions
                .Where(s => s.UserId == user.Id && s.Status == SessionStatus.Active)
                .ToListAsync(cancellationToken);

            foreach (var userSession in userSessions)
            {
                userSession.MarkAsCompromised();
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _auditService.LogEventAsync(
                AuditEventType.TokenReused,
                "Refresh token reuse detected - all sessions revoked",
                user.Id,
                request.IpAddress,
                request.UserAgent,
                cancellationToken: cancellationToken);

            throw new TokenReusedException();
        }

        if (refreshToken.IsExpired())
        {
            throw new InvalidTokenException("Refresh token has expired");
        }

        if (!user.IsActive)
        {
            return Result.Failure<AuthResponse>("Account is inactive");
        }

        var newAccessToken = _tokenService.GenerateAccessToken(user.Id, user.Email, user.Role.ToString());
        var newRefreshTokenString = _tokenService.GenerateRefreshToken();

        refreshToken.Revoke(newRefreshTokenString);

        var newRefreshToken = Domain.Entities.RefreshToken.Create(
            user.Id,
            newRefreshTokenString,
            DateTime.UtcNow.AddDays(_tokenConfig.RefreshTokenExpirationDays),
            request.IpAddress,
            request.UserAgent);

        _context.RefreshTokens.Add(newRefreshToken);

        var session = await _context.Sessions
            .Where(s => s.UserId == user.Id && s.Status == SessionStatus.Active)
            .OrderByDescending(s => s.LastActivityAt)
            .FirstOrDefaultAsync(cancellationToken);

        session?.UpdateActivity();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogEventAsync(
            AuditEventType.TokenRefreshed,
            "Access token refreshed successfully",
            user.Id,
            request.IpAddress,
            request.UserAgent,
            cancellationToken: cancellationToken);

        var response = new AuthResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshTokenString,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_tokenConfig.AccessTokenExpirationMinutes),
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role.ToString(),
                IsActive = user.IsActive,
                LastLoginAt = user.LastLoginAt,
                CreatedAt = user.CreatedAt
            }
        };

        return Result.Success(response);
    }
}