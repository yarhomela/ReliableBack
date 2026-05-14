using MediatR;
using ReliableBack.Application.Common.Interfaces;

namespace ReliableBack.Application.Tasks.Queries.GetTaskById;

public sealed class GetTaskByIdQueryHandler : IRequestHandler<GetTaskByIdQuery, TaskDto?>
{
    private readonly ITaskRepository _taskRepository;
    private readonly ITaskCache _taskCache;

    public GetTaskByIdQueryHandler(ITaskRepository taskRepository, ITaskCache taskCache)
    {
        _taskRepository = taskRepository;
        _taskCache = taskCache;
    }

    public async Task<TaskDto?> Handle(GetTaskByIdQuery request, CancellationToken cancellationToken)
    {
        var cached = await _taskCache.GetAsync(request.Id, cancellationToken);
        if (cached is not null) return MapToDto(cached);
        
        var task = await _taskRepository.GetByIdAsync(request.Id, cancellationToken);
        if (task is null) return null; // remove null
        
        await _taskCache.SetAsync(task, cancellationToken);

        return MapToDto(task);
    }

    private static TaskDto MapToDto(Domain.Tasks.TaskItem task) => new (
        task.Id,
        task.Type,
        task.Payload, // operate null
        task.Status,
        task.Priority,
        task.RetryCount,
        task.MaxRetries,
        task.ErrorMessage,
        task.ScheduledAt,
        task.CreatedAt,
        task.UpdatedAt);
}