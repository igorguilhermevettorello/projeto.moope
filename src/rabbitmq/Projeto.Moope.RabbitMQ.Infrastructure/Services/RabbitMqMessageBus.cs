namespace Projeto.Moope.RabbitMQ.Infrastructure.Services
{
    public class RabbitMqMessageBus
    {
        //private readonly IConfiguration _configuration;

        //public RabbitMqMessageBus(IConfiguration configuration)
        //{
        //    _configuration = configuration;
        //}

        //public async Task Publish<T>(T message) where T : class
        //{
        //    var factory = new ConnectionFactory
        //    {
        //        HostName = _configuration["RabbitMQ:HostName"] ?? "localhost",
        //        UserName = _configuration["RabbitMQ:UserName"] ?? "guest",
        //        Password = _configuration["RabbitMQ:Password"] ?? "guest"
        //    };

        //    using var connection = await factory.CreateConnectionAsync();
        //    using var channel = await connection.CreateChannelAsync();

        //    var queueName = typeof(T).Name;
        //    await channel.QueueDeclareAsync(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

        //    var json = JsonSerializer.Serialize(message);
        //    var body = Encoding.UTF8.GetBytes(json);

        //    await channel.BasicPublishAsync(exchange: "", routingKey: queueName, body: body);
        //}
    }
}