namespace ReliableBack.Infrastructure.Telemetry;

public static class TelemetryConstants
{
    public const string ApiServiceName = "ReliableBack.API";
    public const string WorkerServiceName = "ReliableBack.Worker";

    public const string ApiActivitySource = "ReliableBack.API";
    public const string WorkerActivitySource = "ReliableBack.Worker";

    public static class Spans
    {
        public const string EnqueueTask = "task.enqueue";
        public const string ProcessTask = "task.process";
        public const string PublishMessage = "rabbitmq.publish";
        public const string ConsumeMessage = "rabbitmq.consume";
        public const string RepositoryAdd = "repository.add";
        public const string RepositoryGet = "repository.get";
        public const string RepositoryUpdate = "repository.update";
    }

    public static class Attributes
    {
        public const string TaskId = "task.id";
        public const string TaskType = "task.type";
        public const string TaskStatus = "task.status";
        public const string TaskPriority = "task.priority";
        public const string QueueName = "messaging.destination";
        public const string RetryCount = "task.retry_count";
    }
}