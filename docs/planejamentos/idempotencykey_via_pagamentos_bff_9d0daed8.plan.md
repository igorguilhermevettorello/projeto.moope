---
name: IdempotencyKey via Pagamentos BFF
overview: Trocar a geração de `Idempotency-Key` do frontend para uma chamada backend, via um endpoint `/api/...` que roda no servidor SSR/Express do Angular e faz proxy para `https://localhost:6106/api/pagamentos/intencoes`, guardando a chave em `sessionStorage` por aba e reutilizando no `processarVenda`.
todos:
  - id: add-ssr-proxy-endpoint
    content: Adicionar rota Express `POST /api/pagamentos/intencoes` em `src/server.ts` fazendo proxy para `${PAGAMENTO_API}/api/pagamentos/intencoes` com header da apiKey.
    status: completed
  - id: add-dotenv-vars
    content: Introduzir `.env` (PAGAMENTO_API, PAGAMENTO_API_KEY) e carregar via `dotenv` no SSR, garantindo que a chave não vá para `environment*.ts`.
    status: completed
  - id: add-pagamento-service
    content: Criar `src/app/services/pagamento/pagamento.service.ts` para obter `{idempotencyKey}` via `/api/pagamentos/intencoes` e salvar/reutilizar em `sessionStorage`.
    status: completed
  - id: wire-checkout-flow
    content: Atualizar `VendaService` e `StepCartaoCreditoComponent` para usar a key vinda do backend e parar de usar `gerarIdempotencyKey()`.
    status: completed
isProject: false
---

## Objetivo
- Remover/aposentar a geração local de idempotência (`gerarIdempotencyKey()` no `VendaService`) e passar a obter a chave via backend.
- O browser chama **um endpoint local** (`/api/pagamentos/intencoes`) e **quem chama `https://localhost:6106` é o SSR/Express** (para a `pagamentoApiKey` não ir para o browser).
- Persistir a chave em **`sessionStorage`** para ficar restrita à aba atual.

## Estado atual (referências)
- O `VendaService` hoje gera a chave no client e envia em header `Idempotency-Key`:
  - `src/app/services/venda/venda.service.ts` (`gerarIdempotencyKey()` e `processarVenda(..., idempotencyKey = this.gerarIdempotencyKey())`).
- O checkout dispara `processarVenda(dadosVenda)` direto no componente:
  - `src/app/components/steps-pagamento/step-cartao-credito/step-cartao-credito.component.ts` (chamada em `this.vendaService.processarVenda(dadosVenda)`).
- O projeto já tem SSR com Express configurado em `src/server.ts` (comentário indica onde criar endpoints REST).

## Design proposto
### 1) Endpoint “server-side” (SSR/Express) que faz proxy
- Criar endpoint no `Express` em `src/server.ts`:
  - **Rota**: `POST /api/pagamentos/intencoes`
  - **Ação**: encaminhar (proxy) para `POST ${process.env.PAGAMENTO_API}/api/pagamentos/intencoes` (ex: `https://localhost:6106/api/pagamentos/intencoes`).
  - **Header de validação**: enviar `pagamentoApiKey` no header (nome exato a definir no código; ver TODO abaixo).
  - **Resposta**: repassar somente `{ idempotencyKey: string }` para o browser.
- SSL local (`https://localhost:6106`): em dev pode exigir tratamento do certificado. Preferência:
  - usar um `https.Agent({ rejectUnauthorized: false })` **apenas em desenvolvimento**, ou documentar `NODE_TLS_REJECT_UNAUTHORIZED=0` para dev (menos recomendado).

### 2) Variáveis `.env` (server-only)
- Adicionar `.env` na raiz do repo (não existe hoje) com:
  - `PAGAMENTO_API=https://localhost:6106`
  - `PAGAMENTO_API_KEY=...`
- Carregar `.env` no SSR (`src/server.ts`) via `dotenv` (adicionar dependência `dotenv`).
- Importante: **não colocar `PAGAMENTO_API_KEY` em `src/environments/environment*.ts`** (isso vai para o bundle do browser).

### 3) Novo `PagamentoService` no Angular
- Criar `src/app/services/pagamento/pagamento.service.ts` com:
  - método `obterIdempotencyKey(): Observable<string>`
  - chamar `POST /api/pagamentos/intencoes` (rota local do SSR), esperar `{ idempotencyKey: string }`.
  - salvar em `sessionStorage` (ex: chave `@moope:idempotencyKey`) e reutilizar se já existir.
  - expor também um `limparIdempotencyKey()` para limpar ao finalizar compra (sucesso ou desistência).

### 4) Ajustar o fluxo de pagamento
- Ajustar `src/app/components/steps-pagamento/step-pedido/step-pedido.component.ts` para **obter e armazenar o `idempotencyKey` antes do `onSubmit`** (ou seja, antecipar a criação da intenção/chave no passo de pedido, antes do passo do cartão efetivamente enviar a venda).
- Ajustar `VendaService.processarVenda(...)` para **não gerar** default client-side:
  - opção A (mais explícita): exigir `idempotencyKey: string` como parâmetro obrigatório.
  - opção B: buscar do `sessionStorage` e falhar se não existir (menos explícito).
- Ajustar `StepCartaoCreditoComponent` para:
  - antes de chamar `processarVenda`, obter a chave via `PagamentoService` (ou ler do `sessionStorage`).
  - chamar `vendaService.processarVenda(dadosVenda, idempotencyKey)`.

## Testes / validações manuais
- Verificar que:
  - abrir duas abas cria chaves diferentes (por `sessionStorage` ser por-aba).
  - recarregar a aba mantém a mesma chave durante a sessão (até finalizar/limpar).
  - o header `Idempotency-Key` chega no BFF/venda como hoje.
  - `PAGAMENTO_API_KEY` nunca aparece no bundle/client.

## Notas e decisões
- Contrato do endpoint externo confirmado: retorna JSON `{ idempotencyKey: string }`.
- Decisão confirmada: chamada deve rodar no servidor (SSR/Express) para esconder a `pagamentoApiKey`.
