using MediatR;
using Projeto.Moope.Auth.Core.DTOs;
using Projeto.Moope.Auth.Core.Interfaces.Repositories;
using Projeto.Moope.Auth.Core.Interfaces.Services;
using Projeto.Moope.Auth.Core.Models;
using Projeto.Moope.Core.Enums;
using UsuarioModel = Projeto.Moope.Auth.Core.Models.Usuario;

namespace Projeto.Moope.Auth.Application.Commands.Usuario.Alterar
{
    public class AlterarUsuarioCommandHandler : IRequestHandler<AlterarUsuarioCommand, Result<Guid>>
    {
        private readonly IIdentityUserService _identityUserService;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IPessoaFisicaRepository _pessoaFisicaRepository;
        private readonly IPessoaJuridicaRepository _pessoaJuridicaRepository;

        public AlterarUsuarioCommandHandler(
            IIdentityUserService identityUserService,
            IUsuarioRepository usuarioRepository,
            IPessoaFisicaRepository pessoaFisicaRepository,
            IPessoaJuridicaRepository pessoaJuridicaRepository)
        {
            _identityUserService = identityUserService;
            _usuarioRepository = usuarioRepository;
            _pessoaFisicaRepository = pessoaFisicaRepository;
            _pessoaJuridicaRepository = pessoaJuridicaRepository;
        }

        public async Task<Result<Guid>> Handle(AlterarUsuarioCommand request, CancellationToken cancellationToken)
        {
            if (!request.IsValid())
            {
                return new Result<Guid> { Status = false, Mensagem = "Dados inválidos" };
            }

            var usuario = await _usuarioRepository.BuscarPorIdAsync(request.Id);
            if (usuario == null || usuario.Id == Guid.Empty)
            {
                return new Result<Guid> { Status = false, Mensagem = "Usuário não encontrado" };
            }

            var identityResult = await _identityUserService.AlterarUsuarioAsync(
                request.Id,
                request.Email,
                request.Telefone);

            if (!identityResult.Status)
            {
                return new Result<Guid> { Status = false, Mensagem = identityResult.Mensagem };
            }

            usuario.Nome = request.Nome;
            usuario.Updated = DateTime.UtcNow;
            await _usuarioRepository.AtualizarAsync(usuario);

            if (request.TipoPessoa == TipoPessoa.FISICA)
            {
                var pessoaFisica = await _pessoaFisicaRepository.BuscarPorIdAsync(request.Id);
                if (pessoaFisica != null && pessoaFisica.Id != Guid.Empty)
                {
                    pessoaFisica.Nome = request.Nome;
                    pessoaFisica.Cpf = request.CpfCnpj;
                    pessoaFisica.Updated = DateTime.UtcNow;
                    await _pessoaFisicaRepository.AtualizarAsync(pessoaFisica);
                }
                else
                {
                    var novaPessoaFisica = new PessoaFisica
                    {
                        Id = request.Id,
                        Nome = request.Nome,
                        Cpf = request.CpfCnpj,
                        Created = DateTime.UtcNow,
                        Updated = DateTime.UtcNow
                    };
                    await _pessoaFisicaRepository.SalvarAsync(novaPessoaFisica);
                }
            }
            else if (request.TipoPessoa == TipoPessoa.JURIDICA)
            {
                var pessoaJuridica = await _pessoaJuridicaRepository.BuscarPorIdAsync(request.Id);
                if (pessoaJuridica != null && pessoaJuridica.Id != Guid.Empty)
                {
                    pessoaJuridica.RazaoSocial = request.Nome;
                    pessoaJuridica.Cnpj = request.CpfCnpj;
                    pessoaJuridica.NomeFantasia = request.NomeFantasia;
                    pessoaJuridica.InscricaoEstadual = request.InscricaoEstadual;
                    pessoaJuridica.Updated = DateTime.UtcNow;
                    await _pessoaJuridicaRepository.AtualizarAsync(pessoaJuridica);
                }
                else
                {
                    var novaPessoaJuridica = new PessoaJuridica
                    {
                        Id = request.Id,
                        Cnpj = request.CpfCnpj,
                        RazaoSocial = request.Nome,
                        NomeFantasia = request.NomeFantasia,
                        InscricaoEstadual = request.InscricaoEstadual,
                        Created = DateTime.UtcNow,
                        Updated = DateTime.UtcNow
                    };
                    await _pessoaJuridicaRepository.SalvarAsync(novaPessoaJuridica);
                }
            }

            var sucesso = await _usuarioRepository.UnitOfWork.Commit();
            return new Result<Guid> { Status = sucesso, Dados = usuario.Id };
        }
    }
}
