using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Piranha.Telemetry
{
    /// <summary>
    /// Configuration for Piranha telemetry
    /// </summary>
    public static class PiranhaTelemetryConfiguration
    {
        /// <summary>
        /// Default OTLP endpoint for all Piranha services
        /// </summary>
        public const string DefaultOtlpEndpoint = "http://localhost:4317";
        
        /// <summary>
        /// Get the OTLP endpoint from configuration or use default
        /// </summary>
        public static string GetOtlpEndpoint(IConfiguration configuration)
        {
            return configuration["OTEL_EXPORTER_OTLP_ENDPOINT"] ?? DefaultOtlpEndpoint;
        }
        
        /// <summary>
        /// Configure service information for telemetry
        /// </summary>
        public static (string serviceName, string serviceVersion) GetServiceInfo(string serviceName)
        {
            return (serviceName, PiranhaTelemetry.ServiceVersion);
        }
    }
}