using AuthService.API.Middleware;

namespace AuthService.API.Extensions;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseCustomExceptionHandling(this IApplicationBuilder app)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        return app;
    }

    public static IApplicationBuilder UseCustomRateLimiting(this IApplicationBuilder app)
    {
        app.UseMiddleware<RateLimitingMiddleware>();
        return app;
    }

    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
    {
        app.UseMiddleware<SecurityHeadersMiddleware>();
        return app;
    }

    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
    {
        app.UseMiddleware<RequestLoggingMiddleware>();
        return app;
    }
}