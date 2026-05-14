using System.Text.Json;
using MediatR;
using ReliableBack.Domain.Tasks;

namespace ReliableBack.Application.Tasks.Commands.EnqueueTask;

public sealed record EnqueueTaskCommand(
    string Type,
    JsonDocument Payload,
    TaskPriority Priority = TaskPriority.Normal,
    int MaxRetries = 3,
    DateTime? ScheduledAt = null
) : IRequest<Guid>;