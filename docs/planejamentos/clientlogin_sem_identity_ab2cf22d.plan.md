---
name: ClientLogin sem Identity
overview: "Alterar o endpoint `POST /api/auth/client/login` para autenticar apenas via `Authorization: Basic base64(clienteId:secretKey)` comparando contra `ClientCredentialsOptions`, e emitir um JWT de client credentials (sem usuário no banco e sem refresh token)."
todos:
  - id: refactor-clientlogin
    content: Refatorar `ClientLogin` para validar Basic e emitir JWT sem Identity/DB (sem refresh token).
    status: completed
  - id: remove-technical-seed
    content: Remover/neutralizar seed do usuário técnico em `SeedDataConfig.cs` para o client credentials.
    status: completed
  - id: contract-and-swagger
    content: Ajustar DTO/ProducesResponseType do endpoint para refletir o shape real e manter `data.accessToken`.
    status: completed
  - id: smoke-test-bff
    content: Validar que o BFF continua consumindo `data.accessToken` sem alterações.
    status: completed
isProject: false
---

# ClientLogin sem usuário no banco (client credentials)

## Objetivo
- Remover a dependência do **Identity/DB** no fluxo `POST /api/auth/client/login`.
- Continuar validando **somente** `clienteId` + `secretKey` vindos no header `Authorization: Basic ...`.
- Emitir **apenas Access Token** com `sub=clienteId` e claims mínimas (`role=Administrador`, `perfil=administrador`).

## Estado atual (por que precisa mudar)
- O `ClientLogin` valida o Basic contra `ClientCredentialsOptions`, mas depois faz `PasswordSignInAsync` num usuário técnico e chama `GerarJwt`, que depende de `_userManager.FindByEmailAsync`, roles/claims e ainda cria refresh token no repositório.
- Trecho atual em `[src/auth/Projeto.Moope.Auth.Api/Controllers/AuthController.cs](src/auth/Projeto.Moope.Auth.Api/Controllers/AuthController.cs)`:
  - valida Basic → ok
  - depois:
    - `PasswordSignInAsync(technicalEmail, secretKey, ...)`
    - `GerarJwt(technicalEmail, TipoUsuario.Administrador)`

## Mudança proposta
### 1) Gerar token “client credentials” sem Identity
- Em `[src/auth/Projeto.Moope.Auth.Api/Controllers/AuthController.cs](src/auth/Projeto.Moope.Auth.Api/Controllers/AuthController.cs)`:
  - Manter todo o parsing/validação do header `Authorization: Basic ...`.
  - **Remover** a parte que integra com `_signInManager` e o “usuário técnico”.
  - Criar um gerador dedicado para client token (pode ser método privado no controller no primeiro passo, e depois extraímos para service se quiser):
    - Usa `_jwtSigningKeys.GetSigningCredentials()` + `_jwtSettings`.
    - Monta `ClaimsIdentity` com:
      - `sub = clienteId`
      - `jti`, `nbf`, `iat`
      - `role = "Administrador"` (mesmo padrão do `GerarJwt` atual: claim `"role"`)
      - `perfil = "administrador"`
    - Define `Issuer`/`Audience` e expiração usando `_jwtSettings.ExpiracaoHoras`.
  - Resposta: manter o envelope atual `return CustomResponse(new { data = ... })` e garantir que exista `data.accessToken` (o BFF lê exatamente isso).

### 2) Ajustar o contrato de resposta do endpoint (Swagger/DTO)
- Hoje o endpoint declara `[ProducesResponseType(typeof(ClientLoginResponseDto), ...)]` mas na prática retorna `LoginResponseDto` dentro de `data`.
- Opções (vamos aplicar a consistente com o uso do BFF):
  - Atualizar o `[ProducesResponseType]` para refletir o shape real (objeto com `data.accessToken`, `data.expiresIn`).
  - Alternativamente, alterar para retornar `ClientLoginResponseDto` diretamente dentro de `data`.
- O BFF (`[bff/Projeto.Moope.Gateways.Core/Services/AuthClientLoginService.cs](bff/Projeto.Moope.Gateways.Core/Services/AuthClientLoginService.cs)`) só precisa de `data.accessToken`, então manteremos esse campo garantido.

### 3) Remover seed do “usuário técnico” (para não voltar a depender do DB)
- Em `[src/auth/Projeto.Moope.Auth.Api/Configurations/SeedDataConfig.cs](src/auth/Projeto.Moope.Auth.Api/Configurations/SeedDataConfig.cs)`:
  - Remover (ou desativar) o bloco que cria `technicalEmail = "{authClientId}@client.local"` e grava no Identity/`AppAuthContext`.
  - Manter seed de roles e do admin humano (`admin@moope.com.br`) se isso ainda for necessário para o login normal.

### 4) Configuração de client/secret
- Continuar usando `ClientCredentials` em config (já existe o bind em `[src/auth/Projeto.Moope.Auth.Api/Configurations/AuthConfig.cs](src/auth/Projeto.Moope.Auth.Api/Configurations/AuthConfig.cs)`).
- Recomendações (sem mudar agora, só alinhar):
  - `SecretKey` deve vir de **env var/secret store** no deploy (não commitar segredo em `appsettings.*`).
  - Se houver múltiplos clients, preencher `ClientCredentials:Clients` com lista.

## Fluxo final (novo)
```mermaid
flowchart TD
  caller[Caller(BFF/Service)] --> authHeader[Authorization: Basic base64(clienteId:secretKey)]
  authHeader --> authApi[AuthApi POST /api/auth/client/login]
  authApi --> validate[Validate(clienteId,secret) vs ClientCredentialsOptions]
  validate -->|ok| issueJwt[IssueJwt(sub=clienteId, role=Administrador, perfil=administrador)]
  validate -->|fail| badReq[400 CustomResponse(ModelState)]
  issueJwt --> response[200 { data: { accessToken, expiresIn, claims... } }]
```

## Arquivos impactados
- `[src/auth/Projeto.Moope.Auth.Api/Controllers/AuthController.cs](src/auth/Projeto.Moope.Auth.Api/Controllers/AuthController.cs)`
- `[src/auth/Projeto.Moope.Auth.Api/Configurations/SeedDataConfig.cs](src/auth/Projeto.Moope.Auth.Api/Configurations/SeedDataConfig.cs)`
- (Opcional, se ajustar documentação) `[src/auth/Projeto.Moope.Auth.Core/DTOs/Login/ClientLoginResponseDto.cs](src/auth/Projeto.Moope.Auth.Core/DTOs/Login/ClientLoginResponseDto.cs)`

## Testes/validação
- Chamar `POST /api/auth/client/login` com:
  - Header inválido/ausente → 400.
  - `clienteId`/`secretKey` errados → 400.
  - Credenciais corretas → 200 e `data.accessToken` presente.
- No BFF, confirmar que `AuthClientLoginService` continua extraindo `data.accessToken` sem mudanças.
- Validar no debugger/log que não há mais acesso ao `_signInManager/_userManager/_refreshTokenRepository` dentro de `ClientLogin`.
