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
using Microsoft.Extensions.Logging;
using Piranha.EditorialWorkflow.Repositories;

namespace Piranha.Repositories.EditorialWorkflow;

internal class WorkflowDefinitionRepository : IWorkflowDefinitionRepository
{
    private readonly IDb _db;
    private readonly ILogger<WorkflowDefinitionRepository> _logger;

    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <param name="db">The current db connection</param>
    /// <param name="logger">The logger</param>
    public WorkflowDefinitionRepository(IDb db, ILogger<WorkflowDefinitionRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// Gets all available workflow definitions.
    /// </summary>
    /// <returns>The available workflow definitions</returns>
    public async Task<IEnumerable<Piranha.EditorialWorkflow.Models.WorkflowDefinition>> GetAll()
    {
        _logger.LogInformation("GetAll: Starting retrieval of all workflow definitions from database");

        try
        {
            var definitions = await _db.Set<Data.EditorialWorkflow.WorkflowDefinition>()
                .AsNoTracking()
                .OrderBy(w => w.Name)
                .ToListAsync()
                .ConfigureAwait(false);

            _logger.LogInformation("GetAll: Retrieved {Count} workflow definitions from database", definitions.Count);

            if (definitions.Any())
            {
                foreach (var def in definitions)
                {
                    _logger.LogDebug("GetAll: Found workflow definition in DB - ID: {Id}, Name: {Name}, IsActive: {IsActive}, Created: {Created}", 
                        def.Id, def.Name, def.IsActive, def.Created);
                }
            }
            else
            {
                _logger.LogWarning("GetAll: No workflow definitions found in database table");
            }

            var result = definitions.Select(Transform).ToList();
            _logger.LogInformation("GetAll: Transformed {Count} workflow definitions", result.Count);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetAll: Error retrieving workflow definitions from database");
            throw;
        }
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
        _logger.LogInformation("Save: Starting save operation for workflow definition. ID: {Id}, Name: {Name}", 
            definition.Id, definition.Name);

        try
        {
            var model = await _db.Set<Data.EditorialWorkflow.WorkflowDefinition>()
                .FirstOrDefaultAsync(w => w.Id == definition.Id)
                .ConfigureAwait(false);

            bool isNewRecord = model == null;
            _logger.LogDebug("Save: Workflow definition exists in DB: {Exists}", !isNewRecord);

            if (model == null)
            {
                model = new Data.EditorialWorkflow.WorkflowDefinition
                {
                    Id = definition.Id != Guid.Empty ? definition.Id : Guid.NewGuid(),
                    Created = definition.Created != default ? definition.Created : DateTime.Now,
                    CreatedBy = definition.CreatedBy
                };
                definition.Id = model.Id;
                
                _logger.LogInformation("Save: Creating new workflow definition with ID: {Id}, CreatedBy: {CreatedBy}", 
                    model.Id, model.CreatedBy);
                await _db.Set<Data.EditorialWorkflow.WorkflowDefinition>().AddAsync(model).ConfigureAwait(false);
            }
            else
            {
                _logger.LogInformation("Save: Updating existing workflow definition with ID: {Id}", model.Id);
            }

            model.Name = definition.Name;
            model.Description = definition.Description;
            model.IsActive = definition.IsActive;
            model.Version = definition.Version;
            model.LastModifiedBy = definition.LastModifiedBy;
            model.LastModified = definition.LastModified != default ? definition.LastModified : DateTime.Now;

            _logger.LogDebug("Save: Model properties set - Name: {Name}, Description: {Description}, IsActive: {IsActive}, CreatedBy: {CreatedBy}", 
                model.Name, model.Description, model.IsActive, model.CreatedBy);

            var changeCount = await _db.SaveChangesAsync().ConfigureAwait(false);
            _logger.LogInformation("Save: Database save completed. Changes saved: {ChangeCount}, IsNewRecord: {IsNewRecord}", 
                changeCount, isNewRecord);

            // Verify the save was successful
            var verifyModel = await _db.Set<Data.EditorialWorkflow.WorkflowDefinition>()
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.Id == definition.Id)
                .ConfigureAwait(false);

            if (verifyModel != null)
            {
                _logger.LogInformation("Save: Verification successful - workflow definition exists in DB after save. ID: {Id}, Name: {Name}", 
                    verifyModel.Id, verifyModel.Name);
            }
            else
            {
                _logger.LogError("Save: Verification failed - workflow definition not found in DB after save. ID: {Id}", definition.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Save: Error saving workflow definition. ID: {Id}, Name: {Name}", 
                definition.Id, definition.Name);
            throw;
        }
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
    /// Gets the count of workflow definitions.
    /// </summary>
    /// <returns>The count of workflow definitions</returns>
    public async Task<int> CountAsync()
    {
        _logger.LogInformation("CountAsync: Getting count of workflow definitions");

        try
        {
            var count = await _db.Set<Data.EditorialWorkflow.WorkflowDefinition>()
                .CountAsync()
                .ConfigureAwait(false);

            _logger.LogInformation("CountAsync: Found {Count} workflow definitions in database", count);
            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CountAsync: Error getting count of workflow definitions");
            throw;
        }
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
