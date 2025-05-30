using System;
using System.Diagnostics;
using OpenTelemetry;
using OpenTelemetry.Trace;
using Piranha;
using Piranha.Telemetry;

namespace MvcWeb.Telemetry
{
    /// <summary>
    /// Custom trace enricher that adds Piranha-specific context to all spans
    /// </summary>
    public class PiranhaTraceEnricher : BaseProcessor<Activity>
    {
        private readonly IApi _api;
        
        public PiranhaTraceEnricher(IApi api)
        {
            _api = api;
        }
        
        public override void OnStart(Activity activity)
        {
            // Add Piranha CMS version
            activity.SetTag("piranha.version", Utils.GetAssemblyVersion(typeof(IApi).Assembly));
            
            // Add environment information
            activity.SetTag("piranha.environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production");
            
            // Try to add site context if available
            try
            {
                var currentSite = _api?.Sites?.GetDefaultAsync().GetAwaiter().GetResult();
                if (currentSite != null)
                {
                    activity.SetTag("piranha.site.id", currentSite.Id.ToString());
                    activity.SetTag("piranha.site.title", currentSite.Title);
                    // Culture is available on SiteContent, not Site
                }
            }
            catch
            {
                // Ignore errors when getting site context
            }
            
            // Add cache level information
            activity.SetTag("piranha.cache.level", ((int)App.CacheLevel).ToString());
            
            // Add module information
            activity.SetTag("piranha.modules.count", App.Modules.ToList().Count.ToString());
            
            // Add content type counts  
            activity.SetTag("piranha.types.page_count", App.PageTypes.Count().ToString());
            activity.SetTag("piranha.types.post_count", App.PostTypes.Count().ToString());
            activity.SetTag("piranha.types.site_count", App.SiteTypes.Count().ToString());
            
            base.OnStart(activity);
        }
    }
    
    /// <summary>
    /// Extension methods for configuring the Piranha trace enricher
    /// </summary>
    public static class PiranhaTraceEnricherExtensions
    {
        /// <summary>
        /// Adds the Piranha trace enricher to the tracer provider
        /// </summary>
        public static TracerProviderBuilder AddPiranhaEnricher(this TracerProviderBuilder builder, IApi api)
        {
            if (api == null)
                throw new ArgumentNullException(nameof(api));
                
            return builder.AddProcessor(new PiranhaTraceEnricher(api));
        }
    }
}