using ReliableBack.Domain.Tasks;
using TaskStatus = ReliableBack.Domain.Tasks.TaskStatus;

namespace ReliableBack.Application.Common.Interfaces;

public interface ITaskRepository
{
    Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<IReadOnlyList<TaskItem>> GetAllAsync(
        TaskStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
    
    Task AddAsync(TaskItem task, CancellationToken cancellationToken = default);
    
    Task UpdateAsync(TaskItem task, CancellationToken cancellationToken = default);
}