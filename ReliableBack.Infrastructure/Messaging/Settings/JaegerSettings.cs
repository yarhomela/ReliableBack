namespace ReliableBack.Infrastructure.Messaging.Settings;

public sealed class JaegerSettings
{
    public string Host { get; init; } = string.Empty;
    
    public int Port { get; init; }
}