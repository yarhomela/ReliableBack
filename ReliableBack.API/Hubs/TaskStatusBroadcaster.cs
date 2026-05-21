using Microsoft.AspNetCore.SignalR;
using ReliableBack.Application.Common.Interfaces;
using ReliableBack.Application.Tasks;

namespace ReliableBack.API.Hubs;

public class TaskStatusBroadcaster : BackgroundService
{
    private readonly ITaskEventSubscriber _subscriber;
    private readonly IHubContext<TaskStatusHub> _hubContext;
    private readonly ILogger<TaskStatusBroadcaster> _logger;

    public TaskStatusBroadcaster(
        ITaskEventSubscriber subscriber, 
        IHubContext<TaskStatusHub> hubContext,
        ILogger<TaskStatusBroadcaster> logger)
    {
        _subscriber = subscriber;
        _hubContext = hubContext;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _subscriber.SubscribeAsync(
            async (taskId, previousStatus, newStatus) =>
            {
                var notification = new TaskStatusChangedNotification(
                    taskId,
                    previousStatus,
                    newStatus,
                    DateTime.UtcNow);
                
                await _hubContext.Clients.All.SendAsync(
                    "ReceiveTaskStatusUpdate",
                    notification,
                    stoppingToken);

                _logger.LogInformation(
                    "Broadcasted status change for task {TaskId}: {Previous} -> {New}",
                    taskId, previousStatus, newStatus);
            },
            stoppingToken);
        
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}