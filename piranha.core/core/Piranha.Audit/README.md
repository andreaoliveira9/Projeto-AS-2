# Piranha.Audit

This module provides comprehensive audit and history tracking capabilities for Piranha CMS, designed to support enterprise-level compliance and governance requirements.

## Features

- **Comprehensive Audit Logging**: Track all system actions with detailed context
- **Workflow State Change Tracking**: Specialized logging for editorial workflow transitions
- **Permission Check Tracing**: Record authorization decisions for security analysis
- **Message Queue Integration**: Asynchronous processing of audit events
- **Configurable Retention**: Automatic cleanup of old audit records
- **Performance Optimized**: Designed for high-volume editorial environments

## Components

### Models

- **AuditLog**: General purpose audit record for all system actions
- **StateChangeRecord**: Specialized record for workflow state transitions
- **PermissionCheckTrace**: Record of authorization checks and decisions

### Events

- **AuditEvent**: Base class for all audit events
- **WorkflowStateChangedEvent**: Event for workflow state transitions
- **PermissionCheckEvent**: Event for permission checks
- **GeneralAuditEvent**: Event for general system actions

### Services

- **IAuditService**: Main service interface for audit operations
- **AuditService**: Default implementation of audit service
- **AuditMessageConsumerService**: Background service for processing audit events from message queue

## Usage

### Basic Setup

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddPiranha(options =>
    {
        // ... other configuration
        options.UseAudit(audit =>
        {
            audit.EnableMessageConsumer = true;
            audit.MessageQueueCapacity = 1000;
            audit.DefaultRetentionDays = 365;
        });
    });
}
```

### Manual Audit Logging

```csharp
public class MyController : Controller
{
    private readonly IAuditService _auditService;

    public MyController(IAuditService auditService)
    {
        _auditService = auditService;
    }

    public async Task<IActionResult> UpdateContent(Guid id)
    {
        try
        {
            // Perform content update
            await UpdateContentLogic(id);

            // Log successful audit
            await _auditService.LogAuditAsync(
                actionType: "ContentUpdate",
                entityType: "Page",
                entityId: id.ToString(),
                userId: User.Identity.Name,
                username: User.Identity.Name,
                success: true);

            return Ok();
        }
        catch (Exception ex)
        {
            // Log failed audit
            await _auditService.LogAuditAsync(
                actionType: "ContentUpdate",
                entityType: "Page",
                entityId: id.ToString(),
                userId: User.Identity.Name,
                username: User.Identity.Name,
                success: false,
                errorMessage: ex.Message);

            return StatusCode(500);
        }
    }
}
```

### Retrieving Audit History

```csharp
// Get audit history for a specific content item
var auditLogs = await _auditService.GetAuditLogsAsync("Page", pageId.ToString());

// Get workflow state change history
var stateChanges = await _auditService.GetStateChangeHistoryAsync(contentId);

// Get permission check traces for a user
var permissionTraces = await _auditService.GetPermissionCheckTracesAsync(userId);
```

### Message Queue Integration

The audit service integrates with message queues to process audit events asynchronously:

```csharp
// Events are automatically processed when published to the message queue
// The AuditMessageConsumerService runs as a background service
```

### Cleanup Old Records

```csharp
// Manual cleanup
var deletedCount = await _auditService.CleanupOldRecordsAsync(retentionDays: 365);

// Or configure automatic cleanup
services.AddPiranha(options =>
{
    options.UseAudit(audit =>
    {
        audit.EnableAutomaticCleanup = true;
        audit.CleanupIntervalHours = 24;
        audit.DefaultRetentionDays = 365;
    });
});
```

## Configuration Options

- **EnableMessageConsumer**: Enable/disable the background message consumer service
- **MessageQueueCapacity**: Maximum capacity of the in-memory message queue
- **DefaultRetentionDays**: Default number of days to retain audit records
- **EnableAutomaticCleanup**: Enable automatic cleanup of old records
- **CleanupIntervalHours**: Frequency of automatic cleanup operations

## Database Schema

The module requires the following database tables:

- `AuditLogs`: Main audit log table
- `StateChangeRecords`: Workflow state change records
- `PermissionCheckTraces`: Permission check audit trail

These are automatically created when using the Entity Framework data provider.

## Integration with Editorial Workflow

This module is designed to work seamlessly with the Editorial Workflow module:

- Automatically captures workflow state transitions
- Records permission checks during workflow operations
- Provides complete audit trail for compliance requirements

## Performance Considerations

- Audit operations are designed to be non-blocking
- Message queue processing is asynchronous
- Database operations use efficient indexing strategies
- Automatic cleanup prevents unbounded growth

## Compliance Features

- Complete audit trail for all user actions
- Immutable audit records
- Detailed permission check logging
- Configurable retention policies
- Support for regulatory compliance requirements
