using AuthService.Application.Common.Models;
using MediatR;

namespace AuthService.Application.Auth.Commands.ChangePassword;

public record ChangePasswordCommand : IRequest<Result>
{
    public Guid UserId { get; init; }
    public string CurrentPassword { get; init; } = string.Empty;
    public string NewPassword { get; init; } = string.Empty;
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }
}