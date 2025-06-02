using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MvcWeb.Services;
using Piranha;
using Piranha.Models;

namespace MvcWeb.Controllers;

[ApiController]
[Route("api/test/posts")]
[AllowAnonymous]
public class TestPostsApiController : BaseApiController
{
    private readonly IApi _api;
    private readonly TelemetryService _telemetryService;
    private readonly ILogger<TestPostsApiController> _logger;

    public TestPostsApiController(
        IApi api,
        TelemetryService telemetryService,
        ILogger<TestPostsApiController> logger,
        IWebHostEnvironment environment) : base(environment)
    {
        _api = api;
        _telemetryService = telemetryService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetPosts()
    {
        try
        {
            if (!AllowAnonymousForTesting())
                return Unauthorized("This endpoint is only available in development/testing");

            var sites = await _api.Sites.GetAllAsync();
            var allPosts = new List<object>();

            foreach (var site in sites)
            {
                var posts = await _api.Posts.GetAllBySiteIdAsync(site.Id);
                foreach (var post in posts)
                {
                    allPosts.Add(new
                    {
                        id = post.Id,
                        title = post.Title,
                        slug = post.Slug,
                        blogId = post.BlogId,
                        typeId = post.TypeId,
                        published = post.Published,
                        created = post.Created,
                        lastModified = post.LastModified
                    });
                }
            }

            return Ok(allPosts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting posts");
            return StatusCode(500, "Error getting posts");
        }
    }

    [HttpPost]
    public async Task<ActionResult> CreateTestPost([FromBody] CreatePostRequest request)
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

            // Create a test post
            var post = await _api.Posts.CreateAsync<DynamicPost>("StandardPost");
            post.BlogId = site.Id; // Use site as blog for simplicity
            post.Title = request.Title ?? $"Test Post {DateTime.UtcNow:yyyyMMddHHmmss}";
            post.Slug = request.Slug ?? $"test-post-{DateTime.UtcNow:yyyyMMddHHmmss}";
            post.Excerpt = request.Excerpt ?? "Test post created during load testing";
            
            if (!string.IsNullOrEmpty(request.Content))
            {
                post.Regions.Add("Content", new Piranha.Extend.Fields.HtmlField
                {
                    Value = request.Content
                });
            }

            if (request.IsPublished)
            {
                post.Published = DateTime.UtcNow;
            }

            await _api.Posts.SaveAsync(post);

            // Record telemetry
            _telemetryService.RecordPostCreated(post);
            if (post.Published.HasValue)
            {
                _telemetryService.RecordPostPublished(post);
            }

            _logger.LogInformation("Test post created: {PostId} - {Title}", post.Id, post.Title);

            return Ok(new
            {
                id = post.Id,
                title = post.Title,
                slug = post.Slug,
                published = post.Published,
                message = "Post created successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating test post");
            return StatusCode(500, "Error creating test post");
        }
    }

    public class CreatePostRequest
    {
        public string? Title { get; set; }
        public string? Slug { get; set; }
        public string? Excerpt { get; set; }
        public string? Content { get; set; }
        public bool IsPublished { get; set; }
    }
}