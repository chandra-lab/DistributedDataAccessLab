using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using NotificationService.Api.Data;
using NotificationService.Api.Models;

namespace NotificationService.Api.Services;

public class OrderCreatedConsumer : BackgroundService
{
    private readonly ILogger<OrderCreatedConsumer> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private IConnection? _connection;
    private IModel? _channel;

    public OrderCreatedConsumer(ILogger<OrderCreatedConsumer> logger, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
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
                _channel.QueueDeclare(queue: "order_created", durable: true,
                    exclusive: false, autoDelete: false, arguments: null);

                var consumer = new EventingBasicConsumer(_channel);
                consumer.Received += async (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var order = JsonSerializer.Deserialize<OrderCreatedEvent>(message);

                    if (order != null)
                    {
                        using var scope = _scopeFactory.CreateScope();
                        var db = scope.ServiceProvider
                            .GetRequiredService<NotificationDbContext>();

                        var notification = new Notification
                        {
                            OrderId = order.OrderId,
                            CustomerId = order.CustomerId,
                            Message = $"Order #{order.OrderId} placed for Customer #{order.CustomerId}.",
                            CreatedAt = DateTime.UtcNow
                        };
                        await db.Notifications.AddAsync(notification);
                        await db.SaveChangesAsync();
                        _logger.LogInformation("Notification saved for Order {Id}", order.OrderId);
                    }
                };

                _channel.BasicConsume(queue: "order_created", autoAck: true, consumer: consumer);
                _logger.LogInformation("NotificationService listening for events.");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("RabbitMQ not ready ({Attempt}/10): {Msg}", i + 1, ex.Message);
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

public class OrderCreatedEvent
{
    public int OrderId { get; set; }
    public int CustomerId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}