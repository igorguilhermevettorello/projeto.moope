using Microsoft.AspNetCore.Mvc;
using Projeto.Moope.Api.Controllers;
using Projeto.Moope.Core.Interfaces.Identity;
using Projeto.Moope.Core.Interfaces.Notificacao;
using Projeto.Moope.Pagamento.Api.DTOs;
using Projeto.Moope.Pagamento.Core.DTOs;
using Projeto.Moope.Pagamento.Core.Excecoes.Idempotencia;
using Projeto.Moope.Pagamento.Core.Interfaces.Services;
using Projeto.Moope.Pagamento.Core.Services.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Projeto.Moope.Pagamento.Api.Controllers
{
    [ApiController]
    [Route("api/pagamento")]
    public class PagamentoController : MainController
    {
        private readonly IPagamentoService _pagamentoService;
        private readonly IIdempotenciaService _idempotenciaService;
        private readonly IGeradorHashRequisicao _geradorHashRequisicao;

        public PagamentoController(
            IPagamentoService pagamentoService,
            IIdempotenciaService idempotenciaService,
            IGeradorHashRequisicao geradorHashRequisicao,
            INotificador notificador,
            IUser appUser)
            : base(notificador, appUser)
        {
            _pagamentoService = pagamentoService;
            _idempotenciaService = idempotenciaService;
            _geradorHashRequisicao = geradorHashRequisicao;
        }

        [HttpPost("gateway/token")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> AutenticarGateway([FromBody] GatewayAuthRequestDto dto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return CustomResponse(ModelState);

            if (string.IsNullOrWhiteSpace(dto.Scope))
            {
                ModelState.AddModelError("Scope", "Scope é obrigatório.");
                return CustomResponse(ModelState);
            }

            var result = await _pagamentoService.AutenticarGatewayAsync(dto.Scope, cancellationToken);
            if (!result.Status)
                return CustomResponse(result);

            return Ok(result.Dados);
        }

        [HttpPost("clientes")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CriarCliente([FromBody] CriarClienteRequestDto dto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return CustomResponse(ModelState);

            var obj = new
            {
                myId = dto.ClienteId,
                name = dto.Name,
                emails = new[] { dto.Email },
                document =  dto.Document
            };

            var request = new CriarClienteGatewayRequestDto(
                Scope: "customers.write",
                Payload: obj,
                ClienteId: dto.ClienteId);

            var result = await _pagamentoService.CriarClienteAsync(request, cancellationToken);
            if (!result.Status)
                return CustomResponse(result);

            return Ok(result.Dados);
        }

        [HttpGet("clientes")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarClientes(CancellationToken cancellationToken)
        {
            var query = ToQueryDictionary();
            var result = await _pagamentoService.ListarClientesAsync(query, cancellationToken);
            if (!result.Status)
                return CustomResponse(result);

            return Ok(result.Dados);
        }

        [HttpPost("planos")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CriarPlano([FromBody] CriarPlanoRequestDto dto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return CustomResponse(ModelState);

            var request = new CriarPlanoGatewayRequestDto(
                Scope: "plans.write",
                Payload: dto.Payload);

            var result = await _pagamentoService.CriarPlanoAsync(request, cancellationToken);
            if (!result.Status)
                return CustomResponse(result);

            return Ok(result.Dados);
        }

        [HttpGet("planos")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ListarPlanos(CancellationToken cancellationToken)
        {
            var query = ToQueryDictionary();
            var result = await _pagamentoService.ListarPlanosAsync(query, cancellationToken);
            if (!result.Status)
                return CustomResponse(result);

            return Ok(result.Dados);
        }

        [HttpPost("cartoes")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CriarCartao([FromBody] CriarCartaoRequestDto dto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return CustomResponse(ModelState);

            if (string.IsNullOrWhiteSpace(dto.CustomerId))
            {
                ModelState.AddModelError("CustomerId", "CustomerId é obrigatório.");
                return CustomResponse(ModelState);
            }

            var obj = new
            {
                number = dto.Number,
                holder = dto.Holder,
                expiresAt = dto.ExpiresAt,
                cvv = dto.Cvv
            };

            var request = new CriarCartaoGatewayRequestDto(
                Scope: "cards.write",
                CustomerId: dto.CustomerId,
                TypeId: string.IsNullOrWhiteSpace(dto.TypeId) ? "galaxPayId" : dto.TypeId,
                Payload: obj);

            var result = await _pagamentoService.CriarCartaoAsync(request, cancellationToken);
            if (!result.Status)
                return CustomResponse(result);

            return Ok(result.Dados);
        }

        [HttpPost("assinaturas/com-plano")]
        public async Task<IActionResult> CriarAssinaturaComPlano([FromBody] CriarAssinaturaRequestDto dto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return CustomResponse(ModelState);

            var request = new CriarAssinaturaComPlanoGatewayRequestDto("subscriptions.write", dto.Payload);
            var result = await _pagamentoService.CriarAssinaturaComPlanoAsync(request, cancellationToken);
            if (!result.Status)
                return CustomResponse(result);

            return Ok(result.Dados);
        }

        [HttpPost("assinaturas/sem-plano")]
        public async Task<IActionResult> CriarAssinaturaSemPlano([FromBody] CriarAssinaturaRequestDto dto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return CustomResponse(ModelState);

            if (dto.ClienteId == Guid.Empty)
            {
                ModelState.AddModelError("ClienteId", "ClienteId é obrigatório.");
                return CustomResponse(ModelState);
            }

            var idempotencyKey = Request.Headers["Idempotency-Key"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(idempotencyKey))
            {
                ModelState.AddModelError("Idempotency-Key", "Idempotency-Key é obrigatório.");
                return CustomResponse(ModelState);
            }

            var scope = "Pagamento:CriarAssinaturaSemPlano";
            var requestHash = _geradorHashRequisicao.GerarHash(dto);

            try
            {
                var inicio = await _idempotenciaService.IniciarProcessamentoAsync(
                    idempotencyKey,
                    scope,
                    requestHash,
                    cancellationToken);

                if (inicio.JaConcluido && inicio.ResponseStatusCode.HasValue && !string.IsNullOrWhiteSpace(inicio.ResponseBody))
                {
                    return new ContentResult
                    {
                        StatusCode = inicio.ResponseStatusCode.Value,
                        ContentType = "application/json",
                        Content = inicio.ResponseBody
                    };
                }

                if (!inicio.DeveProcessar)
                    return Conflict("Uma solicitação com a mesma chave de idempotência já está em processamento.");

                var payloadIdempotente = new CelPayAssinaturaSemPlanoRequestDto
                {
                    MyId = dto.PedidoId.ToString(),
                    Value = (int)(dto.Valor * 100),
                    Quantity = 0,
                    Periodicity = dto.Periodicidade,
                    FirstPayDayDate = DateTime.Now.ToString("yyyy-MM-dd"),
                    MainPaymentMethodId = dto.MetodoPagamento,
                    AdditionalInfo = dto.Observacao,
                    Customer = new CelPayCustomerDto
                    {
                        GalaxPayId = dto.GalaxPayCustomerId,
                        Name = dto.Name,
                        Emails = new List<string> { dto.Email }
                    },
                    Card = new CelPayCardDto
                    {
                        GalaxPayId = dto.GalaxPayCardId
                    }

                };

                var requestIdempotente = new CriarAssinaturaSemPlanoGatewayRequestDto("subscriptions.write", payloadIdempotente);
                var resultIdempotente = await _pagamentoService.CriarAssinaturaSemPlanoAsync(requestIdempotente, cancellationToken);
                if (!resultIdempotente.Status)
                {
                    await _idempotenciaService.MarcarFalhaAsync(inicio.IdempotenciaId, cancellationToken);
                    return CustomResponse(resultIdempotente);
                }

                var galaxPayId = TryExtractGalaxPayId(resultIdempotente.Dados);
                if (string.IsNullOrWhiteSpace(galaxPayId))
                {
                    await _idempotenciaService.MarcarFalhaAsync(inicio.IdempotenciaId, cancellationToken);
                    NotificarErro("Gateway", "Gateway não retornou galaxPayId para a assinatura.");
                    return CustomResponse();
                }

                var persistRs = await _pagamentoService.RegistrarPagamentoAssinaturaSemPlanoAsync(
                    dto.ClienteId,
                    dto.GalaxPayCustomerId.ToString(),
                    galaxPayId,
                    gatewayPlanId: null,
                    cancellationToken);

                if (!persistRs.Status)
                {
                    await _idempotenciaService.MarcarFalhaAsync(inicio.IdempotenciaId, cancellationToken);
                    return CustomResponse(persistRs);
                }

                var responseBody = new
                {
                    galaxPayId,
                    gatewayResponse = resultIdempotente.Dados
                };

                var responseJson = JsonSerializer.Serialize(responseBody);
                await _idempotenciaService.ConcluirAsync(
                    inicio.IdempotenciaId,
                    StatusCodes.Status200OK,
                    responseJson,
                    resourceId: dto.PedidoId.ToString(),
                    resourceType: "AssinaturaSemPlano",
                    cancellationToken);

                return Ok(responseBody);
            }
            catch (ChaveIdempotenteReutilizadaComPayloadDiferenteException ex)
            {
                NotificarErro("Idempotencia", ex.Message);
                return CustomResponse();
            }
        }

        [HttpPost("assinaturas/sem-plano-com-taxa")]
        public async Task<IActionResult> CriarAssinaturaSemPlanoComTaxa([FromBody] CriarAssinaturaRequestDto dto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return CustomResponse(ModelState);

            if (dto.ClienteId == Guid.Empty)
            {
                ModelState.AddModelError("ClienteId", "ClienteId é obrigatório.");
                return CustomResponse(ModelState);
            }

            var idempotencyKey = Request.Headers["Idempotency-Key"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(idempotencyKey))
            {
                ModelState.AddModelError("Idempotency-Key", "Idempotency-Key é obrigatório.");
                return CustomResponse(ModelState);
            }

            var scope = "Pagamento:CriarAssinaturaSemPlanoComTaxa";
            var requestHash = _geradorHashRequisicao.GerarHash(dto);

            try
            {
                var inicio = await _idempotenciaService.IniciarProcessamentoAsync(
                    idempotencyKey,
                    scope,
                    requestHash,
                    cancellationToken);

                if (inicio.JaConcluido && inicio.ResponseStatusCode.HasValue && !string.IsNullOrWhiteSpace(inicio.ResponseBody))
                {
                    return new ContentResult
                    {
                        StatusCode = inicio.ResponseStatusCode.Value,
                        ContentType = "application/json",
                        Content = inicio.ResponseBody
                    };
                }

                if (!inicio.DeveProcessar)
                    return Conflict("Uma solicitação com a mesma chave de idempotência já está em processamento.");

                var payloadIdempotente = new CelPayAssinaturaSemPlanoRequestDto
                {
                    MyId = dto.PedidoId.ToString(),
                    Value = (int)(dto.Valor * 100),
                    Quantity = 0,
                    Periodicity = dto.Periodicidade,
                    FirstPayDayDate = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd"),
                    MainPaymentMethodId = dto.MetodoPagamento,
                    AdditionalInfo = dto.Observacao,
                    Customer = new CelPayCustomerDto
                    {
                        GalaxPayId = dto.GalaxPayCustomerId,
                        Name = dto.Name,
                        Emails = new List<string> { dto.Email }
                    },
                    Card = new CelPayCardDto
                    {
                        GalaxPayId = dto.GalaxPayCardId
                    }

                };

                payloadIdempotente.Transactions = new List<CelPayTransactionDto>
                {
                    new CelPayTransactionDto
                    {
                        Installment = 1,
                        Value = (int) (dto.TaxaAdesao * 100),
                        Payday = DateTime.Now.ToString("yyyy-MM-dd"),
                        PayedOutsideGalaxPay = false,
                        AdditionalInfo = $"{dto.Observacao} - Taxa de adesão"
                    },
                };

                var requestIdempotente = new CriarAssinaturaSemPlanoGatewayRequestDto("subscriptions.write", payloadIdempotente);
                var resultIdempotente = await _pagamentoService.CriarAssinaturaSemPlanoAsync(requestIdempotente, cancellationToken);
                if (!resultIdempotente.Status)
                {
                    await _idempotenciaService.MarcarFalhaAsync(inicio.IdempotenciaId, cancellationToken);
                    return CustomResponse(resultIdempotente);
                }

                var galaxPayId = TryExtractGalaxPayId(resultIdempotente.Dados);
                if (string.IsNullOrWhiteSpace(galaxPayId))
                {
                    await _idempotenciaService.MarcarFalhaAsync(inicio.IdempotenciaId, cancellationToken);
                    NotificarErro("Gateway", "Gateway não retornou galaxPayId para a assinatura.");
                    return CustomResponse();
                }

                var persistRs = await _pagamentoService.RegistrarPagamentoAssinaturaSemPlanoAsync(
                    dto.ClienteId,
                    dto.GalaxPayCustomerId.ToString(),
                    galaxPayId,
                    gatewayPlanId: null,
                    cancellationToken);

                if (!persistRs.Status)
                {
                    await _idempotenciaService.MarcarFalhaAsync(inicio.IdempotenciaId, cancellationToken);
                    return CustomResponse(persistRs);
                }

                var responseBody = new
                {
                    galaxPayId,
                    gatewayResponse = resultIdempotente.Dados
                };

                var responseJson = JsonSerializer.Serialize(responseBody);
                await _idempotenciaService.ConcluirAsync(
                    inicio.IdempotenciaId,
                    StatusCodes.Status200OK,
                    responseJson,
                    resourceId: dto.PedidoId.ToString(),
                    resourceType: "AssinaturaSemPlano",
                    cancellationToken);

                return Ok(responseBody);
            }
            catch (ChaveIdempotenteReutilizadaComPayloadDiferenteException ex)
            {
                NotificarErro("Idempotencia", ex.Message);
                return CustomResponse();
            }
        }

        private static string? TryExtractGalaxPayId(JsonElement json)
        {
            var candidates = new[] { "galaxPayId", "id", "subscriptionId" };
            return TryFindFirstPropertyValue(json, candidates, maxDepth: 10);
        }

        private static string? TryFindFirstPropertyValue(JsonElement element, IReadOnlyList<string> candidatePropertyNames, int maxDepth)
        {
            if (maxDepth < 0)
                return null;

            static string? AsNonEmptyString(JsonElement value)
            {
                if (value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
                    return null;

                var s = value.ValueKind == JsonValueKind.String ? value.GetString() : value.ToString();
                return string.IsNullOrWhiteSpace(s) ? null : s;
            }

            static bool TryGetPropertyIgnoreCase(JsonElement obj, string name, out JsonElement value)
            {
                foreach (var prop in obj.EnumerateObject())
                {
                    if (string.Equals(prop.Name, name, StringComparison.OrdinalIgnoreCase))
                    {
                        value = prop.Value;
                        return true;
                    }
                }

                value = default;
                return false;
            }

            if (element.ValueKind == JsonValueKind.Object)
            {
                foreach (var candidate in candidatePropertyNames)
                {
                    if (TryGetPropertyIgnoreCase(element, candidate, out var value))
                    {
                        var s = AsNonEmptyString(value);
                        if (!string.IsNullOrWhiteSpace(s))
                            return s;
                    }
                }

                foreach (var prop in element.EnumerateObject())
                {
                    var nested = TryFindFirstPropertyValue(prop.Value, candidatePropertyNames, maxDepth - 1);
                    if (!string.IsNullOrWhiteSpace(nested))
                        return nested;
                }
            }
            else if (element.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in element.EnumerateArray())
                {
                    var nested = TryFindFirstPropertyValue(item, candidatePropertyNames, maxDepth - 1);
                    if (!string.IsNullOrWhiteSpace(nested))
                        return nested;
                }
            }

            return null;
        }

        [HttpPost("assinaturas/manual")]
        public async Task<IActionResult> CriarAssinaturaManual([FromBody] CriarAssinaturaManualRequestDto dto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return CustomResponse(ModelState);

            var request = new CriarAssinaturaManualGatewayRequestDto("subscriptions.write", dto.Payload);
            var result = await _pagamentoService.CriarAssinaturaManualAsync(request, cancellationToken);
            if (!result.Status)
                return CustomResponse(result);

            return Ok(result.Dados);
        }

        [HttpPost("assinaturas/{subscriptionId}/transacoes")]
        public async Task<IActionResult> AdicionarTransacaoEmAssinatura(
            string subscriptionId,
            [FromBody] AdicionarTransacaoEmAssinaturaRequestDto dto,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return CustomResponse(ModelState);

            if (string.IsNullOrWhiteSpace(subscriptionId))
            {
                ModelState.AddModelError("SubscriptionId", "SubscriptionId é obrigatório.");
                return CustomResponse(ModelState);
            }

            var request = new AdicionarTransacaoEmAssinaturaGatewayRequestDto(
                Scope: "subscriptions.write",
                SubscriptionId: subscriptionId,
                TypeId: string.IsNullOrWhiteSpace(dto.TypeId) ? "galaxPayId" : dto.TypeId,
                Payload: dto.Payload);

            var result = await _pagamentoService.AdicionarTransacaoEmAssinaturaAsync(request, cancellationToken);
            if (!result.Status)
                return CustomResponse(result);

            return Ok(result.Dados);
        }

        [HttpGet("transacoes")]
        public async Task<IActionResult> ListarTransacoes(CancellationToken cancellationToken)
        {
            var query = ToQueryDictionary();
            var result = await _pagamentoService.ListarTransacoesAsync(query, cancellationToken);
            if (!result.Status)
                return CustomResponse(result);

            return Ok(result.Dados);
        }

        [HttpDelete("transacoes/{transactionId}")]
        public async Task<IActionResult> CancelarTransacao(string transactionId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(transactionId))
            {
                ModelState.AddModelError("TransactionId", "TransactionId é obrigatório.");
                return CustomResponse(ModelState);
            }

            var result = await _pagamentoService.CancelarTransacaoAsync(transactionId, cancellationToken);
            if (!result.Status)
                return CustomResponse(result);

            return Ok(result);
        }

        [HttpPost("cobrancas-avulsas")]
        public async Task<IActionResult> CriarCobrancaAvulsa([FromBody] CriarCobrancaAvulsaRequestDto dto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return CustomResponse(ModelState);

            var request = new CriarCobrancaAvulsaGatewayRequestDto("charges.write", dto.Payload);
            var result = await _pagamentoService.CriarCobrancaAvulsaAsync(request, cancellationToken);
            if (!result.Status)
                return CustomResponse(result);

            return Ok(result.Dados);
        }

        [HttpGet("cobrancas-avulsas")]
        public async Task<IActionResult> ListarCobrancasAvulsas(CancellationToken cancellationToken)
        {
            var query = ToQueryDictionary();
            var result = await _pagamentoService.ListarCobrancasAvulsasAsync(query, cancellationToken);
            if (!result.Status)
                return CustomResponse(result);

            return Ok(result.Dados);
        }

        private Dictionary<string, string?> ToQueryDictionary()
        {
            return Request.Query.ToDictionary(k => k.Key, v => (string?)v.Value.ToString());
        }
    }
}
