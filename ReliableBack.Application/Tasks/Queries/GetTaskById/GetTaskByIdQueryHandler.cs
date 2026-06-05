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
        if (cached is not null) return TaskMapper.ToDto(cached);

        var task = await _taskRepository.GetByIdAsync(request.Id, cancellationToken);
        if (task is null) return null; // operate null

        await _taskCache.SetAsync(task, cancellationToken);

        return TaskMapper.ToDto(task);
    }
}