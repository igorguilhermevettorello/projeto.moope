using AutoMapper;
using Projeto.Moope.Gateways.Api.DTOs.Cliente;
using Projeto.Moope.Gateways.Api.DTOs.Endereco;
using Projeto.Moope.Gateways.Core.DTOs.Cliente;
using Projeto.Moope.Gateways.Core.DTOs.Endereco;

namespace Projeto.Moope.Gateways.Api.Mappings
{
    public sealed class ClienteMappingProfile : Profile
    {
        public ClienteMappingProfile()
        {
            CreateMap<EnderecoCreateRequestDto, EnderecoCreateDto>();
            CreateMap<EnderecoUpdateRequestDto, EnderecoUpdateDto>();

            CreateMap<ClienteCreateRequestDto, ClienteCreateDto>();
            CreateMap<ClienteUpdateRequestDto, ClienteUpdateDto>();
        }
    }
}

