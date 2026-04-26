---
name: Propagar Idempotency-Key pagamento
overview: Garantir que o header `Idempotency-Key` recebido no BFF seja propagado até o `Pagamento.Api` via mensagem RabbitMQ e `EfetuarPagamentoService`, habilitando idempotência no fluxo de pagamento (principalmente no endpoint `assinaturas/sem-plano-com-taxa`).
todos:
  - id: dto-fila
    content: Adicionar `IdempotencyKey` em `bff/.../DTOs/Venda/VendaQueueDto.cs` e `src/rabbitmq/.../DTOs/PagamentoDto.cs` (mantendo camelCase no JSON via serializer já usado).
    status: completed
  - id: publicar-chave
    content: Em `bff/.../Services/ProcessarVendaService.cs`, preencher `IdempotencyKey = idempotencyKey` ao montar o `VendaQueueDto` enviado ao `IVendaSendQueueService`.
    status: completed
  - id: preservar-no-worker
    content: Em `src/rabbitmq/.../Worker.cs`, ao recriar o `PagamentoDto` com valores vindos do Pedido, copiar também `IdempotencyKey` do DTO original.
    status: completed
  - id: enviar-header
    content: Em `src/rabbitmq/.../Services/EfetuarPagamentoService.cs`, adicionar `httpRequest.Headers.TryAddWithoutValidation("Idempotency-Key", request.IdempotencyKey)` (validando não-vazio) e, se necessário, ajustar a interface `IEfetuarPagamentoService` apenas se você preferir não acoplar ao DTO.
    status: completed
  - id: validar-end-to-end
    content: Rodar o worker e o Pagamento.Api localmente e confirmar via logs/inspeção que `Idempotency-Key` chega no endpoint correto (`/assinaturas/sem-plano-com-taxa`) e que duplicações não ocorrem em retry.
    status: completed
isProject: false
---

### Diagnóstico (estado atual)
- O `RabbitMQ.Worker` consome mensagens da fila `vendas` e desserializa em `PagamentoDto`, mas esse DTO **não possui** `IdempotencyKey`.
- O [`EfetuarPagamentoService`](src/rabbitmq/Projeto.Moope.RabbitMQ.Core/Services/EfetuarPagamentoService.cs) cria um `HttpRequestMessage` e **só adiciona** `Authorization` (não envia `Idempotency-Key`).
- O [`PagamentoController`](src/pagamento/Projeto.Moope.Pagamento.Api/Controllers/PagamentoController.cs) lê `Request.Headers["Idempotency-Key"]` e:
  - em `POST /api/pagamento/assinaturas/sem-plano` trata como **opcional**;
  - em `POST /api/pagamento/assinaturas/sem-plano-com-taxa` é **obrigatório**.
- O BFF já exige o header `Idempotency-Key` no [`VendaBffController`](bff/Projeto.Moope.Gateways.Api/Controllers/VendaBffController.cs), mas ao publicar na fila ele **não serializa** essa chave (ver [`VendaQueueDto`](bff/Projeto.Moope.Gateways.Core/DTOs/Venda/VendaQueueDto.cs)).

### Abordagem recomendada
Propagar a mesma `Idempotency-Key` do request do BFF até o Pagamento:
1) **BFF** inclui a chave no payload publicado na fila.
2) **RabbitMQ.Worker** preserva a chave ao recalcular `Valor/TaxaAdesao`.
3) **EfetuarPagamentoService** adiciona o header `Idempotency-Key` na chamada HTTP ao `Pagamento.Api`.

### Pontos de mudança (arquitetura/contratos)
- Ajustar o contrato da mensagem (DTO) de fila adicionando `IdempotencyKey`.
  - Isso é o mínimo necessário para o worker reenviar a mesma chave em retries, garantindo idempotência real.

### Teste/validação (manual)
- Publicar uma mensagem na fila com `idempotencyKey` e `taxaAdesao > 0` e validar que:
  - a requisição ao `Pagamento.Api` contém `Idempotency-Key`.
  - retries do worker não geram cobranças duplicadas e o `Pagamento.Api` devolve resposta idempotente.
