using System.Net;

namespace Projeto.Moope.Pagamento.Core.Exceptions
{
    public class CelcoinGatewayException : Exception
    {
        public HttpStatusCode StatusCode { get; }
        public string? ResponseBody { get; }

        public CelcoinGatewayException(string message, HttpStatusCode statusCode, string? responseBody, Exception? innerException = null)
            : base(message, innerException)
        {
            StatusCode = statusCode;
            ResponseBody = responseBody;
        }
    }
}

