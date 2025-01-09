using Beymen.Demo.Application.DTOs;
using Beymen.Demo.Application.Interfaces;
using Beymen.Demo.Domain.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;

namespace Beymen.Demo.Infrastructure.MessageBus;

public class RabbitMQConsumerService(
    IRabbitMQConnection connection,
    IServiceScopeFactory serviceScopeFactory,
    IOptions<RabbitMQSettings> options,
    ILogger<RabbitMQConsumerService> logger) : BackgroundService
{
    private readonly IRabbitMQConnection _connection = connection;
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
    private readonly RabbitMQSettings? settings = options.Value;
    private readonly ILogger<RabbitMQConsumerService> _logger = logger;

    private IChannel? channel;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await SetupExchangesAndQueuesAsync(stoppingToken);

        await channel!.BasicQosAsync(0, 1, false, stoppingToken);
        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (sender, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var properties = new BasicProperties(ea.BasicProperties);
            var retryCount = GetRetryCount(properties);

            try
            {
                await ProcessMessage(message, stoppingToken);
                await channel.BasicAckAsync(ea.DeliveryTag, false, cancellationToken: stoppingToken);
            }
            catch (Exception ex)
            {
                await HandleMessageFailure(properties, ea, body, retryCount, ex, stoppingToken);
            }
        };

        throw new NotImplementedException();
    }

    private async Task SetupExchangesAndQueuesAsync(CancellationToken stoppingToken)
    {
        if (settings is null ||
            string.IsNullOrWhiteSpace(settings.MainExchange) ||
            string.IsNullOrWhiteSpace(settings.DeadLetterExchange) ||
            string.IsNullOrWhiteSpace(settings.RetryExchange) ||
            string.IsNullOrWhiteSpace(settings.DeadLetterQueue) ||
            string.IsNullOrWhiteSpace(settings.RetryQueue) ||
            string.IsNullOrWhiteSpace(settings.MainQueue) ||
            string.IsNullOrWhiteSpace(settings.RetryCountHeader) ||
            string.IsNullOrWhiteSpace(settings.NotificationExchange) ||
            string.IsNullOrWhiteSpace(settings.NotificationQueue))
        {
            throw new OperationCanceledException("RabbitMQ settings are not valid");
        }

        if (_connection is null || _connection.Connection is null)
        {
            throw new ConnectFailureException("RabbitMQ could not connect.", new());
        }

        channel = await _connection!.Connection!.CreateChannelAsync(cancellationToken: stoppingToken);

        if (channel is null)
        {
            throw new OperationCanceledException("Channel could not created.");
        }

        await channel.ExchangeDeclareAsync(settings.MainExchange, ExchangeType.Topic, durable: true, cancellationToken: stoppingToken);

        await channel.ExchangeDeclareAsync(settings.DeadLetterExchange, ExchangeType.Topic, durable: true, cancellationToken: stoppingToken);

        await channel.ExchangeDeclareAsync(settings.RetryExchange, ExchangeType.Topic, durable: true, cancellationToken: stoppingToken);

        var dlqArgs = new Dictionary<string, object?>();
        await channel.QueueDeclareAsync(settings.DeadLetterQueue, durable: true, exclusive: false, autoDelete: false, arguments: dlqArgs, cancellationToken: stoppingToken);

        await channel.QueueBindAsync(settings.DeadLetterQueue, settings.DeadLetterExchange, string.Empty, cancellationToken: stoppingToken);

        var retryArgs = new Dictionary<string, object?>
        {
            { "x-message-ttl", settings.RetryDelayMS },
            { "x-dead-letter-exchange", settings.MainExchange }
        };
        await channel.QueueBindAsync(settings.RetryQueue, settings.RetryExchange, string.Empty, retryArgs, cancellationToken: stoppingToken);

        var mainQueueArgs = new Dictionary<string, object?>
        {
            { "x-dead-letter-exchange", settings.DeadLetterExchange }
        };
        await channel.QueueDeclareAsync(settings.MainQueue, durable: true, exclusive: false, autoDelete: false, arguments: mainQueueArgs, cancellationToken: stoppingToken);

        await channel.QueueBindAsync(settings.MainQueue, settings.MainExchange, string.Empty, cancellationToken: stoppingToken);

        // Notification publisher
        await channel.ExchangeDeclareAsync(settings.NotificationExchange, ExchangeType.Fanout, durable: true, cancellationToken: stoppingToken);
        await channel.QueueDeclareAsync(settings.NotificationQueue, durable: true, exclusive: false, autoDelete: false, cancellationToken: stoppingToken);
        await channel.QueueBindAsync(settings.NotificationQueue, settings.NotificationExchange, string.Empty, cancellationToken: stoppingToken);
    }

    private int GetRetryCount(BasicProperties basicProperties)
    {
        if (basicProperties.Headers != null &&
               basicProperties.Headers.TryGetValue(settings!.RetryCountHeader!, out var retryCountObj) &&
               retryCountObj is int retryCount)
        {
            return retryCount;
        }

        return 0;
    }

    private async Task ProcessMessage(string message, CancellationToken stoppingToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        _logger.LogDebug("Processing message, {Message}", message);

        var updateQuantityDto = await JsonSerializer.DeserializeAsync<UpdateQuantityDto>(new MemoryStream(Encoding.UTF8.GetBytes(message)), cancellationToken: stoppingToken) ?? throw new SerializationException("Message cannot be deserialize to data transfer object.");

        var stockService = scope.ServiceProvider.GetRequiredService<IStockService>();
        await stockService.UpdateQuantityAsync(updateQuantityDto, stoppingToken);

        await PublishNotificationAsync(new NotificationDto("Order Approved", "We would like to inform you that our order has been confirmed, we will be happy to inform you about the next stages.", Domain.Enums.NotificationType.Email, "1234"));

        _logger.LogDebug("Message processed successful.");
        await Task.CompletedTask;
    }

    private async Task HandleMessageFailure(BasicProperties properties, BasicDeliverEventArgs ea, ReadOnlyMemory<byte> body, int retryCount, Exception ex, CancellationToken stoppingToken)
    {
        _logger.LogError(ex, "Error processing message. Retry count: {RetryCount}", retryCount);

        if (retryCount < settings!.MaxRetryCount)
        {
            properties.Headers = new Dictionary<string, object?>
            {
                { settings.RetryCountHeader!, retryCount + 1 }
            };
            properties.Persistent = true;

            await channel!.BasicPublishAsync(
                settings.RetryExchange!,
                string.Empty,
                body,
                stoppingToken
            );

            await channel!.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);

            _logger.LogInformation("Message sent to retry queue. Retry count: {RetryCount}", retryCount + 1);
        }
        else
        {
            // Move to Dead Letter Queue
            await channel!.BasicNackAsync(ea.DeliveryTag, false, false, stoppingToken);
            _logger.LogWarning("Max retry count reached. Message moved to DLQ");
        }
    }

    private async Task PublishNotificationAsync<T>(T message)
    {
        var properties = new BasicProperties
        {
            Persistent = true,
            ContentType = "application/json",
            DeliveryMode = DeliveryModes.Persistent,
            MessageId = Guid.NewGuid().ToString(),
            Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
        };

        try
        {
            var jsonMessage = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(jsonMessage);

            await channel!.BasicPublishAsync(
                exchange: settings!.NotificationExchange!,
                string.Empty,
                mandatory: true,
                properties,
                body
            );

            _logger.LogInformation("Message published successfully: {Message}", jsonMessage);
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing message");
            throw;
        }
    }
}