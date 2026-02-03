using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Application.Middlewares;

public class ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception.");
            var status = ex switch
            {
                ArgumentException or ValidationException or BadHttpRequestException => 400 /*Bad request*/,
                KeyNotFoundException => 404 /*Not found*/,
                _ => 500 /*Internal Server Error*/
            };

            var problem = new ProblemDetails
            {
                Status = status,
                Title = status == 500 ? "Unexpected error" : ex?.Message,
                Type = status == 500 ? "https://example.com/problems/unexpected" : "about:blank",
                Instance = context.Request.Path
            };

            context.Response.StatusCode = status;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsJsonAsync(problem);

        }
    }
}
