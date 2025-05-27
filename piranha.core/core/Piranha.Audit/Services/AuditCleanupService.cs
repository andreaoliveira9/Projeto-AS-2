/*
 * Copyright (c) .NET Foundation and Contributors
 *
 * This software may be modified and distributed under the terms
 * of the MIT license. See the LICENSE file for details.
 *
 * https://github.com/piranhacms/piranha.core
 *
 */

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Piranha.Audit.Extensions;

namespace Piranha.Audit.Services;

/// <summary>
/// Background service that automatically cleans up old audit records.
/// </summary>
public sealed class AuditCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AuditCleanupService> _logger;
    private readonly AuditOptions _options;

    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <param name="serviceProvider">The service provider</param>
    /// <param name="logger">The logger</param>
    /// <param name="options">The audit options</param>
    public AuditCleanupService(
        IServiceProvider serviceProvider,
        ILogger<AuditCleanupService> logger,
        AuditOptions options)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _options = options;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.EnableAutomaticCleanup)
        {
            _logger.LogInformation("Automatic cleanup is disabled");
            return;
        }

        _logger.LogInformation("Audit cleanup service started. Running every {IntervalHours} hours", 
            _options.CleanupIntervalHours);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PerformCleanupAsync(stoppingToken);
                await Task.Delay(TimeSpan.FromHours(_options.CleanupIntervalHours), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during audit cleanup");
                // Wait a shorter time before retrying on error
                await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
            }
        }

        _logger.LogInformation("Audit cleanup service stopped");
    }

    private async Task PerformCleanupAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var auditService = scope.ServiceProvider.GetRequiredService<IAuditService>();

            var deletedCount = await auditService.CleanupOldRecordsAsync(_options.DefaultRetentionDays);
            
            if (deletedCount > 0)
            {
                _logger.LogInformation("Cleanup completed. Deleted {DeletedCount} old audit records", deletedCount);
            }
            else
            {
                _logger.LogDebug("Cleanup completed. No old records to delete");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to perform audit cleanup");
            throw;
        }
    }
}
