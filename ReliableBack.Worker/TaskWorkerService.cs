using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ReliableBack.Application.Common;
using ReliableBack.Application.Common.Interfaces;
using ReliableBack.Domain.Tasks;
using ReliableBack.Infrastructure.Messaging;
using ReliableBack.Infrastructure.Messaging.Settings;

namespace ReliableBack.Worker;

public class TaskWorkerService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    
    private readonly RabbitMqSettings _settings;
    
    private readonly WorkerSettings _workerSettings;
    
    private readonly ILogger<TaskWorkerService> _logger;
    
    private IConnection? _connection;
    
    private IChannel? _channel;
    
    private readonly ITaskEventPublisher _eventPublisher;

    public TaskWorkerService(
        IServiceScopeFactory scopeFactory,
        IOptions<RabbitMqSettings> settings,
        IOptions<WorkerSettings> workerSettings,
        ITaskEventPublisher eventPublisher,
        ILogger<TaskWorkerService> logger)
    {
        _scopeFactory = scopeFactory;
        _settings = settings.Value;
        _workerSettings = workerSettings.Value;
        _eventPublisher =  eventPublisher;
        _logger = logger;
    }
    
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await ConnectToRabbitMqAsync();
        await base.StartAsync(cancellationToken);
        _logger.LogInformation("TaskWorkerService started");
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        foreach (var queue in new[] { QueueNames.High, QueueNames.Normal, QueueNames.Low })
        {
            await ConsumeQueueAsync(queue, stoppingToken);
        }
        
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
    
    private async Task ConsumeQueueAsync(string queueName, CancellationToken stoppingToken)
    {
        await _channel!.BasicQosAsync(
            prefetchSize:  0,
            prefetchCount: (ushort)_workerSettings.MaxConcurrentTasks,
            global:        false);

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (_, ea) =>
        {
            await ProcessMessageAsync(ea, stoppingToken);
        };

        await _channel.BasicConsumeAsync(
            queue:       queueName,
            autoAck:     false,  // підтверджуємо вручну після успішної обробки
            consumer:    consumer);
    }
    
    private async Task ProcessMessageAsync(BasicDeliverEventArgs ea, CancellationToken cancellationToken)
    {
        var taskId = Guid.Empty;

        try
        {
            var body = Encoding.UTF8.GetString(ea.Body.Span);
            var taskItem = JsonSerializer.Deserialize<TaskItem>(body);

            if (taskItem is null)
            {
                await _channel!.BasicRejectAsync(ea.DeliveryTag, requeue: false);
                return;
            }

            taskId = taskItem.Id;

            using var scope = _scopeFactory.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<ITaskRepository>();

            var task = await repository.GetByIdAsync(taskId, cancellationToken);
            if (task is null)
            {
                await _channel!.BasicRejectAsync(ea.DeliveryTag, requeue: false);
                return;
            }
            
            task.MarkAsRunning();
            await repository.UpdateAsync(task, cancellationToken);
            await _eventPublisher.PublishStatusChangedAsync(
                task.Id,
                JobStatus.Queued,
                JobStatus.Running,
                cancellationToken);
            
            await SimulateWorkAsync(task, cancellationToken); // TODO: тут буде виклик реального обробника задачі

            task.MarkAsCompleted();
            await repository.UpdateAsync(task, cancellationToken);
            
            await _eventPublisher.PublishStatusChangedAsync(
                task.Id,
                JobStatus.Running,
                JobStatus.Completed,
                cancellationToken);

            await _channel!.BasicAckAsync(ea.DeliveryTag, multiple: false);

            _logger.LogInformation("Task {TaskId} completed", taskId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process task {TaskId}", taskId);
            await HandleFailureAsync(ea, taskId, ex.Message, cancellationToken);
        }
    }
    
    private async Task HandleFailureAsync(
        BasicDeliverEventArgs ea,
        Guid taskId,
        string errorMessage,
        CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<ITaskRepository>();

            var task = await repository.GetByIdAsync(taskId, cancellationToken);
            if (task is not null)
            {
                task.RecordFailure(errorMessage);
                await repository.UpdateAsync(task, cancellationToken);

                if (task.Status == JobStatus.DeadLettered)
                {
                    await _channel!.BasicRejectAsync(ea.DeliveryTag, requeue: false);
                    _logger.LogWarning("Task {TaskId} moved to dead letter", taskId);
                }
                else
                {
                    await _channel!.BasicNackAsync(ea.DeliveryTag,
                        multiple: false,
                        requeue:  true);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle task failure for {TaskId}", taskId);
            await _channel!.BasicRejectAsync(ea.DeliveryTag, requeue: false);
        }
    }

    private static async Task SimulateWorkAsync(TaskItem task, CancellationToken cancellationToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken); // TODO
    }

    private async Task ConnectToRabbitMqAsync()
    {
        var factory = new ConnectionFactory
        {
            HostName = _settings.Host,
            Port     = _settings.Port,
            UserName = _settings.Username,
            Password = _settings.Password
        };

        _connection = await factory.CreateConnectionAsync();
        _channel = await _connection.CreateChannelAsync();
        
        await RabbitMqInitializer.DeclareQueuesAsync(_channel);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("TaskWorkerService stopping...");
        await base.StopAsync(cancellationToken);
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }
}