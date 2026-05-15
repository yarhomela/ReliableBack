using MediatR;

namespace ReliableBack.Application.Tasks.Queries.GetTaskById;

public sealed record GetTaskByIdQuery(Guid Id) : IRequest<TaskDto?>;