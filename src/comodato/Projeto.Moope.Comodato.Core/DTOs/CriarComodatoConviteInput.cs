namespace Projeto.Moope.Comodato.Core.DTOs
{
    public class CriarComodatoConviteInput
    {
        public int Quantidade { get; set; }
        public Guid VendedorId { get; set; }
        public Guid PlanoId { get; set; }
        public decimal Valor { get; set; }
        public string Estado { get; set; } = string.Empty;
        public DateTime DataPagamento { get; set; }
    }
}
