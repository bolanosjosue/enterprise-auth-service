using AuthService.Application.Common.Interfaces;
using AuthService.Application.Common.Models;
using AuthService.Domain.Entities;
using AuthService.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Auth.Commands.ChangePassword;

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Result>
{
    private readonly IRepository<User> _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuditService _auditService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IApplicationDbContext _context;

    public ChangePasswordCommandHandler(
        IRepository<User> userRepository,
        IPasswordHasher passwordHasher,
        IAuditService auditService,
        IUnitOfWork unitOfWork,
        IApplicationDbContext context)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _auditService = auditService;
        _unitOfWork = unitOfWork;
        _context = context;
    }

    public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);

        if (user == null)
        {
            return Result.Failure("User not found");
        }

        if (!_passwordHasher.VerifyPassword(request.CurrentPassword, user.PasswordHash))
        {
            await _auditService.LogEventAsync(
                AuditEventType.PasswordChanged,
                "Failed password change attempt - incorrect current password",
                user.Id,
                request.IpAddress,
                request.UserAgent,
                cancellationToken: cancellationToken);

            return Result.Failure("Current password is incorrect");
        }

        var newPasswordHash = _passwordHasher.HashPassword(request.NewPassword);

        user.UpdatePassword(newPasswordHash);

        _userRepository.Update(user);

        var activeTokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == user.Id && !rt.IsRevoked)
            .ToListAsync(cancellationToken);

        foreach (var token in activeTokens)
        {
            token.Revoke();
        }

        var activeSessions = await _context.Sessions
            .Where(s => s.UserId == user.Id && s.Status == SessionStatus.Active)
            .ToListAsync(cancellationToken);

        foreach (var userSession in activeSessions)
        {
            userSession.Revoke();
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogEventAsync(
            AuditEventType.PasswordChanged,
            "Password changed successfully - all sessions revoked",
            user.Id,
            request.IpAddress,
            request.UserAgent,
            cancellationToken: cancellationToken);

        return Result.Success();
    }
}