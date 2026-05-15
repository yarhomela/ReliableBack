using ReliableBack.Application.Common;
using ReliableBack.Application.Common.Interfaces;
using ReliableBack.Domain.Tasks;

namespace ReliableBack.Worker;

public class WatchdogService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<WatchdogService> _logger;
    
    private static readonly TimeSpan StalledThreshold = TimeSpan.FromMinutes(10);
    
    private static readonly TimeSpan CheckInterval = TimeSpan.FromSeconds(30);

    public WatchdogService(IServiceScopeFactory scopeFactory, ILogger<WatchdogService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("WatchdogService started");

        while (!stoppingToken.IsCancellationRequested)
        {
            await RunCheckAsync(stoppingToken);
            await Task.Delay(CheckInterval, stoppingToken);
        }
    }

    private async Task RunCheckAsync(CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var repository  = scope.ServiceProvider.GetRequiredService<ITaskRepository>();
        var publisher   = scope.ServiceProvider.GetRequiredService<IMessagePublisher>();

        await RequeueRetryTasksAsync(repository, publisher, cancellationToken);
        await RecoverStalledTasksAsync(repository, publisher, cancellationToken);
    }

    private async Task RequeueRetryTasksAsync(
        ITaskRepository repository,
        IMessagePublisher publisher,
        CancellationToken cancellationToken)
    {
        var tasks = await repository.GetScheduledForRetryAsync(cancellationToken);

        foreach (var task in tasks)
        {
            try
            {
                task.MarkAsQueued();
                await repository.UpdateAsync(task, cancellationToken);

                await publisher.PublishAsync(
                    task,
                    QueueNames.FromPriority(task.Priority),
                    cancellationToken);

                _logger.LogInformation(
                    "Task {TaskId} requeued for retry ({RetryCount}/{MaxRetries})",
                    task.Id, task.RetryCount, task.MaxRetries);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to requeue task {TaskId} for retry", task.Id);
            }
        }
    }

    private async Task RecoverStalledTasksAsync(ITaskRepository repository, IMessagePublisher publisher,
        CancellationToken cancellationToken)
    {
        var stalledTasks = await repository.GetStalledTasksAsync(
            StalledThreshold, cancellationToken);

        foreach (var task in stalledTasks)
        {
            try
            {
                _logger.LogWarning(
                    "Task {TaskId} stalled in Running state, recovering...", task.Id);

                task.RecordFailure("Task stalled — worker likely crashed");
                await repository.UpdateAsync(task, cancellationToken);

                if (task.Status == JobStatus.Retrying)
                {
                    await publisher.PublishAsync(
                        task,
                        QueueNames.FromPriority(task.Priority),
                        cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to recover stalled task {TaskId}", task.Id);
            }
        }
    }
}