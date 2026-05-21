using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using ReliableBack.Application;
using ReliableBack.Infrastructure;
using ReliableBack.Infrastructure.Telemetry;
using ReliableBack.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHostedService<TaskWorkerService>();
builder.Services.AddHostedService<WatchdogService>();

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .SetResourceBuilder(ResourceBuilder
            .CreateDefault()
            .AddService(TelemetryConstants.WorkerServiceName))
        .AddHttpClientInstrumentation()
        .AddSource(TelemetryConstants.WorkerActivitySource)
        .AddJaegerExporter(options =>
        {
            options.AgentHost = builder.Configuration
                .GetValue<string>("Jaeger:Host") ?? "localhost";
            options.AgentPort = builder.Configuration
                .GetValue("Jaeger:Port", 6831);
        }));

var host = builder.Build();
host.Run();