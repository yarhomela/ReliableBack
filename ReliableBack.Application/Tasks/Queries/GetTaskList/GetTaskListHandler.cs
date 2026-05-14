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
        var parameters = new GetTaskListParameters()
        {
            Status = request.Status,
            Page = request.Page,
            PageSize = request.PageSize,
        };
        
        var tasks = await _taskRepository.GetAllAsync(parameters, cancellationToken);

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