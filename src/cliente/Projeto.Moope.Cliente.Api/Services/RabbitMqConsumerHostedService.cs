namespace Projeto.Moope.Cliente.Api.Services
{
    //public class RabbitMqConsumerHostedService : BackgroundService
    public class RabbitMqConsumerHostedService
    {
        //private readonly ILogger<RabbitMqConsumerHostedService> _logger;
        //private readonly IServiceProvider _serviceProvider;
        //private IConnection _connection;
        //private IChannel _channel;

        //public RabbitMqConsumerHostedService(ILogger<RabbitMqConsumerHostedService> _logger, IServiceProvider serviceProvider)
        //{
        //    this._logger = _logger;
        //    _serviceProvider = serviceProvider;
        //}

        //protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        //{
        //    var factory = new ConnectionFactory { HostName = "localhost" }; // Configure as needed
        //    _connection = await factory.CreateConnectionAsync();
        //    _channel = await _connection.CreateChannelAsync();

        //    var queueName = nameof(UserCreatedMessage);
        //    await _channel.QueueDeclareAsync(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

        //    var consumer = new AsyncEventingBasicConsumer(_channel);
        //    consumer.ReceivedAsync += async (model, ea) =>
        //    {
        //        var body = ea.Body.ToArray();
        //        var message = JsonSerializer.Deserialize<UserCreatedMessage>(Encoding.UTF8.GetString(body));

        //        using (var scope = _serviceProvider.CreateScope())
        //        {
        //            var handler = scope.ServiceProvider.GetRequiredService<UserCreatedMessageHandler>();
        //            await handler.Handle(message);
        //        }
        //    };

        //    await _channel.BasicConsumeAsync(queue: queueName, autoAck: true, consumer: consumer);

        //    await Task.Delay(Timeout.Infinite, stoppingToken);
        //}

        //public override async Task StopAsync(CancellationToken cancellationToken)
        //{
        //    await _channel.CloseAsync();
        //    await _connection.CloseAsync();
        //    await base.StopAsync(cancellationToken);
        //}
    }
}