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

namespace Piranha.Data.EF.Audit.Repositories
{
    /// <summary>
    /// Entity Framework implementation of the state change record repository.
    /// </summary>
    public sealed class StateChangeRecordRepository : IStateChangeRecordRepository
    {
        private readonly IAuditDb _db;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="db">The database context</param>
        public StateChangeRecordRepository(IAuditDb db)
        {
            _db = db;
        }

        /// <inheritdoc />
        public async Task<Piranha.Audit.Models.StateChangeRecord> GetByIdAsync(Guid id)
        {
            var entity = await _db.StateChangeRecord.FirstOrDefaultAsync(x => x.Id == id);
            return entity != null ? MapToModel(entity) : null;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Piranha.Audit.Models.StateChangeRecord>> GetByWorkflowInstanceAsync(Guid workflowInstanceId)
        {
            var entities = await _db.StateChangeRecord
                .Where(x => x.WorkflowInstanceId == workflowInstanceId)
                .OrderBy(x => x.Timestamp)
                .ToListAsync();

            return entities.Select(MapToModel);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Piranha.Audit.Models.StateChangeRecord>> GetByContentAsync(Guid contentId)
        {
            var entities = await _db.StateChangeRecord
                .Where(x => x.ContentId == contentId)
                .OrderBy(x => x.Timestamp)
                .ToListAsync();

            return entities.Select(MapToModel);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Piranha.Audit.Models.StateChangeRecord>> GetByUserAsync(string userId, int take = 50, int skip = 0)
        {
            var entities = await _db.StateChangeRecord
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.Timestamp)
                .Skip(skip)
                .Take(take)
                .ToListAsync();

            return entities.Select(MapToModel);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Piranha.Audit.Models.StateChangeRecord>> GetByDateRangeAsync(DateTime from, DateTime to, int take = 50, int skip = 0)
        {
            var entities = await _db.StateChangeRecord
                .Where(x => x.Timestamp >= from && x.Timestamp <= to)
                .OrderByDescending(x => x.Timestamp)
                .Skip(skip)
                .Take(take)
                .ToListAsync();

            return entities.Select(MapToModel);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Piranha.Audit.Models.StateChangeRecord>> GetByTransitionAsync(string fromState, string toState, int take = 50, int skip = 0)
        {
            var entities = await _db.StateChangeRecord
                .Where(x => x.FromState == fromState && x.ToState == toState)
                .OrderByDescending(x => x.Timestamp)
                .Skip(skip)
                .Take(take)
                .ToListAsync();

            return entities.Select(MapToModel);
        }

        /// <inheritdoc />
        public async Task SaveAsync(Piranha.Audit.Models.StateChangeRecord stateChangeRecord)
        {
            var entity = MapToEntity(stateChangeRecord);
            
            var existing = await _db.StateChangeRecord.FirstOrDefaultAsync(x => x.Id == entity.Id);
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
                _db.StateChangeRecord.Add(entity);
            }

            if (_db is DbContext dbContext)
            {
                await dbContext.SaveChangesAsync();
            }
        }

        /// <inheritdoc />
        public async Task DeleteAsync(Guid id)
        {
            var entity = await _db.StateChangeRecord.FirstOrDefaultAsync(x => x.Id == id);
            if (entity != null)
            {
                _db.StateChangeRecord.Remove(entity);
                
                if (_db is DbContext dbContext)
                {
                    await dbContext.SaveChangesAsync();
                }
            }
        }

        /// <inheritdoc />
        public async Task<int> DeleteOlderThanAsync(DateTime cutoffDate)
        {
            var entitiesToDelete = await _db.StateChangeRecord
                .Where(x => x.Timestamp < cutoffDate)
                .ToListAsync();

            if (entitiesToDelete.Any())
            {
                _db.StateChangeRecord.RemoveRange(entitiesToDelete);
                
                if (_db is DbContext dbContext)
                {
                    await dbContext.SaveChangesAsync();
                }
            }

            return entitiesToDelete.Count;
        }

        private static Piranha.Audit.Models.StateChangeRecord MapToModel(Piranha.Data.EF.Audit.StateChangeRecord entity)
        {
            return new Piranha.Audit.Models.StateChangeRecord
            {
                Id = entity.Id,
                WorkflowInstanceId = entity.WorkflowInstanceId,
                ContentId = entity.ContentId,
                ContentType = entity.ContentType,
                FromState = entity.FromState,
                ToState = entity.ToState,
                UserId = entity.UserId,
                Username = entity.Username,
                Timestamp = entity.Timestamp,
                Comments = entity.Comments,
                TransitionRuleId = entity.TransitionRuleId,
                Metadata = entity.Metadata,
                Success = entity.Success,
                ErrorMessage = entity.ErrorMessage
            };
        }

        private static Piranha.Data.EF.Audit.StateChangeRecord MapToEntity(Piranha.Audit.Models.StateChangeRecord model)
        {
            return new Piranha.Data.EF.Audit.StateChangeRecord
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
