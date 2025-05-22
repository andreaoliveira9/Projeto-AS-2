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
using Piranha.EditorialWorkflow.Repositories;

namespace Piranha.Repositories.EditorialWorkflow;

internal class WorkflowStateRepository : IWorkflowStateRepository
{
    private readonly IDb _db;

    public WorkflowStateRepository(IDb db)
    {
        _db = db;
    }

    public async Task<IEnumerable<Piranha.EditorialWorkflow.Models.WorkflowState>> GetByWorkflow(Guid workflowDefinitionId)
    {
        var states = await _db.Set<Data.EditorialWorkflow.WorkflowState>()
            .AsNoTracking()
            .Where(s => s.WorkflowDefinitionId == workflowDefinitionId)
            .OrderBy(s => s.SortOrder)
            .ToListAsync()
            .ConfigureAwait(false);

        return states.Select(Transform);
    }

    public async Task<Piranha.EditorialWorkflow.Models.WorkflowState> GetById(Guid id)
    {
        var state = await _db.Set<Data.EditorialWorkflow.WorkflowState>()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id)
            .ConfigureAwait(false);

        return state != null ? Transform(state) : null;
    }

    public async Task<Piranha.EditorialWorkflow.Models.WorkflowState> GetByStateId(Guid workflowDefinitionId, string stateId)
    {
        var state = await _db.Set<Data.EditorialWorkflow.WorkflowState>()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.WorkflowDefinitionId == workflowDefinitionId && s.StateId == stateId)
            .ConfigureAwait(false);

        return state != null ? Transform(state) : null;
    }

    public async Task<Piranha.EditorialWorkflow.Models.WorkflowState> GetInitialState(Guid workflowDefinitionId)
    {
        var state = await _db.Set<Data.EditorialWorkflow.WorkflowState>()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.WorkflowDefinitionId == workflowDefinitionId && s.IsInitial)
            .ConfigureAwait(false);

        return state != null ? Transform(state) : null;
    }

    public async Task<IEnumerable<Piranha.EditorialWorkflow.Models.WorkflowState>> GetPublishedStates(Guid workflowDefinitionId)
    {
        var states = await _db.Set<Data.EditorialWorkflow.WorkflowState>()
            .AsNoTracking()
            .Where(s => s.WorkflowDefinitionId == workflowDefinitionId && s.IsPublished)
            .OrderBy(s => s.SortOrder)
            .ToListAsync()
            .ConfigureAwait(false);

        return states.Select(Transform);
    }

    public async Task Save(Piranha.EditorialWorkflow.Models.WorkflowState state)
    {
        var model = await _db.Set<Data.EditorialWorkflow.WorkflowState>()
            .FirstOrDefaultAsync(s => s.Id == state.Id)
            .ConfigureAwait(false);

        if (model == null)
        {
            model = new Data.EditorialWorkflow.WorkflowState
            {
                Id = state.Id != Guid.Empty ? state.Id : Guid.NewGuid(),
                Created = DateTime.Now
            };
            state.Id = model.Id;
            await _db.Set<Data.EditorialWorkflow.WorkflowState>().AddAsync(model).ConfigureAwait(false);
        }

        model.StateId = state.StateId;
        model.Name = state.Name;
        model.Description = state.Description;
        model.IsInitial = state.IsInitial;
        model.IsPublished = state.IsPublished;
        model.IsFinal = state.IsFinal;
        model.SortOrder = state.SortOrder;
        model.ColorCode = state.ColorCode;
        model.WorkflowDefinitionId = state.WorkflowDefinitionId;

        await _db.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task Delete(Guid id)
    {
        var model = await _db.Set<Data.EditorialWorkflow.WorkflowState>()
            .FirstOrDefaultAsync(s => s.Id == id)
            .ConfigureAwait(false);

        if (model != null)
        {
            _db.Set<Data.EditorialWorkflow.WorkflowState>().Remove(model);
            await _db.SaveChangesAsync().ConfigureAwait(false);
        }
    }

    public async Task<bool> Exists(Guid id)
    {
        return await _db.Set<Data.EditorialWorkflow.WorkflowState>()
            .AnyAsync(s => s.Id == id)
            .ConfigureAwait(false);
    }

    private Piranha.EditorialWorkflow.Models.WorkflowState Transform(Data.EditorialWorkflow.WorkflowState model)
    {
        return new Piranha.EditorialWorkflow.Models.WorkflowState
        {
            Id = model.Id,
            StateId = model.StateId,
            Name = model.Name,
            Description = model.Description,
            IsInitial = model.IsInitial,
            IsPublished = model.IsPublished,
            IsFinal = model.IsFinal,
            SortOrder = model.SortOrder,
            ColorCode = model.ColorCode,
            Created = model.Created,
            WorkflowDefinitionId = model.WorkflowDefinitionId
        };
    }
}
