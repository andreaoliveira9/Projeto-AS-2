using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Piranha.Telemetry
{
    /// <summary>
    /// OpenTelemetry instrumentation for Piranha CMS
    /// </summary>
    public static class PiranhaTelemetry
    {
        public const string ServiceName = "Piranha.CMS";
        public const string ServiceVersion = "11.0.0";
        
        public static readonly ActivitySource ActivitySource = new(ServiceName, ServiceVersion);
        
        /// <summary>
        /// Semantic convention attribute names
        /// </summary>
        public static class AttributeNames
        {
            public const string HttpMethod = "http.method";
            public const string HttpStatusCode = "http.status_code";
            public const string HttpUrl = "http.url";
            public const string HttpRoute = "http.route";
            public const string DbSystem = "db.system";
            public const string DbOperation = "db.operation";
            public const string DbStatement = "db.statement";
            public const string DbTable = "db.table";
            public const string CacheOperation = "cache.operation";
            public const string CacheKey = "cache.key";
            public const string CacheHit = "cache.hit";
            public const string ContentType = "piranha.content.type";
            public const string ContentId = "piranha.content.id";
            public const string SiteId = "piranha.site.id";
            public const string UserId = "piranha.user.id";
            public const string OperationType = "piranha.operation.type";
            public const string MediaType = "piranha.media.type";
            public const string MediaSize = "piranha.media.size";
            public const string WorkflowState = "piranha.workflow.state";
            public const string WorkflowTransition = "piranha.workflow.transition";
        }
        
        /// <summary>
        /// Activity names for different operations
        /// </summary>
        public static class ActivityNames
        {
            public const string PageOperation = "piranha.page.operation";
            public const string PostOperation = "piranha.post.operation";
            public const string MediaOperation = "piranha.media.operation";
            public const string ContentOperation = "piranha.content.operation";
            public const string CacheOperation = "piranha.cache.operation";
            public const string DatabaseOperation = "piranha.db.operation";
            public const string ApiOperation = "piranha.api.operation";
            public const string WorkflowOperation = "piranha.workflow.operation";
            public const string AuditOperation = "piranha.audit.operation";
        }
        
        /// <summary>
        /// Start a new activity with automatic operation name detection
        /// </summary>
        public static Activity StartActivity(
            string operationType,
            [CallerMemberName] string operationName = "",
            ActivityKind kind = ActivityKind.Internal)
        {
            var activityName = $"{operationType}.{operationName}";
            return ActivitySource.StartActivity(activityName, kind);
        }
        
        /// <summary>
        /// Mask sensitive data in traces
        /// </summary>
        public static string MaskSensitiveData(string input, SensitiveDataType dataType)
        {
            if (string.IsNullOrEmpty(input))
                return input;
                
            return dataType switch
            {
                SensitiveDataType.Email => MaskEmail(input),
                SensitiveDataType.Password => "***MASKED***",
                SensitiveDataType.Token => MaskToken(input),
                SensitiveDataType.UserId => MaskUserId(input),
                SensitiveDataType.IpAddress => MaskIpAddress(input),
                SensitiveDataType.Custom => MaskCustom(input),
                _ => input
            };
        }
        
        private static string MaskEmail(string email)
        {
            var parts = email.Split('@');
            if (parts.Length != 2)
                return "***@***.***";
                
            var localPart = parts[0];
            var domain = parts[1];
            
            if (localPart.Length <= 2)
                return "***@" + domain;
                
            return localPart.Substring(0, 2) + "***@" + domain;
        }
        
        private static string MaskToken(string token)
        {
            if (token.Length <= 8)
                return "***MASKED***";
                
            return token.Substring(0, 4) + "..." + token.Substring(token.Length - 4);
        }
        
        private static string MaskUserId(string userId)
        {
            if (Guid.TryParse(userId, out var guid))
            {
                return guid.ToString().Substring(0, 8) + "-****-****-****-************";
            }
            return MaskCustom(userId);
        }
        
        private static string MaskIpAddress(string ip)
        {
            var parts = ip.Split('.');
            if (parts.Length == 4)
            {
                return $"{parts[0]}.{parts[1]}.***.***";
            }
            return "***.***.***";
        }
        
        private static string MaskCustom(string input)
        {
            if (input.Length <= 4)
                return "****";
                
            return input.Substring(0, 2) + new string('*', input.Length - 4) + input.Substring(input.Length - 2);
        }
        
        /// <summary>
        /// Add standard Piranha attributes to an activity
        /// </summary>
        public static void AddPiranhaAttributes(this Activity activity, Dictionary<string, object> attributes)
        {
            if (activity == null || attributes == null)
                return;
                
            foreach (var attr in attributes)
            {
                if (attr.Value != null)
                {
                    activity.SetTag(attr.Key, attr.Value);
                }
            }
        }
        
        /// <summary>
        /// Record an exception with additional context
        /// </summary>
        public static void RecordException(this Activity activity, Exception ex, Dictionary<string, object> additionalTags = null)
        {
            if (activity == null || ex == null)
                return;
                
            activity.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity.RecordException(ex);
            
            if (additionalTags != null)
            {
                foreach (var tag in additionalTags)
                {
                    activity.SetTag($"exception.{tag.Key}", tag.Value);
                }
            }
        }
    }
    
    public enum SensitiveDataType
    {
        Email,
        Password,
        Token,
        UserId,
        IpAddress,
        Custom
    }
}