using System.Diagnostics;
using MvcWeb.Services;
using Piranha;

namespace MvcWeb.Middleware;

public class TelemetryMiddleware
{
    private readonly RequestDelegate _next;
    private readonly TelemetryService _telemetryService;
    private readonly ILogger<TelemetryMiddleware> _logger;

    public TelemetryMiddleware(RequestDelegate next, TelemetryService telemetryService, ILogger<TelemetryMiddleware> logger)
    {
        _next = next;
        _telemetryService = telemetryService;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IApi api)
    {
        var path = context.Request.Path.Value?.ToLower() ?? "";
        
        // Track page views for public pages
        if (!path.StartsWith("/manager") && !path.StartsWith("/api") && !path.Contains("/assets"))
        {
            var stopwatch = Stopwatch.StartNew();
            
            await _next(context);
            
            stopwatch.Stop();
            
            // If it's a successful page response, record the view
            if (context.Response.StatusCode == 200)
            {
                // Extract page ID from route if available
                var pageId = context.GetRouteValue("id")?.ToString();
                if (!string.IsNullOrEmpty(pageId))
                {
                    _telemetryService.RecordPageView(pageId, stopwatch.ElapsedMilliseconds);
                }
            }
        }
        else
        {
            await _next(context);
        }
    }
}

public static class TelemetryMiddlewareExtensions
{
    public static IApplicationBuilder UseTelemetryMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<TelemetryMiddleware>();
    }
}