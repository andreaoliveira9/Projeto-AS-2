using System.Diagnostics.Metrics;

namespace MvcWeb.Telemetry
{
    public class TestMetricsService : IHostedService
    {
        private readonly ILogger<TestMetricsService> _logger;
        private static readonly Meter TestMeter = new("Piranha.Workflow", "1.0.0");
        private static readonly Counter<long> TestCounter = TestMeter.CreateCounter<long>(
            "test_workflow_counter", 
            "count", 
            "Test counter to verify metrics pipeline");
        private Timer? _timer;

        public TestMetricsService(ILogger<TestMetricsService> logger)
        {
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting test metrics service");
            
            // Increment test counter immediately
            TestCounter.Add(1, new KeyValuePair<string, object?>[] {
                new("test", "startup")
            });

            // Set up a timer to increment every 10 seconds
            _timer = new Timer(_ =>
            {
                TestCounter.Add(1, new KeyValuePair<string, object?>[] {
                    new("test", "periodic")
                });
                _logger.LogInformation("Test counter incremented");
            }, null, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Dispose();
            return Task.CompletedTask;
        }
    }
}