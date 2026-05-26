using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using ReliableBack.Infrastructure.Configuration;
using ReliableBack.Infrastructure.Messaging.Settings;
using ReliableBack.Infrastructure.Telemetry;

namespace ReliableBack.Worker.Extensions;

public static class WorkerObservabilityExtensions
{
    public static IServiceCollection AddWorkerObservability(this IServiceCollection services,
        IConfiguration configuration)
    {
        var jaegerSettings = configuration.GetRequiredSettings<JaegerSettings>("Jaeger");

        services.AddOpenTelemetry()
            .WithTracing(tracing => tracing
                .SetResourceBuilder(ResourceBuilder
                    .CreateDefault()
                    .AddService(TelemetryConstants.WorkerServiceName))
                .AddHttpClientInstrumentation()
                .AddSource(TelemetryConstants.WorkerActivitySource)
                .AddJaegerExporter(options =>
                {
                    options.AgentHost = jaegerSettings.Host;
                    options.AgentPort = jaegerSettings.Port;
                }))
            .WithMetrics(metrics => metrics
                .SetResourceBuilder(ResourceBuilder
                    .CreateDefault()
                    .AddService(TelemetryConstants.WorkerServiceName))
                .AddRuntimeInstrumentation()
                .AddMeter("ReliableBack")
                .AddPrometheusExporter());

        return services;
    }
}