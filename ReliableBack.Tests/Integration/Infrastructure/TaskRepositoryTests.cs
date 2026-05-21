using FluentAssertions;
using ReliableBack.Infrastructure.Persistence.Repositories;
using ReliableBack.Tests.Common.Builders;
using ReliableBack.Tests.Common.Fixtures;

namespace ReliableBack.Tests.Integration.Infrastructure;

[Trait("Category", "Integration")]
public class TaskRepositoryTests : IClassFixture<PostgreSqlFixture>
{
    private readonly PostgreSqlFixture _fixture;
    private readonly TaskRepository _repository;

    public TaskRepositoryTests(PostgreSqlFixture fixture)
    {
        _fixture = fixture;
        _repository = new TaskRepository(
            fixture.DbContext,
            fixture.ConnectionString);
    }

    [Fact]
    public async Task AddAsync_ShouldPersistTask()
    {
        var task = new TaskItemBuilder()
            .WithType("test.persist")
            .Build();

        await _repository.AddAsync(task);

        var persisted = await _repository.GetByIdAsync(task.Id);
        persisted.Should().NotBeNull();
        persisted!.Type.Should().Be("test.persist");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenTaskDoesNotExist()
    {
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_ShouldPersistStatusChange()
    {
        var task = new TaskItemBuilder().Build();
        await _repository.AddAsync(task);

        task.MarkAsRunning();
        await _repository.UpdateAsync(task);

        var updated = await _repository.GetByIdAsync(task.Id);
        updated!.Status.Should().Be(Domain.Tasks.JobStatus.Running);
    }

    [Fact]
    public async Task GetAllAsync_ShouldFilterByStatus()
    {
        var pendingTask = new TaskItemBuilder()
            .WithType("filter.test.pending")
            .Build();

        var completedTask = new TaskItemBuilder()
            .WithType("filter.test.completed")
            .Build();

        await _repository.AddAsync(pendingTask);
        await _repository.AddAsync(completedTask);

        completedTask.MarkAsQueued();
        completedTask.MarkAsRunning();
        completedTask.MarkAsCompleted();
        await _repository.UpdateAsync(completedTask);

        var parameters = new Application.Tasks.Queries.GetTaskList
            .GetTaskListParameters(Domain.Tasks.JobStatus.Pending, 1, 20);

        var results = await _repository.GetAllAsync(parameters);

        results.Should().OnlyContain(t => t.Status == Domain.Tasks.JobStatus.Pending);
    }

    [Fact]
    public async Task GetScheduledForRetryAsync_ShouldReturnOnlyDueTasks()
    {
        var dueTask = new TaskItemBuilder()
            .WithMaxRetries(3)
            .Build();

        await _repository.AddAsync(dueTask);
        dueTask.RecordFailure("Error");
        
        dueTask.MarkAsQueued();
        await _repository.UpdateAsync(dueTask);

        var results = await _repository.GetScheduledForRetryAsync();
        
        results.Should().NotContain(t => t.Id == dueTask.Id);
    }
}