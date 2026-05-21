using ReliableBack.Domain.Tasks;

namespace ReliableBack.Application.Tasks;

public sealed record TaskStatusChangedNotification(
    Guid TaskId,
    JobStatus PreviousStatus,
    JobStatus NewStatus,
    DateTime OccurredAt);