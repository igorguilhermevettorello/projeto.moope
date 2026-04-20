using System.ComponentModel.DataAnnotations;

namespace Projeto.Moope.Pedido.Api.DTOs
{
    public class PedidoUpdateTransacoesDto
    {
        [Required(ErrorMessage = "O Id do pedido é obrigatório")]
        public Guid PedidoId { get; set; }

        [Required(ErrorMessage = "As transações são obrigatórias")]
        [MinLength(1, ErrorMessage = "Informe ao menos uma transação")]
        public List<TransacaoDto> Transacoes { get; set; } = new();
    }
}

