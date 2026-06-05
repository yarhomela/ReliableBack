using ReliableBack.Domain.Tasks;

namespace ReliableBack.Application.Common.Interfaces;

public interface ITaskCache
{
    Task<TaskItem?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task SetAsync(TaskItem task, CancellationToken cancellationToken = default);

    Task RemoveAsync(Guid id, CancellationToken cancellationToken = default);
}