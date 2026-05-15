using ReliableBack.Domain.Tasks;

namespace ReliableBack.Application.Common;

public static class QueueNames
{
    public const string High   = "tasks.high";
    public const string Normal = "tasks.normal";
    public const string Low    = "tasks.low";

    public static string FromPriority(JobPriority priority) => priority switch
    {
        JobPriority.High   => High,
        JobPriority.Normal => Normal,
        JobPriority.Low    => Low,
        _                  => Normal
    };
}