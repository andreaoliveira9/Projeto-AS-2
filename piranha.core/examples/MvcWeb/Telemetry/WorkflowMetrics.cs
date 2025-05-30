#nullable enable
using System.Diagnostics.Metrics;

namespace MvcWeb.Telemetry
{
    /// <summary>
    /// Provides additional metrics for Editorial Workflow operations that are not in core
    /// </summary>
    public static class WorkflowMetrics
    {
        private static readonly Meter Meter = new("Piranha.Workflow", "1.0.0");

        /// <summary>
        /// Histogram for time content spends in each state
        /// </summary>
        public static readonly Histogram<double> StateResidenceTime = Meter.CreateHistogram<double>(
            "workflow_state_residence_time_hours",
            "hours",
            "Time content spends in each workflow state in hours");

        /// <summary>
        /// UpDownCounter for active workflow instances per state
        /// </summary>
        public static readonly UpDownCounter<long> ActiveInstancesByState = Meter.CreateUpDownCounter<long>(
            "workflow_active_instances_by_state",
            "instances",
            "Number of active workflow instances in each state");

        /// <summary>
        /// Histogram for total workflow completion time
        /// </summary>
        public static readonly Histogram<double> CompletionTime = Meter.CreateHistogram<double>(
            "workflow_completion_time_hours",
            "hours",
            "Total time from workflow start to completion in hours");

        /// <summary>
        /// ObservableGauge for current workflow distribution
        /// </summary>
        public static readonly ObservableGauge<long> CurrentStateDistribution = Meter.CreateObservableGauge<long>(
            "workflow_current_state_distribution",
            GetCurrentStateDistribution,
            "instances",
            "Current distribution of workflow instances across states");

        private static IEnumerable<Measurement<long>> GetCurrentStateDistribution()
        {
            // This would be implemented to query the actual state distribution
            // For now, returning empty to avoid compilation errors
            return Enumerable.Empty<Measurement<long>>();
        }
    }
}