/*
 * Copyright (c) .NET Foundation and Contributors
 *
 * This software may be modified and distributed under the terms
 * of the MIT license. See the LICENSE file for details.
 *
 * https://github.com/piranhacms/piranha.core
 *
 */

#nullable enable

using Microsoft.EntityFrameworkCore;
using Piranha.Audit.Repositories;

namespace Piranha.Repositories.Audit;
/// <summary>
/// Entity Framework implementation of the state change record repository.
/// Focused on saving audit records and retrieving by content.
/// </summary>
public sealed class StateChangeRecordRepository : IStateChangeRecordRepository
{
    private readonly IDb _db;

    public StateChangeRecordRepository(IDb db)
    {
        _db = db;
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
    public async Task SaveAsync(Piranha.Audit.Models.StateChangeRecord stateChangeRecord)
    {
        var entity = Transform(stateChangeRecord);
        
        var existing = await _db.Set<Piranha.Data.Audit.StateChangeRecord>().FirstOrDefaultAsync(x => x.Id == entity.Id);
        if (existing != null)
        {
            // Update existing
            existing.ContentId = entity.ContentId;
            existing.ContentName = entity.ContentName;
            existing.FromState = entity.FromState;
            existing.ToState = entity.ToState;
            existing.transitionDescription = entity.transitionDescription;
            existing.approvedBy = entity.approvedBy;
            existing.Timestamp = entity.Timestamp;
            existing.Comments = entity.Comments;
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

    private static Piranha.Audit.Models.StateChangeRecord Transform(Data.Audit.StateChangeRecord model)
    {
        return new Piranha.Audit.Models.StateChangeRecord
        {
            Id = model.Id,
            ContentId = model.ContentId,
            ContentName = model.ContentName,
            FromState = model.FromState,
            ToState = model.ToState,
            transitionDescription = model.transitionDescription,
            approvedBy = model.approvedBy,
            Timestamp = model.Timestamp,
            Comments = model.Comments,
            Success = model.Success,
            ErrorMessage = model.ErrorMessage
        };
    }

    private static Piranha.Data.Audit.StateChangeRecord Transform(Piranha.Audit.Models.StateChangeRecord model)
    {
        return new Piranha.Data.Audit.StateChangeRecord
        {
            Id = model.Id,
            ContentId = model.ContentId,
            ContentName = model.ContentName,
            FromState = model.FromState,
            ToState = model.ToState,
            transitionDescription = model.transitionDescription,
            approvedBy = model.approvedBy,
            Timestamp = model.Timestamp,
            Comments = model.Comments,
            Success = model.Success,
            ErrorMessage = model.ErrorMessage
        };
    }
}
