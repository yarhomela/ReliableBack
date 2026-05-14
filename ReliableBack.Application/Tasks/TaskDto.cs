using System.Text.Json;
using ReliableBack.Domain.Tasks;
using TaskStatus = ReliableBack.Domain.Tasks.TaskStatus;

namespace ReliableBack.Application.Tasks;

public sealed record TaskDto(
    Guid Id,
    string Type,
    JsonDocument Payload,
    TaskStatus Status,
    TaskPriority Priority,
    int RetryCount,
    int MaxRetries,
    string? ErrorMessage,
    DateTime? ScheduledAt,
    DateTime CreatedAt,
    DateTime UpdatedAt
);