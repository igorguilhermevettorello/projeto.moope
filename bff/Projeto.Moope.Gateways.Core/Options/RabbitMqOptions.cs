namespace Projeto.Moope.Gateways.Core.Options
{
    public class RabbitMqOptions
    {
        public const string SectionName = "RabbitMQ";

        public string HostName { get; set; } = string.Empty;

        public int Port { get; set; } = 5672;

        public string UserName { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string VirtualHost { get; set; } = "/";

        public int ConnectionTimeout { get; set; } = 30;

        public int MaxRetryAttempts { get; set; } = 3;

        public int RetryIntervalSeconds { get; set; } = 5;
    }
}

