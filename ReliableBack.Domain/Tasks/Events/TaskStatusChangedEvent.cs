using ReliableBack.Domain.Common;

namespace ReliableBack.Domain.Tasks.Events;

public sealed class TaskStatusChangedEvent : IDomainEvent
{
    public Guid TaskId { get; }
    
    public JobStatus PreviousStatus { get; }
    
    public JobStatus NewStatus { get; }
    
    public DateTime OccurredAt { get; }

    public TaskStatusChangedEvent(
        Guid taskId,
        JobStatus previousStatus,
        JobStatus newStatus)
    {
        TaskId = taskId;
        PreviousStatus = previousStatus;
        NewStatus = newStatus;
        OccurredAt = DateTime.UtcNow;
    }
}