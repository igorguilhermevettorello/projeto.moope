using AutoMapper;
using Projeto.Moope.Gateways.Api.DTOs.Venda;
using Projeto.Moope.Gateways.Core.DTOs.Venda;

namespace Projeto.Moope.Gateways.Api.Mappings
{
    public sealed class VendaMappingProfile : Profile
    {
        public VendaMappingProfile()
        {
            CreateMap<VendaRequestDto, VendaCreateDto>()
                .ForMember(
                    dest => dest.PlanoId,
                    opt => opt.MapFrom(src => src.PlanoId ?? Guid.Empty))
                .ForMember(
                    dest => dest.Descontos,
                    opt => opt.MapFrom(src => (IReadOnlyList<string>?)src.Descontos));
        }
    }
}
