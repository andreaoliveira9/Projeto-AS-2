using Piranha;
using Piranha.Models;
using Piranha.EditorialWorkflow.Models;

namespace MvcWeb.Services;

public class TelemetryHookService
{
    private readonly TelemetryService _telemetryService;
    private readonly ILogger<TelemetryHookService> _logger;

    public TelemetryHookService(TelemetryService telemetryService, ILogger<TelemetryHookService> logger)
    {
        _telemetryService = telemetryService;
        _logger = logger;
    }

    public void RegisterHooks()
    {
        // Page hooks
        App.Hooks.Pages.RegisterOnBeforeSave((page) =>
        {
            using var activity = _telemetryService.StartPageActivity("PageBeforeSave", page);
            _logger.LogInformation("Page {PageId} is being saved", page.Id);
        });

        App.Hooks.Pages.RegisterOnAfterSave((page) =>
        {
            if (page.Created == page.LastModified)
            {
                _telemetryService.RecordPageCreated(page);
                _logger.LogInformation("New page created: {PageId}", page.Id);
            }
            
            if (page.Published.HasValue && page.Published.Value >= DateTime.UtcNow.AddSeconds(-5))
            {
                _telemetryService.RecordPagePublished(page);
                _logger.LogInformation("Page published: {PageId}", page.Id);
            }
        });

        App.Hooks.Pages.RegisterOnBeforeDelete((page) =>
        {
            using var activity = _telemetryService.StartPageActivity("PageBeforeDelete", page);
            _logger.LogInformation("Page {PageId} is being deleted", page.Id);
        });

        // Post hooks
        App.Hooks.Posts.RegisterOnBeforeSave((post) =>
        {
            using var activity = _telemetryService.StartPostActivity("PostBeforeSave", post);
            _logger.LogInformation("Post {PostId} is being saved", post.Id);
        });

        App.Hooks.Posts.RegisterOnAfterSave((post) =>
        {
            if (post.Created == post.LastModified)
            {
                _telemetryService.RecordPostCreated(post);
                _logger.LogInformation("New post created: {PostId}", post.Id);
            }
            
            if (post.Published.HasValue && post.Published.Value >= DateTime.UtcNow.AddSeconds(-5))
            {
                _telemetryService.RecordPostPublished(post);
                _logger.LogInformation("Post published: {PostId}", post.Id);
            }
        });

        // Media hooks
        App.Hooks.Media.RegisterOnBeforeSave((media) =>
        {
            using var activity = _telemetryService.StartMediaActivity("MediaBeforeSave", media);
            _logger.LogInformation("Media {MediaId} is being saved", media.Id);
        });

        App.Hooks.Media.RegisterOnAfterSave((media) =>
        {
            if (media.Created == media.LastModified)
            {
                _telemetryService.RecordMediaUploaded(media);
                _logger.LogInformation("New media uploaded: {MediaId}", media.Id);
            }
        });

        _logger.LogInformation("Telemetry hooks registered successfully");
    }
}