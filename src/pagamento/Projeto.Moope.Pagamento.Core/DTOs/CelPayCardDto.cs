namespace Projeto.Moope.Pagamento.Core.DTOs
{
    public class CelPayCardDto
    {
        public int? GalaxPayId { get; set; }
        public string Number { get; set; }
        public string ExpMonth { get; set; }
        public string ExpYear { get; set; }
        public string Cvv { get; set; }
        public string HolderName { get; set; }
    }
}
