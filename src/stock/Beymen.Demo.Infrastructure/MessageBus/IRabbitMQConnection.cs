using RabbitMQ.Client;

namespace Beymen.Demo.Infrastructure.MessageBus;

public interface IRabbitMQConnection
{
    IConnection? Connection { get; }
    bool IsConnected { get; }
}