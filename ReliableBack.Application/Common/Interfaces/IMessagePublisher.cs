using System.Threading;
using System.Threading.Tasks;

namespace ReliableBack.Application.Common.Interfaces;

public interface IMessagePublisher
{
    Task PublishAsync<T>(T message, string queueName, CancellationToken cancellationToken = default)
        where T : class;
}