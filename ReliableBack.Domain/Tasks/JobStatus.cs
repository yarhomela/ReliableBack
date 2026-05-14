namespace ReliableBack.Domain.Tasks;

public enum JobStatus
{
    None = 0,
    Pending = 1,
    Queued = 2,
    Running = 3,
    Completed = 4,
    Failed = 5, 
    Retrying = 6,
    DeadLettered = 7,
}