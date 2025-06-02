using System.Diagnostics;
using System.Diagnostics.Metrics;
using OpenTelemetry.Trace;
using Piranha;
using Piranha.Models;
using Piranha.EditorialWorkflow.Models;

#nullable enable

namespace MvcWeb.Services;

public class TelemetryService
{
    // Activity sources for tracing
    public static readonly ActivitySource WorkflowActivitySource = new("Piranha.Workflow", "1.0.0");
    public static readonly ActivitySource PageActivitySource = new("Piranha.Page", "1.0.0");
    public static readonly ActivitySource PostActivitySource = new("Piranha.Post", "1.0.0");
    public static readonly ActivitySource MediaActivitySource = new("Piranha.Media", "1.0.0");
    public static readonly ActivitySource ContentActivitySource = new("Piranha.Content", "1.0.0");

    // Meters for metrics
    public static readonly Meter WorkflowMeter = new("Piranha.Workflow.Metrics", "1.0.0");
    public static readonly Meter PageMeter = new("Piranha.Page.Metrics", "1.0.0");
    public static readonly Meter PostMeter = new("Piranha.Post.Metrics", "1.0.0");
    public static readonly Meter MediaMeter = new("Piranha.Media.Metrics", "1.0.0");
    public static readonly Meter SystemMeter = new("Piranha.System.Metrics", "1.0.0");

    // Workflow metrics
    private readonly Counter<int> _workflowStateChanges;
    private readonly Counter<int> _workflowInstancesCreated;
    private readonly Counter<int> _workflowTransitions;
    private readonly Histogram<double> _workflowTransitionDuration;
    private readonly ObservableGauge<int> _activeWorkflowInstances;
    
    // Page metrics
    private readonly Counter<int> _pagesCreated;
    private readonly Counter<int> _pagesPublished;
    private readonly Counter<int> _pagesDeleted;
    private readonly Counter<int> _pageViews;
    private readonly Histogram<double> _pageLoadTime;
    private readonly ObservableGauge<int> _totalPages;
    
    // Post metrics
    private readonly Counter<int> _postsCreated;
    private readonly Counter<int> _postsPublished;
    private readonly Counter<int> _postsDeleted;
    private readonly ObservableGauge<int> _totalPosts;
    
    // Media metrics
    private readonly Counter<int> _mediaUploaded;
    private readonly Counter<int> _mediaDeleted;
    private readonly Histogram<long> _mediaFileSize;
    private readonly ObservableGauge<long> _totalMediaSize;
    
    // System metrics
    private readonly ObservableGauge<int> _totalContentTypes;
    private readonly ObservableGauge<int> _totalUsers;
    private readonly ObservableGauge<int> _cacheHitRate;

    private readonly IApi _api;
    private readonly ILogger<TelemetryService> _logger;

    public TelemetryService(IApi api, ILogger<TelemetryService> logger)
    {
        _api = api;
        _logger = logger;

        // Initialize workflow metrics
        _workflowStateChanges = WorkflowMeter.CreateCounter<int>(
            "piranha.workflow.state_changes",
            description: "Number of workflow state changes");

        _workflowInstancesCreated = WorkflowMeter.CreateCounter<int>(
            "piranha.workflow.instances_created",
            description: "Number of workflow instances created");

        _workflowTransitions = WorkflowMeter.CreateCounter<int>(
            "piranha.workflow.transitions",
            description: "Number of workflow transitions");

        _workflowTransitionDuration = WorkflowMeter.CreateHistogram<double>(
            "piranha.workflow.transition_duration",
            unit: "ms",
            description: "Duration of workflow transitions");

        _activeWorkflowInstances = WorkflowMeter.CreateObservableGauge<int>(
            "piranha.workflow.active_instances",
            () => GetActiveWorkflowInstances().GetAwaiter().GetResult(),
            description: "Number of active workflow instances");

        // Initialize page metrics
        _pagesCreated = PageMeter.CreateCounter<int>(
            "piranha.pages.created",
            description: "Number of pages created");

        _pagesPublished = PageMeter.CreateCounter<int>(
            "piranha.pages.published",
            description: "Number of pages published");

        _pagesDeleted = PageMeter.CreateCounter<int>(
            "piranha.pages.deleted",
            description: "Number of pages deleted");

        _pageViews = PageMeter.CreateCounter<int>(
            "piranha.pages.views",
            description: "Number of page views");

        _pageLoadTime = PageMeter.CreateHistogram<double>(
            "piranha.pages.load_time",
            unit: "ms",
            description: "Page load time");

        _totalPages = PageMeter.CreateObservableGauge<int>(
            "piranha.pages.total",
            () => GetTotalPages().GetAwaiter().GetResult(),
            description: "Total number of pages");

        // Initialize post metrics
        _postsCreated = PostMeter.CreateCounter<int>(
            "piranha.posts.created",
            description: "Number of posts created");

        _postsPublished = PostMeter.CreateCounter<int>(
            "piranha.posts.published",
            description: "Number of posts published");

        _postsDeleted = PostMeter.CreateCounter<int>(
            "piranha.posts.deleted",
            description: "Number of posts deleted");

        _totalPosts = PostMeter.CreateObservableGauge<int>(
            "piranha.posts.total",
            () => GetTotalPosts().GetAwaiter().GetResult(),
            description: "Total number of posts");

        // Initialize media metrics
        _mediaUploaded = MediaMeter.CreateCounter<int>(
            "piranha.media.uploaded",
            description: "Number of media files uploaded");

        _mediaDeleted = MediaMeter.CreateCounter<int>(
            "piranha.media.deleted",
            description: "Number of media files deleted");

        _mediaFileSize = MediaMeter.CreateHistogram<long>(
            "piranha.media.file_size",
            unit: "bytes",
            description: "Size of uploaded media files");

        _totalMediaSize = MediaMeter.CreateObservableGauge<long>(
            "piranha.media.total_size",
            () => GetTotalMediaSize().GetAwaiter().GetResult(),
            description: "Total size of all media files");

        // Initialize system metrics
        _totalContentTypes = SystemMeter.CreateObservableGauge<int>(
            "piranha.system.content_types",
            () => GetTotalContentTypes().GetAwaiter().GetResult(),
            description: "Total number of content types");

        _totalUsers = SystemMeter.CreateObservableGauge<int>(
            "piranha.system.users",
            () => GetTotalUsers().GetAwaiter().GetResult(),
            description: "Total number of users");

        _cacheHitRate = SystemMeter.CreateObservableGauge<int>(
            "piranha.system.cache_hit_rate",
            () => GetCacheHitRate().GetAwaiter().GetResult(),
            description: "Cache hit rate percentage");
    }

    // Workflow tracing methods
    public Activity? StartWorkflowActivity(string operationName, WorkflowInstance? instance = null)
    {
        var activity = WorkflowActivitySource.StartActivity(operationName, ActivityKind.Internal);
        if (activity != null && instance != null)
        {
            activity.SetTag("workflow.id", instance.Id);
            activity.SetTag("workflow.definition_id", instance.WorkflowDefinitionId);
            activity.SetTag("workflow.state", instance.CurrentState);
            activity.SetTag("workflow.content_id", instance.ContentId);
            activity.SetTag("workflow.content_type", instance.ContentType);
        }
        return activity;
    }

    public void RecordWorkflowStateChange(WorkflowInstance instance, string fromState, string toState)
    {
        using var activity = StartWorkflowActivity("WorkflowStateChange", instance);
        activity?.SetTag("workflow.from_state", fromState);
        activity?.SetTag("workflow.to_state", toState);
        
        _workflowStateChanges.Add(1, 
            new KeyValuePair<string, object?>("from_state", fromState),
            new KeyValuePair<string, object?>("to_state", toState),
            new KeyValuePair<string, object?>("content_type", instance.ContentType));
    }

    public void RecordWorkflowTransition(WorkflowInstance instance, double durationMs)
    {
        _workflowTransitions.Add(1,
            new KeyValuePair<string, object?>("content_type", instance.ContentType),
            new KeyValuePair<string, object?>("state", instance.CurrentState));
        
        _workflowTransitionDuration.Record(durationMs,
            new KeyValuePair<string, object?>("content_type", instance.ContentType));
    }

    // Page tracing methods
    public Activity? StartPageActivity(string operationName, PageBase? page = null)
    {
        var activity = PageActivitySource.StartActivity(operationName, ActivityKind.Internal);
        if (activity != null && page != null)
        {
            activity.SetTag("page.id", page.Id);
            activity.SetTag("page.type", page.TypeId);
            activity.SetTag("page.slug", page.Slug);
            activity.SetTag("page.site_id", page.SiteId);
            activity.SetTag("page.is_published", page.Published.HasValue);
        }
        return activity;
    }

    public void RecordPageCreated(PageBase page)
    {
        using var activity = StartPageActivity("PageCreated", page);
        _pagesCreated.Add(1,
            new KeyValuePair<string, object?>("page_type", page.TypeId),
            new KeyValuePair<string, object?>("site_id", page.SiteId));
    }

    public void RecordPagePublished(PageBase page)
    {
        using var activity = StartPageActivity("PagePublished", page);
        _pagesPublished.Add(1,
            new KeyValuePair<string, object?>("page_type", page.TypeId),
            new KeyValuePair<string, object?>("site_id", page.SiteId));
    }

    public void RecordPageView(string pageId, double loadTimeMs)
    {
        _pageViews.Add(1, new KeyValuePair<string, object?>("page_id", pageId));
        _pageLoadTime.Record(loadTimeMs, new KeyValuePair<string, object?>("page_id", pageId));
    }

    // Post tracing methods
    public Activity? StartPostActivity(string operationName, PostBase? post = null)
    {
        var activity = PostActivitySource.StartActivity(operationName, ActivityKind.Internal);
        if (activity != null && post != null)
        {
            activity.SetTag("post.id", post.Id);
            activity.SetTag("post.type", post.TypeId);
            activity.SetTag("post.slug", post.Slug);
            activity.SetTag("post.blog_id", post.BlogId);
            activity.SetTag("post.is_published", post.Published.HasValue);
        }
        return activity;
    }

    public void RecordPostCreated(PostBase post)
    {
        using var activity = StartPostActivity("PostCreated", post);
        _postsCreated.Add(1,
            new KeyValuePair<string, object?>("post_type", post.TypeId),
            new KeyValuePair<string, object?>("blog_id", post.BlogId));
    }

    public void RecordPostPublished(PostBase post)
    {
        using var activity = StartPostActivity("PostPublished", post);
        _postsPublished.Add(1,
            new KeyValuePair<string, object?>("post_type", post.TypeId),
            new KeyValuePair<string, object?>("blog_id", post.BlogId));
    }

    // Media tracing methods
    public Activity? StartMediaActivity(string operationName, Media? media = null)
    {
        var activity = MediaActivitySource.StartActivity(operationName, ActivityKind.Internal);
        if (activity != null && media != null)
        {
            activity.SetTag("media.id", media.Id);
            activity.SetTag("media.filename", media.Filename);
            activity.SetTag("media.type", media.Type);
            activity.SetTag("media.size", media.Size);
            activity.SetTag("media.folder_id", media.FolderId);
        }
        return activity;
    }

    public void RecordMediaUploaded(Media media)
    {
        using var activity = StartMediaActivity("MediaUploaded", media);
        _mediaUploaded.Add(1,
            new KeyValuePair<string, object?>("media_type", media.Type),
            new KeyValuePair<string, object?>("folder_id", media.FolderId?.ToString() ?? "root"));
        
        _mediaFileSize.Record(media.Size,
            new KeyValuePair<string, object?>("media_type", media.Type));
    }

    // Content tracing methods
    public Activity? StartContentActivity(string operationName, string contentType, Guid? contentId = null)
    {
        var activity = ContentActivitySource.StartActivity(operationName, ActivityKind.Internal);
        if (activity != null)
        {
            activity.SetTag("content.type", contentType);
            if (contentId.HasValue)
                activity.SetTag("content.id", contentId.Value);
        }
        return activity;
    }

    // Observable gauge callbacks
    private async Task<int> GetActiveWorkflowInstances()
    {
        try
        {
            // This would need to be implemented based on your workflow repository
            return await Task.FromResult(0); // Placeholder
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active workflow instances");
            return 0;
        }
    }

    private async Task<int> GetTotalPages()
    {
        try
        {
            var sites = await _api.Sites.GetAllAsync();
            var totalPages = 0;
            foreach (var site in sites)
            {
                var pages = await _api.Pages.GetAllAsync(site.Id);
                totalPages += pages.Count();
            }
            return totalPages;
        }
        catch (ObjectDisposedException)
        {
            // Service is being disposed, return default value
            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting total pages");
            return 0;
        }
    }

    private async Task<int> GetTotalPosts()
    {
        try
        {
            var sites = await _api.Sites.GetAllAsync();
            var totalPosts = 0;
            foreach (var site in sites)
            {
                var posts = await _api.Posts.GetAllBySiteIdAsync(site.Id);
                totalPosts += posts.Count();
            }
            return totalPosts;
        }
        catch (ObjectDisposedException)
        {
            // Service is being disposed, return default value
            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting total posts");
            return 0;
        }
    }

    private async Task<long> GetTotalMediaSize()
    {
        try
        {
            var media = await _api.Media.GetAllByFolderIdAsync(null);
            return media.Sum(m => m.Size);
        }
        catch (ObjectDisposedException)
        {
            // Service is being disposed, return default value
            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting total media size");
            return 0;
        }
    }

    private async Task<int> GetTotalContentTypes()
    {
        try
        {
            var pageTypes = await _api.PageTypes.GetAllAsync();
            var postTypes = await _api.PostTypes.GetAllAsync();
            var siteTypes = await _api.SiteTypes.GetAllAsync();
            return pageTypes.Count() + postTypes.Count() + siteTypes.Count();
        }
        catch (ObjectDisposedException)
        {
            // Service is being disposed, return default value
            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting total content types");
            return 0;
        }
    }

    private async Task<int> GetTotalUsers()
    {
        // This would need to be implemented based on your identity provider
        return await Task.FromResult(0); // Placeholder
    }

    private async Task<int> GetCacheHitRate()
    {
        // This would need to be implemented based on your cache implementation
        return await Task.FromResult(0); // Placeholder
    }
}