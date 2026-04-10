using AutoMapper;
using Projeto.Moope.Cliente.Api.DTOs.Clientes;
using Projeto.Moope.Cliente.Core.Commands.Clientes.Atualizar;
using Projeto.Moope.Cliente.Core.Commands.Clientes.Criar;

namespace Projeto.Moope.Cliente.Api.Configurations
{
    public class AutomapperClienteProfile : Profile
    {
        public AutomapperClienteProfile()
        {
            CreateMap<CreateClienteDto, CriarClienteCommand>()
                .ForMember(dest => dest.Nome, opt => opt.MapFrom(src => src.Nome))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.CpfCnpj, opt => opt.MapFrom(src => src.CpfCnpj))
                .ForMember(dest => dest.Telefone, opt => opt.MapFrom(src => src.Telefone))
                .ForMember(dest => dest.TipoPessoa, opt => opt.MapFrom(src => src.TipoPessoa))
                .ForMember(dest => dest.Ativo, opt => opt.MapFrom(src => src.Ativo))
                .ForMember(dest => dest.Senha, opt => opt.MapFrom(src => src.Senha))
                .ForMember(dest => dest.Confirmacao, opt => opt.MapFrom(src => src.Confirmacao))
                .ForMember(dest => dest.NomeFantasia, opt => opt.MapFrom(src => src.NomeFantasia))
                .ForMember(dest => dest.InscricaoEstadual, opt => opt.MapFrom(src => src.InscricaoEstadual))
                .ForMember(dest => dest.VendedorId, opt => opt.MapFrom(src => src.VendedorId))
                .ForMember(dest => dest.Logradouro, opt => opt.MapFrom(src => src.Endereco != null ? src.Endereco.Logradouro : null))
                .ForMember(dest => dest.Numero, opt => opt.MapFrom(src => src.Endereco != null ? src.Endereco.Numero : null))
                .ForMember(dest => dest.Complemento, opt => opt.MapFrom(src => src.Endereco != null ? src.Endereco.Complemento : null))
                .ForMember(dest => dest.Bairro, opt => opt.MapFrom(src => src.Endereco != null ? src.Endereco.Bairro : null))
                .ForMember(dest => dest.Cidade, opt => opt.MapFrom(src => src.Endereco != null ? src.Endereco.Cidade : null))
                .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => src.Endereco != null ? src.Endereco.Estado : null))
                .ForMember(dest => dest.Cep, opt => opt.MapFrom(src => src.Endereco != null ? src.Endereco.Cep : null));

            CreateMap<UpdateClienteDto, AtualizarClienteCommand>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Nome, opt => opt.MapFrom(src => src.Nome))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.CpfCnpj, opt => opt.MapFrom(src => src.CpfCnpj))
                .ForMember(dest => dest.Telefone, opt => opt.MapFrom(src => src.Telefone))
                .ForMember(dest => dest.TipoPessoa, opt => opt.MapFrom(src => src.TipoPessoa))
                .ForMember(dest => dest.Ativo, opt => opt.MapFrom(src => src.Ativo))
                .ForMember(dest => dest.NomeFantasia, opt => opt.MapFrom(src => src.NomeFantasia))
                .ForMember(dest => dest.InscricaoEstadual, opt => opt.MapFrom(src => src.InscricaoEstadual))
                .ForMember(dest => dest.VendedorId, opt => opt.MapFrom(src => src.VendedorId))
                .ForMember(dest => dest.Logradouro, opt => opt.MapFrom(src => src.Endereco != null ? src.Endereco.Logradouro : null))
                .ForMember(dest => dest.Numero, opt => opt.MapFrom(src => src.Endereco != null ? src.Endereco.Numero : null))
                .ForMember(dest => dest.Complemento, opt => opt.MapFrom(src => src.Endereco != null ? src.Endereco.Complemento : null))
                .ForMember(dest => dest.Bairro, opt => opt.MapFrom(src => src.Endereco != null ? src.Endereco.Bairro : null))
                .ForMember(dest => dest.Cidade, opt => opt.MapFrom(src => src.Endereco != null ? src.Endereco.Cidade : null))
                .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => src.Endereco != null ? src.Endereco.Estado : null))
                .ForMember(dest => dest.Cep, opt => opt.MapFrom(src => src.Endereco != null ? src.Endereco.Cep : null));
        }
    }
}
