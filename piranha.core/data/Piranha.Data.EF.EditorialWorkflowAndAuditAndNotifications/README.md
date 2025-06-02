# Piranha.Data.EF.EditorialWorkflowAndAuditAndNotifications

Este projeto foi modificado para incluir suporte às notificações seguindo a lógica dos modelos definidos em `/core/Piranha.Notifications/Models`.

## Modificações Realizadas

### 1. Modelos de Dados
- **Notification.cs**: Modelo base para notificações com `Id` e `Timestamp`
- **StateChangedNotification.cs**: Modelo especializado que herda de `Notification`

### 2. Repositórios
- **INotificationRepository.cs**: Interface para operações CRUD de notificações
- **IStateChangedNotificationRepository.cs**: Interface para operações CRUD de notificações de mudança de estado
- **NotificationRepository.cs**: Implementação EF do repositório de notificações
- **StateChangedNotificationRepository.cs**: Implementação EF do repositório de notificações de mudança de estado

### 3. Configuração do Entity Framework
- **EditorialWorkflowAndAuditAndNotificationsDbExtensions.cs**: 
  - Adicionada interface `INotificationsDb`
  - Configuração das tabelas `Piranha_Notifications` e `Piranha_StateChangedNotifications`
  - Configuração de chaves primárias e índices

### 4. Módulo de Injeção de Dependência
- **Module.cs**: 
  - Adicionados métodos `AddNotificationsRepositories()` e `UseNotificationsEF()`
  - Método `UseEditorialWorkflowAndAuditAndNotificationsEF()` para usar todos os repositórios

### 5. Configuração do Projeto
- **Piranha.Data.EF.EditorialWorkflowAndAuditAndNotifications.csproj**: 
  - Adicionada referência ao projeto `Piranha.Notifications`
  - Atualizada descrição do projeto

## Estrutura de Tabelas

### Piranha_Notifications (TPH - Table Per Hierarchy)
- `Id` (Guid, PK)
- `Timestamp` (DateTime, Required, Indexed)
- `NotificationType` (String, Discriminador para TPH)
- `ContentId` (Guid, para StateChangedNotification)
- `ContentName` (String, MaxLength: 100, para StateChangedNotification)
- `FromState` (String, MaxLength: 100, para StateChangedNotification)
- `ToState` (String, MaxLength: 100, para StateChangedNotification)
- `TransitionDescription` (String, MaxLength: 500, para StateChangedNotification)
- `ApprovedBy` (String, MaxLength: 256, para StateChangedNotification)

**Índices adicionais:**
- `ContentId` (individual)
- `ContentId + Timestamp` (composto)
- `ApprovedBy` (individual)
- `FromState + ToState` (composto)

## Como Usar

```csharp
// Registrar todos os repositórios
services.UsePiranha()
    .UseEditorialWorkflowAndAuditAndNotificationsEF();

// Ou registrar apenas as notificações
services.UsePiranha()
    .UseNotificationsEF();

// Implementar a interface INotificationsDb no seu DbContext
public class MyDbContext : DbContext, IEditorialWorkflowDb, IAuditDb, INotificationsDb
{
    public DbSet<Notification> Notifications { get; set; }
    // Note: StateChangedNotification é acessado através do DbSet<Notification> com herança TPH
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ConfigureEditorialWorkflowAndAuditAndNotifications();
    }
}
```

## Namespace Atualizado

O projeto agora usa o namespace `Piranha.Data.EF.EditorialWorkflowAndAuditAndNotifications` para refletir a inclusão das notificações.
