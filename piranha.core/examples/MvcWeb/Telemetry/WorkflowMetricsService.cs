#nullable enable
using System.Diagnostics.Metrics;
using Microsoft.AspNetCore.Identity;
using Piranha;
using Piranha.Audit.Events;
using Piranha.Audit.Services;
using Piranha.EditorialWorkflow.Services;
using Piranha.EditorialWorkflow.Models;

namespace MvcWeb.Telemetry
{
    /// <summary>
    /// Service for collecting and reporting additional workflow metrics not in core
    /// </summary>
    public class WorkflowMetricsService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<WorkflowMetricsService> _logger;
        private Timer? _stateDistributionTimer;

        public WorkflowMetricsService(
            IServiceProvider serviceProvider,
            ILogger<WorkflowMetricsService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting Workflow Metrics Service");

            // Set up periodic state distribution collection (every 30 seconds)
            _stateDistributionTimer = new Timer(
                CollectStateDistribution,
                null,
                TimeSpan.Zero,
                TimeSpan.FromSeconds(30));

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping Workflow Metrics Service");
            _stateDistributionTimer?.Dispose();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Records time spent in a state
        /// </summary>
        public static void RecordStateResidenceTime(
            string workflowId,
            string state,
            string contentType,
            double hours)
        {
            var tags = new KeyValuePair<string, object?>[] {
                new("workflow_id", workflowId),
                new("state", state),
                new("content_type", contentType)
            };

            WorkflowMetrics.StateResidenceTime.Record(hours, tags);
        }

        /// <summary>
        /// Records workflow completion
        /// </summary>
        public static void RecordWorkflowCompletion(
            string workflowId,
            string contentType,
            double totalHours)
        {
            var tags = new KeyValuePair<string, object?>[] {
                new("workflow_id", workflowId),
                new("content_type", contentType)
            };

            WorkflowMetrics.CompletionTime.Record(totalHours, tags);
        }

        /// <summary>
        /// Updates active instance counters
        /// </summary>
        public static void UpdateActiveInstances(
            string workflowId,
            string fromState,
            string toState)
        {
            WorkflowMetrics.ActiveInstancesByState.Add(-1, new KeyValuePair<string, object?>[] {
                new("workflow_id", workflowId),
                new("state", fromState)
            });

            WorkflowMetrics.ActiveInstancesByState.Add(1, new KeyValuePair<string, object?>[] {
                new("workflow_id", workflowId),
                new("state", toState)
            });
        }

        private async void CollectStateDistribution(object? state)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var workflowService = scope.ServiceProvider.GetService<IEditorialWorkflowService>();
                
                if (workflowService == null)
                {
                    return;
                }

                // This would need to be implemented in the actual workflow service
                // For now, we'll log that the collection would happen
                _logger.LogDebug("Would collect workflow state distribution metrics");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error collecting workflow state distribution");
            }
        }
    }

    /// <summary>
    /// Extension methods for easier metrics integration
    /// </summary>
    public static class WorkflowMetricsExtensions
    {
        /// <summary>
        /// Records a workflow state change event as metrics
        /// </summary>
        public static void RecordAsMetrics(this WorkflowStateChangedEvent evt, string? userRole = null)
        {
            if (evt.Timestamp != default && !string.IsNullOrEmpty(evt.FromState))
            {
                // Calculate time spent in previous state
                var timeInState = DateTime.UtcNow - evt.Timestamp;
                WorkflowMetricsService.RecordStateResidenceTime(
                    evt.WorkflowInstanceId.ToString(),
                    evt.FromState,
                    evt.ContentType,
                    timeInState.TotalHours);
            }

            // Update active instances counter
            if (!string.IsNullOrEmpty(evt.FromState) && !string.IsNullOrEmpty(evt.ToState) && evt.Success)
            {
                WorkflowMetricsService.UpdateActiveInstances(
                    evt.WorkflowInstanceId.ToString(),
                    evt.FromState,
                    evt.ToState);
            }
        }
    }
}