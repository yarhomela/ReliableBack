using ReliableBack.Domain.Common;

namespace ReliableBack.Domain.Tasks.Events;

public sealed class TaskStatusChangedEvent : IDomainEvent
{
    public Guid TaskId { get; }
    
    public TaskStatus PreviousStatus { get; }
    
    public TaskStatus NewStatus { get; }
    
    public DateTime OccurredAt { get; }

    public TaskStatusChangedEvent(
        Guid taskId,
        TaskStatus previousStatus,
        TaskStatus newStatus)
    {
        TaskId = taskId;
        PreviousStatus = previousStatus;
        NewStatus = newStatus;
        OccurredAt = DateTime.UtcNow;
    }
}