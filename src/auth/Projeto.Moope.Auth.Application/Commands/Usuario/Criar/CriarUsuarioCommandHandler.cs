using MediatR;
using Projeto.Moope.Auth.Core.DTOs;
using Projeto.Moope.Auth.Core.Interfaces.Repositories;
using Projeto.Moope.Auth.Core.Interfaces.Services;
using Projeto.Moope.Auth.Core.Models;
using Projeto.Moope.Core.Enums;
using UsuarioModel = Projeto.Moope.Auth.Core.Models.Usuario;

namespace Projeto.Moope.Auth.Application.Commands.Usuario.Criar
{
    public class CriarUsuarioCommandHandler : IRequestHandler<CriarUsuarioCommand, Result<Guid>>
    {
        private readonly IIdentityUserService _identityUserService;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IPapelRepository _papelRepository;
        private readonly IPessoaFisicaRepository _pessoaFisicaRepository;
        private readonly IPessoaJuridicaRepository _pessoaJuridicaRepository;
        
        public CriarUsuarioCommandHandler(
            IIdentityUserService identityUserService,
            IPapelRepository papelRepository,
            IUsuarioRepository usuarioRepository,
            IPessoaFisicaRepository pessoaFisicaRepository,
            IPessoaJuridicaRepository pessoaJuridicaRepository)
        {
            _identityUserService = identityUserService;
            _usuarioRepository = usuarioRepository;
            _papelRepository = papelRepository;
            _pessoaFisicaRepository = pessoaFisicaRepository;
            _pessoaJuridicaRepository = pessoaJuridicaRepository;
        }

        public async Task<Result<Guid>> Handle(CriarUsuarioCommand request, CancellationToken cancellationToken)
        {
            // validate business rules / duplicates
            if (!request.IsValid())
            {
                return new Result<Guid> { Status = false, Mensagem = "teste" };
            }

            // Create IdentityUser
            var identityResult = await _identityUserService.CriarUsuarioAsync(
                request.Email,
                request.Senha,
                request.Telefone,
                request.TipoUsuario);

            if (!identityResult.Status)
            {
                return new Result<Guid> { Status = false, Mensagem = identityResult.Mensagem };
            }

            var identityUser = identityResult.Dados;
            if (identityUser == null)
            {
                return new Result<Guid> { Status = false, Mensagem = "Erro ao criar usuário de identidade" };
            }

            // Create Usuario
            var usuario = CriarUsuario(request, identityUser.Id);

            // Create Pessoa based on TipoPessoa
            if (request.TipoPessoa == TipoPessoa.FISICA)
            {
                var pessoaFisica = CriarPessoaFisica(request, identityUser.Id);
                await _pessoaFisicaRepository.SalvarAsync(pessoaFisica);
                //usuario.PessoaFisicaId = pessoaFisica.Id;
                //usuario.PessoaFisica = pessoaFisica;
            }
            else if (request.TipoPessoa == TipoPessoa.JURIDICA)
            {
                var pessoaJuridica = CriarPessoaJuridica(request, identityUser.Id);
                await _pessoaJuridicaRepository.SalvarAsync(pessoaJuridica);
                //usuario.PessoaJuridicaId = pessoaJuridica.Id;
                //usuario.PessoaJuridica = pessoaJuridica;
            }

            await _papelRepository.SalvarAsync(new Papel()
            {
                UsuarioId = identityUser.Id,
                TipoUsuario = request.TipoUsuario,
                Created = DateTime.UtcNow
            });

            // Save Usuario
            await _usuarioRepository.SalvarAsync(usuario);

            var sucesso = await _usuarioRepository.UnitOfWork.Commit();

            return new Result<Guid> { Status = sucesso, Dados = usuario.Id };
        }

        private UsuarioModel CriarUsuario(CriarUsuarioCommand request, Guid id)
        {
            return new UsuarioModel
            {
                Id = id,
                Nome = request.Nome,
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow
            };
        }

        private PessoaFisica CriarPessoaFisica(CriarUsuarioCommand request, Guid id)
        {
            return new PessoaFisica
            {
                Id = id,
                Nome = request.Nome,
                Cpf = request.CpfCnpj,
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow
            };
        }

        private PessoaJuridica CriarPessoaJuridica(CriarUsuarioCommand request, Guid id)
        {
            return new PessoaJuridica
            {
                Id = id,
                Cnpj = request.CpfCnpj,
                RazaoSocial = request.Nome,
                NomeFantasia = request.NomeFantasia,
                InscricaoEstadual = request.InscricaoEstadual,
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow
            };
        }
    }
}
