# Piranha Audit Module

O módulo de Auditoria do Piranha CMS é responsável por **consumir mensagens** de uma message queue RabbitMQ sobre mudanças de estado de conteúdo, armazenando os registros na base de dados para auditoria e rastreamento histórico.

## Objetivo

Este módulo foi redesenhado para ser **exclusivamente um consumidor RabbitMQ**:

- **Receber mensagens** de eventos de mudança de estado através do RabbitMQ
- **Processar StateChangeRecord** automaticamente a partir das mensagens recebidas
- **Armazenar registros de auditoria** na base de dados
- **Fornecer API simples** para consultar histórico por ContentID

> **⚠️ Importante**: Este módulo **NÃO publica mensagens**. Ele apenas consome mensagens enviadas por outros componentes do sistema através do RabbitMQ.

## Arquitetura

### Core (Piranha.Audit)
- **Configuration**: `RabbitMQOptions` - Opções de configuração do RabbitMQ
- **Models**: `StateChangeRecord` - Modelo de dados para registros de mudança de estado
- **Events**: `WorkflowStateChangedEvent` - Evento que representa uma mudança de estado
- **Services**: 
  - `IAuditService` / `AuditService` - Serviço principal para processamento de mensagens
  - `IRabbitMQConnectionService` / `RabbitMQConnectionService` - Serviço de conexão RabbitMQ
  - `AuditMessageConsumerService` - Serviço em background que consome mensagens do RabbitMQ

## Funcionamento

1. **RabbitMQ Connection**: Estabelece conexão com RabbitMQ baseada na configuração
2. **Queue Declaration**: Declara automaticamente a exchange e queue se `AutoDeclare` estiver ativo
3. **Consumer Service**: O `AuditMessageConsumerService` consome mensagens em background
4. **Message Processing**: Mensagens são deserializadas para `WorkflowStateChangedEvent`
5. **Retry Logic**: Implementa retry automático com backoff configurável
6. **Acknowledgment**: Mensagens são reconhecidas apenas após processamento bem-sucedido
7. **Persistência**: Eventos são convertidos em `StateChangeRecord` e salvos na base de dados

## Configuração

### Via IConfiguration

```csharp
// appsettings.json
{
  "Piranha": {
    "Audit": {
      "RabbitMQ": {
        "HostName": "localhost",
        "Port": 5672,
        "UserName": "guest",
        "Password": "guest",
        "QueueName": "piranha.audit.events",
        "ExchangeName": "piranha.audit",
        "RoutingKey": "state.changed",
        "MaxRetryAttempts": 3,
        "RetryDelayMs": 1000,
        "AutoDeclare": true,
        "QueueDurable": true
      }
    }
  }
}

// Startup.cs
services.AddPiranha(options => {
    options.UseAudit(configuration);
});
```

### Via Action

```csharp
services.AddPiranha(options => {
    options.UseAudit(rabbitMQOptions => {
        rabbitMQOptions.HostName = "my-rabbitmq-server";
        rabbitMQOptions.UserName = "piranha-user";
        rabbitMQOptions.Password = "secure-password";
        rabbitMQOptions.QueueName = "custom.audit.queue";
        rabbitMQOptions.MaxRetryAttempts = 5;
    });
});
```

## Opções de Configuração

### Conexão
- **HostName**: Hostname do RabbitMQ (padrão: "localhost")
- **Port**: Porta de conexão (padrão: 5672)
- **UserName**: Usuário (padrão: "guest")
- **Password**: Senha (padrão: "guest")
- **UseSsl**: Usar SSL/TLS (padrão: false)

### Queue
- **QueueName**: Nome da queue (padrão: "piranha.audit.events")
- **ExchangeName**: Nome da exchange (padrão: "piranha.audit")
- **RoutingKey**: Chave de roteamento (padrão: "state.changed")
- **AutoDeclare**: Declarar automaticamente (padrão: true)
- **QueueDurable**: Queue durável (padrão: true)

### Retry
- **MaxRetryAttempts**: Máximo de tentativas (padrão: 3)
- **RetryDelayMs**: Delay entre tentativas (padrão: 1000ms)

## API de Uso

### Consultar Histórico

```csharp
private readonly IAuditService _auditService;

var history = await _auditService.GetStateChangeHistoryAsync(contentId);
```

## Resilência

- **Automatic Retry**: Mensagens com falha são automaticamente re-processadas
- **Connection Recovery**: Conexões perdidas são recuperadas automaticamente
- **Manual ACK**: Mensagens são reconhecidas apenas após processamento bem-sucedido
- **Error Handling**: Mensagens com falha após todos os retries são rejeitadas

## Logs

```json
{
  "Logging": {
    "LogLevel": {
      "Piranha.Audit": "Information",
      "Piranha.Audit.Services.AuditMessageConsumerService": "Debug"
    }
  }
}
```

## Docker Setup

```yaml
version: '3.8'
services:
  rabbitmq:
    image: rabbitmq:3-management
    ports:
      - "5672:5672"
      - "15672:15672"
    environment:
      RABBITMQ_DEFAULT_USER: piranha
      RABBITMQ_DEFAULT_PASS: piranha123
```

## Publicar Mensagens

Outros serviços podem publicar mensagens para o audit:

```csharp
var auditEvent = new WorkflowStateChangedEvent
{
    EventId = Guid.NewGuid(),
    ContentId = contentId,
    ContentType = "Page",
    FromState = "Draft",
    ToState = "Published",
    UserId = userId,
    Username = username,
    Timestamp = DateTime.UtcNow,
    Success = true
};

var message = JsonSerializer.Serialize(auditEvent);
var body = Encoding.UTF8.GetBytes(message);

await channel.BasicPublishAsync(
    exchange: "piranha.audit",
    routingKey: "state.changed",
    body: body);
```

## Troubleshooting

### Problemas Comuns

1. **Falha na conexão RabbitMQ**: Verificar se RabbitMQ está rodando e credenciais estão corretas
2. **Mensagens não processadas**: Verificar logs do consumer e se a queue tem mensagens
3. **Erros de serialização**: Verificar formato JSON e ativar logs Debug
4. **Registros não aparecem**: Verificar se mensagens estão sendo ACK'd e conectividade com BD

## Resumo das Alterações

✅ **Adicionado:**
- Integração completa com RabbitMQ
- Configuração flexível via IConfiguration ou Action
- Connection management com recovery automático
- Retry logic configurável
- Manual message acknowledgment
- SSL/TLS support

✅ **Mantido:**
- Consumo de mensagens (agora do RabbitMQ)
- Processamento de `WorkflowStateChangedEvent`
- Salvamento na base de dados
- Consulta de histórico por ContentId

❌ **Removido:**
- Sistema de Channel interno
- Publicação de mensagens
- Funcionalidades de limpeza automática

## Dependências

- **RabbitMQ.Client**: 6.8.1
- **Microsoft.Extensions.Hosting.Abstractions**: 9.0.0

O resultado é um módulo robusto e production-ready que integra com RabbitMQ para consumir eventos de auditoria de forma confiável e eficiente.
