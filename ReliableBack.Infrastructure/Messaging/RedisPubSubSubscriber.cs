using System.Text.Json;
using Microsoft.Extensions.Logging;
using ReliableBack.Application.Common.Interfaces;
using ReliableBack.Application.Tasks;
using ReliableBack.Domain.Tasks;
using StackExchange.Redis;

namespace ReliableBack.Infrastructure.Messaging;

public class RedisPubSubSubscriber : ITaskEventSubscriber
{
    private const string Channel = "task-status-changed";

    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisPubSubSubscriber> _logger;

    public RedisPubSubSubscriber(IConnectionMultiplexer redis, ILogger<RedisPubSubSubscriber> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task SubscribeAsync(Func<Guid, JobStatus, JobStatus, Task> handler,
        CancellationToken cancellationToken = default)
    {
        var subscriber = _redis.GetSubscriber();

        await subscriber.SubscribeAsync(
            RedisChannel.Literal(Channel),
            async (_, message) =>
            {
                try
                {
                    var notification = JsonSerializer
                        .Deserialize<TaskStatusChangedNotification>(message!);

                    if (notification is null) return;

                    await handler(
                        notification.TaskId,
                        notification.PreviousStatus,
                        notification.NewStatus);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Failed to handle task status change notification");
                }
            });

        _logger.LogInformation(
            "Subscribed to Redis channel '{Channel}'", Channel);
    }
}