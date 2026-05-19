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
            new GetTaskListParameters(request.Status, request.Page, request.PageSize),
            cancellationToken);

        return tasks
            .Select(TaskMapper.ToDto)
            .ToList();
    }
}