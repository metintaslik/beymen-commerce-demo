namespace Beymen.Demo.Domain.Settings;

public class RabbitMQSettings
{
    public string? HostName { get; set; }
    public string? UserName { get; set; }
    public string? Password { get; set; }
    public int Port { get; set; }
    public string? MainExchange { get; set; }
    public string? DeadLetterExchange { get; set; }
    public string? RetryExchange { get; set; }
    public string? MainQueue { get; set; }
    public string? DeadLetterQueue { get; set; }
    public string? RetryQueue { get; set; }
    public string? RetryCountHeader { get; set; }
    public int MaxRetryCount { get; set; }
    public int RetryDelayMS { get; set; }
    public string? NotificationExchange { get; set; }
    public string? NotificationQueue { get; set; }
}