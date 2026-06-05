using System.Text.Json;
using FluentAssertions;
using NSubstitute;
using ReliableBack.Application.Common.Interfaces;
using ReliableBack.Application.Common.Telemetry;
using ReliableBack.Application.Tasks.Commands.EnqueueTask;
using ReliableBack.Domain.Tasks;

namespace ReliableBack.Tests.Unit.Application;

[Trait("Category", "Unit")]
public class EnqueueTaskCommandHandlerTests
{
    private readonly ITaskRepository _repository;
    private readonly IMessagePublisher _publisher;
    private readonly EnqueueTaskCommandHandler _handler;

    public EnqueueTaskCommandHandlerTests()
    {
        _repository = Substitute.For<ITaskRepository>();
        _publisher = Substitute.For<IMessagePublisher>();
        var metrics = new TaskMetrics();
        _handler = new EnqueueTaskCommandHandler(_repository, _publisher, metrics);
    }

    [Fact]
    public async Task Handle_ShouldReturnTaskId()
    {
        var command = new EnqueueTaskCommand(
            Type: "email.send",
            Payload: JsonDocument.Parse("{}"));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldCallRepositoryAddAsync()
    {
        var command = new EnqueueTaskCommand(
            Type: "email.send",
            Payload: JsonDocument.Parse("{}"));

        await _handler.Handle(command, CancellationToken.None);

        await _repository.Received(1)
            .AddAsync(Arg.Any<TaskItem>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldPublishToCorrectQueue_WhenPriorityIsHigh()
    {
        var command = new EnqueueTaskCommand(
            Type: "email.send",
            Payload: JsonDocument.Parse("{}"),
            Priority: JobPriority.High);

        await _handler.Handle(command, CancellationToken.None);

        await _publisher.Received(1)
            .PublishAsync(
                Arg.Any<TaskItem>(),
                "tasks.high",
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldPublishToNormalQueue_WhenPriorityIsNormal()
    {
        var command = new EnqueueTaskCommand(
            Type: "email.send",
            Payload: JsonDocument.Parse("{}"),
            Priority: JobPriority.Normal);

        await _handler.Handle(command, CancellationToken.None);

        await _publisher.Received(1)
            .PublishAsync(
                Arg.Any<TaskItem>(),
                "tasks.normal",
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldCreateTaskWithPendingStatus()
    {
        TaskItem? capturedTask = null;

        await _repository.AddAsync(
            Arg.Do<TaskItem>(t => capturedTask = t),
            Arg.Any<CancellationToken>());

        var command = new EnqueueTaskCommand(
            Type: "email.send",
            Payload: JsonDocument.Parse("{}"));

        await _handler.Handle(command, CancellationToken.None);

        capturedTask.Should().NotBeNull();
        capturedTask!.Status.Should().Be(JobStatus.Pending);
        capturedTask.Type.Should().Be("email.send");
    }
}