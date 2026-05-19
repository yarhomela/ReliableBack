using ReliableBack.Domain.Tasks;

namespace ReliableBack.Application.Common.Interfaces;

public interface ITaskEventSubscriber
{
    Task SubscribeAsync(Func<Guid, JobStatus, JobStatus, Task> handler, 
        CancellationToken cancellationToken = default);
}