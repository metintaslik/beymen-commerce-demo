using RabbitMQ.Client;

namespace Beymen.Demo.Infrastructure.MessageBus.Interfaces;

public interface IRabbitMQConnection
{
    IConnection Connection { get; }
    bool IsConnected { get; }
}