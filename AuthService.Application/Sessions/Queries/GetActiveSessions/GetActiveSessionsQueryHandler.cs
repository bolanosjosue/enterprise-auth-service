using AuthService.Application.Common.Interfaces;
using AuthService.Application.Common.Models;
using AuthService.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Sessions.Queries.GetActiveSessions;

public class GetActiveSessionsQueryHandler : IRequestHandler<GetActiveSessionsQuery, Result<List<SessionDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetActiveSessionsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<SessionDto>>> Handle(
        GetActiveSessionsQuery request,
        CancellationToken cancellationToken)
    {
        var sessions = await _context.Sessions
            .Where(s => s.UserId == request.UserId && s.Status == SessionStatus.Active)
            .OrderByDescending(s => s.LastActivityAt)
            .Select(s => new SessionDto
            {
                Id = s.Id,
                DeviceName = s.DeviceName,
                IpAddress = s.IpAddress,
                UserAgent = s.UserAgent,
                LastActivityAt = s.LastActivityAt,
                Status = s.Status.ToString(),
                CreatedAt = s.CreatedAt,
                IsCurrent = request.CurrentSessionId.HasValue && s.Id == request.CurrentSessionId.Value
            })
            .ToListAsync(cancellationToken);

        return Result.Success(sessions);
    }
}