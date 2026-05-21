using ReliableBack.Domain.Tasks;

namespace ReliableBack.Application.Common.Interfaces;

public interface ITaskEventPublisher
{
    Task PublishStatusChangedAsync(Guid taskId, JobStatus previousStatus, JobStatus newStatus,
        CancellationToken cancellationToken = default);
}