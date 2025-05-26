using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MvcWeb.Models;
using Newtonsoft.Json;
using Piranha.EditorialWorkflow.Models;
using Piranha.EditorialWorkflow.Repositories;
using Piranha.Manager;

namespace MvcWeb.Controllers;

[Route("manager/workflow")]
[Authorize(Policy = Permission.Admin)]
public class WorkflowController : Controller
{
    private readonly IWorkflowDefinitionRepository _workflowRepo;
    private readonly IWorkflowStateRepository _stateRepo;
    private readonly ITransitionRuleRepository _transitionRepo;

    public WorkflowController(
        IWorkflowDefinitionRepository workflowRepo,
        IWorkflowStateRepository stateRepo,
        ITransitionRuleRepository transitionRepo)
    {
        _workflowRepo = workflowRepo;
        _stateRepo = stateRepo;
        _transitionRepo = transitionRepo;
    }

    [Route("")]
    public async Task<IActionResult> Index()
    {
        var model = new WorkflowManagementViewModel();
        try
        {
            var workflows = await _workflowRepo.GetAll();
            model.Workflows = workflows.ToList();
        }
        catch (Exception ex)
        {
            model.ErrorMessage = $"Error loading workflows: {ex.Message}";
        }
        return View("~/Views/Workflow/Index.cshtml", model);
    }

    [HttpGet("create")]
    public IActionResult Create()
    {
        var model = new CreateWorkflowViewModel
        {
            WorkflowJson = GetDefaultWorkflowJson()
        };
        return View("~/Views/Workflow/Create.cshtml", model);
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create([FromForm] CreateWorkflowViewModel model)
    {
        try
        {
            var workflowDefinition = JsonConvert.DeserializeObject<WorkflowJsonDefinition>(model.WorkflowJson);
            if (workflowDefinition == null)
            {
                model.ErrorMessage = "Invalid workflow JSON";
                return View("~/Views/Workflow/Create.cshtml", model);
            }

            var workflowId = Guid.NewGuid();
            var workflow = new WorkflowDefinition
            {
                Id = workflowId,
                Name = workflowDefinition.WorkflowName,
                Description = workflowDefinition.Description,
                IsActive = true,
                CreatedBy = User.Identity?.Name ?? "system",
                Created = DateTime.UtcNow,
                LastModified = DateTime.UtcNow
            };

            await _workflowRepo.Save(workflow);

            // Create states
            var stateMapping = new Dictionary<string, Guid>();
            var sortOrder = 1;

            foreach (var stateDef in workflowDefinition.States)
            {
                var stateId = Guid.NewGuid();
                stateMapping[stateDef.Id] = stateId;

                var state = new WorkflowState
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

                await _stateRepo.Save(state);
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
                        var transition = new TransitionRule
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

                        await _transitionRepo.Save(transition);
                    }
                }
            }

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            model.ErrorMessage = $"Error creating workflow: {ex.Message}";
            return View("~/Views/Workflow/Create.cshtml", model);
        }
    }

    private string GetDefaultWorkflowJson()
    {
        var defaultWorkflow = new
        {
            workflowName = "Standard Editorial Workflow",
            description = "Standard editorial workflow with draft, review, approval, and publish stages",
            states = new[] {
                new {
                    id = "draft",
                    name = "Draft",
                    description = "Initial content creation phase",
                    isInitial = true,
                    isPublished = false,
                    transitions = new[] {
                        new {
                            toState = "review",
                            roles = new[] { "Editor", "Admin" },
                            comment = (string?)null
                        }
                    }
                },
                new {
                    id = "review",
                    name = "Review",
                    description = "Content being reviewed by editors",
                    isInitial = false,
                    isPublished = false,
                    transitions = new[] {
                        new {
                            toState = "draft",
                            roles = new[] { "Editor", "Admin" },
                            comment = "Requires revision"
                        },
                        new {
                            toState = "approved",
                            roles = new[] { "Editor", "Admin" },
                            comment = "Approved for publication"
                        }
                    }
                },
                new {
                    id = "approved",
                    name = "Approved",
                    description = "Content approved and ready for publication",
                    isInitial = false,
                    isPublished = false,
                    transitions = new[] {
                        new {
                            toState = "published",
                            roles = new[] { "Admin" },
                            comment = "Final publication"
                        }
                    }
                },
                new {
                    id = "published",
                    name = "Published",
                    description = "Content is live on the site",
                    isInitial = false,
                    isPublished = true,
                    transitions = new[] {
                        new {
                            toState = "published",
                            roles = Array.Empty<string>(),
                            comment = (string?)null
                        }
                    }
                }
            }
        };

        return JsonConvert.SerializeObject(defaultWorkflow, Formatting.Indented);
    }
} 