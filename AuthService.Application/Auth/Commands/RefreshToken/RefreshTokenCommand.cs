using AuthService.Application.Common.Models;
using MediatR;

namespace AuthService.Application.Auth.Commands.RefreshToken;

public record RefreshTokenCommand : IRequest<Result<AuthResponse>>
{
    public string RefreshToken { get; init; } = string.Empty;
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }
}