using System.Collections.Concurrent;
using System.Net;

namespace AuthService.API.Middleware;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private static readonly ConcurrentDictionary<string, (int Count, DateTime ResetTime)> _requestCounts = new();
    private const int MaxRequests = 100;
    private const int TimeWindowMinutes = 1;

    public RateLimitingMiddleware(
        RequestDelegate next,
        ILogger<RateLimitingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var clientId = GetClientIdentifier(context);

        if (!IsRequestAllowed(clientId))
        {
            context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync("{\"error\":\"Too many requests. Please try again later.\"}");
            _logger.LogWarning("Rate limit exceeded for client: {ClientId}", clientId);
            return;
        }

        await _next(context);
    }

    private string GetClientIdentifier(HttpContext context)
    {
        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return ipAddress;
    }

    private bool IsRequestAllowed(string clientId)
    {
        var now = DateTime.UtcNow;

        if (_requestCounts.TryGetValue(clientId, out var requestData))
        {
            if (now < requestData.ResetTime)
            {
                if (requestData.Count >= MaxRequests)
                {
                    return false;
                }

                _requestCounts[clientId] = (requestData.Count + 1, requestData.ResetTime);
            }
            else
            {
                _requestCounts[clientId] = (1, now.AddMinutes(TimeWindowMinutes));
            }
        }
        else
        {
            _requestCounts[clientId] = (1, now.AddMinutes(TimeWindowMinutes));
        }

        CleanupExpiredEntries();

        return true;
    }

    private void CleanupExpiredEntries()
    {
        var now = DateTime.UtcNow;
        var expiredKeys = _requestCounts
            .Where(kvp => now > kvp.Value.ResetTime)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in expiredKeys)
        {
            _requestCounts.TryRemove(key, out _);
        }
    }
}