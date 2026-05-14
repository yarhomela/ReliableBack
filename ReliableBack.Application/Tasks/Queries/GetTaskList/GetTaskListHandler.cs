using MediatR;
using ReliableBack.Application.Common.Interfaces;

namespace ReliableBack.Application.Tasks.Queries.GetTaskList;

public sealed class GetTaskListQueryHandler : IRequestHandler<GetTaskListQuery, IReadOnlyList<TaskDto>>
{
    private readonly ITaskRepository _taskRepository;

    public GetTaskListQueryHandler(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<IReadOnlyList<TaskDto>> Handle(GetTaskListQuery request, CancellationToken cancellationToken)
    {
        var tasks = await _taskRepository.GetAllAsync(
            request.Status,
            request.Page,
            request.PageSize,
            cancellationToken
        );

        return tasks
            .Select(task => new TaskDto(
                Id:           task.Id,
                Type:         task.Type,
                Payload:      task.Payload, // operate null
                Status:       task.Status,
                Priority:     task.Priority,
                RetryCount:   task.RetryCount,
                MaxRetries:   task.MaxRetries,
                ErrorMessage: task.ErrorMessage,
                ScheduledAt:  task.ScheduledAt,
                CreatedAt:    task.CreatedAt,
                UpdatedAt:    task.UpdatedAt
            ))
            .ToList();
    }
}