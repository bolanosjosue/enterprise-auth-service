using AuthService.Application.Common.Models;
using MediatR;

namespace AuthService.Application.Auth.Commands.Logout;

public record LogoutCommand : IRequest<Result>
{
    public string RefreshToken { get; init; } = string.Empty;
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }
}