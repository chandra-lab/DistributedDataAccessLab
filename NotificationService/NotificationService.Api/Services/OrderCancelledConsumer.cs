using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using NotificationService.Api.Data;
using NotificationService.Api.Models;

namespace NotificationService.Api.Services;

/// <summary>
/// Listens on the "order_cancelled" queue and saves a notification record
/// for every OrderCancelled event published by OrderService.
/// </summary>
public class OrderCancelledConsumer : BackgroundService
{
    private readonly ILogger<OrderCancelledConsumer> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private IConnection? _connection;
    private IModel? _channel;

    public OrderCancelledConsumer(
        ILogger<OrderCancelledConsumer> logger,
        IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Retry loop so we survive RabbitMQ startup latency
        for (int i = 0; i < 10; i++)
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "rabbitmq",
                    UserName = "guest",
                    Password = "guest"
                };
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();
                _channel.QueueDeclare(
                    queue: "order_cancelled",
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                var consumer = new EventingBasicConsumer(_channel);
                consumer.Received += async (_, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var evt = JsonSerializer.Deserialize<OrderCancelledEvent>(message);

                    if (evt != null)
                    {
                        using var scope = _scopeFactory.CreateScope();
                        var db = scope.ServiceProvider
                            .GetRequiredService<NotificationDbContext>();

                        var notification = new Notification
                        {
                            OrderId = evt.OrderId,
                            CustomerId = evt.CustomerId,
                            Message = $"Order #{evt.OrderId} was cancelled for Customer #{evt.CustomerId}.",
                            EventType = "OrderCancelled",
                            CreatedAt = DateTime.UtcNow
                        };
                        await db.Notifications.AddAsync(notification);
                        await db.SaveChangesAsync();
                        _logger.LogInformation(
                            "Cancellation notification saved for Order {Id}", evt.OrderId);
                    }
                };

                _channel.BasicConsume(
                    queue: "order_cancelled",
                    autoAck: true,
                    consumer: consumer);

                _logger.LogInformation("OrderCancelledConsumer is listening.");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    "RabbitMQ not ready ({Attempt}/10): {Msg}", i + 1, ex.Message);
                await Task.Delay(5000, stoppingToken);
            }
        }

        while (!stoppingToken.IsCancellationRequested)
            await Task.Delay(1000, stoppingToken);
    }

    public override void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        base.Dispose();
    }
}

public class OrderCancelledEvent
{
    public int OrderId { get; set; }
    public int CustomerId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}
