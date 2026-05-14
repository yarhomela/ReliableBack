using MediatR;
using TaskStatus = ReliableBack.Domain.Tasks.TaskStatus;

namespace ReliableBack.Application.Tasks.Queries.GetTaskList;

public sealed record GetTaskListQuery(
    TaskStatus? Status = null,
    int Page = 1,
    int PageSize = 20
) : IRequest<IReadOnlyList<TaskDto>>;