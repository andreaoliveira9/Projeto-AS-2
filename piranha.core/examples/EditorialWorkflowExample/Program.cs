/*
 * Copyright (c) .NET Foundation and Contributors
 *
 * This software may be modified and distributed under the terms
 * of the MIT license. See the LICENSE file for details.
 *
 * https://github.com/piranhacms/piranha.core
 *
 */

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Piranha.Data.EF.EditorialWorkflow;
using Piranha.EditorialWorkflow.Repositories;
using static Piranha.Data.EditorialWorkflow.EditorialWorkflowDbExtensions;
using WorkflowDefinitionModel = Piranha.EditorialWorkflow.Models.WorkflowDefinition;
using WorkflowStateModel = Piranha.EditorialWorkflow.Models.WorkflowState;
using TransitionRuleModel = Piranha.EditorialWorkflow.Models.TransitionRule;
using WorkflowInstanceModel = Piranha.EditorialWorkflow.Models.WorkflowInstance;
using WorkflowInstanceStatusModel = Piranha.EditorialWorkflow.Models.WorkflowInstanceStatus;

namespace EditorialWorkflowExample;

/// <summary>
/// Simplified DbContext for demonstration (without Piranha Db inheritance)
/// </summary>
public class SimpleWorkflowDb : DbContext
{
    public DbSet<Piranha.Data.EditorialWorkflow.WorkflowDefinition> WorkflowDefinitions { get; set; }
    public DbSet<Piranha.Data.EditorialWorkflow.WorkflowState> WorkflowStates { get; set; }
    public DbSet<Piranha.Data.EditorialWorkflow.TransitionRule> TransitionRules { get; set; }
    public DbSet<Piranha.Data.EditorialWorkflow.WorkflowInstance> WorkflowInstances { get; set; }
    public DbSet<Piranha.Data.EditorialWorkflow.WorkflowContentExtension> WorkflowContentExtensions { get; set; }

    public SimpleWorkflowDb(DbContextOptions<SimpleWorkflowDb> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ConfigureEditorialWorkflow();
    }
}

/// <summary>
/// Data structure to match your JSON example
/// </summary>
public class WorkflowJsonDefinition
{
    public string WorkflowName { get; set; } = "";
    public string Description { get; set; } = "";
    public List<StateDefinition> States { get; set; } = new();
}

public class StateDefinition
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public bool IsInitial { get; set; }
    public bool IsPublished { get; set; }
    public List<TransitionDefinition> Transitions { get; set; } = new();
}

public class TransitionDefinition
{
    public string ToState { get; set; } = "";
    public List<string> Roles { get; set; } = new();
    public string Comment { get; set; } = "";
}

/// <summary>
/// Simple implementation of IDb for the Editorial Workflow repositories
/// </summary>
public class SimpleDbWrapper : Piranha.IDb
{
    private readonly SimpleWorkflowDb _context;

    public SimpleDbWrapper(SimpleWorkflowDb context)
    {
        _context = context;
    }

    public DbSet<T> Set<T>() where T : class
    {
        return _context.Set<T>();
    }

    public int SaveChanges()
    {
        return _context.SaveChanges();
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    // We don't need to implement all IDb properties for this example
    public DbSet<Piranha.Data.Alias> Aliases { get; set; }
    public DbSet<Piranha.Data.Block> Blocks { get; set; }
    public DbSet<Piranha.Data.BlockField> BlockFields { get; set; }
    public DbSet<Piranha.Data.Category> Categories { get; set; }
    public DbSet<Piranha.Data.Content> Content { get; set; }
    public DbSet<Piranha.Data.ContentBlock> ContentBlocks { get; set; }
    public DbSet<Piranha.Data.ContentBlockField> ContentBlockFields { get; set; }
    public DbSet<Piranha.Data.ContentBlockFieldTranslation> ContentBlockFieldTranslations { get; set; }
    public DbSet<Piranha.Data.ContentField> ContentFields { get; set; }
    public DbSet<Piranha.Data.ContentFieldTranslation> ContentFieldTranslations { get; set; }
    public DbSet<Piranha.Data.ContentTaxonomy> ContentTaxonomies { get; set; }
    public DbSet<Piranha.Data.ContentTranslation> ContentTranslations { get; set; }
    public DbSet<Piranha.Data.ContentGroup> ContentGroups { get; set; }
    public DbSet<Piranha.Data.ContentType> ContentTypes { get; set; }
    public DbSet<Piranha.Data.Language> Languages { get; set; }
    public DbSet<Piranha.Data.Media> Media { get; set; }
    public DbSet<Piranha.Data.MediaFolder> MediaFolders { get; set; }
    public DbSet<Piranha.Data.MediaVersion> MediaVersions { get; set; }
    public DbSet<Piranha.Data.Page> Pages { get; set; }
    public DbSet<Piranha.Data.PageBlock> PageBlocks { get; set; }
    public DbSet<Piranha.Data.PageComment> PageComments { get; set; }
    public DbSet<Piranha.Data.PageField> PageFields { get; set; }
    public DbSet<Piranha.Data.PagePermission> PagePermissions { get; set; }
    public DbSet<Piranha.Data.PageRevision> PageRevisions { get; set; }
    public DbSet<Piranha.Data.PageType> PageTypes { get; set; }
    public DbSet<Piranha.Data.Param> Params { get; set; }
    public DbSet<Piranha.Data.Post> Posts { get; set; }
    public DbSet<Piranha.Data.PostBlock> PostBlocks { get; set; }
    public DbSet<Piranha.Data.PostComment> PostComments { get; set; }
    public DbSet<Piranha.Data.PostField> PostFields { get; set; }
    public DbSet<Piranha.Data.PostPermission> PostPermissions { get; set; }
    public DbSet<Piranha.Data.PostRevision> PostRevisions { get; set; }
    public DbSet<Piranha.Data.PostTag> PostTags { get; set; }
    public DbSet<Piranha.Data.PostType> PostTypes { get; set; }
    public DbSet<Piranha.Data.Site> Sites { get; set; }
    public DbSet<Piranha.Data.SiteField> SiteFields { get; set; }
    public DbSet<Piranha.Data.SiteType> SiteTypes { get; set; }
    public DbSet<Piranha.Data.Tag> Tags { get; set; }
    public DbSet<Piranha.Data.Taxonomy> Taxonomies { get; set; }
}

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("🚀 Piranha Editorial Workflow Example");
        Console.WriteLine("=====================================");

        // Setup services
        var services = new ServiceCollection();

        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));
        services.AddDbContext<SimpleWorkflowDb>(options =>
            options.UseInMemoryDatabase("EditorialWorkflowExample"));
        
        // Register our simple wrapper
        services.AddScoped<Piranha.IDb>(provider =>
        {
            var context = provider.GetRequiredService<SimpleWorkflowDb>();
            return new SimpleDbWrapper(context);
        });

        services.AddEditorialWorkflowRepositories();

        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

        try
        {
            // Ensure database is created
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<SimpleWorkflowDb>();
            await dbContext.Database.EnsureCreatedAsync();

            // Get repositories
            var workflowRepo = serviceProvider.GetRequiredService<IWorkflowDefinitionRepository>();
            var stateRepo = serviceProvider.GetRequiredService<IWorkflowStateRepository>();
            var transitionRepo = serviceProvider.GetRequiredService<ITransitionRuleRepository>();
            var instanceRepo = serviceProvider.GetRequiredService<IWorkflowInstanceRepository>();

            logger.LogInformation("✅ Services configured successfully");

            // Create your JSON workflow
            var jsonWorkflow = @"{
                ""workflowName"": ""Standard Editorial Workflow"",
                ""description"": ""Standard editorial workflow with draft, review, approval, and publish stages"",
                ""states"": [
                    {
                        ""id"": ""draft"",
                        ""name"": ""Draft"",
                        ""description"": ""Initial content creation phase"",
                        ""isInitial"": true,
                        ""transitions"": [
                            {
                                ""toState"": ""review"",
                                ""roles"": [""Editor"", ""Admin""]
                            }
                        ]
                    },
                    {
                        ""id"": ""review"",
                        ""name"": ""Review"",
                        ""description"": ""Content being reviewed by editors"",
                        ""transitions"": [
                            {
                                ""toState"": ""draft"",
                                ""roles"": [""Editor"", ""Admin""],
                                ""comment"": ""Requires revision""
                            },
                            {
                                ""toState"": ""approved"",
                                ""roles"": [""Editor"", ""Admin""],
                                ""comment"": ""Approved for publication""
                            }
                        ]
                    },
                    {
                        ""id"": ""approved"",
                        ""name"": ""Approved"",
                        ""description"": ""Content approved and ready for publication"",
                        ""transitions"": [
                            {
                                ""toState"": ""published"",
                                ""roles"": [""Admin""],
                                ""comment"": ""Final publication""
                            }
                        ]
                    },
                    {
                        ""id"": ""published"",
                        ""name"": ""Published"",
                        ""description"": ""Content is live on the site"",
                        ""isPublished"": true
                    }
                ]
            }";

            // Parse and create workflow
            logger.LogInformation("🔧 Creating workflow from JSON...");
            var workflowDefinition = JsonConvert.DeserializeObject<WorkflowJsonDefinition>(jsonWorkflow);
            
            if (workflowDefinition == null)
            {
                throw new Exception("Failed to parse JSON workflow definition");
            }

            var workflowId = Guid.NewGuid();
            var workflow = new WorkflowDefinitionModel
            {
                Id = workflowId,
                Name = workflowDefinition.WorkflowName,
                Description = workflowDefinition.Description,
                IsActive = true,
                CreatedBy = "system",
                Created = DateTime.UtcNow,
                LastModified = DateTime.UtcNow
            };

            await workflowRepo.Save(workflow);
            logger.LogInformation("✅ Created workflow: {WorkflowName}", workflow.Name);

            // Create states
            var stateMapping = new Dictionary<string, Guid>();
            var sortOrder = 1;

            foreach (var stateDef in workflowDefinition.States)
            {
                var stateId = Guid.NewGuid();
                stateMapping[stateDef.Id] = stateId;

                var state = new WorkflowStateModel
                {
                    Id = stateId,
                    StateId = stateDef.Id,
                    Name = stateDef.Name,
                    Description = stateDef.Description,
                    IsInitial = stateDef.IsInitial,
                    IsPublished = stateDef.IsPublished,
                    SortOrder = sortOrder++,
                    WorkflowDefinitionId = workflowId,
                    Created = DateTime.UtcNow
                };

                await stateRepo.Save(state);
                logger.LogInformation("✅ Created state: {StateName} ({StateId})", state.Name, state.StateId);
            }

            // Create transitions
            foreach (var stateDef in workflowDefinition.States)
            {
                var fromStateId = stateMapping[stateDef.Id];
                var transitionSortOrder = 1;

                foreach (var transitionDef in stateDef.Transitions)
                {
                    if (stateMapping.TryGetValue(transitionDef.ToState, out var toStateId))
                    {
                        var transition = new TransitionRuleModel
                        {
                            Id = Guid.NewGuid(),
                            FromStateId = fromStateId,
                            ToStateId = toStateId,
                            AllowedRoles = JsonConvert.SerializeObject(transitionDef.Roles),
                            CommentTemplate = transitionDef.Comment,
                            RequiresComment = !string.IsNullOrEmpty(transitionDef.Comment),
                            IsActive = true,
                            SortOrder = transitionSortOrder++,
                            Created = DateTime.UtcNow
                        };

                        await transitionRepo.Save(transition);
                        logger.LogInformation("✅ Created transition: {FromState} -> {ToState}", stateDef.Id, transitionDef.ToState);
                    }
                }
            }

            // Test workflow retrieval
            logger.LogInformation("🔍 Testing workflow retrieval...");
            var completeWorkflow = await workflowRepo.GetWithStatesAndTransitions(workflowId);
            
            if (completeWorkflow != null)
            {
                logger.LogInformation("✅ Retrieved workflow: {WorkflowName}", completeWorkflow.Name);
                logger.LogInformation("   - States: {StateCount}", completeWorkflow.States.Count);
                
                var initialState = completeWorkflow.States.FirstOrDefault(s => s.IsInitial);
                if (initialState != null)
                {
                    logger.LogInformation("   - Initial state: {StateName}", initialState.Name);
                }

                var publishedState = completeWorkflow.States.FirstOrDefault(s => s.IsPublished);
                if (publishedState != null)
                {
                    logger.LogInformation("   - Published state: {StateName}", publishedState.Name);
                }
            }

            // Test workflow instance creation
            logger.LogInformation("📝 Testing workflow instance creation...");
            var contentId = Guid.NewGuid().ToString();
            var draftStateId = stateMapping["draft"];

            var instance = new WorkflowInstanceModel
            {
                Id = Guid.NewGuid(),
                ContentId = contentId,
                ContentType = "Page",
                ContentTitle = "Test Article",
                WorkflowDefinitionId = workflowId,
                CurrentStateId = draftStateId,
                CreatedBy = "test-user",
                Status = WorkflowInstanceStatusModel.Active,
                Created = DateTime.UtcNow,
                LastModified = DateTime.UtcNow
            };

            await instanceRepo.Save(instance);
            logger.LogInformation("✅ Created workflow instance for content: {ContentTitle}", instance.ContentTitle);

            // Test instance retrieval
            var retrievedInstance = await instanceRepo.GetByContent(contentId);
            if (retrievedInstance != null)
            {
                logger.LogInformation("✅ Retrieved instance: {ContentTitle}", retrievedInstance.ContentTitle);
                logger.LogInformation("   - Status: {Status}", retrievedInstance.Status);
                logger.LogInformation("   - Current State ID: {StateId}", retrievedInstance.CurrentStateId);
            }

            // Test transitions
            logger.LogInformation("🔄 Testing state transitions...");
            var transitions = await transitionRepo.GetActiveTransitions(draftStateId);
            logger.LogInformation("✅ Found {TransitionCount} available transitions from draft state", transitions.Count());

            foreach (var transition in transitions)
            {
                logger.LogInformation("   - Can transition to state ID: {ToStateId}", transition.ToStateId);
                logger.LogInformation("   - Allowed roles: {AllowedRoles}", transition.AllowedRoles);
            }

            logger.LogInformation("");
            logger.LogInformation("🎉 All tests completed successfully!");
            logger.LogInformation("📋 Summary:");
            logger.LogInformation("   - Workflow created: {WorkflowName}", workflow.Name);
            logger.LogInformation("   - States created: {StateCount}", workflowDefinition.States.Count);
            logger.LogInformation("   - Transitions created: {TransitionCount}", workflowDefinition.States.Sum(s => s.Transitions.Count));
            logger.LogInformation("   - Instance created for content: {ContentTitle}", instance.ContentTitle);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error occurred during testing");
            Environment.Exit(1);
        }
    }
}
