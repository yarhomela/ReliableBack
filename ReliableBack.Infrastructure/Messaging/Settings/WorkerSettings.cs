namespace ReliableBack.Infrastructure.Messaging.Settings;

public class WorkerSettings
{
    public int MaxConcurrentTasks { get; init; }

    public int StalledThresholdMinutes { get; set; }
    
    public int CheckIntervalSeconds { get; set; }
}