using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Projeto.Moope.Core.DTOs;
using Projeto.Moope.Core.Enums;
using Projeto.Moope.Gateways.Core.DTOs.Cartao;
using Projeto.Moope.Gateways.Core.DTOs.Cliente;
using Projeto.Moope.Gateways.Core.DTOs.Cliente.GalaxPay;
using Projeto.Moope.Gateways.Core.DTOs.Pedido;
using Projeto.Moope.Gateways.Core.DTOs.Venda;
using Projeto.Moope.Gateways.Core.Helpers;
using Projeto.Moope.Gateways.Core.Interfaces.Services;
using Projeto.Moope.Gateways.Core.Interfaces.Services.Cliente;
using Projeto.Moope.Gateways.Core.Interfaces.Services.GalaxPay;
using Projeto.Moope.Gateways.Core.Interfaces.Services.Pedido;
using Projeto.Moope.Gateways.Core.Interfaces.Services.RabbitMQ;
using Projeto.Moope.Gateways.Core.Interfaces.Services.Vendedor;
using Projeto.Moope.Gateways.Core.Options;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Projeto.Moope.Gateways.Core.Services
{
    public class ProcessarVendaService : IProcessarVendaService
    {
        private const string SenhaTemporariaClienteVenda = "ClienteVenda123!";

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly DownstreamApisOptions _apis;
        private readonly IClienteCreateService _clienteCreateService;
        private readonly IAuthClientLoginService _authClientLoginService;
        private readonly IPlanoGetById _planoGetById;
        private readonly IVendedorGetByCupom _vendedorGetByCupom;
        private readonly IClienteGalaxPayCreateService _clienteGalaxPayCreateService;
        private readonly ICartaoGalaxPayCreateService _cartaoGalaxPayCreateService;
        private readonly IVendaSendQueueService _vendaSendQueueService;
        private readonly IClienteGalaxPayUpdateService _clienteGalaxPayUpdateService;
        private readonly IPedidoCreateService _pedidoCreateService;
        public ProcessarVendaService(
            IHttpClientFactory httpClientFactory,
            IOptions<DownstreamApisOptions> apis,
            IClienteCreateService clienteCreateService,
            IAuthClientLoginService authClientLoginService,
            IPlanoGetById planoGetById,
            IVendedorGetByCupom vendedorGetByCupom,
            IClienteGalaxPayCreateService clienteGalaxPayCreateService,
            ICartaoGalaxPayCreateService cartaoGalaxPayCreateService,
            IVendaSendQueueService vendaSendQueueService,
            IClienteGalaxPayUpdateService clienteGalaxPayUpdateService,
            IPedidoCreateService pedidoCreateService)   
        {
            _httpClientFactory = httpClientFactory;
            _apis = apis.Value;
            _clienteCreateService = clienteCreateService;
            _authClientLoginService = authClientLoginService;
            _planoGetById = planoGetById;
            _vendedorGetByCupom = vendedorGetByCupom;
            _clienteGalaxPayCreateService = clienteGalaxPayCreateService;
            _cartaoGalaxPayCreateService = cartaoGalaxPayCreateService;
            _vendaSendQueueService = vendaSendQueueService;
            _clienteGalaxPayUpdateService = clienteGalaxPayUpdateService;
            _pedidoCreateService = pedidoCreateService;
        }

        public async Task<ResultDto<VendaProcessingDto>> ExecutarAsync(
            VendaCreateDto request,
            VendaUsuarioDto? usuario,
            string? authorizationHeader,
            string? idempotencyKey,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(_apis.Cliente)
                || string.IsNullOrWhiteSpace(_apis.Pedido))
            {
                return Utils.FalhaConfig<VendaProcessingDto>("DownstreamApis (Cliente, Pedido) nao configurados.");
            }

            if (string.IsNullOrWhiteSpace(_apis.Plano))
            {
                return Utils.FalhaConfig<VendaProcessingDto>("DownstreamApis:Plano nao configurado.");
            }

            if (string.IsNullOrWhiteSpace(_apis.ClienteApiKey))
            {
                return Utils.FalhaConfig<VendaProcessingDto>("DownstreamApis:ClienteApiKey e obrigatorio para acessar o endpoint anonimo do BC Cliente.");
            }

            if (string.IsNullOrWhiteSpace(_apis.Auth)
                || string.IsNullOrWhiteSpace(_apis.AuthClientId)
                || string.IsNullOrWhiteSpace(_apis.AuthClientSecret))
            {
                return Utils.FalhaConfig<VendaProcessingDto>("DownstreamApis (Auth, AuthClientId, AuthClientSecret) sao obrigatorios para obter token via client/login.");
            }

            if (!string.IsNullOrWhiteSpace(request.CodigoCupom)
                && string.IsNullOrWhiteSpace(_apis.Vendedor))
            {
                return Utils.FalhaConfig<VendaProcessingDto>("DownstreamApis:Vendedor e obrigatorio quando CodigoCupom e informado.");
            }

            var httpClient = _httpClientFactory.CreateClient();

            var rsAuth = await _authClientLoginService.ExecutarAsync(cancellationToken);
            if (rsAuth == null || !rsAuth.Status || string.IsNullOrWhiteSpace(rsAuth.Dados)) 
            {
                return new ResultDto<VendaProcessingDto>
                {
                    Status = false,
                    Mensagem = "Nao foi possivel obter token do servico Auth (client/login).",
                    Dados = null,
                };
            }
            var authBearerHeader = rsAuth.Dados;

            Guid? vendedorId = request.VendedorId;
            if (!string.IsNullOrWhiteSpace(request.CodigoCupom))
            {
                var cupomResult = await _vendedorGetByCupom.ExecutarAsync(request.CodigoCupom, authorizationHeader, cancellationToken);
                if (!cupomResult.Status)
                {
                    return new ResultDto<VendaProcessingDto>
                    {
                        Status = false,
                        Mensagem = cupomResult.Mensagem,
                        Dados = null,
                    };
                }

                vendedorId = cupomResult.Dados?.Id;
            }

            var clienteId = await ClienteAsync(
                httpClient,
                request,
                usuario,
                vendedorId,
                authBearerHeader,
                cancellationToken);

            if (clienteId is null)
            {
                return new ResultDto<VendaProcessingDto>
                {
                    Status = false,
                    StatusCode = StatusCodes.Status502BadGateway,
                    Mensagem = "Nao foi possivel obter ou criar o cliente.",
                    Dados = null
                };
            }
            
            // Verificacao de plano:
            var planoResult = await _planoGetById.ExecutarAsync(request.PlanoId, authBearerHeader, cancellationToken);
            if (!planoResult.Status)
            { 
                return new ResultDto<VendaProcessingDto>
                {
                    Status = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Mensagem = "Plano nao encontrado.",
                    Dados = null
                };
            }
            
            var plano = planoResult.Dados;
            if (!plano.Status)
            {
                return new ResultDto<VendaProcessingDto>
                {
                    Status = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Mensagem = "Plano inativo.",
                    Dados = null
                };
            }

            var documento = string.IsNullOrWhiteSpace(request.CpfCnpj)
                ? string.Empty
                : Regex.Replace(request.CpfCnpj, @"\D", "");

            var rsPedido = await _pedidoCreateService.ExecutarAsync(new PedidoCreateDto
            {
                ClienteId = clienteId.Value,
                VendedorId = vendedorId == Guid.Empty ? (Guid?)null : vendedorId,
                PlanoId = request.PlanoId,
                Quantidade = request.Quantidade,
                TipoPessoa = request.TipoPessoa,
                Estado = request.Estado,
                TipoPlataforma = request.TipoPlataforma,
                Rastreamento = request.Rastreamento,
                IdempotencyKey = idempotencyKey
            }, authBearerHeader, cancellationToken);
            if (!rsPedido.Status)
            {
                return new ResultDto<VendaProcessingDto>
                {
                    Status = false,
                    StatusCode = rsPedido.StatusCode,
                    Mensagem = rsPedido.Mensagem ?? "Erro desconhecido ao criar pedido.",
                    Dados = null
                };
            }
            var pedidoId = rsPedido.Dados?.Id;
            var total = rsPedido.Dados?.Total;
            var taxaAdesao = rsPedido.Dados?.PlanoTaxaAdesao;
            var rsCliente = await _clienteGalaxPayCreateService.ExecutarAsync(new ClienteGalaxPayCreateDto
            {
                Name = request.NomeCliente,
                Email = request.Email,
                Document = documento
            }, cancellationToken);
            if (rsCliente == null || !rsCliente.Status || rsCliente.Dados == null)
            {
                return new ResultDto<VendaProcessingDto>
                {
                    Status = false,
                    StatusCode = StatusCodes.Status502BadGateway,
                    Mensagem = "Nao foi possivel criar cliente no GalaxPay.",
                    Dados = null
                };
            }
            var galaxPayCustomerId = rsCliente.Dados.GalaxPayId;

            var rsClienteGalaxPay = await _clienteGalaxPayUpdateService.ExecutarAsync(new ClienteGalaxPayUpdateDto()
            {
                ClienteId = clienteId.Value,
                GalaxPayCustomerId = galaxPayCustomerId
            }, authBearerHeader, cancellationToken);

            if (rsClienteGalaxPay == null || !rsClienteGalaxPay.Status || rsClienteGalaxPay.Dados == null)
            {
                return new ResultDto<VendaProcessingDto>
                {
                    Status = false,
                    StatusCode = StatusCodes.Status502BadGateway,
                    Mensagem = "Nao foi possivel atualizar cliente no GalaxPay.",
                    Dados = null
                };
            }

            var (mes, ano) = ExtrairMesAnoValidade(request.DataValidade);
            var rsCartao = await _cartaoGalaxPayCreateService.ExecutarAsync(new CartaoGalaxPayCreateDto
            {
                CustomerId = galaxPayCustomerId,
                Holder = request.NomeCartao,
                Number = request.NumeroCartao,
                ExpiresAt = $"{ano}-{mes}",
                Cvv = request.Cvv
            }, cancellationToken);
            if (rsCartao == null || !rsCartao.Status || rsCartao.Dados == null)
            {
                return new ResultDto<VendaProcessingDto>
                {
                    Status = false,
                    StatusCode = StatusCodes.Status502BadGateway,
                    Mensagem = "Nao foi possivel criar cartao no GalaxPay.",
                    Dados = null
                };
            }

            var galaxPayCardId = rsCartao.Dados.GalaxPayId;
            var taxa = true;
            var rsQueue = await _vendaSendQueueService.ExecutarAsync(new VendaQueueDto
            {
                Name = request.NomeCliente,
                Email = request.Email,
                IdempotencyKey = idempotencyKey,
                ClienteId = clienteId.Value,
                PedidoId = pedidoId.Value,
                Valor = total ?? 0,
                TaxaAdesao = taxaAdesao ?? 0,
                Periodicidade = Periodicidade.Monthly,
                MetodoPagamento = MetodoPagamento.CreditCard,
                GalaxPayCustomerId = galaxPayCustomerId,
                GalaxPayCardId = galaxPayCardId,
                Observacao = $"{plano.Descricao} - Mensalidade"
            }, cancellationToken);

            return new ResultDto<VendaProcessingDto>
            {
                Status = true,
                StatusCode = StatusCodes.Status200OK,
                Mensagem = null,
                Dados = new VendaProcessingDto
                {
                    teste = "ok"
                }
            };
        }

        private (string mes, string ano) ExtrairMesAnoValidade(string dataValidade)
        {
            var partes = dataValidade.Split('/');
            if (partes.Length == 2)
            {
                var mes = partes[0];
                var ano = "20" + partes[1]; // Assumindo formato MM/YY
                return (mes, ano);
            }

            throw new ArgumentException("Formato de data de validade inválido. Use MM/YY");
        }

        private async Task<Guid?> ClienteAsync(
            HttpClient httpClient,
            VendaCreateDto request,
            VendaUsuarioDto? usuario,
            Guid? vendedorCupom,
            string? authorizationHeader,
            CancellationToken cancellationToken)
        {
            var trimmedEmail = request.Email.Trim();
            var encodedEmail = Uri.EscapeDataString(trimmedEmail);
            var buscaUrl = Utils.Combine(_apis.Cliente, $"/api/cliente/email?email={encodedEmail}");

            using (var getRequest = new HttpRequestMessage(HttpMethod.Get, buscaUrl))
            {
                Utils.AplicarAutorizacao(getRequest, authorizationHeader);
                getRequest.Headers.TryAddWithoutValidation("x-api-key", _apis.ClienteApiKey.Trim());
                using var getResponse = await httpClient.SendAsync(getRequest, cancellationToken);
                if (getResponse.IsSuccessStatusCode)
                {
                    var id = await Utils.LerGuidDoCorpoJsonAsync(getResponse, cancellationToken);
                    if (id != null)
                        return id;
                }
                else if (getResponse.StatusCode != System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }
            }

            Guid? vendedorId;
            if (usuario?.IsAdministrador == true)
                vendedorId = vendedorCupom;
            else if (usuario?.UsuarioId is { } uid && uid != Guid.Empty)
                vendedorId = uid;
            else
                vendedorId = vendedorCupom;

            var clienteCreateDto = new ClienteCreateDto
            {
                Nome = request.NomeCliente,
                Email = request.Email,
                TipoPessoa = request.TipoPessoa,
                CpfCnpj = request.CpfCnpj,
                Telefone = request.Telefone,
                Ativo = true,
                Senha = SenhaTemporariaClienteVenda,
                Confirmacao = SenhaTemporariaClienteVenda,
                NomeFantasia = request.NomeCliente,
                InscricaoEstadual = string.Empty,
                VendedorId = vendedorId
            };

            var rs = await _clienteCreateService.ExecutarAsync(clienteCreateDto, authorizationHeader, cancellationToken);

            if (!rs.Status) return null;

            return rs.Dados?.ClienteId;
        }
    }
}
