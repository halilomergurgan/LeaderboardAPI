using System.Net;
using System.Text.Json;
using RuneGames.Application.Common.Exceptions;

namespace RuneGames.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);

            if (!context.Response.HasStarted)
            {
                if (context.Response.StatusCode == 401)
                {
                    await WriteJsonResponse(context, HttpStatusCode.Unauthorized, "Authentication required. Please provide a valid token.");
                    return;
                }

                if (context.Response.StatusCode == 403)
                {
                    await WriteJsonResponse(context, HttpStatusCode.Forbidden, "You do not have permission to access this resource.");
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        HttpStatusCode statusCode;
        string message;
        IReadOnlyDictionary<string, string[]>? errors = null;

        switch (exception)
        {
            case ValidationException ex:
                statusCode = HttpStatusCode.BadRequest;
                message = "Validation failed.";
                errors = ex.Errors;
                break;
            case NotFoundException ex:
                statusCode = HttpStatusCode.NotFound;
                message = ex.Message;
                break;
            case UnauthorizedAccessException:
                statusCode = HttpStatusCode.Unauthorized;
                message = "Unauthorized.";
                break;
            default:
                statusCode = HttpStatusCode.InternalServerError;
                message = "An unexpected error occurred.";
                break;
        }

        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            status = (int)statusCode,
            message,
            errors
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
    }

    private static async Task WriteJsonResponse(HttpContext context, HttpStatusCode statusCode, string message)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            status = (int)statusCode,
            message
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
    }
}
