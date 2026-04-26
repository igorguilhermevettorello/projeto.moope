using System.ComponentModel.DataAnnotations;

namespace Projeto.Moope.Pedido.Api.DTOs
{
    public sealed class PedidoUpdateGalaxPayIdRequestDto
    {
        [Required(ErrorMessage = "O Id do pedido é obrigatório")]
        public Guid PedidoId { get; set; }

        [Required(ErrorMessage = "O GalaxPayId é obrigatório")]
        [Range(1, int.MaxValue, ErrorMessage = "O GalaxPayId deve ser maior que zero")]
        public int GalaxPayId { get; set; }
    }
}

