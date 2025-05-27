/*
 * Copyright (c) .NET Foundation and Contributors
 *
 * This software may be modified and distributed under the terms
 * of the MIT license. See the LICENSE file for details.
 *
 * https://github.com/piranhacms/piranha.core
 *
 */

using Microsoft.EntityFrameworkCore;
using Piranha.Audit.Repositories;

namespace Piranha.Repositories.Audit
{
    /// <summary>
    /// Entity Framework implementation of the state change record repository.
    /// </summary>
    public sealed class StateChangeRecordRepository : IStateChangeRecordRepository
    {
        private readonly IDb _db;

        public StateChangeRecordRepository(IDb db)
        {
            _db = db;
        }

        /// <inheritdoc />
        public async Task<Piranha.Audit.Models.StateChangeRecord> GetByIdAsync(Guid id)
        {
            var entity = await _db.Set<Piranha.Data.Audit.StateChangeRecord>().FirstOrDefaultAsync(x => x.Id == id);
            return entity != null ? Transform(entity) : null;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Piranha.Audit.Models.StateChangeRecord>> GetByWorkflowInstanceAsync(Guid workflowInstanceId)
        {
            var entities = await _db.Set<Piranha.Data.Audit.StateChangeRecord>()
                .Where(x => x.WorkflowInstanceId == workflowInstanceId)
                .OrderBy(x => x.Timestamp)
                .ToListAsync();

            return entities.Select(Transform);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Piranha.Audit.Models.StateChangeRecord>> GetByContentAsync(Guid contentId)
        {
            var entities = await _db.Set<Piranha.Data.Audit.StateChangeRecord>()
                .Where(x => x.ContentId == contentId)
                .OrderBy(x => x.Timestamp)
                .ToListAsync();

            return entities.Select(Transform);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Piranha.Audit.Models.StateChangeRecord>> GetByUserAsync(string userId, int take = 50, int skip = 0)
        {
            var entities = await _db.Set<Piranha.Data.Audit.StateChangeRecord>()
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.Timestamp)
                .Skip(skip)
                .Take(take)
                .ToListAsync();

            return entities.Select(Transform);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Piranha.Audit.Models.StateChangeRecord>> GetByDateRangeAsync(DateTime from, DateTime to, int take = 50, int skip = 0)
        {
            var entities = await _db.Set<Piranha.Data.Audit.StateChangeRecord>()
                .Where(x => x.Timestamp >= from && x.Timestamp <= to)
                .OrderByDescending(x => x.Timestamp)
                .Skip(skip)
                .Take(take)
                .ToListAsync();

            return entities.Select(Transform);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Piranha.Audit.Models.StateChangeRecord>> GetByTransitionAsync(string fromState, string toState, int take = 50, int skip = 0)
        {
            var entities = await _db.Set<Piranha.Data.Audit.StateChangeRecord>()
                .Where(x => x.FromState == fromState && x.ToState == toState)
                .OrderByDescending(x => x.Timestamp)
                .Skip(skip)
                .Take(take)
                .ToListAsync();

            return entities.Select(Transform);
        }

        /// <inheritdoc />
        public async Task SaveAsync(Piranha.Audit.Models.StateChangeRecord stateChangeRecord)
        {
            var entity = Transform(stateChangeRecord);
            
            var existing = await _db.Set<Piranha.Data.Audit.StateChangeRecord>().FirstOrDefaultAsync(x => x.Id == entity.Id);
            if (existing != null)
            {
                // Update existing
                existing.WorkflowInstanceId = entity.WorkflowInstanceId;
                existing.ContentId = entity.ContentId;
                existing.ContentType = entity.ContentType;
                existing.FromState = entity.FromState;
                existing.ToState = entity.ToState;
                existing.UserId = entity.UserId;
                existing.Username = entity.Username;
                existing.Timestamp = entity.Timestamp;
                existing.Comments = entity.Comments;
                existing.TransitionRuleId = entity.TransitionRuleId;
                existing.Metadata = entity.Metadata;
                existing.Success = entity.Success;
                existing.ErrorMessage = entity.ErrorMessage;
            }
            else
            {
                // Add new
                _db.Set<Piranha.Data.Audit.StateChangeRecord>().Add(entity);
            }

            if (_db is DbContext dbContext)
            {
                await dbContext.SaveChangesAsync();
            }
        }

        /// <inheritdoc />
        public async Task DeleteAsync(Guid id)
        {
            var entity = await _db.Set<Piranha.Data.Audit.StateChangeRecord>().FirstOrDefaultAsync(x => x.Id == id);
            if (entity != null)
            {
                _db.Set<Piranha.Data.Audit.StateChangeRecord>().Remove(entity);
                
                if (_db is DbContext dbContext)
                {
                    await dbContext.SaveChangesAsync();
                }
            }
        }

        /// <inheritdoc />
        public async Task<int> DeleteOlderThanAsync(DateTime cutoffDate)
        {
            var entitiesToDelete = await _db.Set<Piranha.Data.Audit.StateChangeRecord>()
                .Where(x => x.Timestamp < cutoffDate)
                .ToListAsync();

            if (entitiesToDelete.Any())
            {
                _db.Set<Piranha.Data.Audit.StateChangeRecord>().RemoveRange(entitiesToDelete);
                
                if (_db is DbContext dbContext)
                {
                    await dbContext.SaveChangesAsync();
                }
            }

            return entitiesToDelete.Count;
        }

        private static Piranha.Audit.Models.StateChangeRecord Transform(Data.Audit.StateChangeRecord model)
        {
            return new Piranha.Audit.Models.StateChangeRecord
            {
                Id = model.Id,
                WorkflowInstanceId = model.WorkflowInstanceId,
                ContentId = model.ContentId,
                ContentType = model.ContentType,
                FromState = model.FromState,
                ToState = model.ToState,
                UserId = model.UserId,
                Username = model.Username,
                Timestamp = model.Timestamp,
                Comments = model.Comments,
                TransitionRuleId = model.TransitionRuleId,
                Metadata = model.Metadata,
                Success = model.Success,
                ErrorMessage = model.ErrorMessage
            };
        }

        private static Piranha.Data.Audit.StateChangeRecord Transform(Piranha.Audit.Models.StateChangeRecord model)
        {
            return new Piranha.Data.Audit.StateChangeRecord
            {
                Id = model.Id,
                WorkflowInstanceId = model.WorkflowInstanceId,
                ContentId = model.ContentId,
                ContentType = model.ContentType,
                FromState = model.FromState,
                ToState = model.ToState,
                UserId = model.UserId,
                Username = model.Username,
                Timestamp = model.Timestamp,
                Comments = model.Comments,
                TransitionRuleId = model.TransitionRuleId,
                Metadata = model.Metadata,
                Success = model.Success,
                ErrorMessage = model.ErrorMessage
            };
        }
    }
}
