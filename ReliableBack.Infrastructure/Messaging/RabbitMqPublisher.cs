using Microsoft.Extensions.Logging;
using ReliableBack.Application.Common.Interfaces;

namespace ReliableBack.Infrastructure.Messaging;

public class RabbitMqPublisher : IMessagePublisher
{
    private readonly ILogger<RabbitMqPublisher> _logger;

    public RabbitMqPublisher(ILogger<RabbitMqPublisher> logger)
    {
        _logger = logger;
    }
    
    public Task PublishAsync<T>(T message, string queueName, CancellationToken cancellationToken = default)
        where T : class
    {
        // TODO: замінити на реальну публікацію в RabbitMQ
        _logger.LogInformation(
            "Publishing message of type {MessageType} to queue {QueueName}",
            typeof(T).Name,
            queueName);

        return Task.CompletedTask;
    }
}