using Microsoft.AspNetCore.Mvc;
using MvcWeb.Services;
using Piranha.Models;
using Piranha.EditorialWorkflow.Models;

namespace MvcWeb.Controllers;

[ApiController]
[Route("test-workflow-metrics")]
public class TestWorkflowController : ControllerBase
{
    private readonly TelemetryService _telemetryService;
    private readonly ILogger<TestWorkflowController> _logger;

    public TestWorkflowController(TelemetryService telemetryService, ILogger<TestWorkflowController> logger)
    {
        _telemetryService = telemetryService;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult TriggerTestWorkflowMetrics()
    {
        try
        {
            // Trigger multiple metrics for comprehensive testing
            
            // Page metrics
            _telemetryService.RecordPageCreated(new DynamicPage
            {
                Id = Guid.NewGuid(),
                TypeId = "TestPage",
                Slug = $"test-page-{DateTime.UtcNow:yyyyMMddHHmmss}",
                SiteId = Guid.NewGuid(),
                Title = "Test Page for Load Testing",
                Created = DateTime.UtcNow,
                LastModified = DateTime.UtcNow,
                Published = DateTime.UtcNow
            });

            _telemetryService.RecordPageView(Guid.NewGuid().ToString(), Random.Shared.Next(100, 2000));

            // Workflow metrics
            var testInstance = new WorkflowInstance
            {
                Id = Guid.NewGuid(),
                WorkflowDefinitionId = Guid.NewGuid(),
                ContentId = Guid.NewGuid().ToString(),
                ContentType = "Piranha.Models.PageBase",
                CurrentState = new WorkflowState { Name = "Draft" },
                CreatedBy = "test-user",
                Created = DateTime.UtcNow
            };

            _telemetryService.RecordWorkflowStateChange(testInstance, "Draft", "Review");
            _telemetryService.RecordWorkflowTransition(testInstance, Random.Shared.Next(100, 1500));

            // Media metrics
            _telemetryService.RecordMediaUploaded(new Media
            {
                Id = Guid.NewGuid(),
                Filename = $"test-image-{DateTime.UtcNow:yyyyMMddHHmmss}.jpg",
                Type = MediaType.Image,
                Size = Random.Shared.Next(50000, 2000000),
                Created = DateTime.UtcNow,
                LastModified = DateTime.UtcNow
            });

            _logger.LogInformation("Test workflow metrics triggered successfully");

            return Ok(new
            {
                success = true,
                message = "Test workflow metrics triggered successfully",
                timestamp = DateTime.UtcNow,
                metricsTriggered = new[]
                {
                    "page_created",
                    "page_view", 
                    "workflow_state_change",
                    "workflow_transition",
                    "media_uploaded"
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error triggering test workflow metrics");
            return StatusCode(500, new
            {
                success = false,
                message = "Error triggering test workflow metrics",
                error = ex.Message
            });
        }
    }
}