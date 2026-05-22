using System.Diagnostics;
using MediatR;
using ReliableBack.Application.Common;
using ReliableBack.Application.Common.Interfaces;
using ReliableBack.Application.Common.Telemetry;
using ReliableBack.Domain.Tasks;

namespace ReliableBack.Application.Tasks.Commands.EnqueueTask;

public sealed class EnqueueTaskCommandHandler : IRequestHandler<EnqueueTaskCommand, Guid>
{
    private static readonly ActivitySource ActivitySource =
        new("ReliableBack.API");

    private readonly ITaskRepository _taskRepository;
    private readonly IMessagePublisher _messagePublisher;
    private readonly TaskMetrics _metrics;

    public EnqueueTaskCommandHandler(
        ITaskRepository taskRepository,
        IMessagePublisher messagePublisher,
        TaskMetrics metrics)
    {
        _taskRepository = taskRepository;
        _messagePublisher = messagePublisher;
        _metrics = metrics;
    }

    public async Task<Guid> Handle(EnqueueTaskCommand request, CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity("task.enqueue");

        var task = new TaskItem(
            type: request.Type,
            payload: request.Payload,
            priority: request.Priority,
            maxRetries: request.MaxRetries,
            scheduledAt: request.ScheduledAt);

        activity?.SetTag("task.id", task.Id.ToString());
        activity?.SetTag("task.type", task.Type);
        activity?.SetTag("task.priority", task.Priority.ToString());

        await _taskRepository.AddAsync(task, cancellationToken);

        await _messagePublisher.PublishAsync(
            message: task,
            queueName: QueueNames.FromPriority(request.Priority),
            cancellationToken: cancellationToken
        );

        activity?.SetTag("task.status", "enqueued");

        _metrics.RecordTaskEnqueued(task.Type, task.Priority.ToString());

        return task.Id;
    }
}