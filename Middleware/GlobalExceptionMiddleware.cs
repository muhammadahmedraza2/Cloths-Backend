using System.Text.Json;
using ClothingErp.Api.Dtos;

namespace ClothingErp.Api.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(
                ex,
                "Resource not found.");

            await WriteAsync(
                context,
                StatusCodes.Status404NotFound,
                ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(
                ex,
                "Invalid operation.");

            await WriteAsync(
                context,
                StatusCodes.Status400BadRequest,
                ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unhandled API exception.");

            var message =
                _environment.IsDevelopment()
                    ? ex.Message
                    : "Something went wrong.";

            await WriteAsync(
                context,
                StatusCodes.Status500InternalServerError,
                message);
        }
    }

    private static async Task WriteAsync(
        HttpContext context,
        int status,
        string message)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.StatusCode = status;
        context.Response.ContentType = "application/json";

        var body = new ApiResponse<object>
        {
            Success = false,
            Message = message
        };

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(body));
    }
}