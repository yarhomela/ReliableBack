using System.Text.Json;
using ReliableBack.Domain.Tasks;

namespace ReliableBack.Tests.Common.Builders;

public class TaskItemBuilder
{
    private string _type = "test.task";
    private JsonDocument _payload = JsonDocument.Parse("{}");
    private JobPriority _priority = JobPriority.Normal;
    private int _maxRetries = 3;
    private DateTime? _scheduledAt = null;

    public TaskItemBuilder WithType(string type)
    {
        _type = type;
        return this;
    }

    public TaskItemBuilder WithPayload(object payload)
    {
        _payload = JsonDocument.Parse(JsonSerializer.Serialize(payload));
        return this;
    }

    public TaskItemBuilder WithPriority(JobPriority priority)
    {
        _priority = priority;
        return this;
    }

    public TaskItemBuilder WithMaxRetries(int maxRetries)
    {
        _maxRetries = maxRetries;
        return this;
    }

    public TaskItemBuilder WithScheduledAt(DateTime scheduledAt)
    {
        _scheduledAt = scheduledAt;
        return this;
    }

    public TaskItem Build() => new(
        _type,
        _payload,
        _priority,
        _maxRetries,
        _scheduledAt);
}