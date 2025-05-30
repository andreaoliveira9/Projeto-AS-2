#nullable enable
using Piranha.Audit.Events;
using Piranha.EditorialWorkflow.Models;
using Piranha.EditorialWorkflow.Services;
using System.Collections.Concurrent;

namespace MvcWeb.Telemetry
{
    /// <summary>
    /// Handles workflow events and converts them to additional metrics
    /// </summary>
    public class WorkflowMetricsEventHandler : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<WorkflowMetricsEventHandler> _logger;
        private readonly ConcurrentDictionary<Guid, DateTime> _instanceCreationTimes = new();

        public WorkflowMetricsEventHandler(
            IServiceProvider serviceProvider,
            ILogger<WorkflowMetricsEventHandler> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting Workflow Metrics Event Handler");

            // Subscribe to workflow hooks/events if available
            await SubscribeToWorkflowEvents(stoppingToken);

            // Keep the service running
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        private async Task SubscribeToWorkflowEvents(CancellationToken stoppingToken)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                
                // Here we would subscribe to actual workflow events
                // For now, we'll set up a pattern that could be used
                _logger.LogInformation("Workflow metrics event handler initialized");

                // Example of how to handle events when they arrive
                // In a real implementation, this would be event-driven
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error subscribing to workflow events");
            }
        }

        /// <summary>
        /// Handles workflow state change events
        /// </summary>
        public void HandleWorkflowStateChange(WorkflowStateChangedEvent evt)
        {
            try
            {
                _logger.LogDebug("Processing workflow state change: {FromState} -> {ToState}",
                    evt.FromState, evt.ToState);

                // Get user role if available
                string? userRole = null;
                if (!string.IsNullOrEmpty(evt.UserId))
                {
                    // In a real implementation, look up the user's role
                    userRole = "editor"; // placeholder
                }

                // Record additional metrics using extension method
                evt.RecordAsMetrics(userRole);

                // Check if this is a final state to calculate completion time
                if (IsFinalState(evt.ToState) && evt.WorkflowInstanceId != default)
                {
                    if (_instanceCreationTimes.TryGetValue(evt.WorkflowInstanceId, out var creationTime))
                    {
                        var completionTime = DateTime.UtcNow - creationTime;
                        WorkflowMetricsService.RecordWorkflowCompletion(
                            evt.WorkflowInstanceId.ToString(),
                            evt.ContentType,
                            completionTime.TotalHours);

                        // Clean up the tracking
                        _instanceCreationTimes.TryRemove(evt.WorkflowInstanceId, out _);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling workflow state change event");
            }
        }

        /// <summary>
        /// Handles workflow instance creation
        /// </summary>
        public void HandleWorkflowInstanceCreated(Guid instanceId, string workflowId, string contentType, string initialState)
        {
            try
            {
                _logger.LogDebug("Processing workflow instance creation: {InstanceId}", instanceId);

                // Track creation time for completion metrics
                _instanceCreationTimes[instanceId] = DateTime.UtcNow;

                // Update active instances counter
                WorkflowMetricsService.UpdateActiveInstances(workflowId, "", initialState);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling workflow instance creation");
            }
        }

        /// <summary>
        /// Periodically collect state distribution metrics
        /// </summary>
        public async Task<Dictionary<string, Dictionary<string, long>>> GetStateDistribution()
        {
            var distribution = new Dictionary<string, Dictionary<string, long>>();

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var workflowService = scope.ServiceProvider.GetService<IEditorialWorkflowService>();

                if (workflowService != null)
                {
                    // Get all workflow definitions
                    var definitions = await workflowService.GetAllWorkflowDefinitionsAsync();

                    foreach (var definition in definitions)
                    {
                        // This would need a method to get instance counts by state
                        // For now, create a placeholder structure
                        distribution[definition.Id.ToString()] = new Dictionary<string, long>();
                        
                        if (definition.States != null)
                        {
                            foreach (var state in definition.States)
                            {
                                distribution[definition.Id.ToString()][state.StateId] = 0;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting state distribution");
            }

            return distribution;
        }

        private static bool IsFinalState(string? state)
        {
            if (string.IsNullOrEmpty(state))
                return false;

            // Common final states
            var finalStates = new[] { "published", "archived", "rejected", "completed", "closed" };
            return finalStates.Contains(state.ToLower());
        }
    }
}