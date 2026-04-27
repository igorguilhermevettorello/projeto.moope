---
name: Fix swagger-auth ambiguity
overview: Resolver o AmbiguousMatchException no `Cliente.Api` removendo o acoplamento indevido com o projeto Web do BFF (`Gateways.Api`) e movendo o utilitário compartilhado `EnumHelper` para um projeto Core/Shared, mantendo `/api/swagger-auth` em cada API separadamente.
todos:
  - id: extract-enumhelper
    content: Criar `core/Projeto.Moope.Api/Utils/EnumHelper.cs` (mesma API pública) e decidir namespace compartilhado.
    status: completed
  - id: update-cliente-controller
    content: Atualizar `ClienteController.cs` para usar o helper no projeto core/shared (sem depender de `Gateways.Api`).
    status: completed
  - id: remove-web-project-reference
    content: Remover o `ProjectReference` do `Cliente.Api` para `bff/Projeto.Moope.Gateways.Api` e ajustar referências necessárias.
    status: completed
  - id: dedupe-bff-helper
    content: "Opcional: trocar o BFF para usar o helper compartilhado e remover duplicação em `Gateways.Api`."
    status: completed
  - id: verify-routing
    content: Rodar e confirmar que `GET /api/swagger-auth` não está ambíguo no `Cliente.Api` e que o Swagger redireciona corretamente.
    status: completed
isProject: false
---

## Causa raiz
O `Cliente.Api` está carregando controllers do `Gateways.Api` porque ele referencia diretamente o projeto Web `bff/Projeto.Moope.Gateways.Api` (veja `[src/cliente/Projeto.Moope.Cliente.Api/Projeto.Moope.Cliente.Api.csproj](src/cliente/Projeto.Moope.Cliente.Api/Projeto.Moope.Cliente.Api.csproj)`), que contém um `SwaggerAuthController` com a mesma rota `api/swagger-auth`. Isso faz a rota bater em **dois endpoints no mesmo host**, gerando `Microsoft.AspNetCore.Routing.Matching.AmbiguousMatchException`.

## Estratégia (correção limpa)
- Remover a referência do `Cliente.Api` para o projeto Web `Gateways.Api`.
- Extrair o que o `Cliente.Api` estava consumindo do BFF (hoje: `Projeto.Moope.Gateways.Api.Utils.EnumHelper`) para um projeto compartilhável (preferência: `core/Projeto.Moope.Api` em `Utils`, já usado pelo `Cliente.Api`).
- Atualizar `using`/namespaces no `Cliente.Api` para apontar para o novo local do helper.
- Garantir que cada API continue com seu próprio `SwaggerAuthController` em `/api/swagger-auth` (sem conflito, pois cada serviço roda isolado).

## Arquivos principais envolvidos
- Remover referência: `[src/cliente/Projeto.Moope.Cliente.Api/Projeto.Moope.Cliente.Api.csproj](src/cliente/Projeto.Moope.Cliente.Api/Projeto.Moope.Cliente.Api.csproj)`
- Ajustar controller que usa helper do BFF: `[src/cliente/Projeto.Moope.Cliente.Api/Controllers/ClienteController.cs](src/cliente/Projeto.Moope.Cliente.Api/Controllers/ClienteController.cs)`
- Origem do helper a mover: `[bff/Projeto.Moope.Gateways.Api/Utils/EnumHelper.cs](bff/Projeto.Moope.Gateways.Api/Utils/EnumHelper.cs)`
- Destino sugerido do helper: `core/Projeto.Moope.Api/Utils/EnumHelper.cs`

## Verificações
- Compilar/rodar `Cliente.Api` e validar que `GET /api/swagger-auth` resolve apenas para `Projeto.Moope.Cliente.Api.Controllers.SwaggerAuthController.Index`.
- Validar que `GET /api/cliente/tipo-pessoa` continua funcionando e retornando a lista.
- Garantir que o BFF continua usando seu próprio `EnumHelper` (ou migrar o BFF também para o helper compartilhado para evitar duplicidade).
