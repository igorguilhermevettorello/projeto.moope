using AutoMapper;
using Projeto.Moope.Pedido.Api.DTOs;
using Projeto.Moope.Pedido.Core.DTOs.Pedido;

namespace Projeto.Moope.Pedido.Api.Mappings
{
    public class PedidoMappingProfile : Profile
    {
        public PedidoMappingProfile()
        {
            CreateMap<PedidoCreateRequestDto, PedidoCreateDto>();
        }
    }
}
