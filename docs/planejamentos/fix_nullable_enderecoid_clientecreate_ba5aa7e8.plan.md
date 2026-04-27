---
name: fix nullable enderecoid clientecreate
overview: Corrigir NullReferenceException causado por uso indevido de `??` ao atribuir `EnderecoId` no `ClienteCreateService`, que estoura quando o cliente é criado sem endereço (caminho usado pelo fluxo de venda).
todos:
  - id: fix-enderecoid
    content: Substituir `EnderecoId = enderecoId ?? enderecoId.Value` por `EnderecoId = enderecoId` em ClienteCreateService.cs (linha 199)
    status: completed
  - id: build-validate
    content: Compilar a solução do BFF e validar fluxo de venda que dispara ClienteCreateService sem Endereco
    status: completed
isProject: false
---

## Causa raiz

No retorno de [bff/Projeto.Moope.Gateways.Core/Services/Cliente/ClienteCreateService.cs](bff/Projeto.Moope.Gateways.Core/Services/Cliente/ClienteCreateService.cs) (linha 199), a expressão:

```csharp
EnderecoId = enderecoId ?? enderecoId.Value
```

aciona `enderecoId.Value` quando `enderecoId` é `null`, lançando `InvalidOperationException: Nullable object must have a value`.

O bug se manifesta sempre que `request.Endereco` é `null`, exatamente o caminho usado por [bff/Projeto.Moope.Gateways.Core/Services/ProcessarVendaService.cs](bff/Projeto.Moope.Gateways.Core/Services/ProcessarVendaService.cs) (`ClienteAsync`, linhas 343–356), que monta o `ClienteCreateDto` sem `Endereco`.

Como `ClienteCreateResultDto.EnderecoId` já é `Guid?`, basta atribuir o nullable diretamente.

## Alteração

Em [bff/Projeto.Moope.Gateways.Core/Services/Cliente/ClienteCreateService.cs](bff/Projeto.Moope.Gateways.Core/Services/Cliente/ClienteCreateService.cs), linha 199, trocar:

```csharp
EnderecoId = enderecoId ?? enderecoId.Value
```

por:

```csharp
EnderecoId = enderecoId
```

Nenhum outro arquivo precisa de mudança: o consumidor `ProcessarVendaService.ClienteAsync` só usa `rs.Dados?.ClienteId` (linha 362), portanto continuar com `EnderecoId` nulo quando não há endereço é o comportamento esperado.

## Validação

- Compilar `bff/Projeto.Moope.Gateways.Core` e `bff/Projeto.Moope.Gateways.Api`.
- Reproduzir o cenário de venda (POST em `VendaBffController.Processar`) que originou o stack trace e confirmar resposta de sucesso, sem `InvalidOperationException`.
- Verificar (opcional) que o caminho com `Endereco` preenchido continua retornando `EnderecoId` corretamente.
