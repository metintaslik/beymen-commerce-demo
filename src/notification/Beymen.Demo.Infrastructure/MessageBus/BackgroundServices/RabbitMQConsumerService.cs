using Beymen.Demo.Application.DTOs;
using Beymen.Demo.Application.Interfaces;
using Beymen.Demo.Domain.Entities;
using Beymen.Demo.Domain.Settings;
using Beymen.Demo.Infrastructure.MessageBus.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Beymen.Demo.Infrastructure.MessageBus.BackgroundServices;

public class RabbitMQConsumerService(
    IRabbitMQConnection rabbitConnection,
    IServiceScopeFactory serviceScopeFactory,
    IOptions<RabbitMQSettings> options,
    ILogger<RabbitMQConsumerService> logger) : BackgroundService
{
    private readonly ILogger<RabbitMQConsumerService> _logger = logger;
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
    private IChannel _channel = default!;
    private readonly RabbitMQSettings rabbitMQSettings = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await SetupExchangesAndQueuesAsync(stoppingToken);

        await _channel.BasicQosAsync(0, 1, false, stoppingToken);
        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (sender, @event) =>
        {
            var body = @event.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            var basicProperties = (IBasicProperties)new BasicProperties(@event.BasicProperties);
            var retryCount = GetRetryCount(basicProperties);

            try
            {
                await ProcessMessage(message, stoppingToken);
                await _channel.BasicAckAsync(@event.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                await HandleMessageFailure(basicProperties, @event, body, retryCount, ex, stoppingToken);
            }
        };

        await _channel.BasicConsumeAsync(rabbitMQSettings.MainQueue!, false, consumer, stoppingToken);

        await Task.CompletedTask;
    }

    private async Task SetupExchangesAndQueuesAsync(CancellationToken stoppingToken)
    {
        if (rabbitMQSettings is null ||
            string.IsNullOrEmpty(rabbitMQSettings.MainQueue) ||
            string.IsNullOrEmpty(rabbitMQSettings.DeadLetterQueue) ||
            string.IsNullOrEmpty(rabbitMQSettings.RetryQueue) ||
            string.IsNullOrEmpty(rabbitMQSettings.DeadLetterExchange) ||
            string.IsNullOrEmpty(rabbitMQSettings.MainExchange) ||
            string.IsNullOrEmpty(rabbitMQSettings.RetryCountHeader) ||
            string.IsNullOrEmpty(rabbitMQSettings.MainQueue) ||
            string.IsNullOrEmpty(rabbitMQSettings.RetryExchange))
        {
            throw new InvalidOperationException("RabbitMQ settings are not valid");
        }

        _channel = await rabbitConnection.Connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await _channel.ExchangeDeclareAsync(rabbitMQSettings.MainExchange, ExchangeType.Fanout, durable: true, cancellationToken: stoppingToken);

        await _channel.ExchangeDeclareAsync(rabbitMQSettings.DeadLetterExchange, ExchangeType.Fanout, durable: true, cancellationToken: stoppingToken);

        await _channel.ExchangeDeclareAsync(rabbitMQSettings.RetryExchange, ExchangeType.Fanout, durable: true, cancellationToken: stoppingToken);

        var dlqArgs = new Dictionary<string, object?>();
        await _channel.QueueDeclareAsync(rabbitMQSettings.DeadLetterQueue, durable: true, exclusive: false, autoDelete: false, arguments: dlqArgs, cancellationToken: stoppingToken);

        await _channel.QueueBindAsync(rabbitMQSettings.DeadLetterQueue, rabbitMQSettings.DeadLetterExchange, string.Empty, cancellationToken: stoppingToken);

        var retryArgs = new Dictionary<string, object?>
        {
            { "x-message-ttl", rabbitMQSettings.RetryDelayMS },
            { "x-dead-letter-exchange", rabbitMQSettings.MainExchange }
        };

        await _channel.QueueBindAsync(rabbitMQSettings.RetryQueue, rabbitMQSettings.RetryExchange, string.Empty, retryArgs, cancellationToken: stoppingToken);

        var mainQueueArgs = new Dictionary<string, object?>
        {
            { "x-dead-letter-exchange", rabbitMQSettings.DeadLetterExchange }
        };

        await _channel.QueueDeclareAsync(rabbitMQSettings.MainQueue, durable: true, exclusive: false, autoDelete: false, arguments: mainQueueArgs, cancellationToken: stoppingToken);

        await _channel.QueueBindAsync(rabbitMQSettings.MainQueue, rabbitMQSettings.MainExchange, string.Empty, cancellationToken: stoppingToken);
    }

    private int GetRetryCount(IBasicProperties basicProperties)
    {
        if (basicProperties.Headers != null &&
               basicProperties.Headers.TryGetValue(rabbitMQSettings.RetryCountHeader!, out var retryCountObj) &&
               retryCountObj is int retryCount)
        {
            return retryCount;
        }

        return 0;
    }

    private async Task ProcessMessage(string message, CancellationToken stoppingToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var notificationHandler = scope.ServiceProvider.GetRequiredService<INotificationHandler>();
        var notificationDto = (await JsonSerializer.DeserializeAsync<NotificationDto>(new MemoryStream(Encoding.UTF8.GetBytes(message)), cancellationToken: stoppingToken)) ?? throw new InvalidCastException("Error while deserialize message.");

        await notificationHandler.HandleAsync(Notification.Create(notificationDto.Title, notificationDto.Content, notificationDto.Type, notificationDto.Recipient), notificationDto, stoppingToken);

        await Task.CompletedTask;
    }

    private async Task HandleMessageFailure(IBasicProperties properties, BasicDeliverEventArgs ea, ReadOnlyMemory<byte> body, int retryCount, Exception ex, CancellationToken stoppingToken)
    {
        _logger.LogError(ex, "Error processing message. Retry count: {RetryCount}", retryCount);

        if (retryCount < rabbitMQSettings.MaxRetryCount)
        {
            properties.Headers = new Dictionary<string, object?>
            {
                { rabbitMQSettings.RetryCountHeader!, retryCount + 1 }
            };

            properties.Persistent = true;

            await _channel.BasicPublishAsync(
                rabbitMQSettings.RetryExchange!,
                string.Empty,
                body,
                stoppingToken
            );

            await _channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);

            _logger.LogInformation("Message sent to retry queue. Retry count: {RetryCount}", retryCount + 1);
        }
        else
        {
            // Move to Dead Letter Queue
            await _channel.BasicNackAsync(ea.DeliveryTag, false, false, stoppingToken);
            _logger.LogWarning("Max retry count reached. Message moved to DLQ");
        }
    }
}