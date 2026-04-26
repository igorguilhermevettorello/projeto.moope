---
name: Registrar pagamento sem plano
overview: Ao criar assinatura sem plano (com ou sem taxa), persistir um registro em `Pagamento` e retornar explicitamente o `GalaxPayId` (usado como `GatewaySubscriptionId`/GatewaySubscriptionId) para consumo do BFF, mantendo idempotência e Clean Architecture.
todos:
  - id: add-clienteid-dtos
    content: Adicionar `ClienteId` em `CriarAssinaturaRequestDto`, `VendaQueueDto` e `PagamentoDto`, ajustando produtor/consumer para preencher e transportar o campo.
    status: completed
  - id: persist-pagamento-on-success
    content: Após sucesso no gateway em `CriarAssinaturaSemPlano` e `CriarAssinaturaSemPlanoComTaxa`, extrair `galaxPayId`, persistir `Pagamento` via service/repository e retornar wrapper `{ galaxPayId, gatewayResponse }`.
    status: completed
  - id: remove-unique-indexes
    content: Alterar `PagamentoMapping` para permitir histórico (remover índices únicos) e criar migration correspondente.
    status: completed
isProject: false
---

## Contexto atual (o que já existe)
- Os endpoints `POST /api/pagamento/assinaturas/sem-plano` e `/sem-plano-com-taxa` ([`src/pagamento/Projeto.Moope.Pagamento.Api/Controllers/PagamentoController.cs`](src/pagamento/Projeto.Moope.Pagamento.Api/Controllers/PagamentoController.cs)) chamam `IPagamentoService.CriarAssinaturaSemPlanoAsync(...)` e retornam o JSON do gateway, mas **não registram** nada na tabela `Pagamento`.
- Já existe persistência para `Pagamento`: entidade [`src/pagamento/Projeto.Moope.Pagamento.Core/Models/Pagamento.cs`](src/pagamento/Projeto.Moope.Pagamento.Core/Models/Pagamento.cs), repositório [`src/pagamento/Projeto.Moope.Pagamento.Infrastructure/Repositories/PagamentoRepository.cs`](src/pagamento/Projeto.Moope.Pagamento.Infrastructure/Repositories/PagamentoRepository.cs) e EF mapping [`src/pagamento/Projeto.Moope.Pagamento.Infrastructure/Mapping/PagamentoMapping.cs`](src/pagamento/Projeto.Moope.Pagamento.Infrastructure/Mapping/PagamentoMapping.cs).
- O gateway retorna um JSON bruto (via [`CelcoinPaymentGatewayClient`](src/pagamento/Projeto.Moope.Pagamento.Infrastructure/Services/CelcoinPaymentGatewayClient.cs)); hoje a extração de `galaxPayId` existe no `PagamentoService` só para **cliente/cartão**, não para assinatura.

## Decisões já confirmadas
- Você quer **adicionar `ClienteId` no request** (em vez de buscar por `PedidoId`).
- Você quer **criar um novo registro de `Pagamento` a cada assinatura/pagamento** (histórico), não atualizar um único registro.
- Você quer o retorno em formato **wrapper**: `{ galaxPayId: "...", gatewayResponse: <jsonOriginal> }`.

## Mudanças necessárias
### 1) Propagar `ClienteId` até a API de Pagamento
- Adicionar `ClienteId` em:
  - [`src/pagamento/Projeto.Moope.Pagamento.Api/DTOs/GatewayRequests.cs`](src/pagamento/Projeto.Moope.Pagamento.Api/DTOs/GatewayRequests.cs) (`CriarAssinaturaRequestDto`)
  - [`bff/Projeto.Moope.Gateways.Core/DTOs/Venda/VendaQueueDto.cs`](bff/Projeto.Moope.Gateways.Core/DTOs/Venda/VendaQueueDto.cs)
  - [`src/rabbitmq/Projeto.Moope.RabbitMQ.Core/DTOs/PagamentoDto.cs`](src/rabbitmq/Projeto.Moope.RabbitMQ.Core/DTOs/PagamentoDto.cs)
- Ajustar o produtor da fila para preencher o `ClienteId`:
  - [`bff/Projeto.Moope.Gateways.Core/Services/ProcessarVendaService.cs`](bff/Projeto.Moope.Gateways.Core/Services/ProcessarVendaService.cs) já possui método `ClienteAsync(...)` que retorna `Guid?` — usar o valor retornado e colocar no `VendaQueueDto`.
- Ajustar o consumer (worker) para carregar `ClienteId` do payload e repassar para a chamada HTTP:
  - [`src/rabbitmq/Projeto.Moope.RabbitMQ.Worker/Worker.cs`](src/rabbitmq/Projeto.Moope.RabbitMQ.Worker/Worker.cs)
  - [`src/rabbitmq/Projeto.Moope.RabbitMQ.Core/Services/EfetuarPagamentoService.cs`](src/rabbitmq/Projeto.Moope.RabbitMQ.Core/Services/EfetuarPagamentoService.cs) (body enviado deve conter `ClienteId` no mesmo nível dos demais campos).

### 2) Persistir `Pagamento` após sucesso no gateway (mantendo idempotência)
- Implementar no Core (Clean Architecture):
  - Criar um método no `IPagamentoService` (ex.: `RegistrarPagamentoAssinaturaSemPlanoAsync(...)`) que receba `clienteId`, `gatewayCustomerId`, `gatewaySubscriptionId` e grave via `IPagamentoRepository.SalvarAsync`.
  - Alternativamente, manter a responsabilidade no `PagamentoService.CriarAssinaturaSemPlanoAsync` criando um overload/novo método que receba também metadados do `CriarAssinaturaRequestDto`.
- No controller, no fluxo “processa de verdade” (quando `inicio.DeveProcessar`), após `resultIdempotente.Status == true`:
  - Extrair `galaxPayId` do JSON do gateway (subscription id) com o mesmo algoritmo de busca genérica (hoje existe `TryGetFirstId(...)` no `PagamentoService`; pode ser movido para um helper compartilhado ou replicado de forma controlada).
  - Chamar o service de persistência para gravar:
    - `ClienteId = dto.ClienteId`
    - `GatewayCustomerId = dto.GalaxPayCustomerId.ToString()`
    - `GatewaySubscriptionId = galaxPayId`
    - `GatewayPlanId = null`
    - `Created/Updated = DateTime.UtcNow`
  - Montar e retornar o wrapper: `{ galaxPayId, gatewayResponse = resultIdempotente.Dados }`.
- No fluxo `inicio.JaConcluido`, continua retornando o `ResponseBody` cacheado (não duplica insert), preservando idempotência.

### 3) Ajustar a modelagem para suportar histórico (remover unicidade)
Hoje o mapping tem:
- `builder.HasIndex(x => x.ClienteId).IsUnique();`
- `builder.HasIndex(x => x.GatewayCustomerId).IsUnique();`

Como você quer **um registro por pagamento/assinatura**, vamos:
- Remover `IsUnique()` desses índices (ou substituir por índices não únicos).
- Criar uma nova migration no projeto Infrastructure de Pagamento removendo as constraints únicas no banco.

### 4) Teste rápido e validações
- Testar manualmente os dois endpoints:
  - `POST /api/pagamento/assinaturas/sem-plano`
  - `POST /api/pagamento/assinaturas/sem-plano-com-taxa`
- Validar:
  - resposta contém `galaxPayId` e `gatewayResponse`.
  - registro novo em `Pagamento` com `GatewaySubscriptionId == galaxPayId`.
  - reenvio com mesmo `Idempotency-Key` retorna resposta cacheada e **não** cria novo registro.
