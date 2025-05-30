using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Piranha.Telemetry;

namespace Piranha.AspNetCore.Telemetry
{
    /// <summary>
    /// Extension methods for ASP.NET Core tracing
    /// </summary>
    public static class AspNetCoreTracingExtensions
    {
        /// <summary>
        /// Enrich activity with HTTP context information
        /// </summary>
        public static Activity EnrichWithHttpContext(this Activity activity, HttpContext context)
        {
            if (activity == null || context == null)
                return activity;
                
            var request = context.Request;
            
            // Set standard HTTP attributes
            activity.SetTag(PiranhaTelemetry.AttributeNames.HttpMethod, request.Method);
            activity.SetTag(PiranhaTelemetry.AttributeNames.HttpRoute, request.Path.Value);
            activity.SetTag("http.scheme", request.Scheme);
            activity.SetTag("http.host", request.Host.Value);
            activity.SetTag("http.path", request.Path.Value);
            
            // Mask sensitive query parameters
            if (!string.IsNullOrEmpty(request.QueryString.Value))
            {
                activity.SetTag("http.query", PiranhaTelemetry.MaskSensitiveData(request.QueryString.Value, SensitiveDataType.Custom));
            }
            
            // Add masked user information
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                var userId = context.User.Identity.Name;
                activity.SetTag(PiranhaTelemetry.AttributeNames.UserId, 
                    PiranhaTelemetry.MaskSensitiveData(userId, SensitiveDataType.UserId));
            }
            
            // Set a more intuitive operation name
            var operationName = $"{request.Method} {request.Path.Value}";
            activity.DisplayName = operationName;
            
            return activity;
        }
        
        /// <summary>
        /// Create a custom operation name for better trace visibility
        /// </summary>
        public static string CreateOperationName(string service, string operation, string resourceType = null)
        {
            if (!string.IsNullOrEmpty(resourceType))
            {
                return $"{service}: {operation} {resourceType}";
            }
            return $"{service}: {operation}";
        }
    }
}