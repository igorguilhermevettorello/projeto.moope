namespace Projeto.Moope.RabbitMQ.Core.Helpers
{
    internal static class UrlHelper
    {
        public static string Combine(string? baseUrl, string path)
        {
            baseUrl ??= string.Empty;
            path ??= string.Empty;

            if (string.IsNullOrWhiteSpace(baseUrl))
                return path;

            if (string.IsNullOrWhiteSpace(path))
                return baseUrl;

            return $"{baseUrl.TrimEnd('/')}/{path.TrimStart('/')}";
        }
    }
}

