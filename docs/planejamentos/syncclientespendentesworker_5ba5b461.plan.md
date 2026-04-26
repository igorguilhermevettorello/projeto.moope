---
name: SyncClientesPendentesWorker
overview: Adicionar uma rotina periódica no `src/rabbitmq` que autentica via `api/auth/client/login`, busca `api/usuario/clientes-pendentes` no Auth e cadastra cada cliente pendente no serviço de Cliente via `api/cliente` — separando em HostedServices/workers e mantendo o worker de RabbitMQ isolado.
todos:
  - id: map-options
    content: Expandir/criar options para `DownstreamApis` incluindo `Auth`, `AuthClientId`, `AuthClientSecret`, `Cliente` (mantendo bind no Worker).
    status: completed
  - id: add-http-services-core
    content: "Adicionar serviços no `Projeto.Moope.RabbitMQ.Core` para: login client credentials, listar clientes pendentes, e criar cliente no serviço Cliente (via `IHttpClientFactory`)."
    status: completed
  - id: add-sync-worker
    content: Criar um novo `BackgroundService` no `Projeto.Moope.RabbitMQ.Worker` usando `PeriodicTimer` (1 min), com token cache e loop de sincronização.
    status: completed
  - id: wire-di
    content: Registrar os novos serviços e o novo hosted service no `Program.cs` sem misturar com o worker atual de RabbitMQ.
    status: completed
  - id: smoke-run
    content: Executar o worker localmente e validar logs/requests contra endpoints locais (Auth em `6101` e Cliente em `6102`).
    status: completed
isProject: false
---

## Objetivo
Criar um **worker periódico** no projeto `[c:\projetos\Moope\projeto.moope\src\rabbitmq\Projeto.Moope.RabbitMQ.Worker](c:\projetos\Moope\projeto.moope\src\rabbitmq\Projeto.Moope.RabbitMQ.Worker)` que, **a cada 1 minuto**, executa o fluxo:

- **Login client credentials** no Auth: `POST {DownstreamApis:Auth}/api/auth/client/login` com **Header** `Authorization: Basic base64(AuthClientId:AuthClientSecret)` (conforme `[AuthController](c:\projetos\Moope\projeto.moope\src\auth\Projeto.Moope.Auth.Api\Controllers\AuthController.cs)`)
- **Buscar pendentes** no Auth: `GET {DownstreamApis:Auth}/api/usuario/clientes-pendentes` com `Authorization: Bearer {accessToken}` (conforme `[UsuarioController](c:\projetos\Moope\projeto.moope\src\auth\Projeto.Moope.Auth.Api\Controllers\UsuarioController.cs)`)
- **Criar cliente** no serviço Cliente: `POST {DownstreamApis:Cliente}/api/cliente` com `Authorization: Bearer {accessToken}` e body mínimo `{"usuarioId": <guid>}` (conforme `[ClienteController.Criar](c:\projetos\Moope\projeto.moope\src\cliente\Projeto.Moope.Cliente.Api\Controllers\ClienteController.cs)` e `[ClienteCreateDto](c:\projetos\Moope\projeto.moope\src\cliente\Projeto.Moope.Cliente.Api\DTOs\ClienteCreateDto.cs)`).

O worker deve ser **separado** do consumidor RabbitMQ atual (não misturar lógica no mesmo `BackgroundService`).

## Observações de contrato (já confirmadas no código)
- **Credenciais** vêm de `[appsettings.Development.json](c:\projetos\Moope\projeto.moope\src\rabbitmq\Projeto.Moope.RabbitMQ.Worker\appsettings.Development.json)`:
  - `DownstreamApis:Auth`, `DownstreamApis:AuthClientId`, `DownstreamApis:AuthClientSecret`, `DownstreamApis:Cliente`
- A lista de pendentes hoje devolve itens com **Id + Email** (`ClientePendenteDto`): `[c:\projetos\Moope\projeto.moope\src\auth\Projeto.Moope.Auth.Core\DTOs\Usuario\ClientePendenteDto.cs](c:\projetos\Moope\projeto.moope\src\auth\Projeto.Moope.Auth.Core\DTOs\Usuario\ClientePendenteDto.cs)`
- `ClienteCreateDto` requer `UsuarioId` (os outros campos podem ficar nulos).

## Ajuste necessário em options/config
O worker já faz bind de `DownstreamApis` em `DownstreamApisOptions` no `Program.cs`, porém `DownstreamApisOptions` hoje só contém `Pagamento`:
- `[c:\projetos\Moope\projeto.moope\src\rabbitmq\Projeto.Moope.RabbitMQ.Core\Options\DownstreamApisOptions.cs](c:\projetos\Moope\projeto.moope\src\rabbitmq\Projeto.Moope.RabbitMQ.Core\Options\DownstreamApisOptions.cs)`

Para suportar a rotina, o plano inclui **expandir** essa options (ou criar uma nova dedicada) para expor pelo menos: `Auth`, `AuthClientId`, `AuthClientSecret`, `Cliente`.

## Estrutura proposta (separação em workers)
- **Manter** o worker atual de RabbitMQ (consumo da fila `vendas`) isolado.
- **Adicionar** um novo `BackgroundService` (ex.: `ClientesPendentesSyncWorker`) que executa o job periódico.
- **Extrair** chamadas HTTP para serviços na camada `Projeto.Moope.RabbitMQ.Core` (para manter separação e testabilidade):
  - `IAuthClientTokenService` → obtém token via Basic.
  - `IClientesPendentesQueryService` → busca pendentes no Auth.
  - `IClienteProvisioningService` → cria cliente no serviço Cliente.

## Comportamento do job periódico
- Usar `PeriodicTimer` com período **1 minuto**.
- Implementar **token cache** simples (guardar `accessToken` + `expiresAt` e renovar antes de expirar) para evitar logins excessivos.
- Para cada pendente:
  - Tentar criar no Cliente.
  - Se resposta indicar “já existe” (ex.: `409 Conflict` ou erro equivalente), **logar e pular** (sua escolha: “skip”).
  - Em falhas transitórias (ex.: 5xx/timeouts), aplicar poucas tentativas com delay curto (backoff leve) para não perder o ciclo inteiro.

## Logging e resiliência
- Logs por ciclo: quantos pendentes encontrados, quantos criados, quantos pulados, quantos falharam.
- Cancelamento respeitando `CancellationToken` do host.

## Testabilidade
- Serviços HTTP com interfaces no Core para poder mockar em testes unitários.
- (Opcional) Um teste de integração simples usando `HttpMessageHandler` fake para simular Auth + Cliente.
