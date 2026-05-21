using RabbitMQ.Client;
using ReliableBack.Application.Common;

namespace ReliableBack.Infrastructure.Messaging;

public static class RabbitMqInitializer
{
    public static async Task DeclareQueuesAsync(IChannel channel)
    {
        foreach (var queue in new[]
                 {
                     QueueNames.High,
                     QueueNames.Normal,
                     QueueNames.Low
                 })
        {
            await channel.QueueDeclareAsync(
                queue: queue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);
        }
    }
}