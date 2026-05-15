using Dapper;
using Npgsql;
using ReliableBack.Application.Common.Interfaces;
using ReliableBack.Application.Tasks.Queries.GetTaskList;
using ReliableBack.Domain.Tasks;

namespace ReliableBack.Infrastructure.Persistence.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly ReliableBackDbContext _context;
    private readonly string _connectionString;
    
    public TaskRepository(ReliableBackDbContext context, string connectionString)
    {
        _context = context;
        _connectionString = connectionString;
    }
    
    public async Task AddAsync(TaskItem task, CancellationToken cancellationToken = default)
    {
        await _context.Tasks.AddAsync(task, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(TaskItem task, CancellationToken cancellationToken = default)
    {
        _context.Tasks.Update(task);
        await _context.SaveChangesAsync(cancellationToken);
    }
    
    public async Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);

        var result = await connection.QuerySingleOrDefaultAsync<TaskItem>(
            TaskQueries.GetById,
            new { Id = id });

        return result;
    }

    public async Task<IReadOnlyList<TaskItem>> GetAllAsync(GetTaskListParameters parameters, 
        CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);

        var results = await connection.QueryAsync<TaskItem>(
            TaskQueries.GetAll, new
            {
                Status   = parameters.Status?.ToString(),
                PageSize = parameters.PageSize,
                Offset   = (parameters.Page - 1) * parameters.PageSize
            });

        return results.ToList();
    }
}