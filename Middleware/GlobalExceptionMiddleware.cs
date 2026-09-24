using System.Text.Json;
using ClothingErp.Api.Dtos;

namespace ClothingErp.Api.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next; _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try { await _next(context); }
        catch (KeyNotFoundException ex) { await WriteAsync(context, 404, ex.Message); }
        catch (InvalidOperationException ex) { await WriteAsync(context, 400, ex.Message); }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled API exception.");
            await WriteAsync(context, 500, "Something went wrong.");
        }
    }

    private static async Task WriteAsync(HttpContext context, int status, string message)
    {
        context.Response.StatusCode = status;
        context.Response.ContentType = "application/json";
        var body = new ApiResponse<object> { Success = false, Message = message };
        await context.Response.WriteAsync(JsonSerializer.Serialize(body));
    }
}
