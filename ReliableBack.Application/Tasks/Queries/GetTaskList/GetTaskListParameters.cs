using ReliableBack.Domain.Tasks;

namespace ReliableBack.Application.Tasks.Queries.GetTaskList;

public sealed record GetTaskListParameters(
    JobStatus? Status = null,
    int Page = 1,
    int PageSize = 20);