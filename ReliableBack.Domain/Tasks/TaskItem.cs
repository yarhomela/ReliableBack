using System.Text.Json;
using ReliableBack.Domain.Common;
using ReliableBack.Domain.Tasks.Events;

namespace ReliableBack.Domain.Tasks;

public class TaskItem : Entity
{
    private TaskItem() { }
    
    public TaskItem(
        string type,
        JsonDocument payload,
        JobPriority priority = JobPriority.Normal,
        int maxRetries = 3,
        DateTime? scheduledAt = null)
    {
        Type = type;
        Payload = payload;
        Priority = priority;
        MaxRetries = maxRetries;
        ScheduledAt = scheduledAt;
        Status = JobStatus.Pending;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public string Type { get; private set; } = string.Empty;
    
    public JsonDocument? Payload { get; private set; }
    
    public JobStatus Status { get; private set; }
    
    public JobPriority Priority { get; private set; }
    
    public int RetryCount { get; private set; }
    
    public int MaxRetries { get; private set; }
    
    public DateTime? ScheduledAt { get; private set; }
    
    public string? ErrorMessage { get; private set; }
    
    public DateTime CreatedAt { get; private set; }
    
    public DateTime UpdatedAt { get; private set; }
    
    public void ChangeStatus(JobStatus newStatus)
    {
        var previousStatus = Status;
        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new TaskStatusChangedEvent(Id, previousStatus, newStatus));
    }
    
    public void RecordFailure(string errorMessage)
    {
        RetryCount++;
        ErrorMessage = errorMessage;
        UpdatedAt = DateTime.UtcNow;

        var newStatus = RetryCount >= MaxRetries
            ? JobStatus.DeadLettered
            : JobStatus.Retrying;

        ChangeStatus(newStatus);
    }
    
    public void MarkAsRunning() => ChangeStatus(JobStatus.Running);
    
    public void MarkAsCompleted() => ChangeStatus(JobStatus.Completed);
    
    public void MarkAsQueued() => ChangeStatus(JobStatus.Queued);
}