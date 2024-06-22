using AuthService.Application.Common.Models;
using MediatR;

namespace AuthService.Application.Auth.Commands.Login;

public record LoginCommand : IRequest<Result<AuthResponse>>
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }
    public string? DeviceName { get; init; }
}