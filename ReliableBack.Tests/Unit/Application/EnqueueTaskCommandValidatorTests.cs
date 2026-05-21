using System.Text.Json;
using FluentAssertions;
using ReliableBack.Application.Tasks.Commands.EnqueueTask;
using ReliableBack.Domain.Tasks;

namespace ReliableBack.Tests.Unit.Application;

[Trait("Category", "Unit")]
public class EnqueueTaskCommandValidatorTests
{
    private readonly EnqueueTaskCommandValidator _validator = new();

    [Fact]
    public async Task Validate_ShouldPass_WhenCommandIsValid()
    {
        var command = new EnqueueTaskCommand(
            Type:        "email.send",
            Payload:     JsonDocument.Parse("{}"),
            Priority:    JobPriority.Normal,
            MaxRetries:  3,
            ScheduledAt: null);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenTypeIsEmpty()
    {
        var command = new EnqueueTaskCommand(
            Type:    string.Empty,
            Payload: JsonDocument.Parse("{}"));

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == nameof(EnqueueTaskCommand.Type));
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenTypeExceedsMaxLength()
    {
        var command = new EnqueueTaskCommand(
            Type:    new string('a', 101),
            Payload: JsonDocument.Parse("{}"));

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == nameof(EnqueueTaskCommand.Type));
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenMaxRetriesIsNegative()
    {
        var command = new EnqueueTaskCommand(
            Type:       "email.send",
            Payload:    JsonDocument.Parse("{}"),
            MaxRetries: -1);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == nameof(EnqueueTaskCommand.MaxRetries));
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenMaxRetriesExceedsTen()
    {
        var command = new EnqueueTaskCommand(
            Type:       "email.send",
            Payload:    JsonDocument.Parse("{}"),
            MaxRetries: 11);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenScheduledAtIsInThePast()
    {
        var command = new EnqueueTaskCommand(
            Type:        "email.send",
            Payload:     JsonDocument.Parse("{}"),
            ScheduledAt: DateTime.UtcNow.AddHours(-1));

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == nameof(EnqueueTaskCommand.ScheduledAt));
    }

    [Fact]
    public async Task Validate_ShouldPass_WhenScheduledAtIsNull()
    {
        var command = new EnqueueTaskCommand(
            Type:        "email.send",
            Payload:     JsonDocument.Parse("{}"),
            ScheduledAt: null);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeTrue();
    }
}