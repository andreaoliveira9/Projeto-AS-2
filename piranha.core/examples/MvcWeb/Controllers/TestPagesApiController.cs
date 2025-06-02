using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MvcWeb.Services;
using Piranha;
using Piranha.Models;

namespace MvcWeb.Controllers;

[ApiController]
[Route("api/test/pages")]
[AllowAnonymous]
public class TestPagesApiController : BaseApiController
{
    private readonly IApi _api;
    private readonly TelemetryService _telemetryService;
    private readonly ILogger<TestPagesApiController> _logger;

    public TestPagesApiController(
        IApi api,
        TelemetryService telemetryService,
        ILogger<TestPagesApiController> logger,
        IWebHostEnvironment environment) : base(environment)
    {
        _api = api;
        _telemetryService = telemetryService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetPages()
    {
        try
        {
            if (!AllowAnonymousForTesting())
                return Unauthorized("This endpoint is only available in development/testing");

            var sites = await _api.Sites.GetAllAsync();
            var allPages = new List<object>();

            foreach (var site in sites)
            {
                var pages = await _api.Pages.GetAllAsync(site.Id);
                foreach (var page in pages)
                {
                    allPages.Add(new
                    {
                        id = page.Id,
                        title = page.Title,
                        slug = page.Slug,
                        siteId = page.SiteId,
                        typeId = page.TypeId,
                        published = page.Published,
                        created = page.Created,
                        lastModified = page.LastModified
                    });
                }
            }

            return Ok(allPages);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pages");
            return StatusCode(500, "Error getting pages");
        }
    }

    [HttpPost]
    public async Task<ActionResult> CreateTestPage([FromBody] CreatePageRequest request)
    {
        try
        {
            if (!AllowAnonymousForTesting())
                return Unauthorized("This endpoint is only available in development/testing");

            // Get the first site or create default
            var sites = await _api.Sites.GetAllAsync();
            var site = sites.FirstOrDefault();
            
            if (site == null)
            {
                return BadRequest("No sites available");
            }

            // Create a test page
            var page = await _api.Pages.CreateAsync<DynamicPage>("StandardPage");
            page.SiteId = site.Id;
            page.Title = request.Title ?? $"Test Page {DateTime.UtcNow:yyyyMMddHHmmss}";
            page.Slug = request.Slug ?? $"test-page-{DateTime.UtcNow:yyyyMMddHHmmss}";
            
            if (!string.IsNullOrEmpty(request.Content))
            {
                page.Regions.Add("Content", new Piranha.Extend.Fields.HtmlField
                {
                    Value = request.Content
                });
            }

            if (request.IsPublished)
            {
                page.Published = DateTime.UtcNow;
            }

            await _api.Pages.SaveAsync(page);

            // Record telemetry
            _telemetryService.RecordPageCreated(page);
            if (page.Published.HasValue)
            {
                _telemetryService.RecordPagePublished(page);
            }

            _logger.LogInformation("Test page created: {PageId} - {Title}", page.Id, page.Title);

            return Ok(new
            {
                id = page.Id,
                title = page.Title,
                slug = page.Slug,
                published = page.Published,
                message = "Page created successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating test page");
            return StatusCode(500, "Error creating test page");
        }
    }

    public class CreatePageRequest
    {
        public string? Title { get; set; }
        public string? Slug { get; set; }
        public string? Content { get; set; }
        public bool IsPublished { get; set; }
    }
}