using Microsoft.AspNetCore.SignalR;

namespace ReliableBack.API.Hubs;

public class TaskStatusHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }
}