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

internal class WorkflowDefinitionRepository : IWorkflowDefinitionRepository
{
    private readonly IDb _db;

    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <param name="db">The current db connection</param>
    public WorkflowDefinitionRepository(IDb db)
    {
        _db = db;
    }

    /// <summary>
    /// Gets all available workflow definitions.
    /// </summary>
    /// <returns>The available workflow definitions</returns>
    public async Task<IEnumerable<Piranha.EditorialWorkflow.Models.WorkflowDefinition>> GetAll()
    {
        var definitions = await _db.Set<Data.EditorialWorkflow.WorkflowDefinition>()
            .AsNoTracking()
            .OrderBy(w => w.Name)
            .ToListAsync()
            .ConfigureAwait(false);

        return definitions.Select(Transform);
    }

    /// <summary>
    /// Gets all active workflow definitions.
    /// </summary>
    /// <returns>The active workflow definitions</returns>
    public async Task<IEnumerable<Piranha.EditorialWorkflow.Models.WorkflowDefinition>> GetActive()
    {
        var definitions = await _db.Set<Data.EditorialWorkflow.WorkflowDefinition>()
            .AsNoTracking()
            .Where(w => w.IsActive)
            .OrderBy(w => w.Name)
            .ToListAsync()
            .ConfigureAwait(false);

        return definitions.Select(Transform);
    }

    /// <summary>
    /// Gets the workflow definition with the specified id.
    /// </summary>
    /// <param name="id">The unique id</param>
    /// <returns>The workflow definition</returns>
    public async Task<Piranha.EditorialWorkflow.Models.WorkflowDefinition> GetById(Guid id)
    {
        var definition = await _db.Set<Data.EditorialWorkflow.WorkflowDefinition>()
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Id == id)
            .ConfigureAwait(false);

        return definition != null ? Transform(definition) : null;
    }

    /// <summary>
    /// Gets the workflow definition with the specified name.
    /// </summary>
    /// <param name="name">The workflow name</param>
    /// <returns>The workflow definition</returns>
    public async Task<Piranha.EditorialWorkflow.Models.WorkflowDefinition> GetByName(string name)
    {
        var definition = await _db.Set<Data.EditorialWorkflow.WorkflowDefinition>()
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Name == name)
            .ConfigureAwait(false);

        return definition != null ? Transform(definition) : null;
    }

    /// <summary>
    /// Gets the workflow definition with its states.
    /// </summary>
    /// <param name="id">The unique id</param>
    /// <returns>The workflow definition with states</returns>
    public async Task<Piranha.EditorialWorkflow.Models.WorkflowDefinition> GetWithStates(Guid id)
    {
        var definition = await _db.Set<Data.EditorialWorkflow.WorkflowDefinition>()
            .AsNoTracking()
            .Include(w => w.States.OrderBy(s => s.SortOrder))
            .FirstOrDefaultAsync(w => w.Id == id)
            .ConfigureAwait(false);

        return definition != null ? Transform(definition) : null;
    }

    /// <summary>
    /// Gets the workflow definition with its states and transitions.
    /// </summary>
    /// <param name="id">The unique id</param>
    /// <returns>The workflow definition with states and transitions</returns>
    public async Task<Piranha.EditorialWorkflow.Models.WorkflowDefinition> GetWithStatesAndTransitions(Guid id)
    {
        var definition = await _db.Set<Data.EditorialWorkflow.WorkflowDefinition>()
            .AsNoTracking()
            .Include(w => w.States.OrderBy(s => s.SortOrder))
                .ThenInclude(s => s.OutgoingTransitions.Where(t => t.IsActive).OrderBy(t => t.SortOrder))
                    .ThenInclude(t => t.ToState)
            .FirstOrDefaultAsync(w => w.Id == id)
            .ConfigureAwait(false);

        return definition != null ? Transform(definition) : null;
    }

    /// <summary>
    /// Saves the given workflow definition.
    /// </summary>
    /// <param name="definition">The workflow definition</param>
    public async Task Save(Piranha.EditorialWorkflow.Models.WorkflowDefinition definition)
    {
        var model = await _db.Set<Data.EditorialWorkflow.WorkflowDefinition>()
            .FirstOrDefaultAsync(w => w.Id == definition.Id)
            .ConfigureAwait(false);

        if (model == null)
        {
            model = new Data.EditorialWorkflow.WorkflowDefinition
            {
                Id = definition.Id != Guid.Empty ? definition.Id : Guid.NewGuid(),
                Created = DateTime.Now
            };
            definition.Id = model.Id;
            await _db.Set<Data.EditorialWorkflow.WorkflowDefinition>().AddAsync(model).ConfigureAwait(false);
        }

        model.Name = definition.Name;
        model.Description = definition.Description;
        model.IsActive = definition.IsActive;
        model.Version = definition.Version;
        model.CreatedBy = definition.CreatedBy;
        model.LastModifiedBy = definition.LastModifiedBy;
        model.LastModified = DateTime.Now;

        await _db.SaveChangesAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Deletes the workflow definition with the specified id.
    /// </summary>
    /// <param name="id">The unique id</param>
    public async Task Delete(Guid id)
    {
        var model = await _db.Set<Data.EditorialWorkflow.WorkflowDefinition>()
            .FirstOrDefaultAsync(w => w.Id == id)
            .ConfigureAwait(false);

        if (model != null)
        {
            _db.Set<Data.EditorialWorkflow.WorkflowDefinition>().Remove(model);
            await _db.SaveChangesAsync().ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Checks if a workflow definition with the specified id exists.
    /// </summary>
    /// <param name="id">The unique id</param>
    /// <returns>True if the workflow exists</returns>
    public async Task<bool> Exists(Guid id)
    {
        return await _db.Set<Data.EditorialWorkflow.WorkflowDefinition>()
            .AnyAsync(w => w.Id == id)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Checks if a workflow definition with the specified name exists.
    /// </summary>
    /// <param name="name">The workflow name</param>
    /// <param name="excludeId">Optional id to exclude from the check</param>
    /// <returns>True if a workflow with the name exists</returns>
    public async Task<bool> ExistsByName(string name, Guid? excludeId = null)
    {
        var query = _db.Set<Data.EditorialWorkflow.WorkflowDefinition>()
            .Where(w => w.Name == name);

        if (excludeId.HasValue)
        {
            query = query.Where(w => w.Id != excludeId.Value);
        }

        return await query.AnyAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Transforms the data model to the public model.
    /// </summary>
    /// <param name="model">The data model</param>
    /// <returns>The public model</returns>
    private Piranha.EditorialWorkflow.Models.WorkflowDefinition Transform(Data.EditorialWorkflow.WorkflowDefinition model)
    {
        return new Piranha.EditorialWorkflow.Models.WorkflowDefinition
        {
            Id = model.Id,
            Name = model.Name,
            Description = model.Description,
            IsActive = model.IsActive,
            Version = model.Version,
            Created = model.Created,
            LastModified = model.LastModified,
            CreatedBy = model.CreatedBy,
            LastModifiedBy = model.LastModifiedBy,
            States = model.States?.Select(s => new Piranha.EditorialWorkflow.Models.WorkflowState
            {
                Id = s.Id,
                StateId = s.StateId,
                Name = s.Name,
                Description = s.Description,
                IsInitial = s.IsInitial,
                IsPublished = s.IsPublished,
                IsFinal = s.IsFinal,
                SortOrder = s.SortOrder,
                ColorCode = s.ColorCode,
                Created = s.Created,
                WorkflowDefinitionId = s.WorkflowDefinitionId,
                OutgoingTransitions = s.OutgoingTransitions?.Select(t => new Piranha.EditorialWorkflow.Models.TransitionRule
                {
                    Id = t.Id,
                    Description = t.Description,
                    CommentTemplate = t.CommentTemplate,
                    RequiresComment = t.RequiresComment,
                    AllowedRoles = t.AllowedRoles,
                    SortOrder = t.SortOrder,
                    IsActive = t.IsActive,
                    Created = t.Created,
                    FromStateId = t.FromStateId,
                    ToStateId = t.ToStateId
                }).ToList()
            }).ToList()
        };
    }
}
