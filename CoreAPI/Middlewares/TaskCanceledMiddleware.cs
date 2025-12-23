namespace CoreAPI.Middlewares;

public class TaskCanceledMiddleware(RequestDelegate next, ILogger<TaskCanceledException> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<TaskCanceledException> _logger = logger;
    
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (TaskCanceledException)
        {
           _logger.LogInformation("Operation is canceled."); 
        }
    }
}
