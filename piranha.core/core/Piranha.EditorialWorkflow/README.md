# Piranha Editorial Workflow Module

This module extends Piranha CMS with advanced editorial workflow capabilities, allowing for complex content approval processes with state machines and role-based permissions.

## Features

- **Configurable Workflow Definitions**: Create custom editorial workflows with multiple states and transitions
- **State Machine Pattern**: Content progresses through well-defined states with validation rules
- **Role-Based Permissions**: Control who can transition content between states based on existing Piranha roles
- **Content Integration**: Seamlessly integrates with existing Piranha content without modifying core models
- **Audit Trail**: Track all workflow state changes for compliance and debugging

## Architecture

The module follows Domain-Driven Design principles with the following core entities:

- **WorkflowDefinition**: Template defining the structure and rules of an editorial process
- **WorkflowState**: Individual states within a workflow (draft, review, approved, published, etc.)
- **TransitionRule**: Rules governing transitions between states with role-based permissions
- **WorkflowInstance**: Active instance of a workflow applied to specific content
- **WorkflowContentExtension**: Bridge between Piranha content and workflows

## Installation

### 1. Add Project References

Add the Editorial Workflow projects to your solution:

```xml
<ProjectReference Include="path/to/Piranha.EditorialWorkflow/Piranha.EditorialWorkflow.csproj" />
<ProjectReference Include="path/to/Piranha.Data.EF.EditorialWorkflow/Piranha.Data.EF.EditorialWorkflow.csproj" />
```

### 2. Configure Services

In your `Startup.cs` or `Program.cs`, register the Editorial Workflow services:

```csharp
services.AddEditorialWorkflowRepositories();
```

### 3. Update DbContext

Extend your existing Piranha DbContext to include Editorial Workflow tables:

```csharp
public class YourDbContext : Db<YourDbContext>, IEditorialWorkflowDb
{
    // Editorial Workflow DbSets
    public DbSet<Data.EditorialWorkflow.WorkflowDefinition> WorkflowDefinitions { get; set; }
    public DbSet<Data.EditorialWorkflow.WorkflowState> WorkflowStates { get; set; }
    public DbSet<Data.EditorialWorkflow.TransitionRule> TransitionRules { get; set; }
    public DbSet<Data.EditorialWorkflow.WorkflowInstance> WorkflowInstances { get; set; }
    public DbSet<Data.EditorialWorkflow.WorkflowContentExtension> WorkflowContentExtensions { get; set; }

    public YourDbContext(DbContextOptions<YourDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configure Editorial Workflow entities
        modelBuilder.ConfigureEditorialWorkflow();
    }
}
```

### 4. Run Migrations

Generate and run the migration for Editorial Workflow tables:

```bash
dotnet ef migrations add InitialEditorialWorkflow
dotnet ef database update
```

## Usage Example

### Creating a Standard Editorial Workflow

```json
{
  "workflowName": "Standard Editorial Workflow",
  "description": "Standard editorial workflow with draft, review, approval, and publish stages",
  "states": [
    {
      "id": "draft",
      "name": "Draft",
      "description": "Initial content creation phase",
      "isInitial": true,
      "transitions": [
        {
          "toState": "review",
          "roles": ["Editor", "Admin"]
        }
      ]
    },
    {
      "id": "review",
      "name": "Review",
      "description": "Content being reviewed by editors",
      "transitions": [
        {
          "toState": "draft",
          "roles": ["Editor", "Admin"],
          "comment": "Requires revision"
        },
        {
          "toState": "approved",
          "roles": ["Editor", "Admin"],
          "comment": "Approved for publication"
        }
      ]
    },
    {
      "id": "approved",
      "name": "Approved",
      "description": "Content approved and ready for publication",
      "transitions": [
        {
          "toState": "published",
          "roles": ["Admin"],
          "comment": "Final publication"
        }
      ]
    },
    {
      "id": "published",
      "name": "Published",
      "description": "Content is live on the site",
      "isPublished": true
    }
  ]
}
```

### Repository Usage

```csharp
// Inject repositories
public class WorkflowService
{
    private readonly IWorkflowDefinitionRepository _workflowRepo;
    private readonly IWorkflowInstanceRepository _instanceRepo;

    public WorkflowService(
        IWorkflowDefinitionRepository workflowRepo,
        IWorkflowInstanceRepository instanceRepo)
    {
        _workflowRepo = workflowRepo;
        _instanceRepo = instanceRepo;
    }

    // Get all active workflows
    public async Task<IEnumerable<WorkflowDefinition>> GetActiveWorkflows()
    {
        return await _workflowRepo.GetActive();
    }

    // Start a workflow for content
    public async Task<WorkflowInstance> StartWorkflow(string contentId, Guid workflowId, string userId)
    {
        var workflow = await _workflowRepo.GetWithStates(workflowId);
        var initialState = workflow.States.FirstOrDefault(s => s.IsInitial);

        var instance = new WorkflowInstance
        {
            ContentId = contentId,
            WorkflowDefinitionId = workflowId,
            CurrentStateId = initialState.Id,
            CreatedBy = userId,
            Status = WorkflowInstanceStatus.Active
        };

        await _instanceRepo.Save(instance);
        return instance;
    }
}
```

## Database Schema

The module creates the following tables with the `Piranha_` prefix:

- `Piranha_WorkflowDefinitions`
- `Piranha_WorkflowStates`
- `Piranha_TransitionRules`
- `Piranha_WorkflowInstances`
- `Piranha_WorkflowContentExtensions`

All tables are properly indexed for performance and include foreign key constraints to maintain data integrity.

## Integration with Existing Content

The module uses a separate extension table (`WorkflowContentExtensions`) to link with existing Piranha content, ensuring:

- **No modifications** to core Piranha content models
- **Backward compatibility** with existing content
- **Clean separation** of concerns
- **Easy adoption** for existing Piranha installations

## Role Integration

The module integrates with existing Piranha roles by storing role names in the `AllowedRoles` JSON field of transition rules. This ensures compatibility with:

- Custom role implementations
- Existing user management systems
- Role-based authorization mechanisms

## Next Steps

This foundation provides the core data layer and repositories. The next development phases would include:

1. **Workflow Services**: Business logic for state transitions and validation
2. **API Controllers**: RESTful endpoints for workflow management
3. **Manager UI**: Administrative interface for workflow configuration
4. **Content Integration**: Hooks into content saving/publishing processes
5. **Notification System**: Alerts for workflow state changes
6. **Dashboard**: Overview of workflow status and pending tasks

## Contributing

Please follow the existing Piranha CMS coding standards and include appropriate unit tests for new functionality.
