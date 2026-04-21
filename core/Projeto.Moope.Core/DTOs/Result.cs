namespace Projeto.Moope.Core.DTOs
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
    
    public class ResultUserDto<T>
    {
        public bool Status { get; set; }
        public int StatusCode { get; init; }
        public string? Mensagem { get; set; }
        public T? Dados { get; set; }
        public bool UsuarioExiste { get; set; } = false;
    }
}
