namespace Projeto.Moope.Plano.Api.DTOs
{
    public class DetailPlanoDto
    {
        public Guid Id { get; set; }
        public string Codigo { get; set; }
        public string Descricao { get; set; }
        public decimal Valor { get; set; }
        public decimal? TaxaAdesao { get; set; }
        public bool Status { get; set; }
        public bool Plataforma { get; set; }
    }
}
