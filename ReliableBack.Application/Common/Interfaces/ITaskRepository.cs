using ReliableBack.Application.Tasks.Queries.GetTaskList;
using ReliableBack.Domain.Tasks;

namespace ReliableBack.Application.Common.Interfaces;

public interface ITaskRepository
{
    Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<IReadOnlyList<TaskItem>> GetAllAsync(GetTaskListParameters parameters,
        CancellationToken cancellationToken = default);
    
    Task AddAsync(TaskItem task, CancellationToken cancellationToken = default);
    
    Task UpdateAsync(TaskItem task, CancellationToken cancellationToken = default);
}