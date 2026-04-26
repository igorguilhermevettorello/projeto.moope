---
name: Endpoint valores pagamento
overview: Adicionar no serviço de Pedido um endpoint que, dado o `pedidoId`, retorna os valores calculados para pagamento (taxa total e mensalidade total) e integrar o Worker do RabbitMQ para buscar esses dados antes de chamar o downstream de Pagamento, autenticando via client token.
todos:
  - id: pedido-endpoint
    content: Criar `GET /api/pedido/{id}/valores-pagamento` e DTO de resposta no projeto `Projeto.Moope.Pedido.Api`.
    status: completed
  - id: rabbitmq-config
    content: Adicionar `DownstreamApisOptions.Pedido` no RabbitMQ Core e garantir binding de config.
    status: completed
  - id: rabbitmq-client
    content: Implementar `PedidoValoresPagamentoQueryService` que autentica via `AuthClientTokenService` e consulta o endpoint do Pedido.
    status: completed
  - id: worker-integration
    content: Atualizar `Worker.cs` para enriquecer `PagamentoDto` (Valor=mensalidadeTotal, TaxaAdesao=valorTotalTaxaAdesao) e passar Authorization no pagamento.
    status: completed
  - id: smoke-test
    content: "Rodar smoke test manual: chamar endpoint e processar 1 mensagem RabbitMQ em dev."
    status: completed
isProject: false
---

## Escopo
- Criar um endpoint no serviço `Pedido` que recebe `pedidoId` e devolve:
  - `valorTotalTaxaAdesao` = `PlanoTaxaAdesao * Quantidade`
  - `mensalidade` = `PlanoValorComDesconto * Quantidade`
- Ajustar o RabbitMQ Worker para:
  - obter Bearer token via `AuthClientTokenService`
  - consultar o endpoint do Pedido para obter `mensalidade` e `valorTotalTaxaAdesao`
  - preencher `PagamentoDto.Valor` com **mensalidade** e `PagamentoDto.TaxaAdesao` com **taxa total**
  - então chamar `EfetuarPagamentoService` normalmente

## Mudanças no serviço de Pedido
- **API**: adicionar endpoint GET no controller existente.
  - Arquivo: [src/pedido/Projeto.Moope.Pedido.Api/Controllers/PedidoController.cs](src/pedido/Projeto.Moope.Pedido.Api/Controllers/PedidoController.cs)
  - Nova rota sugerida: `GET /api/pedido/{id:guid}/valores-pagamento`
  - Reutilizar o `_pedidoService.BuscarPorIdComDadosAsync(id)` já usado em `BuscarPorId`.
  - Calcular valores a partir do snapshot já persistido no `Pedido` (`PlanoTaxaAdesao`, `PlanoValorComDesconto`, `Quantidade`).
- **DTO de resposta**: criar um DTO específico para o consumo interno.
  - Arquivo novo (sugestão): `src/pedido/Projeto.Moope.Pedido.Api/DTOs/PedidoValoresPagamentoResponseDto.cs`
  - Campos:
    - `pedidoId: Guid`
    - `quantidade: int`
    - `taxaAdesaoUnitaria: decimal`
    - `valorTotalTaxaAdesao: decimal`
    - `mensalidadeUnitaria: decimal`
    - `mensalidadeTotal: decimal`
- **Contratos/erros**:
  - `400` se `id` vazio
  - `404` se pedido não encontrado
  - `200` com payload calculado

## Mudanças no RabbitMQ
- **Config**: estender `DownstreamApisOptions` para conter a URL base do serviço de Pedido.
  - Arquivo: [src/rabbitmq/Projeto.Moope.RabbitMQ.Core/Options/DownstreamApisOptions.cs](src/rabbitmq/Projeto.Moope.RabbitMQ.Core/Options/DownstreamApisOptions.cs)
  - Adicionar `public string? Pedido { get; init; }`
  - Esperar `DownstreamApis:Pedido` em configuração (env var / json quando existir).
- **Novo client de consulta**: criar service no RabbitMQ Core para buscar os valores.
  - Arquivos novos (sugestão):
    - `src/rabbitmq/Projeto.Moope.RabbitMQ.Core/Interfaces/Services/IPedidoValoresPagamentoQueryService.cs`
    - `src/rabbitmq/Projeto.Moope.RabbitMQ.Core/Services/PedidoValoresPagamentoQueryService.cs`
  - Implementação:
    - Montar URL `GET {apis.Pedido}/api/pedido/{pedidoId}/valores-pagamento`
    - Obter token via `IAuthClientTokenService.GetClientAccessTokenAsync`
    - Enviar header `Authorization: Bearer <token>`
    - Desserializar e retornar via `ResultDto<...>` (sem retornar `null`)
- **Worker flow**: atualizar o processamento da mensagem para enriquecer `PagamentoDto` antes do pagamento.
  - Arquivo: [src/rabbitmq/Projeto.Moope.RabbitMQ.Worker/Worker.cs](src/rabbitmq/Projeto.Moope.RabbitMQ.Worker/Worker.cs)
  - Passos:
    - Após desserializar `PagamentoDto`, chamar `PedidoValoresPagamentoQueryService`
    - Setar `dto.Valor = mensalidadeTotal` e `dto.TaxaAdesao = valorTotalTaxaAdesao`
    - (Opcional) logar valores retornados para auditoria
    - Seguir com `efetuarPagamentoService.EfetuarPagamento(dto, authorizationHeader: bearer, ...)`
- **DI**: registrar o novo serviço no Worker `Program.cs`.
  - Arquivo: [src/rabbitmq/Projeto.Moope.RabbitMQ.Worker/Program.cs](src/rabbitmq/Projeto.Moope.RabbitMQ.Worker/Program.cs)

## Test plan (prático)
- Executar o serviço de `Pedido` e chamar `GET /api/pedido/{id}/valores-pagamento` com um pedido existente; validar:
  - `valorTotalTaxaAdesao == PlanoTaxaAdesao * Quantidade`
  - `mensalidadeTotal == PlanoValorComDesconto * Quantidade`
- Rodar o Worker localmente com `DownstreamApis:Auth`, `AuthClientId/Secret`, `Pedido`, `Pagamento` configurados e publicar mensagem na fila `vendas` contendo ao menos `pedidoId` e dados do cliente/cartão; verificar logs e chamada ao serviço de Pagamento.

## Observações de arquitetura
- O endpoint usa **snapshot** persistido no `Pedido` (evita divergência caso o plano mude após a venda).
- O RabbitMQ continua responsável apenas por orquestrar chamadas externas; o cálculo fica no bounded-context de Pedido.
