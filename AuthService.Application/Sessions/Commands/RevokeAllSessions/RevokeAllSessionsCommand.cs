using AuthService.Application.Common.Models;
using MediatR;

namespace AuthService.Application.Sessions.Commands.RevokeAllSessions;

public record RevokeAllSessionsCommand : IRequest<Result>
{
    public Guid UserId { get; init; }
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }
}