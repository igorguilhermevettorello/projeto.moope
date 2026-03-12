using MediatR;
using Projeto.Moope.Auth.Core.DTOs.Usuario;
using Projeto.Moope.Auth.Core.Interfaces.Repositories;
using Projeto.Moope.Auth.Core.Interfaces.Services;
using Projeto.Moope.Core.Enums;

namespace Projeto.Moope.Auth.Application.Queries.Usuario.ObterPorId
{
    public class ObterUsuarioPorIdQueryHandler : IRequestHandler<ObterUsuarioPorIdQuery, UsuarioDetalheDto?>
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IIdentityUserService _identityUserService;
        private readonly IPessoaFisicaRepository _pessoaFisicaRepository;
        private readonly IPessoaJuridicaRepository _pessoaJuridicaRepository;

        public ObterUsuarioPorIdQueryHandler(
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

        public async Task<UsuarioDetalheDto?> Handle(ObterUsuarioPorIdQuery request, CancellationToken cancellationToken)
        {
            var usuario = await _usuarioRepository.BuscarPorIdAsync(request.Id);
            if (usuario == null || usuario.Id == Guid.Empty)
            {
                return null;
            }

            var identityUser = await _identityUserService.BuscarPorIdAsync(request.Id);
            var pessoaFisica = await _pessoaFisicaRepository.BuscarPorIdAsync(request.Id);
            var pessoaJuridica = await _pessoaJuridicaRepository.BuscarPorIdAsync(request.Id);

            var cpfCnpj = string.Empty;
            var nomeFantasia = (string?)null;
            var inscricaoEstadual = (string?)null;
            TipoPessoa tipoPessoa;

            if (pessoaFisica != null && pessoaFisica.Id != Guid.Empty)
            {
                cpfCnpj = pessoaFisica.Cpf;
                tipoPessoa = TipoPessoa.FISICA;
            }
            else if (pessoaJuridica != null && pessoaJuridica.Id != Guid.Empty)
            {
                cpfCnpj = pessoaJuridica.Cnpj;
                nomeFantasia = pessoaJuridica.NomeFantasia;
                inscricaoEstadual = pessoaJuridica.InscricaoEstadual;
                tipoPessoa = TipoPessoa.JURIDICA;
            }
            else
            {
                tipoPessoa = TipoPessoa.FISICA;
            }

            return new UsuarioDetalheDto
            {
                Id = usuario.Id,
                Nome = usuario.Nome,
                Email = identityUser?.Email ?? string.Empty,
                CpfCnpj = cpfCnpj,
                Telefone = identityUser?.PhoneNumber ?? string.Empty,
                TipoPessoa = tipoPessoa,
                NomeFantasia = nomeFantasia,
                InscricaoEstadual = inscricaoEstadual,
                Created = usuario.Created,
                Updated = usuario.Updated
            };
        }
    }
}
