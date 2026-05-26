using RabbitMQ.Client;
using ReliableBack.API.Hubs;
using ReliableBack.Infrastructure.Configuration;
using ReliableBack.Infrastructure.Messaging.Settings;

namespace ReliableBack.API.Extensions;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddApiInfrastructure(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSignalR();
        services.AddHostedService<TaskStatusBroadcaster>();

        services.AddCorsPolicy(configuration);
        services.AddApiHealthChecks(configuration);
        services.AddRabbitMqConnectionFactory(configuration);

        return services;
    }

    private static void AddCorsPolicy(this IServiceCollection services, IConfiguration configuration)
    {
        var corsSettings = configuration.GetRequiredSettings<CorsSettings>("Cors");

        services.AddCors(options =>
            options.AddPolicy("Dashboard", policy =>
                policy
                    .WithOrigins(corsSettings.AllowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials()));
    }

    private static void AddApiHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddHealthChecks()
            .AddNpgSql(
                configuration.GetConnectionString("PostgreSQL")!,
                name: "postgres",
                tags: ["infrastructure"])
            .AddRabbitMQ(
                name: "rabbitmq",
                tags: ["infrastructure"])
            .AddRedis(
                configuration["Redis:ConnectionString"]!,
                name: "redis",
                tags: ["infrastructure"]);
    }

    private static void AddRabbitMqConnectionFactory(this IServiceCollection services, IConfiguration configuration)
    {
        var rabbitMqSettings = configuration.GetRequiredSettings<RabbitMqSettings>("RabbitMQ");

        services.AddSingleton<IConnectionFactory>(_ =>
            new ConnectionFactory
            {
                HostName = rabbitMqSettings.Host,
                Port = rabbitMqSettings.Port,
                UserName = rabbitMqSettings.Username,
                Password = rabbitMqSettings.Password
            });
    }
}