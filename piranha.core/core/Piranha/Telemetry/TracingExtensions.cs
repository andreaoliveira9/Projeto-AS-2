using System;
using System.Diagnostics;

namespace Piranha.Telemetry
{
    /// <summary>
    /// Extension methods for tracing
    /// </summary>
    public static class TracingExtensions
    {
        /// <summary>
        /// Enrich activity with HTTP information
        /// </summary>
        public static Activity EnrichWithHttp(this Activity activity, string method, string path, string userId = null)
        {
            if (activity == null)
                return activity;
                
            activity.SetTag(PiranhaTelemetry.AttributeNames.HttpMethod, method);
            activity.SetTag(PiranhaTelemetry.AttributeNames.HttpRoute, path);
            
            if (!string.IsNullOrEmpty(userId))
            {
                activity.SetTag(PiranhaTelemetry.AttributeNames.UserId, 
                    PiranhaTelemetry.MaskSensitiveData(userId, SensitiveDataType.UserId));
            }
            
            return activity;
        }
        
        /// <summary>
        /// Enrich activity with database operation information
        /// </summary>
        public static Activity EnrichWithDatabaseInfo(this Activity activity, string operation, string table, string statement = null)
        {
            if (activity == null)
                return activity;
                
            activity.SetTag(PiranhaTelemetry.AttributeNames.DbSystem, "ef_core");
            activity.SetTag(PiranhaTelemetry.AttributeNames.DbOperation, operation);
            activity.SetTag(PiranhaTelemetry.AttributeNames.DbTable, table);
            
            if (!string.IsNullOrEmpty(statement))
            {
                activity.SetTag(PiranhaTelemetry.AttributeNames.DbStatement, statement);
            }
            
            return activity;
        }
        
        /// <summary>
        /// Enrich activity with cache operation information
        /// </summary>
        public static Activity EnrichWithCacheInfo(this Activity activity, string operation, string key, bool? hit = null)
        {
            if (activity == null)
                return activity;
                
            activity.SetTag(PiranhaTelemetry.AttributeNames.CacheOperation, operation);
            activity.SetTag(PiranhaTelemetry.AttributeNames.CacheKey, key);
            
            if (hit.HasValue)
            {
                activity.SetTag(PiranhaTelemetry.AttributeNames.CacheHit, hit.Value);
            }
            
            return activity;
        }
        
        /// <summary>
        /// Enrich activity with content information
        /// </summary>
        public static Activity EnrichWithContentInfo(this Activity activity, string contentType, Guid? contentId, Guid? siteId)
        {
            if (activity == null)
                return activity;
                
            activity.SetTag(PiranhaTelemetry.AttributeNames.ContentType, contentType);
            
            if (contentId.HasValue)
            {
                activity.SetTag(PiranhaTelemetry.AttributeNames.ContentId, contentId.Value.ToString());
            }
            
            if (siteId.HasValue)
            {
                activity.SetTag(PiranhaTelemetry.AttributeNames.SiteId, siteId.Value.ToString());
            }
            
            return activity;
        }
        
        /// <summary>
        /// Set operation status based on result
        /// </summary>
        public static Activity SetOperationStatus(this Activity activity, bool success, string description = null)
        {
            if (activity == null)
                return activity;
                
            activity.SetStatus(
                success ? ActivityStatusCode.Ok : ActivityStatusCode.Error,
                description
            );
            
            return activity;
        }
    }
}