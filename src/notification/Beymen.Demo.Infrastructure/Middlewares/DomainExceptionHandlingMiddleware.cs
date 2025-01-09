using Beymen.Demo.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Beymen.Demo.Infrastructure.Middlewares;

public class DomainExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<DomainExceptionHandlingMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<DomainExceptionHandlingMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (DomainException ex)
        {
            await HandleDomainExceptionAsync(context, ex);
        }
    }

    private async Task HandleDomainExceptionAsync(HttpContext context, DomainException exception)
    {
        _logger.LogError(exception, "Domain exception occurred");

        var response = new DomainExceptionResponse("error", exception.Message, GetExceptionDetails(exception));

        context.Response.ContentType = "application/json";

        switch (exception)
        {
            case NotificationNotFoundException:
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                break;

            case NotificationValidationException validationEx:
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                response = new DomainExceptionResponse("error", validationEx.Message, validationEx.Errors);
                break;

            default:
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                break;
        }

        var jsonResponse = JsonSerializer.Serialize(response);
        await context.Response.WriteAsync(jsonResponse);
    }

    private static object? GetExceptionDetails(DomainException exception)
    {
        return exception switch
        {
            NotificationValidationException validationEx => new { ValidationErrors = validationEx.Errors },
            NotificationNotFoundException notFoundEx => new { notFoundEx.NotificationId },
            _ => null
        };
    }
}