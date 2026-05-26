using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using ReliableBack.Infrastructure.Configuration;
using ReliableBack.Infrastructure.Messaging.Settings;
using ReliableBack.Infrastructure.Telemetry;

namespace ReliableBack.API.Extensions;

public static class ObservabilityExtensions
{
    public static void AddObservability(this IServiceCollection services, IConfiguration configuration)
    {
        var jaegerSettings = configuration.GetRequiredSettings<JaegerSettings>("Jaeger");

        services.AddOpenTelemetry()
            .WithTracing(tracing => tracing
                .SetResourceBuilder(ResourceBuilder
                    .CreateDefault()
                    .AddService(TelemetryConstants.ApiServiceName))
                .AddAspNetCoreInstrumentation(options =>
                {
                    options.Filter = ctx =>
                        !ctx.Request.Path.StartsWithSegments("/health");
                })
                .AddHttpClientInstrumentation()
                .AddEntityFrameworkCoreInstrumentation()
                .AddSource(TelemetryConstants.ApiActivitySource)
                .AddJaegerExporter(options =>
                {
                    options.AgentHost = jaegerSettings.Host;
                    options.AgentPort = jaegerSettings.Port;
                }))
            .WithMetrics(metrics => metrics
                .SetResourceBuilder(ResourceBuilder
                    .CreateDefault()
                    .AddService(TelemetryConstants.ApiServiceName))
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddMeter("ReliableBack")
                .AddPrometheusExporter());
    }
}