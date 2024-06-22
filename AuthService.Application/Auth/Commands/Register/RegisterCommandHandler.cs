using AuthService.Application.Common.Interfaces;
using AuthService.Application.Common.Models;
using AuthService.Domain.Entities;
using AuthService.Domain.Enums;
using MediatR;

namespace AuthService.Application.Auth.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<UserDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IRepository<User> _userRepository;
    private readonly IAuditService _auditService;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterCommandHandler(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IRepository<User> userRepository,
        IAuditService auditService,
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _userRepository = userRepository;
        _auditService = auditService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<UserDto>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var emailExists = await _userRepository.AnyAsync(
            u => u.Email == request.Email.ToLower(),
            cancellationToken);

        if (emailExists)
        {
            return Result.Failure<UserDto>("Email is already registered");
        }

        var passwordHash = _passwordHasher.HashPassword(request.Password);

        var user = User.Create(
            request.Email,
            passwordHash,
            request.FullName,
            UserRole.User);

        _userRepository.Add(user);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogEventAsync(
            AuditEventType.UserRegistered,
            $"New user registered: {user.Email}",
            user.Id,
            request.IpAddress,
            request.UserAgent,
            cancellationToken: cancellationToken);

        var userDto = new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Role = user.Role.ToString(),
            IsActive = user.IsActive,
            LastLoginAt = user.LastLoginAt,
            CreatedAt = user.CreatedAt
        };

        return Result.Success(userDto);
    }
}