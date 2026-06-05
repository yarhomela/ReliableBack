using System.Text.Json;
using ReliableBack.Domain.Tasks;

namespace ReliableBack.Application.Tasks;

public sealed record TaskDto(
    Guid Id,
    string Type,
    JsonDocument Payload,
    JobStatus Status,
    JobPriority Priority,
    int RetryCount,
    int MaxRetries,
    string? ErrorMessage,
    DateTime? ScheduledAt,
    DateTime CreatedAt,
    DateTime UpdatedAt
);