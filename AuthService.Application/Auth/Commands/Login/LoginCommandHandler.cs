using AuthService.Application.Common.Interfaces;
using AuthService.Application.Common.Models;
using AuthService.Domain.Entities;
using AuthService.Domain.Enums;
using AuthService.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RefreshTokenEntity = AuthService.Domain.Entities.RefreshToken;
namespace AuthService.Application.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IAuditService _auditService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenConfiguration _tokenConfig;

    public LoginCommandHandler(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IAuditService auditService,
        IUnitOfWork unitOfWork,
        ITokenConfiguration tokenConfig)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _auditService = auditService;
        _unitOfWork = unitOfWork;
        _tokenConfig = tokenConfig;
    }

    public async Task<Result<AuthResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.RefreshTokens)
            .Include(u => u.Sessions)
            .FirstOrDefaultAsync(u => u.Email == request.Email.ToLower(), cancellationToken);

        if (user == null)
        {
            await _auditService.LogEventAsync(
                AuditEventType.LoginFailed,
                $"Login attempt with non-existent email: {request.Email}",
                null,
                request.IpAddress,
                request.UserAgent,
                cancellationToken: cancellationToken);

            return Result.Failure<AuthResponse>("Invalid email or password");
        }

        if (user.IsLockedOut())
        {
            await _auditService.LogEventAsync(
                AuditEventType.LoginFailed,
                "Login attempt on locked account",
                user.Id,
                request.IpAddress,
                request.UserAgent,
                cancellationToken: cancellationToken);

            throw new AccountLockedException(user.LockoutEndDate!.Value);
        }

        if (!user.IsActive)
        {
            return Result.Failure<AuthResponse>("Account is inactive");
        }

        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            user.RecordFailedLogin();

            if (user.FailedLoginAttempts >= 5)
            {
                user.LockAccount(15);

                await _auditService.LogEventAsync(
                    AuditEventType.AccountLocked,
                    $"Account locked due to {user.FailedLoginAttempts} failed login attempts",
                    user.Id,
                    request.IpAddress,
                    request.UserAgent,
                    cancellationToken: cancellationToken);
            }
            else
            {
                await _auditService.LogEventAsync(
                    AuditEventType.LoginFailed,
                    $"Invalid password (attempt {user.FailedLoginAttempts}/5)",
                    user.Id,
                    request.IpAddress,
                    request.UserAgent,
                    cancellationToken: cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Failure<AuthResponse>("Invalid email or password");
        }

        var accessToken = _tokenService.GenerateAccessToken(user.Id, user.Email, user.Role.ToString());
        var refreshTokenString = _tokenService.GenerateRefreshToken();

        var refreshToken = RefreshTokenEntity.Create(
            user.Id,
            refreshTokenString,
            DateTime.UtcNow.AddDays(_tokenConfig.RefreshTokenExpirationDays),
            request.IpAddress,
            request.UserAgent);

        _context.RefreshTokens.Add(refreshToken);

        var deviceName = request.DeviceName ?? "Unknown Device";
        var session = Session.Create(user.Id, deviceName, request.IpAddress, request.UserAgent);
        _context.Sessions.Add(session);

        user.RecordLogin();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogEventAsync(
            AuditEventType.LoginSuccessful,
            $"User logged in from {request.IpAddress}",
            user.Id,
            request.IpAddress,
            request.UserAgent,
            cancellationToken: cancellationToken);

        var response = new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenString,
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