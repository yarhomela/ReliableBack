using Microsoft.AspNetCore.Builder;
using OpenTelemetry.Metrics;
using ReliableBack.Application;
using ReliableBack.Infrastructure;
using ReliableBack.Infrastructure.Configuration;
using ReliableBack.Infrastructure.Messaging.Settings;
using ReliableBack.Worker;
using ReliableBack.Worker.Extensions;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddWorkerObservability(builder.Configuration)
    .AddHostedService<TaskWorkerService>()
    .AddHostedService<WatchdogService>();

var webBuilder = WebApplication.CreateBuilder(args);
webBuilder.Services.AddOpenTelemetry()
    .WithMetrics(metrics => metrics
        .AddMeter("ReliableBack")
        .AddPrometheusExporter());

var metricsApp = webBuilder.Build();
metricsApp.MapPrometheusScrapingEndpoint("/metrics");

var host = builder.Build();

var metricsSettings = builder.Configuration.GetRequiredSettings<MetricsSettings>("Metrics");

await Task.WhenAll(host.RunAsync(), metricsApp.RunAsync(metricsSettings.Url));