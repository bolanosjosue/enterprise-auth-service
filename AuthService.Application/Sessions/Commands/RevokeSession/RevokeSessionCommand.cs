using AuthService.Application.Common.Models;
using MediatR;

namespace AuthService.Application.Sessions.Commands.RevokeSession;

public record RevokeSessionCommand : IRequest<Result>
{
    public Guid SessionId { get; init; }
    public Guid UserId { get; init; }
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }
}