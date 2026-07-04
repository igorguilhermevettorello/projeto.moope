using System.Security.Cryptography;
using System.Text;
using Projeto.Moope.Comodato.Core.DTOs;
using Projeto.Moope.Comodato.Core.Interfaces.Repositories;
using Projeto.Moope.Comodato.Core.Interfaces.Services;
using Projeto.Moope.Comodato.Core.Models;
using Projeto.Moope.Core.DTOs;
using Projeto.Moope.Core.Enums;
using Projeto.Moope.Core.Interfaces.Notificacao;
using Projeto.Moope.Core.Services;

namespace Projeto.Moope.Comodato.Core.Services
{
    public class ComodatoConviteService : BaseService, IComodatoConviteService
    {
        private readonly IComodatoConviteRepository _comodatoConviteRepository;

        public ComodatoConviteService(
            IComodatoConviteRepository comodatoConviteRepository,
            INotificador notificador) : base(notificador)
        {
            _comodatoConviteRepository = comodatoConviteRepository;
        }

        public async Task<ComodatoConvite?> BuscarPorIdAsNotrackingAsync(Guid id)
        {
            return await _comodatoConviteRepository.BuscarPorIdAsNotrackingAsync(id);
        }

        public async Task<ResultDto<CriarComodatoConviteResultado>> CriarAsync(CriarComodatoConviteInput input, Guid adminId)
        {
            if (!ValidarInput(input))
            {
                return new ResultDto<CriarComodatoConviteResultado> { Status = false };
            }

            if (adminId == Guid.Empty)
            {
                Notificar(nameof(adminId), "Usuário administrador não identificado.");
                return new ResultDto<CriarComodatoConviteResultado> { Status = false };
            }

            var token = GerarToken();
            var tokenHash = GerarTokenHash(token);
            var dataPagamentoUtc = DateTime.SpecifyKind(input.DataPagamento.Date, DateTimeKind.Utc);

            var convite = new ComodatoConvite
            {
                TokenHash = tokenHash,
                CreatedByAdminId = adminId,
                PlanoId = input.PlanoId,
                Quantidade = input.Quantidade,
                Valor = input.Valor,
                VendedorId = input.VendedorId,
                CriadoEm = DateTime.UtcNow,
                ExpiradoEm = dataPagamentoUtc.AddDays(1).AddTicks(-1),
                Status = ComodatoConviteStatus.Criado,
                Estado = input.Estado,
                DataPagamento = dataPagamentoUtc
            };

            try
            {
                var entity = await _comodatoConviteRepository.SalvarAsync(convite);
                return new ResultDto<CriarComodatoConviteResultado>
                {
                    Status = true,
                    Dados = new CriarComodatoConviteResultado
                    {
                        Convite = entity,
                        Token = token
                    }
                };
            }
            catch (Exception)
            {
                Notificar(nameof(ComodatoConvite.TokenHash), "Não foi possível criar o convite. Tente novamente.");
                return new ResultDto<CriarComodatoConviteResultado> { Status = false };
            }
        }

        private bool ValidarInput(CriarComodatoConviteInput input)
        {
            var valido = true;

            if (input.Quantidade <= 0)
            {
                Notificar(nameof(input.Quantidade), "A quantidade deve ser maior que zero.");
                valido = false;
            }

            if (input.Valor < 0)
            {
                Notificar(nameof(input.Valor), "O valor deve ser maior ou igual a zero.");
                valido = false;
            }

            if (input.PlanoId == Guid.Empty)
            {
                Notificar(nameof(input.PlanoId), "O PlanoId é obrigatório.");
                valido = false;
            }

            if (input.VendedorId == Guid.Empty)
            {
                Notificar(nameof(input.VendedorId), "O VendedorId é obrigatório.");
                valido = false;
            }

            if (string.IsNullOrWhiteSpace(input.Estado))
            {
                Notificar(nameof(input.Estado), "O Estado é obrigatório.");
                valido = false;
            }
            else if (input.Estado.Length > 50)
            {
                Notificar(nameof(input.Estado), "O Estado deve ter no máximo 50 caracteres.");
                valido = false;
            }

            if (input.DataPagamento == default)
            {
                Notificar(nameof(input.DataPagamento), "A DataPagamento é obrigatória.");
                valido = false;
            }

            return valido;
        }

        private static string GerarToken()
        {
            return Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();
        }

        private static string GerarTokenHash(string token)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }
    }
}
