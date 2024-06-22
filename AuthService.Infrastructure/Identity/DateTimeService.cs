using AuthService.Application.Common.Interfaces;

namespace AuthService.Infrastructure.Identity;

public class DateTimeService : IDateTime
{
    public DateTime UtcNow => DateTime.UtcNow;
}