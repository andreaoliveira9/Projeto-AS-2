# Piranha Audit Module

O módulo de Auditoria do Piranha CMS é responsável por receber e processar eventos de mudança de estado através de uma message queue, armazenando os registros na base de dados para auditoria e rastreamento histórico.

## Objetivo

Este módulo foi projetado especificamente para:

- **Conectar-se a uma message queue** e receber mensagens de eventos
- **Processar StateChangeRecord** automaticamente
- **Armazenar registros de auditoria** na base de dados
- **Fornecer API** para consultar histórico de mudanças
- **Executar limpeza automática** de registros antigos (opcional)

## Arquitetura

O módulo é composto por duas camadas principais:

### Core (Piranha.Audit)
- **Models**: `StateChangeRecord` - Modelo de dados para registros de mudança de estado
- **Events**: `WorkflowStateChangedEvent` - Evento que representa uma mudança de estado
- **Services**: 
  - `IAuditService` / `AuditService` - Serviço principal para processamento de auditoria
  - `IAuditMessagePublisher` / `AuditMessagePublisher` - Serviço para publicar mensagens na fila
  - `AuditMessageConsumerService` - Serviço em background que consome mensagens da fila
  - `AuditCleanupService` - Serviço em background para limpeza automática (opcional)
- **Repositories**: `IStateChangeRecordRepository` - Interface para acesso aos dados

### Data Layer (Piranha.Data.EF.Audit)
- **Data**: `StateChangeRecord` - Entidade Entity Framework
- **Repositories**: `StateChangeRecordRepository` - Implementação EF do repositório
- **Extensions**: `AuditDbExtensions` - Configuração das entidades no DbContext

## Funcionamento

1. **Message Queue**: O sistema utiliza um Channel<string> interno para processar mensagens
2. **Consumer Service**: O `AuditMessageConsumerService` roda em background consumindo mensagens
3. **Processamento**: Cada mensagem é deserializada para `WorkflowStateChangedEvent`
4. **Persistência**: Os eventos são convertidos em `StateChangeRecord` e salvos na base de dados
5. **Cleanup**: Opcionalmente, registros antigos são removidos automaticamente

## Configuração

### Registro Básico

```csharp
// No Startup.cs ou Program.cs
services.AddPiranha(options => {
    options.UseAudit(); // Registra o módulo de auditoria
})
.UseAuditEF(); // Registra os repositórios Entity Framework
```

### Configuração Avançada

```csharp
services.AddPiranha(options => {
    options.UseAudit(auditOptions => {
        auditOptions.EnableMessageConsumer = true; // Ativa o consumer (padrão: true)
        auditOptions.MessageQueueCapacity = 1000; // Capacidade da fila (padrão: 1000)
        auditOptions.EnableAutomaticCleanup = true; // Ativa limpeza automática (padrão: false)
        auditOptions.DefaultRetentionDays = 365; // Dias de retenção (padrão: 365)
        auditOptions.CleanupIntervalHours = 24; // Intervalo de limpeza em horas (padrão: 24)
    });
})
.UseAuditEF();
```

### Configuração do DbContext

```csharp
public class ApplicationDbContext : DbContext, IAuditDb
{
    public DbSet<StateChangeRecord> StateChangeRecord { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configura as entidades de auditoria
        modelBuilder.ConfigureAudit();
    }
}
```

## API de Uso

### Publicar Eventos

```csharp
// Injetar o publisher
private readonly IAuditMessagePublisher _messagePublisher;

// Criar e publicar evento
var stateChangedEvent = new WorkflowStateChangedEvent
{
    WorkflowInstanceId = workflowId,
    ContentId = contentId,
    ContentType = "Page",
    FromState = "Draft",
    ToState = "Published",
    UserId = userId,
    Username = username,
    Timestamp = DateTime.UtcNow,
    Success = true
};

await _messagePublisher.PublishWorkflowStateChangedAsync(stateChangedEvent);
```

### Consultar Histórico

```csharp
// Injetar o serviço de auditoria
private readonly IAuditService _auditService;

// Obter histórico de um conteúdo
var history = await _auditService.GetStateChangeHistoryAsync(contentId);

// Limpeza manual de registros antigos
var deletedCount = await _auditService.CleanupOldRecordsAsync(retentionDays: 90);
```

### Acesso Direto ao Repositório

```csharp
// Injetar o repositório
private readonly IStateChangeRecordRepository _repository;

// Buscar por workflow instance
var records = await _repository.GetByWorkflowInstanceAsync(workflowInstanceId);

// Buscar por usuário
var userRecords = await _repository.GetByUserAsync(userId, take: 20);

// Buscar por período
var dateRangeRecords = await _repository.GetByDateRangeAsync(fromDate, toDate);
```

## Estrutura da Base de Dados

### Tabela: Piranha_StateChangeRecords

| Campo | Tipo | Descrição |
|-------|------|-----------|
| Id | uniqueidentifier | Chave primária |
| WorkflowInstanceId | uniqueidentifier | ID da instância do workflow |
| ContentId | uniqueidentifier | ID do conteúdo |
| ContentType | nvarchar(50) | Tipo de conteúdo (Page, Post, etc.) |
| FromState | nvarchar(100) | Estado anterior |
| ToState | nvarchar(100) | Novo estado |
| UserId | nvarchar(450) | ID do usuário |
| Username | nvarchar(256) | Nome do usuário |
| Timestamp | datetime2 | Data/hora da mudança |
| Comments | nvarchar(1000) | Comentários opcionais |
| TransitionRuleId | uniqueidentifier | ID da regra de transição (opcional) |
| Metadata | nvarchar(max) | Metadados em JSON |
| Success | bit | Se a ação foi bem-sucedida |
| ErrorMessage | nvarchar(2000) | Mensagem de erro (se falhou) |

### Índices para Performance

- `WorkflowInstanceId`
- `ContentId`
- `ContentId, Timestamp`
- `UserId`
- `Timestamp`
- `FromState, ToState`
- `TransitionRuleId`
- `Success`

## Logs e Monitoramento

O módulo produz logs estruturados para:

- Início/parada dos serviços em background
- Processamento de mensagens
- Erros de deserialização
- Operações de limpeza
- Estatísticas de performance

Exemplo de configuração de logging:

```json
{
  "Logging": {
    "LogLevel": {
      "Piranha.Audit": "Information",
      "Piranha.Audit.Services.AuditMessageConsumerService": "Debug",
      "Piranha.Audit.Services.AuditCleanupService": "Information"
    }
  }
}
```

## Características Técnicas

### Message Queue
- Utiliza `System.Threading.Channels` para processamento assíncrono
- Configurável: capacidade, comportamento quando cheia
- Resiliente: continua processando mesmo se uma mensagem falhar
- Thread-safe: suporta múltiplos produtores e consumidores

### Serialização
- JSON com `System.Text.Json`
- Case-insensitive para flexibilidade
- Tratamento robusto de erros de deserialização

### Performance
- Índices otimizados para consultas comuns
- Paginação em consultas
- Limpeza automática configurável
- Processamento assíncrono

### Escalabilidade
- Pode ser executado em múltiplas instâncias
- Fila interna por instância
- Compatível com load balancers
- Suporte a clustering (com message broker externo)

## Extensibilidade

### Implementar Message Broker Externo

Para usar um message broker externo (RabbitMQ, Azure Service Bus, etc.):

```csharp
// Implementar custom publisher
public class ExternalMessagePublisher : IAuditMessagePublisher
{
    public async Task PublishWorkflowStateChangedAsync(
        WorkflowStateChangedEvent stateChangedEvent, 
        CancellationToken cancellationToken = default)
    {
        // Publicar para broker externo
        await _messageBroker.PublishAsync(stateChangedEvent, cancellationToken);
    }
}

// Registrar implementation customizada
services.AddScoped<IAuditMessagePublisher, ExternalMessagePublisher>();
```

### Custom Repository

Para usar um sistema de persistência diferente:

```csharp
public class CustomStateChangeRecordRepository : IStateChangeRecordRepository
{
    // Implementar métodos usando MongoDB, CosmosDB, etc.
}

services.AddScoped<IStateChangeRecordRepository, CustomStateChangeRecordRepository>();
```

## Migração e Deployment

### Entity Framework Migrations

```bash
# Adicionar migration
dotnet ef migrations add AddAuditModule -p Piranha.Data.EF.Audit

# Aplicar migration
dotnet ef database update -p Piranha.Data.EF.Audit
```

### Docker

O módulo é compatível com containerização:

```dockerfile
# Exemplo Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0
COPY . /app
WORKDIR /app
EXPOSE 80
ENTRYPOINT ["dotnet", "YourApp.dll"]
```

### Health Checks

```csharp
// Adicionar health check para auditoria
services.AddHealthChecks()
    .AddCheck<AuditHealthCheck>("audit");

public class AuditHealthCheck : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, 
        CancellationToken cancellationToken = default)
    {
        // Verificar status da fila, conectividade DB, etc.
        return HealthCheckResult.Healthy();
    }
}
```

## Troubleshooting

### Problemas Comuns

**1. Mensagens não são processadas**
- Verificar se `EnableMessageConsumer = true`
- Verificar logs do `AuditMessageConsumerService`
- Verificar se o serviço está registrado como HostedService

**2. Erros de serialização**
- Verificar formato JSON das mensagens
- Verificar compatibilidade de versões dos eventos
- Ativar logs Debug para ver detalhes

**3. Performance lenta**
- Verificar índices da base de dados
- Considerar aumentar capacidade da fila
- Verificar se há deadlocks na base de dados

**4. Memoria alta**
- Reduzir `MessageQueueCapacity`
- Ativar `EnableAutomaticCleanup`
- Reduzir `DefaultRetentionDays`

### Métricas Recomendadas

- Taxa de processamento de mensagens/segundo
- Tempo médio de processamento por mensagem
- Tamanho da fila
- Número de erros de deserialização
- Tempo de resposta das consultas
- Uso de memória e CPU

## Roadmap

### Funcionalidades Futuras
- [ ] Suporte nativo para message brokers externos
- [ ] Dashboard de monitoramento
- [ ] Alertas configuráveis
- [ ] Exportação de relatórios
- [ ] API REST para consultas
- [ ] Compressão de dados antigos
- [ ] Particionamento automático de tabelas

### Melhorias Planejadas
- [ ] Batch processing para melhor performance
- [ ] Retry policies configuráveis
- [ ] Circuit breaker pattern
- [ ] Métricas integradas com Prometheus
- [ ] Suporte a multi-tenancy

## Contribuição

Para contribuir com o módulo:

1. Fork o repositório
2. Criar branch para a feature
3. Implementar testes unitários
4. Seguir os padrões de código existentes
5. Atualizar documentação
6. Criar pull request

### Executar Testes

```bash
dotnet test test/Piranha.Tests/
```

### Padrões de Código

- Usar async/await consistentemente
- Implementar proper logging
- Tratar exceções adequadamente
- Seguir princípios SOLID
- Documentar APIs públicas
