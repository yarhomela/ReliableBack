using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using RabbitMQ.Client;
using ReliableBack.API.Hubs;
using ReliableBack.API.Middleware;
using ReliableBack.Application;
using ReliableBack.Infrastructure;
using ReliableBack.Infrastructure.Telemetry;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) =>
        configuration.ReadFrom.Configuration(context.Configuration));

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Services.AddSignalR();

    builder.Services.AddApplication();

    builder.Services.AddOpenTelemetry()
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
                options.AgentHost = builder.Configuration
                    .GetValue<string>("Jaeger:Host") ?? "localhost";
                options.AgentPort = builder.Configuration
                    .GetValue("Jaeger:Port", 6831);
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

    builder.Services.AddInfrastructure(builder.Configuration);

    builder.Services.AddSingleton<IConnectionFactory>(sp =>
        new ConnectionFactory
        {
            HostName = builder.Configuration["RabbitMQ:Host"],
            Port = builder.Configuration.GetValue<int>("RabbitMQ:Port"),
            UserName = builder.Configuration["RabbitMQ:Username"],
            Password = builder.Configuration["RabbitMQ:Password"]
        });

    builder.Services
        .AddHealthChecks()
        .AddNpgSql(
            builder.Configuration.GetConnectionString("PostgreSQL")!,
            name: "postgres",
            tags: new[] { "infrastructure" })
        .AddRabbitMQ(
            name: "rabbitmq",
            tags: new[] { "infrastructure" })
        .AddRedis(
            builder.Configuration["Redis:ConnectionString"]!,
            name: "redis",
            tags: new[] { "infrastructure" });

    builder.Services.AddHostedService<TaskStatusBroadcaster>();

    var allowedOrigins = builder.Configuration
                             .GetSection("Cors:AllowedOrigins")
                             .Get<string[]>()
                         ?? Array.Empty<string>();
    
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("Dashboard", policy =>
            policy
                .WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials());
    });

    var app = builder.Build();

    app.UseMiddleware<CorrelationIdMiddleware>();
    app.UseMiddleware<ExceptionHandlerMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseCors("Dashboard");
    app.UseAuthorization();
    app.MapHub<TaskStatusHub>("/hubs/tasks");
    app.MapControllers();
    app.MapPrometheusScrapingEndpoint("/metrics");

    app.MapHealthChecks("/health");
    app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("infrastructure")
    });

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}