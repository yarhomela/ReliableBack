using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using ReliableBack.API.Hubs;
using ReliableBack.API.Middleware;

namespace ReliableBack.API.Extensions;

public static class MiddlewareExtensions
{
    public static void UseApiMiddleware(this WebApplication app)
    {
        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseMiddleware<ExceptionHandlerMiddleware>();
    }

    public static void UseApiEndpoints(this WebApplication app)
    {
        app.MapControllers();
        app.MapHub<TaskStatusHub>("/hubs/tasks");
        app.MapPrometheusScrapingEndpoint("/metrics");
        app.MapHealthChecks("/health");
        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("infrastructure")
        });
    }
}