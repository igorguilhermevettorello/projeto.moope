namespace Projeto.Moope.Pedido.Core.DTOs.Plano
{
    public class PlanoDetailDto
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
