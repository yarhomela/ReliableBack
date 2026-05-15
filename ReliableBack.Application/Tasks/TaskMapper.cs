using System.Text.Json;
using ReliableBack.Domain.Tasks;

namespace ReliableBack.Application.Tasks;

internal static class TaskMapper
{
    internal static TaskDto ToDto(TaskItem task) => new(
        task.Id,
        task.Type,
        task.Payload ?? JsonDocument.Parse("{}"),
        task.Status,
        task.Priority,
        task.RetryCount,
        task.MaxRetries,
        task.ErrorMessage,
        task.ScheduledAt,
        task.CreatedAt,
        task.UpdatedAt);
}