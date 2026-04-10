using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace OrderService.Api.Services;

public interface IEventPublisher
{
    void PublishOrderCreated(OrderCreatedEvent orderEvent);
    void PublishOrderCancelled(OrderCancelledEvent orderEvent);
}

public class RabbitMqPublisher : IEventPublisher, IDisposable
{
    private readonly ILogger<RabbitMqPublisher> _logger;
    private IConnection? _connection;
    private IModel? _channel;

    public RabbitMqPublisher(ILogger<RabbitMqPublisher> logger)
    {
        _logger = logger;
        TryConnect();
    }

    private void TryConnect()
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

            _channel.QueueDeclare(queue: "order_cancelled", durable: true,
                exclusive: false, autoDelete: false, arguments: null);

            _logger.LogInformation("Connected to RabbitMQ.");
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Could not connect to RabbitMQ: {Msg}", ex.Message);
        }
    }

    public void PublishOrderCreated(OrderCreatedEvent orderEvent)
    {
        Publish("order_created", orderEvent);
    }

    public void PublishOrderCancelled(OrderCancelledEvent orderEvent)
    {
        Publish("order_cancelled", orderEvent);
    }

    private void Publish<T>(string queue, T eventObj)
    {
        if (_channel == null)
        {
            _logger.LogWarning("RabbitMQ channel not available. Skipping publish to {Queue}.", queue);
            return;
        }
        var message = JsonSerializer.Serialize(eventObj);
        var body = Encoding.UTF8.GetBytes(message);
        var props = _channel.CreateBasicProperties();
        props.Persistent = true;
        _channel.BasicPublish(exchange: "", routingKey: queue,
            basicProperties: props, body: body);
        _logger.LogInformation("Published to {Queue}: {Message}", queue, message);
    }

    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
    }
}

public class OrderCreatedEvent
{
    public int OrderId { get; set; }
    public int CustomerId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}

public class OrderCancelledEvent
{
    public int OrderId { get; set; }
    public int CustomerId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}
