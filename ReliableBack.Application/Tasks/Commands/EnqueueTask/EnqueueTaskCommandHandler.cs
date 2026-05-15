using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using ReliableBack.Application.Common.Interfaces;
using ReliableBack.Domain.Tasks;

namespace ReliableBack.Application.Tasks.Commands.EnqueueTask;

public sealed class EnqueueTaskCommandHandler : IRequestHandler<EnqueueTaskCommand, Guid>
{
    private readonly ITaskRepository _taskRepository;
    private readonly IMessagePublisher _messagePublisher;

    public EnqueueTaskCommandHandler(ITaskRepository taskRepository, IMessagePublisher messagePublisher)
    {
        _taskRepository = taskRepository;
        _messagePublisher = messagePublisher;
    }

    public async Task<Guid> Handle(EnqueueTaskCommand request, CancellationToken cancellationToken)
    {
        var task = new TaskItem(
            type: request.Type,
            payload: request.Payload,
            priority: request.Priority,
            maxRetries: request.MaxRetries,
            scheduledAt: request.ScheduledAt);
        
        await _taskRepository.AddAsync(task, cancellationToken);
        
        await _messagePublisher.PublishAsync(
            message: task,
            queueName: GetQueueName(request.Priority),
            cancellationToken: cancellationToken
        );

        return task.Id;
    }

    private static string GetQueueName(JobPriority priority) => priority switch
    {
        JobPriority.High   => "tasks.high",
        JobPriority.Normal => "tasks.normal",
        JobPriority.Low    => "tasks.low",
        _                   => "tasks.normal"
    };
}