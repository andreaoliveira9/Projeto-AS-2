#nullable enable
using System.Diagnostics.Metrics;

namespace Piranha.EditorialWorkflow.Services
{
    /// <summary>
    /// Provides workflow metrics instruments
    /// </summary>
    public static class WorkflowMetricsProvider
    {
        private static readonly Meter Meter = new("Piranha.Workflow", "1.0.0");
        
        // Initialize metrics to ensure they appear in Prometheus even with zero values
        static WorkflowMetricsProvider()
        {
            // Force initialization of all metrics
            _ = TransitionCount;
            _ = TransitionFailureCount;
            _ = TransitionDuration;
            _ = InstancesCreated;
            _ = TransitionsByRole;
            _ = ContentPublished;
            _ = ContentRejected;
        }
        
        private static Counter<long>? _transitionCount;
        private static Counter<long>? _transitionFailureCount;
        private static Histogram<double>? _transitionDuration;
        private static Counter<long>? _instancesCreated;
        private static Counter<long>? _transitionsByRole;
        private static Counter<long>? _contentPublished;
        private static Counter<long>? _contentRejected;

        public static Counter<long> TransitionCount => 
            _transitionCount ??= Meter.CreateCounter<long>(
                "workflow_transitions", 
                "transitions", 
                "Total number of workflow state transitions");

        public static Counter<long> TransitionFailureCount => 
            _transitionFailureCount ??= Meter.CreateCounter<long>(
                "workflow_transition_failures", 
                "failures", 
                "Total number of failed workflow transitions");

        public static Histogram<double> TransitionDuration => 
            _transitionDuration ??= Meter.CreateHistogram<double>(
                "workflow_transition_duration", 
                "milliseconds", 
                "Duration of workflow transitions in milliseconds");

        public static Counter<long> InstancesCreated => 
            _instancesCreated ??= Meter.CreateCounter<long>(
                "workflow_instances_created", 
                "instances", 
                "Total number of workflow instances created");

        public static Counter<long> TransitionsByRole => 
            _transitionsByRole ??= Meter.CreateCounter<long>(
                "workflow_transitions_by_role", 
                "transitions", 
                "Total number of transitions performed by each role");

        public static Counter<long> ContentPublished => 
            _contentPublished ??= Meter.CreateCounter<long>(
                "workflow_content_published", 
                "items", 
                "Total number of content items published through workflow");

        public static Counter<long> ContentRejected => 
            _contentRejected ??= Meter.CreateCounter<long>(
                "workflow_content_rejected", 
                "items", 
                "Total number of content items rejected in workflow");
    }
}