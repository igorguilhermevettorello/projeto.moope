namespace Projeto.Moope.Gateways.Core.DTOs
{
    public class ResultDto
    {
        public bool Status { get; set; }
        public int StatusCode { get; init; }
        public string? Mensagem { get; set; }
    }

    public class ResultDto<T>
    {
        public bool Status { get; set; }
        public int StatusCode { get; init; }
        public string? Mensagem { get; set; }
        public T? Dados { get; set; }
    }
}
