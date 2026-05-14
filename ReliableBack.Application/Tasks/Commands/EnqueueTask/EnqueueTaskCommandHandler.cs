using MediatR;
using ReliableBack.Application.Common.Interfaces;
using ReliableBack.Domain.Tasks;

namespace ReliableBack.Application.Tasks.Commands.EnqueueTask;

public sealed class EnqueueTaskCommandHandler
    : IRequestHandler<EnqueueTaskCommand, Guid>
{
    private readonly ITaskRepository _taskRepository;
    private readonly IMessagePublisher _messagePublisher;

    public EnqueueTaskCommandHandler(
        ITaskRepository taskRepository,
        IMessagePublisher messagePublisher)
    {
        _taskRepository = taskRepository;
        _messagePublisher = messagePublisher;
    }

    public async Task<Guid> Handle(EnqueueTaskCommand request, CancellationToken cancellationToken)
    {
        var task = new TaskItem
        {
            Type = request.Type,
            Payload = request.Payload,
            Priority = request.Priority,
            MaxRetries = request.MaxRetries,
            ScheduledAt = request.ScheduledAt
        };
        await _taskRepository.AddAsync(task, cancellationToken);
        
        await _messagePublisher.PublishAsync(
            message: task,
            queueName: GetQueueName(request.Priority),
            cancellationToken: cancellationToken
        );

        return task.Id;
    }

    private static string GetQueueName(TaskPriority priority) => priority switch
    {
        TaskPriority.High   => "tasks.high",
        TaskPriority.Normal => "tasks.normal",
        TaskPriority.Low    => "tasks.low",
        _                   => "tasks.normal"
    };
}