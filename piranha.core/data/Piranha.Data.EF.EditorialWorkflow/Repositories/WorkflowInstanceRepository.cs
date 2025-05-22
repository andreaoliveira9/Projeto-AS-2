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

internal class WorkflowInstanceRepository : IWorkflowInstanceRepository
{
    private readonly IDb _db;

    public WorkflowInstanceRepository(IDb db)
    {
        _db = db;
    }

    public async Task<IEnumerable<Piranha.EditorialWorkflow.Models.WorkflowInstance>> GetAll()
    {
        var instances = await _db.Set<Data.EditorialWorkflow.WorkflowInstance>()
            .AsNoTracking()
            .Include(w => w.WorkflowDefinition)
            .Include(w => w.CurrentState)
            .OrderByDescending(w => w.LastModified)
            .ToListAsync()
            .ConfigureAwait(false);

        return instances.Select(Transform);
    }

    public async Task<IEnumerable<Piranha.EditorialWorkflow.Models.WorkflowInstance>> GetActive()
    {
        var instances = await _db.Set<Data.EditorialWorkflow.WorkflowInstance>()
            .AsNoTracking()
            .Include(w => w.WorkflowDefinition)
            .Include(w => w.CurrentState)
            .Where(w => w.Status == Data.EditorialWorkflow.WorkflowInstanceStatus.Active)
            .OrderByDescending(w => w.LastModified)
            .ToListAsync()
            .ConfigureAwait(false);

        return instances.Select(Transform);
    }

    public async Task<IEnumerable<Piranha.EditorialWorkflow.Models.WorkflowInstance>> GetByWorkflow(Guid workflowDefinitionId)
    {
        var instances = await _db.Set<Data.EditorialWorkflow.WorkflowInstance>()
            .AsNoTracking()
            .Include(w => w.CurrentState)
            .Where(w => w.WorkflowDefinitionId == workflowDefinitionId)
            .OrderByDescending(w => w.LastModified)
            .ToListAsync()
            .ConfigureAwait(false);

        return instances.Select(Transform);
    }

    public async Task<IEnumerable<Piranha.EditorialWorkflow.Models.WorkflowInstance>> GetByState(Guid stateId)
    {
        var instances = await _db.Set<Data.EditorialWorkflow.WorkflowInstance>()
            .AsNoTracking()
            .Include(w => w.WorkflowDefinition)
            .Include(w => w.CurrentState)
            .Where(w => w.CurrentStateId == stateId && w.Status == Data.EditorialWorkflow.WorkflowInstanceStatus.Active)
            .OrderByDescending(w => w.LastModified)
            .ToListAsync()
            .ConfigureAwait(false);

        return instances.Select(Transform);
    }

    public async Task<IEnumerable<Piranha.EditorialWorkflow.Models.WorkflowInstance>> GetByUser(string userId)
    {
        var instances = await _db.Set<Data.EditorialWorkflow.WorkflowInstance>()
            .AsNoTracking()
            .Include(w => w.WorkflowDefinition)
            .Include(w => w.CurrentState)
            .Where(w => w.CreatedBy == userId)
            .OrderByDescending(w => w.LastModified)
            .ToListAsync()
            .ConfigureAwait(false);

        return instances.Select(Transform);
    }

    public async Task<Piranha.EditorialWorkflow.Models.WorkflowInstance> GetById(Guid id)
    {
        var instance = await _db.Set<Data.EditorialWorkflow.WorkflowInstance>()
            .AsNoTracking()
            .Include(w => w.WorkflowDefinition)
            .Include(w => w.CurrentState)
            .FirstOrDefaultAsync(w => w.Id == id)
            .ConfigureAwait(false);

        return instance != null ? Transform(instance) : null;
    }

    public async Task<Piranha.EditorialWorkflow.Models.WorkflowInstance> GetByContent(string contentId)
    {
        var instance = await _db.Set<Data.EditorialWorkflow.WorkflowInstance>()
            .AsNoTracking()
            .Include(w => w.WorkflowDefinition)
            .Include(w => w.CurrentState)
            .FirstOrDefaultAsync(w => w.ContentId == contentId && w.Status == Data.EditorialWorkflow.WorkflowInstanceStatus.Active)
            .ConfigureAwait(false);

        return instance != null ? Transform(instance) : null;
    }

    public async Task Save(Piranha.EditorialWorkflow.Models.WorkflowInstance instance)
    {
        var model = await _db.Set<Data.EditorialWorkflow.WorkflowInstance>()
            .FirstOrDefaultAsync(w => w.Id == instance.Id)
            .ConfigureAwait(false);

        if (model == null)
        {
            model = new Data.EditorialWorkflow.WorkflowInstance
            {
                Id = instance.Id != Guid.Empty ? instance.Id : Guid.NewGuid(),
                Created = DateTime.Now
            };
            instance.Id = model.Id;
            await _db.Set<Data.EditorialWorkflow.WorkflowInstance>().AddAsync(model).ConfigureAwait(false);
        }

        model.ContentId = instance.ContentId;
        model.ContentType = instance.ContentType;
        model.ContentTitle = instance.ContentTitle;
        model.Status = (Data.EditorialWorkflow.WorkflowInstanceStatus)instance.Status;
        model.CreatedBy = instance.CreatedBy;
        model.LastModified = DateTime.Now;
        model.CompletedAt = instance.CompletedAt;
        model.Metadata = instance.Metadata;
        model.WorkflowDefinitionId = instance.WorkflowDefinitionId;
        model.CurrentStateId = instance.CurrentStateId;

        await _db.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task Delete(Guid id)
    {
        var model = await _db.Set<Data.EditorialWorkflow.WorkflowInstance>()
            .FirstOrDefaultAsync(w => w.Id == id)
            .ConfigureAwait(false);

        if (model != null)
        {
            _db.Set<Data.EditorialWorkflow.WorkflowInstance>().Remove(model);
            await _db.SaveChangesAsync().ConfigureAwait(false);
        }
    }

    public async Task<bool> Exists(Guid id)
    {
        return await _db.Set<Data.EditorialWorkflow.WorkflowInstance>()
            .AnyAsync(w => w.Id == id)
            .ConfigureAwait(false);
    }

    public async Task<bool> ExistsByContent(string contentId)
    {
        return await _db.Set<Data.EditorialWorkflow.WorkflowInstance>()
            .AnyAsync(w => w.ContentId == contentId && w.Status == Data.EditorialWorkflow.WorkflowInstanceStatus.Active)
            .ConfigureAwait(false);
    }

    private Piranha.EditorialWorkflow.Models.WorkflowInstance Transform(Data.EditorialWorkflow.WorkflowInstance model)
    {
        return new Piranha.EditorialWorkflow.Models.WorkflowInstance
        {
            Id = model.Id,
            ContentId = model.ContentId,
            ContentType = model.ContentType,
            ContentTitle = model.ContentTitle,
            Status = (Piranha.EditorialWorkflow.Models.WorkflowInstanceStatus)model.Status,
            CreatedBy = model.CreatedBy,
            Created = model.Created,
            LastModified = model.LastModified,
            CompletedAt = model.CompletedAt,
            Metadata = model.Metadata,
            WorkflowDefinitionId = model.WorkflowDefinitionId,
            CurrentStateId = model.CurrentStateId
        };
    }
}
