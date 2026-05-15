using System.Collections.Generic;
using MediatR;
using ReliableBack.Domain.Tasks;

namespace ReliableBack.Application.Tasks.Queries.GetTaskList;

public sealed record GetTaskListQuery(
    JobStatus? Status = null,
    int Page = 1,
    int PageSize = 20
) : IRequest<IReadOnlyList<TaskDto>>;