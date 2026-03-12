using MediatR;
using Projeto.Moope.Auth.Core.DTOs;
using Projeto.Moope.Auth.Core.Interfaces.Repositories;
using Projeto.Moope.Auth.Core.Interfaces.Services;

namespace Projeto.Moope.Auth.Application.Commands.Usuario.Deletar
{
    public class DeletarUsuarioCommandHandler : IRequestHandler<DeletarUsuarioCommand, Result>
    {
        private readonly IIdentityUserService _identityUserService;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IPapelRepository _papelRepository;
        private readonly IPessoaFisicaRepository _pessoaFisicaRepository;
        private readonly IPessoaJuridicaRepository _pessoaJuridicaRepository;

        public DeletarUsuarioCommandHandler(
            IIdentityUserService identityUserService,
            IUsuarioRepository usuarioRepository,
            IPapelRepository papelRepository,
            IPessoaFisicaRepository pessoaFisicaRepository,
            IPessoaJuridicaRepository pessoaJuridicaRepository)
        {
            _identityUserService = identityUserService;
            _usuarioRepository = usuarioRepository;
            _papelRepository = papelRepository;
            _pessoaFisicaRepository = pessoaFisicaRepository;
            _pessoaJuridicaRepository = pessoaJuridicaRepository;
        }

        public async Task<Result> Handle(DeletarUsuarioCommand request, CancellationToken cancellationToken)
        {
            if (!request.IsValid())
            {
                return new Result { Status = false, Mensagem = "Dados inválidos" };
            }

            var usuario = await _usuarioRepository.BuscarPorIdAsync(request.Id);
            if (usuario == null || usuario.Id == Guid.Empty)
            {
                return new Result { Status = false, Mensagem = "Usuário não encontrado" };
            }

            var papeis = await _papelRepository.BuscarPorUsuarioIdAsync(request.Id);
            foreach (var papel in papeis)
            {
                await _papelRepository.RemoverAsync(papel.Id);
            }

            var pessoaFisica = await _pessoaFisicaRepository.BuscarPorIdAsync(request.Id);
            if (pessoaFisica != null)
            {
                await _pessoaFisicaRepository.RemoverAsync(request.Id);
            }

            var pessoaJuridica = await _pessoaJuridicaRepository.BuscarPorIdAsync(request.Id);
            if (pessoaJuridica != null)
            {
                await _pessoaJuridicaRepository.RemoverAsync(request.Id);
            }

            await _usuarioRepository.RemoverAsync(request.Id);

            var commitSucesso = await _usuarioRepository.UnitOfWork.Commit();
            if (!commitSucesso)
            {
                return new Result { Status = false, Mensagem = "Erro ao remover usuário" };
            }

            var identityResult = await _identityUserService.RemoverUsuarioAsync(request.Id);
            if (!identityResult.Status)
            {
                return new Result { Status = false, Mensagem = identityResult.Mensagem ?? "Erro ao remover usuário de identidade" };
            }

            return new Result { Status = true };
        }
    }
}
