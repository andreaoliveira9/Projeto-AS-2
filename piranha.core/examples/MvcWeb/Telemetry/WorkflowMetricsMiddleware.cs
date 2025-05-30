#nullable enable
using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace MvcWeb.Telemetry
{
    /// <summary>
    /// Middleware to capture workflow-related HTTP requests and emit metrics
    /// </summary>
    public class WorkflowMetricsMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<WorkflowMetricsMiddleware> _logger;

        public WorkflowMetricsMiddleware(
            RequestDelegate next,
            ILogger<WorkflowMetricsMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Check if this is a workflow-related request
            if (IsWorkflowRequest(context.Request.Path))
            {
                var stopwatch = Stopwatch.StartNew();
                var originalStatusCode = context.Response.StatusCode;

                try
                {
                    await _next(context);

                    // Capture metrics based on the request
                    if (context.Request.Path.Value?.Contains("/api/editorialworkflow/transition") == true)
                    {
                        // Extract workflow details from request if possible
                        var userRole = GetUserRole(context.User);
                        var success = context.Response.StatusCode >= 200 && context.Response.StatusCode < 300;

                        // The actual state transition details would be captured in the controller
                        // This just tracks the HTTP-level metrics
                        _logger.LogDebug("Workflow transition request completed with status {StatusCode} in {ElapsedMs}ms",
                            context.Response.StatusCode, stopwatch.ElapsedMilliseconds);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in workflow request");
                    throw;
                }
            }
            else
            {
                await _next(context);
            }
        }

        private static bool IsWorkflowRequest(PathString path)
        {
            var pathValue = path.Value?.ToLower() ?? "";
            return pathValue.Contains("/api/editorialworkflow") ||
                   pathValue.Contains("/manager/workflow") ||
                   pathValue.Contains("/workflow");
        }

        private static string? GetUserRole(ClaimsPrincipal user)
        {
            if (!user.Identity?.IsAuthenticated ?? true)
                return null;

            // Get the first role claim
            var roleClaim = user.FindFirst(ClaimTypes.Role) ?? 
                           user.FindFirst("role");
            
            return roleClaim?.Value;
        }
    }

    /// <summary>
    /// Extension method to register the middleware
    /// </summary>
    public static class WorkflowMetricsMiddlewareExtensions
    {
        public static IApplicationBuilder UseWorkflowMetrics(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<WorkflowMetricsMiddleware>();
        }
    }
}