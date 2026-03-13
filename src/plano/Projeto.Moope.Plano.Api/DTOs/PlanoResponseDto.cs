namespace Projeto.Moope.Plano.Api.DTOs
{
    public class PlanoResponseDto
    {
        public Guid Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public decimal? TaxaAdesao { get; set; }
        public bool Status { get; set; }
        public bool Plataforma { get; set; }
    }
}
