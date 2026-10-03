using System.Net;
using System.Text.Json;
using TaskMasterPro.API.Models;
using TaskMasterPro.Application.Exceptions;

namespace TaskMasterPro.API.Middlewares;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
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
        context.Response.ContentType = "application/json";
        
        var response = ApiResponse<object>.Fail("An error occurred while processing your request.");
        
        switch (exception)
        {
            case ValidationException validationEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Message = "Validation failed";
                response.Errors = validationEx.Errors;
                break;
                
            case NotFoundException notFoundEx:
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                response.Message = notFoundEx.Message;
                // Log ownership breaches explicitly
                var userId = context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                _logger.LogWarning("Not Found or Ownership breach: {Message} | User: {UserId}", notFoundEx.Message, userId ?? "Anonymous");
                break;
                
            case UnauthorizedException unauthEx:
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                response.Message = unauthEx.Message;
                break;

            default:
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response.Message = "Internal Server Error";
                var errorContext = new { 
                    Path = context.Request.Path,
                    UserId = context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value 
                };
                _logger.LogError(exception, "Unhandled exception caught. Context: {@ErrorContext}", errorContext);
                break;
        }

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        await context.Response.WriteAsync(json);
    }
}
