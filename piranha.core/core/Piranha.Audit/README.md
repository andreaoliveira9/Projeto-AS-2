# Piranha Audit Module

O módulo de Auditoria do Piranha CMS é responsável por **consumir mensagens** de uma message queue sobre mudanças de estado de conteúdo, armazenando os registros na base de dados para auditoria e rastreamento histórico.

## Objetivo

Este módulo foi redesenhado para ser **exclusivamente um consumidor**:

- **Receber mensagens** de eventos de mudança de estado através de message queue
- **Processar StateChangeRecord** automaticamente a partir das mensagens recebidas
- **Armazenar registros de auditoria** na base de dados
- **Fornecer API simples** para consultar histórico por ContentID

> **⚠️ Importante**: Este módulo **NÃO publica mensagens**. Ele apenas consome mensagens enviadas por outros componentes do sistema.

## Arquitetura Simplificada

O módulo agora tem uma arquitetura focada apenas no consumo:

### Core (Piranha.Audit)
- **Models**: `StateChangeRecord` - Modelo de dados para registros de mudança de estado
- **Events**: `WorkflowStateChangedEvent` - Evento que representa uma mudança de estado (apenas para deserialização)
- **Services**: 
  - `IAuditService` / `AuditService` - Serviço principal para processamento de mensagens recebidas
  - `AuditMessageConsumerService` - Serviço em background que consome mensagens da fila
- **Repositories**: `IStateChangeRecordRepository` - Interface simplificada para acesso aos dados

### Data Layer (Piranha.Data.EF.EditorialWorkflowAndAudit)
- **Data**: `StateChangeRecord` - Entidade Entity Framework
- **Repositories**: `StateChangeRecordRepository` - Implementação EF simplificada do repositório

## Funcionamento

1. **Message Queue**: O sistema utiliza um Channel<string> interno para receber mensagens
2. **Consumer Service**: O `AuditMessageConsumerService` roda em background consumindo mensagens
3. **Processamento**: Cada mensagem é deserializada para `WorkflowStateChangedEvent`
4. **Persistência**: Os eventos são convertidos em `StateChangeRecord` e salvos na base de dados

## Configuração

### Registro Simples

```csharp
// No Startup.cs ou Program.cs
services.AddPiranha(options => {
    options.UseAudit(); // Registra o módulo de auditoria (apenas consumo)
})
.UseAuditEF(); // Registra os repositórios Entity Framework
```

> **Nota**: Não há configurações adicionais necessárias. O módulo está configurado com valores otimizados por padrão.

## API de Uso (Apenas Consulta)

### Consultar Histórico

```csharp
// Injetar o serviço de auditoria
private readonly IAuditService _auditService;

// Obter histórico de um conteúdo específico
var history = await _auditService.GetStateChangeHistoryAsync(contentId);
```

### Acesso Direto ao Repositório

```csharp
// Injetar o repositório
private readonly IStateChangeRecordRepository _repository;

// Buscar registros por ContentId
var records = await _repository.GetByContentAsync(contentId);

// Salvar um novo registro (geralmente não usado diretamente)
await _repository.SaveAsync(stateChangeRecord);
```

## Interface Simplificada

### IAuditService

```csharp
public interface IAuditService
{
    // Processa mensagens recebidas da queue
    Task ProcessWorkflowStateChangedEventAsync(WorkflowStateChangedEvent stateChangedEvent, CancellationToken cancellationToken = default);
    
    // Consulta histórico por ContentId
    Task<IEnumerable<StateChangeRecord>> GetStateChangeHistoryAsync(Guid contentId);
}
```

### IStateChangeRecordRepository

```csharp
public interface IStateChangeRecordRepository
{
    // Consulta registros por ContentId
    Task<IEnumerable<StateChangeRecord>> GetByContentAsync(Guid contentId);
    
    // Salva um registro de mudança de estado
    Task SaveAsync(StateChangeRecord stateChangeRecord);
}
```

### Configuração Interna

- **Capacidade da Queue**: 1000 mensagens (valor fixo otimizado)
- **Consumer Service**: Sempre ativo (sem opção de desativar)
- **Channel Mode**: Bounded com Wait (garante que mensagens não sejam perdidas)

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

### Índices Essenciais

- `ContentId, Timestamp` (para consultas de histórico)
- `ContentId` (consulta principal)
- `Timestamp` (para ordenação)

## Logs e Monitoramento

O módulo produz logs estruturados para:

- Início/parada do serviço de consumo
- Processamento de mensagens
- Erros de deserialização
- Salvamento de registros

Exemplo de configuração de logging:

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

## Características Técnicas

### Message Queue
- Utiliza `System.Threading.Channels` para processamento assíncrono
- Capacidade fixa: 1000 mensagens (otimizada para a maioria dos cenários)
- Resiliente: continua processando mesmo se uma mensagem falhar
- Thread-safe para consumo

### Serialização
- JSON com `System.Text.Json`
- Case-insensitive para flexibilidade
- Tratamento robusto de erros de deserialização

### Performance
- Índices otimizados para consultas por ContentId
- Processamento assíncrono de mensagens
- Transformação eficiente entre modelos

## Como Enviar Mensagens para o Audit

Como este módulo é apenas um consumidor, outros componentes do sistema devem enviar mensagens para ele. Exemplo de como outro serviço pode enviar uma mensagem:

```csharp
// Em outro serviço que publique mensagens
public class WorkflowService
{
    private readonly Channel<string> _auditMessageQueue;

    public async Task ChangeStateAsync(/* parâmetros */)
    {
        // Lógica de mudança de estado...

        // Criar evento de auditoria
        var auditEvent = new WorkflowStateChangedEvent
        {
            WorkflowInstanceId = workflowInstanceId,
            ContentId = contentId,
            ContentType = "Page",
            FromState = "Draft",
            ToState = "Published",
            UserId = userId,
            Username = username,
            Timestamp = DateTime.UtcNow,
            Success = true
        };

        // Serializar e enviar para a queue do audit
        var message = JsonSerializer.Serialize(auditEvent);
        await _auditMessageQueue.Writer.WriteAsync(message);
    }
}
```

## Ficheiros Removidos

Os seguintes ficheiros foram removidos ou renomeados por não serem necessários para um módulo apenas de consumo:

- `IAuditMessagePublisher.cs` → `.bak` (funcionalidade de publicação removida)
- `AuditCleanupService.cs` → `.bak` (limpeza automática removida)

## Métodos Removidos

### Do IAuditService:
- `LogStateChangeAsync()` - substituído pelo processamento direto de eventos
- `CleanupOldRecordsAsync()` - funcionalidade de limpeza removida

### Do IStateChangeRecordRepository:
- `GetByIdAsync()`
- `GetByWorkflowInstanceAsync()`
- `GetByUserAsync()`
- `GetByDateRangeAsync()`
- `GetByTransitionAsync()`
- `DeleteAsync()`
- `DeleteOlderThanAsync()`

Apenas os métodos essenciais para consumo e consulta por ContentId foram mantidos.

## Troubleshooting

### Problemas Comuns

**1. Mensagens não são processadas**
- Verificar logs do `AuditMessageConsumerService`
- Verificar se o serviço está registrado como HostedService
- Verificar se a aplicação está enviando mensagens para a queue correta

**2. Erros de serialização**
- Verificar formato JSON das mensagens
- Verificar compatibilidade de versões dos eventos
- Ativar logs Debug para ver detalhes

**3. Registros não aparecem**
- Verificar se as mensagens estão chegando à queue
- Verificar conectividade com a base de dados
- Verificar se o ContentId está correto nas consultas

## Resumo das Alterações

Este módulo foi simplificado para ser **exclusivamente um consumidor de mensagens**:

✅ **Mantido:**
- Consumo de mensagens da queue
- Processamento de `WorkflowStateChangedEvent`
- Salvamento na base de dados
- Consulta de histórico por ContentId

❌ **Removido:**
- Publicação de mensagens
- Funcionalidades de limpeza automática
- Métodos de consulta complexos
- Múltiplas interfaces desnecessárias

O resultado é um módulo mais leve, focado e fácil de manter que cumpre apenas o objetivo de receber mudanças de estado e guardá-las na base de dados.
