using System.Text.Json;
using ReliableBack.Domain.Common;
using ReliableBack.Domain.Tasks.Events;

namespace ReliableBack.Domain.Tasks;

public class TaskItem : Entity
{
    public string Type { get; set; } = string.Empty;

    public JsonDocument? Payload { get; set; }

    public TaskStatus Status { get; set; }

    public TaskPriority Priority { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public int RetryCount { get; set; }

    public int MaxRetries { get; set; }

    public DateTime? ScheduledAt { get; set; }

    public string? ErrorMessage { get; set; }
    
    public void ChangeStatus(TaskStatus newStatus)
    {
        var previousStatus = Status;
        Status = newStatus;
        
        RaiseDomainEvent(new TaskStatusChangedEvent(Id, previousStatus, newStatus));
    }
}