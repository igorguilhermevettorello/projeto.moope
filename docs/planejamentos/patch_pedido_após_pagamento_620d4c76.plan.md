---
name: Patch pedido após pagamento
overview: Após efetuar pagamento com sucesso, extrair somente `galaxPayId` do JSON de resposta e chamar o endpoint PATCH do serviço Pedido para atualizar `Pedido.GalaxPayId`. Se o PATCH falhar, tratar como falha para requeue/retry.
todos: []
isProject: false
---

## Objetivo
No fluxo do worker RabbitMQ, quando a chamada ao serviço de Pagamento retornar sucesso com body no formato:
```json
{ "galaxPayId": "157370", "gatewayResponse": {} }
```
extrair **apenas** o `galaxPayId` e disparar uma requisição ao serviço `Pedido` no endpoint:
- `PATCH /api/pedido/{id:guid}/galaxpayid/{galaxPayId:int}`

## Onde implementar
Implementar dentro de [`src/rabbitmq/Projeto.Moope.RabbitMQ.Core/Services/EfetuarPagamentoService.cs`](src/rabbitmq/Projeto.Moope.RabbitMQ.Core/Services/EfetuarPagamentoService.cs), porque:
- já possui `DownstreamApisOptions apis` (inclui `apis.Pedido`)
- já recebe `authorizationHeader`
- você quer que **se o PATCH falhar, o worker retente** (logo, `EfetuarPagamento` deve retornar `Status=false` quando o PATCH falhar)

## Mudanças propostas
### 1) Extrair `galaxPayId` do `responseBody`
Em `EfetuarPagamentoService.EfetuarPagamento(...)`, depois de `response.IsSuccessStatusCode`:
- fazer parse do `responseBody` usando `System.Text.Json` e ler a propriedade `galaxPayId`
- aceitar `galaxPayId` vindo como string e converter para `int`
- se não existir / não converter: retornar `ResultDto { Status=false, StatusCode=502, Mensagem="Resposta inesperada: galaxPayId não retornado" }`

### 2) Chamar PATCH no serviço Pedido
Ainda no mesmo fluxo de sucesso do pagamento:
- validar `apis.Pedido` configurado
- montar URL: `UrlHelper.Combine(apis.Pedido, $"/api/pedido/{request.PedidoId}/galaxpayid/{galaxPayId}")`
- enviar `HttpMethod.Patch` sem body
- incluir header `Authorization` (igual ao `PedidoValoresPagamentoQueryService` faz)
- se status != 2xx: retornar `ResultDto { Status=false, StatusCode=(int)response.StatusCode, Mensagem=responseBodyDoPatch }`

### 3) Retorno do `EfetuarPagamento`
- **Só retornar `Status=true`** quando:
  - pagamento (serviço Pagamento) foi 2xx
  - PATCH no Pedido foi 2xx
- Caso PATCH falhe: `Status=false` para o worker entrar no retry/requeue (conforme sua decisão)

## Arquivos envolvidos
- [`src/rabbitmq/Projeto.Moope.RabbitMQ.Core/Services/EfetuarPagamentoService.cs`](src/rabbitmq/Projeto.Moope.RabbitMQ.Core/Services/EfetuarPagamentoService.cs)
- (sem mudanças necessárias) [`src/rabbitmq/Projeto.Moope.RabbitMQ.Core/Options/DownstreamApisOptions.cs`](src/rabbitmq/Projeto.Moope.RabbitMQ.Core/Options/DownstreamApisOptions.cs) já tem `Pedido`

## Teste manual sugerido
- Publicar uma mensagem de pagamento com `PedidoId` válido e `Authorization` válido.
- Confirmar:
  - quando Pagamento retorna `{ galaxPayId, gatewayResponse }`, o worker chama PATCH e atualiza o Pedido.
  - se o PATCH retornar erro, `EfetuarPagamento` retorna falha e o worker retenta/requeue conforme sua política.
