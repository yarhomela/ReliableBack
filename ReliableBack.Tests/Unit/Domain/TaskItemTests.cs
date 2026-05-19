using FluentAssertions;
using ReliableBack.Domain.Tasks;
using ReliableBack.Domain.Tasks.Events;
using ReliableBack.Tests.Common.Builders;

namespace ReliableBack.Tests.Unit.Domain;

public class TaskItemTests
{
    [Fact]
    public void Constructor_ShouldSetPendingStatus_WhenCreated()
    {
        var task = new TaskItemBuilder().Build();

        task.Status.Should().Be(JobStatus.Pending);
    }

    [Fact]
    public void Constructor_ShouldSetCorrectType_WhenCreated()
    {
        var task = new TaskItemBuilder()
            .WithType("email.send")
            .Build();

        task.Type.Should().Be("email.send");
    }

    [Fact]
    public void Constructor_ShouldRaiseNoDomainEvents_WhenCreated()
    {
        var task = new TaskItemBuilder().Build();

        task.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void ChangeStatus_ShouldUpdateStatus()
    {
        var task = new TaskItemBuilder().Build();

        task.MarkAsQueued();

        task.Status.Should().Be(JobStatus.Queued);
    }

    [Fact]
    public void ChangeStatus_ShouldRaiseDomainEvent()
    {
        var task = new TaskItemBuilder().Build();

        task.MarkAsQueued();

        task.DomainEvents.Should().HaveCount(1);
        task.DomainEvents.First().Should().BeOfType<TaskStatusChangedEvent>();
    }

    [Fact]
    public void ChangeStatus_ShouldRecordPreviousAndNewStatus_InDomainEvent()
    {
        var task = new TaskItemBuilder().Build();

        task.MarkAsQueued();

        var domainEvent = task.DomainEvents
            .OfType<TaskStatusChangedEvent>()
            .First();

        domainEvent.PreviousStatus.Should().Be(JobStatus.Pending);
        domainEvent.NewStatus.Should().Be(JobStatus.Queued);
    }

    [Fact]
    public void ChangeStatus_ShouldUpdateUpdatedAt()
    {
        var task = new TaskItemBuilder().Build();
        var before = DateTime.UtcNow;

        task.MarkAsRunning();

        task.UpdatedAt.Should().BeOnOrAfter(before);
    }

    [Fact]
    public void RecordFailure_ShouldIncrementRetryCount()
    {
        var task = new TaskItemBuilder().Build();

        task.RecordFailure("Something went wrong");

        task.RetryCount.Should().Be(1);
    }

    [Fact]
    public void RecordFailure_ShouldSetErrorMessage()
    {
        var task = new TaskItemBuilder().Build();

        task.RecordFailure("Connection timeout");

        task.ErrorMessage.Should().Be("Connection timeout");
    }

    [Fact]
    public void RecordFailure_ShouldSetRetryingStatus_WhenRetriesRemaining()
    {
        var task = new TaskItemBuilder()
            .WithMaxRetries(3)
            .Build();

        task.RecordFailure("Error");

        task.Status.Should().Be(JobStatus.Retrying);
    }

    [Fact]
    public void RecordFailure_ShouldSetDeadLetteredStatus_WhenMaxRetriesReached()
    {
        var task = new TaskItemBuilder()
            .WithMaxRetries(1)
            .Build();

        task.RecordFailure("Final error");

        task.Status.Should().Be(JobStatus.DeadLettered);
    }

    [Fact]
    public void RecordFailure_ShouldScheduleRetry_WhenRetriesRemaining()
    {
        var task = new TaskItemBuilder()
            .WithMaxRetries(3)
            .Build();

        var before = DateTime.UtcNow;
        task.RecordFailure("Error");

        task.ScheduledAt.Should().NotBeNull();
        task.ScheduledAt.Should().BeAfter(before);
    }

    [Fact]
    public void RecordFailure_ShouldNotScheduleRetry_WhenDeadLettered()
    {
        var task = new TaskItemBuilder()
            .WithMaxRetries(1)
            .Build();

        task.RecordFailure("Final error");

        task.Status.Should().Be(JobStatus.DeadLettered);
    }

    [Fact]
    public void GetRetryDelay_ShouldReturnThirtySeconds_OnFirstRetry()
    {
        var task = new TaskItemBuilder().Build();
        task.RecordFailure("Error");

        var delay = task.GetRetryDelay();

        delay.Should().Be(TimeSpan.FromSeconds(30));
    }

    [Fact]
    public void GetRetryDelay_ShouldDouble_OnEachRetry()
    {
        var task = new TaskItemBuilder()
            .WithMaxRetries(5)
            .Build();

        task.RecordFailure("Error 1");
        var firstDelay = task.GetRetryDelay();

        task.RecordFailure("Error 2");
        var secondDelay = task.GetRetryDelay();

        secondDelay.Should().Be(firstDelay * 2);
    }

    [Fact]
    public void GetRetryDelay_ShouldNotExceedOneHour()
    {
        var task = new TaskItemBuilder()
            .WithMaxRetries(20)
            .Build();

        for (var i = 0; i < 15; i++)
        {
            task.RecordFailure("Error");
        }
        
        var delay = task.GetRetryDelay();

        delay.Should().BeLessThanOrEqualTo(TimeSpan.FromHours(1));
    }

    [Fact]
    public void ClearDomainEvents_ShouldRemoveAllEvents()
    {
        var task = new TaskItemBuilder().Build();
        task.MarkAsQueued();
        task.MarkAsRunning();

        task.ClearDomainEvents();

        task.DomainEvents.Should().BeEmpty();
    }
}