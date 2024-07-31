using Microsoft.AspNetCore.Authorization;

namespace AuthService.API.Extensions;

public static class AuthorizationExtensions
{
    public static IServiceCollection AddCustomAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly", policy =>
                policy.RequireRole("Admin"));

            options.AddPolicy("ManagerOrAbove", policy =>
                policy.RequireRole("Admin", "Manager"));

            options.AddPolicy("AuthenticatedUsers", policy =>
                policy.RequireAuthenticatedUser());
        });

        return services;
    }
}