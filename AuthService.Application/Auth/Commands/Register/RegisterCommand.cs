using AuthService.Application.Common.Models;
using MediatR;

namespace AuthService.Application.Auth.Commands.Register;

public record RegisterCommand : IRequest<Result<UserDto>>
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }
}