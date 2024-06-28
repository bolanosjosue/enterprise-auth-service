using AuthService.Application.Common.Models;
using MediatR;

namespace AuthService.Application.Sessions.Queries.GetActiveSessions;

public record GetActiveSessionsQuery : IRequest<Result<List<SessionDto>>>
{
    public Guid UserId { get; init; }
    public Guid? CurrentSessionId { get; init; }
}