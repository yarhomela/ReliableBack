using System.Diagnostics.Metrics;

namespace ReliableBack.Application.Common.Telemetry;

public sealed class TaskMetrics : IDisposable
{
    private readonly Meter _meter;
    
    private readonly Counter<long> _tasksEnqueued;
    private readonly Counter<long> _tasksCompleted;
    private readonly Counter<long> _tasksFailed;
    private readonly Counter<long> _tasksDeadLettered;
    private readonly Counter<long> _tasksRetried;
    
    private readonly Histogram<double> _processingDuration;
    
    private readonly UpDownCounter<long> _tasksPending;
    private readonly UpDownCounter<long> _tasksRunning;

    public TaskMetrics()
    {
        _meter = new Meter("ReliableBack", "1.0.0");

        _tasksEnqueued = _meter.CreateCounter<long>(
            name:        "reliableback_tasks_enqueued_total",
            unit:        "{tasks}",
            description: "Total number of tasks enqueued");

        _tasksCompleted = _meter.CreateCounter<long>(
            name:        "reliableback_tasks_completed_total",
            unit:        "{tasks}",
            description: "Total number of tasks completed successfully");

        _tasksFailed = _meter.CreateCounter<long>(
            name:        "reliableback_tasks_failed_total",
            unit:        "{tasks}",
            description: "Total number of task failures");

        _tasksDeadLettered = _meter.CreateCounter<long>(
            name:        "reliableback_tasks_dead_lettered_total",
            unit:        "{tasks}",
            description: "Total number of tasks moved to dead letter");

        _tasksRetried = _meter.CreateCounter<long>(
            name:        "reliableback_tasks_retried_total",
            unit:        "{tasks}",
            description: "Total number of task retries");

        _processingDuration = _meter.CreateHistogram<double>(
            name:        "reliableback_task_processing_duration_seconds",
            unit:        "s",
            description: "Task processing duration in seconds");

        _tasksPending = _meter.CreateUpDownCounter<long>(
            name:        "reliableback_tasks_pending",
            unit:        "{tasks}",
            description: "Current number of pending tasks");

        _tasksRunning = _meter.CreateUpDownCounter<long>(
            name:        "reliableback_tasks_running",
            unit:        "{tasks}",
            description: "Current number of running tasks");
    }

    public void RecordTaskEnqueued(string taskType, string priority)
    {
        _tasksEnqueued.Add(1,
            new KeyValuePair<string, object?>("task_type", taskType),
            new KeyValuePair<string, object?>("priority",  priority));

        _tasksPending.Add(1,
            new KeyValuePair<string, object?>("task_type", taskType));
    }

    public void RecordTaskCompleted(string taskType, double durationSeconds)
    {
        _tasksCompleted.Add(1,
            new KeyValuePair<string, object?>("task_type", taskType));

        _processingDuration.Record(durationSeconds,
            new KeyValuePair<string, object?>("task_type", taskType),
            new KeyValuePair<string, object?>("status",    "completed"));

        _tasksPending.Add(-1,
            new KeyValuePair<string, object?>("task_type", taskType));
        _tasksRunning.Add(-1,
            new KeyValuePair<string, object?>("task_type", taskType));
    }

    public void RecordTaskFailed(string taskType, double durationSeconds)
    {
        _tasksFailed.Add(1,
            new KeyValuePair<string, object?>("task_type", taskType));

        _processingDuration.Record(durationSeconds,
            new KeyValuePair<string, object?>("task_type", taskType),
            new KeyValuePair<string, object?>("status",    "failed"));

        _tasksRunning.Add(-1,
            new KeyValuePair<string, object?>("task_type", taskType));
    }

    public void RecordTaskDeadLettered(string taskType)
    {
        _tasksDeadLettered.Add(1,
            new KeyValuePair<string, object?>("task_type", taskType));

        _tasksPending.Add(-1,
            new KeyValuePair<string, object?>("task_type", taskType));
    }

    public void RecordTaskRetried(string taskType, int retryCount)
    {
        _tasksRetried.Add(1,
            new KeyValuePair<string, object?>("task_type",   taskType),
            new KeyValuePair<string, object?>("retry_count", retryCount));
    }

    public void RecordTaskStarted(string taskType)
    {
        _tasksPending.Add(-1,
            new KeyValuePair<string, object?>("task_type", taskType));
        _tasksRunning.Add(1,
            new KeyValuePair<string, object?>("task_type", taskType));
    }

    public void Dispose() => _meter.Dispose();
}