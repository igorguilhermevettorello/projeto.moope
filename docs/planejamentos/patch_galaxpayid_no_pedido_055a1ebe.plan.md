---
name: Patch galaxPayId no pedido
overview: Criar um endpoint PATCH em `PedidoController` para receber `pedidoId` e `galaxPayId`, atualizar `Pedido.GalaxPayId` e `Updated` no banco, retornando 204 NoContent e mantendo autenticação.
todos:
  - id: add-patch-dto
    content: Criar DTO `PedidoUpdateGalaxPayIdRequestDto` na API com validações.
    status: completed
  - id: add-service-method
    content: Adicionar `AtualizarGalaxPayIdAsync` em `IPedidoService` e implementar em `PedidoService` persistindo no repositório.
    status: completed
  - id: add-controller-endpoint
    content: Adicionar endpoint PATCH no `PedidoController` com validações, retornos 204/400/404.
    status: completed
isProject: false
---

## Objetivo
Adicionar um endpoint `PATCH` em [`src/pedido/Projeto.Moope.Pedido.Api/Controllers/PedidoController.cs`](src/pedido/Projeto.Moope.Pedido.Api/Controllers/PedidoController.cs) para atualizar `Pedido.GalaxPayId` (int?) dado um `pedidoId` (guid) e um `galaxPayId`.

## Escopo e decisões
- **Campo a atualizar**: `Pedido.GalaxPayId` (já existe em [`src/pedido/Projeto.Moope.Pedido.Core/Models/Pedido.cs`](src/pedido/Projeto.Moope.Pedido.Core/Models/Pedido.cs)).
- **Resposta**: `204 NoContent`.
- **Auth**: manter `[Authorize]` (o worker/bff devem continuar enviando `Authorization` nas chamadas internas).

## Abordagem (Clean Architecture)
### 1) DTO de request para o PATCH
Criar um DTO simples na API (ex.: `PedidoUpdateGalaxPayIdRequestDto`) em [`src/pedido/Projeto.Moope.Pedido.Api/DTOs/`](src/pedido/Projeto.Moope.Pedido.Api/DTOs/) contendo:
- `Guid PedidoId` (Required)
- `int GalaxPayId` (Required)

### 2) Contrato e implementação no Core
- Adicionar método no [`IPedidoService`](src/pedido/Projeto.Moope.Pedido.Core/Interfaces/Services/IPedidoService.cs):
  - `Task<ResultDto> AtualizarGalaxPayIdAsync(Guid pedidoId, int galaxPayId)`
- Implementar no [`PedidoService`](src/pedido/Projeto.Moope.Pedido.Core/Services/PedidoService.cs):
  - Validar `pedidoId != Guid.Empty` e `galaxPayId > 0`
  - Carregar pedido via `_pedidoRepository.BuscarPorIdAsync(pedidoId)`
  - Se não encontrado: retornar `ResultDto { Status=false, Mensagem="Pedido não encontrado" }`
  - Atualizar `pedido.GalaxPayId = galaxPayId` e `pedido.Updated = DateTime.UtcNow`
  - Persistir via `_pedidoRepository.AtualizarAsync(pedido)` e `_pedidoRepository.UnitOfWork.Commit()` (mantendo padrão do serviço)

### 3) Endpoint PATCH no controller
Adicionar em `PedidoController` algo como:
- Rota: `PATCH /api/pedido/{id:guid}/galaxpay-id`
- Body: `{ pedidoId, galaxPayId }` (validar que `id == dto.PedidoId` para evitar inconsistência)
- Retornos:
  - `204` quando sucesso
  - `404` quando pedido não encontrado
  - `400` quando ids inválidos/ModelState inválido

## Teste manual sugerido
- Chamar `PATCH /api/pedido/{pedidoId}/galaxpay-id` com JWT válido e body `{ "pedidoId": "...", "galaxPayId": 123 }`
- Confirmar no banco que `Pedido.GalaxPayId` e `Pedido.Updated` foram atualizados.
