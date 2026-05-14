using FluentValidation;

namespace ReliableBack.Application.Tasks.Commands.EnqueueTask;

public sealed class EnqueueTaskCommandValidator
    : AbstractValidator<EnqueueTaskCommand>
{
    public EnqueueTaskCommandValidator()
    {
        RuleFor(x => x.Type)
            .NotEmpty()
            .WithMessage("Task type is required.")
            .MaximumLength(100)
            .WithMessage("Task type must not exceed 100 characters.");

        RuleFor(x => x.Payload)
            .NotNull()
            .WithMessage("Payload is required.");

        RuleFor(x => x.MaxRetries)
            .InclusiveBetween(0, 10)
            .WithMessage("MaxRetries must be between 0 and 10.");

        RuleFor(x => x.ScheduledAt)
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("ScheduledAt must be in the future.")
            .When(x => x.ScheduledAt.HasValue);
    }
}