namespace Projeto.Moope.RabbitMQ.Core.Options
{
    public sealed class RabbitMqOptions
    {
        public const string SectionName = "RabbitMQ";

        public string HostName { get; init; } = "localhost";
        public int Port { get; init; } = 5672;
        public string UserName { get; init; } = "guest";
        public string Password { get; init; } = "guest";
        public string VirtualHost { get; init; } = "/";
        public int ConnectionTimeout { get; init; } = 30;
        public int MaxRetryAttempts { get; init; } = 3;
        public int RetryIntervalSeconds { get; init; } = 5;
    }
}

