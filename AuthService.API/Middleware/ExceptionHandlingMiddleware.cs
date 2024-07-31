using System.Net;
using System.Text.Json;
using AuthService.Domain.Exceptions;
using FluentValidation;

namespace AuthService.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        var errorResponse = new
        {
            error = exception.Message,
            details = (string?)null
        };

        switch (exception)
        {
            case ValidationException validationException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse = new
                {
                    error = "Validation failed",
                    details = (string?)string.Join("; ", validationException.Errors.Select(e => e.ErrorMessage))
                };
                _logger.LogWarning("Validation error: {Details}", errorResponse.details);
                break;

            case InvalidCredentialsException:
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                _logger.LogWarning("Invalid credentials attempt");
                break;

            case AccountLockedException accountLockedException:
                response.StatusCode = (int)HttpStatusCode.Forbidden;
                errorResponse = new
                {
                    error = exception.Message,
                    details = (string?)$"Account locked until {accountLockedException.LockoutEndDate:yyyy-MM-dd HH:mm:ss} UTC"
                };
                _logger.LogWarning("Account locked: {Message}", exception.Message);
                break;

            case InvalidTokenException:
            case TokenReusedException:
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                _logger.LogWarning("Token error: {Message}", exception.Message);
                break;

            case DomainException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                _logger.LogWarning("Domain error: {Message}", exception.Message);
                break;

            case UnauthorizedAccessException:
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                errorResponse = new
                {
                    error = "Unauthorized access",
                    details = (string?)null
                };
                _logger.LogWarning("Unauthorized access attempt");
                break;

            default:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorResponse = new
                {
                    error = "An error occurred while processing your request",
                    details = (string?)null
                };
                _logger.LogError(exception, "Unhandled exception occurred");
                break;
        }

        var result = JsonSerializer.Serialize(errorResponse);
        await response.WriteAsync(result);
    }
}