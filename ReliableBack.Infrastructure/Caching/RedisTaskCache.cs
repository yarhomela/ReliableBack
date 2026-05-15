using Microsoft.Extensions.Logging;
using ReliableBack.Application.Common.Interfaces;
using ReliableBack.Domain.Tasks;

namespace ReliableBack.Infrastructure.Caching;

public class RedisTaskCache : ITaskCache
{
    private readonly ILogger<RedisTaskCache> _logger;
    
    public RedisTaskCache(ILogger<RedisTaskCache> logger)
    {
        _logger = logger;
    }
    
    public Task<TaskItem?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // TODO: замінити на реальний Redis
        _logger.LogDebug("Cache GET for task {TaskId}", id);
        return Task.FromResult<TaskItem?>(null);
    }

    public Task SetAsync(TaskItem task, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Cache SET for task {TaskId}", task.Id);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Cache REMOVE for task {TaskId}", id);
        return Task.CompletedTask;
    }
}