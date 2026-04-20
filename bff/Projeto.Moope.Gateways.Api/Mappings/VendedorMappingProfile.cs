using AutoMapper;
using Projeto.Moope.Gateways.Api.DTOs.Endereco;
using Projeto.Moope.Gateways.Api.DTOs.Vendedor;
using Projeto.Moope.Gateways.Core.DTOs.Endereco;
using Projeto.Moope.Gateways.Core.DTOs.Vendedor;

namespace Projeto.Moope.Gateways.Api.Mappings
{
    public sealed class VendedorMappingProfile : Profile
    {
        public VendedorMappingProfile()
        {
            CreateMap<EnderecoCreateRequestDto, EnderecoCreateDto>();
            CreateMap<EnderecoUpdateRequestDto, EnderecoUpdateDto>();

            CreateMap<VendedorCreateRequestDto, VendedorCreateDto>();
            CreateMap<VendedorUpdateRequestDto, VendedorUpdateDto>();
        }
    }
}

