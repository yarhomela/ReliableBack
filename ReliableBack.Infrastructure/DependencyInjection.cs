using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ReliableBack.Application.Common.Interfaces;
using ReliableBack.Application.Common.Telemetry;
using ReliableBack.Infrastructure.Caching;
using ReliableBack.Infrastructure.Configuration;
using ReliableBack.Infrastructure.Messaging;
using ReliableBack.Infrastructure.Messaging.Settings;
using ReliableBack.Infrastructure.Persistence;
using ReliableBack.Infrastructure.Persistence.Repositories;
using StackExchange.Redis;

namespace ReliableBack.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PostgreSQL")
                               ?? throw new InvalidOperationException(
                                   "Connection string 'PostgreSQL' is not configured.");

        services.AddDbContext<ReliableBackDbContext>(options => options.UseNpgsql(connectionString));

        services.AddScoped<ITaskRepository>(sp => new TaskRepository(
            sp.GetRequiredService<ReliableBackDbContext>(),
            connectionString));

        services.AddScoped<IMessagePublisher, RabbitMqPublisher>();
        services.AddScoped<ITaskCache, RedisTaskCache>();

        services.Configure<RabbitMqSettings>(configuration.GetSection("RabbitMQ"));
        services.Configure<WorkerSettings>(configuration.GetSection("Worker"));

        var redisSettings = configuration.GetRequiredSettings<RedisSettings>("Redis");

        services.AddSingleton<IConnectionMultiplexer>(
            ConnectionMultiplexer.Connect(redisSettings.ConnectionString));

        services.AddScoped<ITaskEventPublisher, RedisPubSubPublisher>();
        services.AddSingleton<ITaskEventSubscriber, RedisPubSubSubscriber>();
        services.AddSingleton<TaskMetrics>();

        return services;
    }
}