namespace Beymen.Demo.Domain.Settings;

public class RabbitMQSettings
{
    public string? HostName { get; set; }
    public string? UserName { get; set; }
    public string? Password { get; set; }
    public int Port { get; set; }
    public int MaxRetryCount { get; set; }
    public int RetryDelayMS { get; set; }
    public string? StockExchange { get; set; }
    public string? StockQueue { get; set; }
}