---
name: Idempotency-Key Pedido
overview: Encaminhar o header `Idempotency-Key` do BFF para a API de Pedido e tornar esse header obrigatório no endpoint `POST /api/pedido`, retornando 400 quando ausente.
todos:
  - id: bff-forward-idempotency-header
    content: Adicionar `Idempotency-Key` no `HttpRequestMessage` do `PedidoCreateService` usando `request.IdempotencyKey` e falhar com 400 quando ausente.
    status: completed
  - id: pedido-api-require-idempotency-header
    content: No `PedidoController.Criar`, retornar 400 se `Idempotency-Key` não vier (ou vier vazio) e remover o fluxo sem idempotência.
    status: completed
  - id: sanity-check
    content: "Rodar checagens básicas: compilar e validar manualmente 400 sem header e 201 com header."
    status: completed
isProject: false
---

## Contexto atual (achados)
- No BFF, `VendaBffController.Processar` já **exige** `Idempotency-Key` e valida que é um **GUID** (retorna 400 se faltar/inválido). Ele repassa o valor para `ProcessarVendaService`.
- `ProcessarVendaService` já coloca esse valor em `PedidoCreateDto.IdempotencyKey`, mas `PedidoCreateService` ainda **não adiciona** esse header na chamada HTTP para `Pedido.Api`.
- Na API de Pedido, `PedidoController.Criar` hoje trata `Idempotency-Key` como **opcional**: se não vier, ele cria pedido “sem idempotência”.

## Mudanças propostas
### BFF: enviar `Idempotency-Key` para Pedido.Api
- Alterar `[bff/Projeto.Moope.Gateways.Core/Services/Pedido/PedidoCreateService.cs](bff/Projeto.Moope.Gateways.Core/Services/Pedido/PedidoCreateService.cs)`:
  - Antes do `SendAsync`, adicionar o header `Idempotency-Key` em `pedidoRequest.Headers` usando `request.IdempotencyKey`.
  - Caso `request.IdempotencyKey` esteja nulo/vazio (defensivo), retornar `ResultDto<PedidoDetailDto>` com `Status=false` e `StatusCode=400` (evita mandar requisição que a API vai rejeitar).

### Pedido.Api: tornar `Idempotency-Key` obrigatório no `Criar`
- Alterar `[src/pedido/Projeto.Moope.Pedido.Api/Controllers/PedidoController.cs](src/pedido/Projeto.Moope.Pedido.Api/Controllers/PedidoController.cs)` no método `Criar([FromBody] PedidoCreateRequestDto dto)`:
  - Ler `Idempotency-Key` via `Request.Headers.TryGetValue("Idempotency-Key", out var idem)`.
  - Se **não existir** ou for **whitespace**, retornar `BadRequest` (mensagem simples).
  - Remover o fluxo “sem idempotência” (ou seja, sempre executar o bloco que usa `_idempotenciaService`).
  - Manter o tratamento já existente para `ChaveIdempotenteReutilizadaComPayloadDiferenteException`.

## Testes rápidos (manuais)
- Chamar `POST /api/pedido` **sem** `Idempotency-Key` → deve retornar **400**.
- Chamar `POST /api/pedido` **com** `Idempotency-Key` → deve processar normalmente (201/Conflict dependendo do cenário).
- Chamar `POST /api/bff/venda/processar` → confirmar que o BFF envia o header para o BC Pedido (por logs/debug) e que o fluxo segue funcionando.

## Arquivos principais
- `[bff/Projeto.Moope.Gateways.Core/Services/Pedido/PedidoCreateService.cs](bff/Projeto.Moope.Gateways.Core/Services/Pedido/PedidoCreateService.cs)`
- `[src/pedido/Projeto.Moope.Pedido.Api/Controllers/PedidoController.cs](src/pedido/Projeto.Moope.Pedido.Api/Controllers/PedidoController.cs)`
