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

internal class WorkflowContentExtensionRepository : IWorkflowContentExtensionRepository
{
    private readonly IDb _db;

    public WorkflowContentExtensionRepository(IDb db)
    {
        _db = db;
    }

    public async Task<Piranha.EditorialWorkflow.Models.WorkflowContentExtension> GetByContentId(string contentId)
    {
        var extension = await _db.Set<Data.EditorialWorkflow.WorkflowContentExtension>()
            .AsNoTracking()
            .Include(e => e.CurrentWorkflowInstance)
                .ThenInclude(w => w.WorkflowDefinition)
            .Include(e => e.CurrentWorkflowInstance)
                .ThenInclude(w => w.CurrentState)
            .FirstOrDefaultAsync(e => e.ContentId == contentId)
            .ConfigureAwait(false);

        return extension != null ? Transform(extension) : null;
    }

    public async Task<IEnumerable<Piranha.EditorialWorkflow.Models.WorkflowContentExtension>> GetActiveWorkflows()
    {
        var extensions = await _db.Set<Data.EditorialWorkflow.WorkflowContentExtension>()
            .AsNoTracking()
            .Include(e => e.CurrentWorkflowInstance)
                .ThenInclude(w => w.WorkflowDefinition)
            .Include(e => e.CurrentWorkflowInstance)
                .ThenInclude(w => w.CurrentState)
            .Where(e => e.IsInWorkflow && e.CurrentWorkflowInstance != null)
            .OrderByDescending(e => e.LastModified)
            .ToListAsync()
            .ConfigureAwait(false);

        return extensions.Select(Transform);
    }

    public async Task<IEnumerable<Piranha.EditorialWorkflow.Models.WorkflowContentExtension>> GetByContentType(string contentType)
    {
        var extensions = await _db.Set<Data.EditorialWorkflow.WorkflowContentExtension>()
            .AsNoTracking()
            .Where(e => e.ContentType == contentType)
            .OrderByDescending(e => e.LastModified)
            .ToListAsync()
            .ConfigureAwait(false);

        return extensions.Select(Transform);
    }

    public async Task Save(Piranha.EditorialWorkflow.Models.WorkflowContentExtension extension)
    {
        var model = await _db.Set<Data.EditorialWorkflow.WorkflowContentExtension>()
            .FirstOrDefaultAsync(e => e.ContentId == extension.ContentId)
            .ConfigureAwait(false);

        if (model == null)
        {
            model = new Data.EditorialWorkflow.WorkflowContentExtension
            {
                Id = extension.Id != Guid.Empty ? extension.Id : Guid.NewGuid(),
                ContentId = extension.ContentId,
                Created = DateTime.Now
            };
            extension.Id = model.Id;
            await _db.Set<Data.EditorialWorkflow.WorkflowContentExtension>().AddAsync(model).ConfigureAwait(false);
        }

        model.ContentType = extension.ContentType;
        model.CurrentWorkflowInstanceId = extension.CurrentWorkflowInstanceId;
        model.IsInWorkflow = extension.IsInWorkflow;
        model.LastWorkflowState = extension.LastWorkflowState;
        model.LastModified = DateTime.Now;

        await _db.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task Delete(string contentId)
    {
        var model = await _db.Set<Data.EditorialWorkflow.WorkflowContentExtension>()
            .FirstOrDefaultAsync(e => e.ContentId == contentId)
            .ConfigureAwait(false);

        if (model != null)
        {
            _db.Set<Data.EditorialWorkflow.WorkflowContentExtension>().Remove(model);
            await _db.SaveChangesAsync().ConfigureAwait(false);
        }
    }

    public async Task<bool> Exists(string contentId)
    {
        return await _db.Set<Data.EditorialWorkflow.WorkflowContentExtension>()
            .AnyAsync(e => e.ContentId == contentId)
            .ConfigureAwait(false);
    }

    private Piranha.EditorialWorkflow.Models.WorkflowContentExtension Transform(Data.EditorialWorkflow.WorkflowContentExtension model)
    {
        return new Piranha.EditorialWorkflow.Models.WorkflowContentExtension
        {
            Id = model.Id,
            ContentId = model.ContentId,
            ContentType = model.ContentType,
            CurrentWorkflowInstanceId = model.CurrentWorkflowInstanceId,
            IsInWorkflow = model.IsInWorkflow,
            LastWorkflowState = model.LastWorkflowState,
            Created = model.Created,
            LastModified = model.LastModified
        };
    }
}
