using Microsoft.AspNetCore.Mvc;
using MvcWeb.Services;
using Piranha.Models;
using Piranha.EditorialWorkflow.Models;

namespace MvcWeb.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MetricsTestController : ControllerBase
{
    private readonly TelemetryService _telemetryService;
    private readonly ILogger<MetricsTestController> _logger;

    public MetricsTestController(TelemetryService telemetryService, ILogger<MetricsTestController> logger)
    {
        _telemetryService = telemetryService;
        _logger = logger;
    }

    [HttpPost("trigger-page-metrics")]
    public IActionResult TriggerPageMetrics()
    {
        // Create a fake page for testing using DynamicPage
        var testPage = new DynamicPage
        {
            Id = Guid.NewGuid(),
            TypeId = "TestPage",
            Slug = $"test-page-{DateTime.UtcNow:yyyyMMddHHmmss}",
            SiteId = Guid.NewGuid(),
            Title = "Test Page for Metrics",
            Created = DateTime.UtcNow,
            LastModified = DateTime.UtcNow,
            Published = DateTime.UtcNow
        };

        // Trigger page metrics
        _telemetryService.RecordPageCreated(testPage);
        _telemetryService.RecordPagePublished(testPage);
        _telemetryService.RecordPageView(testPage.Id.ToString(), Random.Shared.Next(100, 2000));

        _logger.LogInformation("Triggered page metrics for test page {PageId}", testPage.Id);

        return Ok(new { message = "Page metrics triggered", pageId = testPage.Id });
    }

    [HttpPost("trigger-workflow-metrics")]
    public IActionResult TriggerWorkflowMetrics()
    {
        // Create a fake workflow instance for testing
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

        // Trigger workflow metrics
        _telemetryService.RecordWorkflowStateChange(testInstance, "Draft", "Review");
        _telemetryService.RecordWorkflowTransition(testInstance, Random.Shared.Next(100, 1500));

        _logger.LogInformation("Triggered workflow metrics for test instance {InstanceId}", testInstance.Id);

        return Ok(new { message = "Workflow metrics triggered", instanceId = testInstance.Id });
    }

    [HttpPost("trigger-media-metrics")]
    public IActionResult TriggerMediaMetrics()
    {
        // Create a fake media for testing
        var testMedia = new Media
        {
            Id = Guid.NewGuid(),
            Filename = $"test-image-{DateTime.UtcNow:yyyyMMddHHmmss}.jpg",
            Type = MediaType.Image,
            Size = Random.Shared.Next(50000, 2000000),
            Created = DateTime.UtcNow,
            LastModified = DateTime.UtcNow
        };

        // Trigger media metrics
        _telemetryService.RecordMediaUploaded(testMedia);

        _logger.LogInformation("Triggered media metrics for test media {MediaId}", testMedia.Id);

        return Ok(new { message = "Media metrics triggered", mediaId = testMedia.Id });
    }

    [HttpPost("trigger-all-metrics")]
    public IActionResult TriggerAllMetrics()
    {
        TriggerPageMetrics();
        TriggerWorkflowMetrics();
        TriggerMediaMetrics();

        return Ok(new { message = "All metrics triggered successfully" });
    }

    [HttpGet("metrics-status")]
    public IActionResult GetMetricsStatus()
    {
        return Ok(new
        {
            message = "Metrics test endpoints are working",
            endpoints = new[]
            {
                "/api/metricstest/trigger-page-metrics",
                "/api/metricstest/trigger-workflow-metrics", 
                "/api/metricstest/trigger-media-metrics",
                "/api/metricstest/trigger-all-metrics"
            },
            prometheusEndpoint = "/metrics"
        });
    }
}