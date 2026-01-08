using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CoreAPI.Exceptions;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger = logger;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var status = exception switch
        {
            ArgumentException or ValidationException or BadHttpRequestException => 400 /*Bad request*/,
            KeyNotFoundException => 404 /*Not found*/,
            _ => 500 /*Internal Server Error*/
        };

        if (_logger.IsEnabled(LogLevel.Error) || _logger.IsEnabled(LogLevel.Warning))
        {
            if (status == 500)
            {
                _logger.LogError(exception, "Unhandled exception: {Message}, path: {Path}",
                    exception.Message, context.Request.Path);
            }
            else
            {
                
                _logger.LogWarning(exception, "Application exception: {Message}, path: {Path}",
                    exception.Message, context.Request.Path);
            }
        }

        var problem = new ProblemDetails
        {
            Status = status,
            Title = status == 500 ? "Server Error" : "Unhandled exception",
            Type = status == 500 ? "https://example.com/problems/unexpected" : "about:blank",
            Instance = context.Request.Path,
            Detail = status != 500 ? exception.Message : null,
        };

        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(problem, cancellationToken: cancellationToken);

        return true;
    }
}