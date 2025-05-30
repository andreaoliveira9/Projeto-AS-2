using System;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Piranha.Data;
using Piranha.Telemetry;

namespace Piranha.Repositories
{
    /// <summary>
    /// Base repository class with built-in tracing support
    /// </summary>
    internal abstract class BaseRepository
    {
        protected readonly IDb _db;
        
        protected BaseRepository(IDb db)
        {
            _db = db;
        }
        
        /// <summary>
        /// Execute a database query with tracing
        /// </summary>
        protected async Task<T> ExecuteWithTracingAsync<T>(
            string operation,
            string table,
            Func<Task<T>> query,
            Dictionary<string, object> additionalTags = null)
        {
            using var activity = PiranhaTelemetry.StartActivity(
                PiranhaTelemetry.ActivityNames.DatabaseOperation, 
                operation);
                
            activity?.EnrichWithDatabaseInfo(operation, table);
            
            if (additionalTags != null)
            {
                foreach (var tag in additionalTags)
                {
                    activity?.SetTag(tag.Key, tag.Value);
                }
            }
            
            try
            {
                var result = await query().ConfigureAwait(false);
                activity?.SetOperationStatus(true);
                return result;
            }
            catch (Exception ex)
            {
                activity?.RecordException(ex);
                throw;
            }
        }
        
        /// <summary>
        /// Execute a database command with tracing
        /// </summary>
        protected async Task ExecuteWithTracingAsync(
            string operation,
            string table,
            Func<Task> command,
            Dictionary<string, object> additionalTags = null)
        {
            using var activity = PiranhaTelemetry.StartActivity(
                PiranhaTelemetry.ActivityNames.DatabaseOperation, 
                operation);
                
            activity?.EnrichWithDatabaseInfo(operation, table);
            
            if (additionalTags != null)
            {
                foreach (var tag in additionalTags)
                {
                    activity?.SetTag(tag.Key, tag.Value);
                }
            }
            
            try
            {
                await command().ConfigureAwait(false);
                activity?.SetOperationStatus(true);
            }
            catch (Exception ex)
            {
                activity?.RecordException(ex);
                throw;
            }
        }
    }
}