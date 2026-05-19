using System.Text.Json;
using Microsoft.Extensions.Logging;
using ReliableBack.Application.Common.Interfaces;
using ReliableBack.Application.Tasks;
using ReliableBack.Domain.Tasks;
using StackExchange.Redis;

namespace ReliableBack.Infrastructure.Messaging;

public class RedisPubSubPublisher : ITaskEventPublisher
{
    private const string Channel = "task-status-changed";

    private readonly ILogger<RedisPubSubPublisher> _logger;
    private readonly IConnectionMultiplexer _redis;

    public RedisPubSubPublisher(IConnectionMultiplexer redis, ILogger<RedisPubSubPublisher> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task PublishStatusChangedAsync(Guid taskId, JobStatus previousStatus, JobStatus newStatus,
        CancellationToken cancellationToken = default)
    {
        var notification = new TaskStatusChangedNotification(
            taskId,
            previousStatus,
            newStatus,
            DateTime.UtcNow);

        var message = JsonSerializer.Serialize(notification);

        var subscriber = _redis.GetSubscriber();
        await subscriber.PublishAsync(
            RedisChannel.Literal(Channel),
            message);

        _logger.LogDebug(
            "Published status change for task {TaskId}: {Previous} -> {New}",
            taskId, previousStatus, newStatus);
    }
}