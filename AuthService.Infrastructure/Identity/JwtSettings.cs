using AuthService.Application.Common.Interfaces;

namespace AuthService.Infrastructure.Identity;

public class JwtSettings : ITokenConfiguration
{
    public string Secret { get; set; } = string.Empty;
    public int AccessTokenExpirationMinutes { get; set; }
    public int RefreshTokenExpirationDays { get; set; }
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
}