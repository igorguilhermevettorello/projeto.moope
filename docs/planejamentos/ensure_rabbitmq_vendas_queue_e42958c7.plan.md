---
name: Ensure RabbitMQ vendas queue
overview: Ao subir o `Projeto.Moope.RabbitMQ.Worker`, garantir que a fila `vendas` exista no RabbitMQ; se não existir, criar automaticamente como fila Classic durável, antes de iniciar o consumo.
todos:
  - id: update-queue-declare
    content: Atualizar `EnsureVendaQueueExistsAsync` para declarar a fila `vendas` como Classic/durable (sem `x-queue-type`).
    status: pending
  - id: improve-error-handling
    content: Adicionar tratamento/log claro para falhas por precondition (fila existente com parâmetros incompatíveis) e garantir comportamento idempotente.
    status: pending
  - id: validate-startup
    content: "Validar fluxo de startup: cria fila quando ausente e consome normalmente quando presente."
    status: pending
isProject: false
---

## Objetivo
- Garantir que, ao subir o worker, a fila `vendas` exista; se não existir no RabbitMQ, o processo cria.
- Padronizar a criação como **Classic + durable=true** (sem exchange/binding), conforme você definiu.

## Onde está hoje
- O `Worker` já chama `EnsureVendaQueueExistsAsync(channel, token)` antes do `BasicConsumeAsync`.
- Implementação atual tenta `QueueDeclarePassiveAsync("vendas")` e, se não existir, declara a fila com `arguments["x-queue-type"] = "stream"`.

Trecho relevante:

```204:225:c:\projetos\Moope\projeto.moope\src\rabbitmq\Projeto.Moope.RabbitMQ.Worker\Worker.cs
        private static async Task EnsureVendaQueueExistsAsync(IChannel channel, CancellationToken cancellationToken)
        {
            try
            {
                await channel.QueueDeclarePassiveAsync(queue: QueueName, cancellationToken: cancellationToken);
            }
            catch (OperationInterruptedException)
            {
                var arguments = new Dictionary<string, object?>
                {
                    ["x-queue-type"] = "stream"
                };

                await channel.QueueDeclareAsync(
                    queue: QueueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: arguments,
                    cancellationToken: cancellationToken);
            }
        }
```

## Mudanças propostas
- Ajustar `EnsureVendaQueueExistsAsync` em `[c:\projetos\Moope\projeto.moope\src\rabbitmq\Projeto.Moope.RabbitMQ.Worker\Worker.cs](c:\projetos\Moope\projeto.moope\src\rabbitmq\Projeto.Moope.RabbitMQ.Worker\Worker.cs)` para declarar a fila como **Classic** quando ela não existir:
  - Remover `x-queue-type=stream`.
  - Declarar com `durable: true`, `exclusive: false`, `autoDelete: false`, `arguments: null`.

- Tornar a verificação/criação **idempotente e mais previsível**:
  - Preferir fazer `QueueDeclareAsync(...)` diretamente (RabbitMQ declara se não existe; se já existe com os mesmos parâmetros, é no-op).
  - Tratar exceções de pré-condição falhada (fila existente com parâmetros incompatíveis) com log claro e falha controlada, para evitar loop infinito sem diagnóstico.

- Manter a chamada no ponto atual (logo após criar canal e antes do consumo) em `RunConsumerLoopAsync`, pois isso já garante a fila no “startup do consumidor” e também em reconexões.

## Teste/validação (manual)
- Subir RabbitMQ sem a fila `vendas` e iniciar o worker: verificar que a fila é criada e o consumo inicia.
- Subir RabbitMQ com a fila `vendas` já existente (classic/durable): worker deve iniciar normalmente.
- (Opcional) Criar fila `vendas` com parâmetros incompatíveis (ex.: autoDelete=true) e iniciar o worker: confirmar que loga erro explicando incompatibilidade.

## Arquivos a alterar
- `[c:\projetos\Moope\projeto.moope\src\rabbitmq\Projeto.Moope.RabbitMQ.Worker\Worker.cs](c:\projetos\Moope\projeto.moope\src\rabbitmq\Projeto.Moope.RabbitMQ.Worker\Worker.cs)`
