using MediatR;
using Projeto.Moope.Auth.Core.DTOs.Usuario;
using Projeto.Moope.Auth.Core.Interfaces.Repositories;
using Projeto.Moope.Auth.Core.Interfaces.Services;
using Projeto.Moope.Core.Enums;

namespace Projeto.Moope.Auth.Application.Queries.Usuario.Listar
{
    public class ListarUsuarioQueryHandler : IRequestHandler<ListarUsuarioQuery, IEnumerable<ListarUsuarioDto>>
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IIdentityUserService _identityUserService;
        private readonly IPessoaFisicaRepository _pessoaFisicaRepository;
        private readonly IPessoaJuridicaRepository _pessoaJuridicaRepository;

        public ListarUsuarioQueryHandler(
            IUsuarioRepository usuarioRepository,
            IIdentityUserService identityUserService,
            IPessoaFisicaRepository pessoaFisicaRepository,
            IPessoaJuridicaRepository pessoaJuridicaRepository)
        {
            _usuarioRepository = usuarioRepository;
            _identityUserService = identityUserService;
            _pessoaFisicaRepository = pessoaFisicaRepository;
            _pessoaJuridicaRepository = pessoaJuridicaRepository;
        }

        public async Task<IEnumerable<ListarUsuarioDto>> Handle(ListarUsuarioQuery request, CancellationToken cancellationToken)
        {
            var usuarios = await _usuarioRepository.BuscarTodosAsync();
            var resultado = new List<ListarUsuarioDto>();

            foreach (var usuario in usuarios)
            {
                var identityUser = await _identityUserService.BuscarPorIdAsync(usuario.Id);
                var tipoPessoa = await ObterTipoPessoaAsync(usuario.Id);

                resultado.Add(new ListarUsuarioDto
                {
                    Id = usuario.Id,
                    Nome = usuario.Nome,
                    Email = identityUser?.Email ?? string.Empty,
                    Telefone = identityUser?.PhoneNumber ?? string.Empty,
                    TipoPessoa = tipoPessoa,
                    Created = usuario.Created
                });
            }

            return resultado;
        }

        private async Task<TipoPessoa> ObterTipoPessoaAsync(Guid usuarioId)
        {
            var pessoaFisica = await _pessoaFisicaRepository.BuscarPorIdAsync(usuarioId);
            if (pessoaFisica != null && pessoaFisica.Id != Guid.Empty)
            {
                return TipoPessoa.FISICA;
            }

            var pessoaJuridica = await _pessoaJuridicaRepository.BuscarPorIdAsync(usuarioId);
            if (pessoaJuridica != null && pessoaJuridica.Id != Guid.Empty)
            {
                return TipoPessoa.JURIDICA;
            }

            return TipoPessoa.FISICA;
        }
    }
}
