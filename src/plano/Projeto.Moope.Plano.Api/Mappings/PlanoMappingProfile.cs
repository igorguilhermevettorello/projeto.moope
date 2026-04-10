using AutoMapper;
using Projeto.Moope.Plano.Api.DTOs;
using PlanoModel = Projeto.Moope.Plano.Core.Models.Plano;

namespace Projeto.Moope.Plano.Api.Mappings
{
    public class PlanoMappingProfile : Profile
    {
        public PlanoMappingProfile()
        {
            CreateMap<PlanoModel, PlanoResponseDto>();
            CreateMap<PlanoModel, DetailPlanoDto>();
        }
    }
}
