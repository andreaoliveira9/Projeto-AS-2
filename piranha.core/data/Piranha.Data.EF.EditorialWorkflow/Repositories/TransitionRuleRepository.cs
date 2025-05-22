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

internal class TransitionRuleRepository : ITransitionRuleRepository
{
    private readonly IDb _db;

    public TransitionRuleRepository(IDb db)
    {
        _db = db;
    }

    public async Task<IEnumerable<Piranha.EditorialWorkflow.Models.TransitionRule>> GetByFromState(Guid fromStateId)
    {
        var rules = await _db.Set<Data.EditorialWorkflow.TransitionRule>()
            .AsNoTracking()
            .Include(t => t.ToState)
            .Where(t => t.FromStateId == fromStateId)
            .OrderBy(t => t.SortOrder)
            .ToListAsync()
            .ConfigureAwait(false);

        return rules.Select(Transform);
    }

    public async Task<IEnumerable<Piranha.EditorialWorkflow.Models.TransitionRule>> GetByToState(Guid toStateId)
    {
        var rules = await _db.Set<Data.EditorialWorkflow.TransitionRule>()
            .AsNoTracking()
            .Include(t => t.FromState)
            .Where(t => t.ToStateId == toStateId)
            .OrderBy(t => t.SortOrder)
            .ToListAsync()
            .ConfigureAwait(false);

        return rules.Select(Transform);
    }

    public async Task<Piranha.EditorialWorkflow.Models.TransitionRule> GetById(Guid id)
    {
        var rule = await _db.Set<Data.EditorialWorkflow.TransitionRule>()
            .AsNoTracking()
            .Include(t => t.FromState)
            .Include(t => t.ToState)
            .FirstOrDefaultAsync(t => t.Id == id)
            .ConfigureAwait(false);

        return rule != null ? Transform(rule) : null;
    }

    public async Task<Piranha.EditorialWorkflow.Models.TransitionRule> GetTransition(Guid fromStateId, Guid toStateId)
    {
        var rule = await _db.Set<Data.EditorialWorkflow.TransitionRule>()
            .AsNoTracking()
            .Include(t => t.FromState)
            .Include(t => t.ToState)
            .FirstOrDefaultAsync(t => t.FromStateId == fromStateId && t.ToStateId == toStateId)
            .ConfigureAwait(false);

        return rule != null ? Transform(rule) : null;
    }

    public async Task<IEnumerable<Piranha.EditorialWorkflow.Models.TransitionRule>> GetActiveTransitions(Guid fromStateId)
    {
        var rules = await _db.Set<Data.EditorialWorkflow.TransitionRule>()
            .AsNoTracking()
            .Include(t => t.ToState)
            .Where(t => t.FromStateId == fromStateId && t.IsActive)
            .OrderBy(t => t.SortOrder)
            .ToListAsync()
            .ConfigureAwait(false);

        return rules.Select(Transform);
    }

    public async Task Save(Piranha.EditorialWorkflow.Models.TransitionRule rule)
    {
        var model = await _db.Set<Data.EditorialWorkflow.TransitionRule>()
            .FirstOrDefaultAsync(t => t.Id == rule.Id)
            .ConfigureAwait(false);

        if (model == null)
        {
            model = new Data.EditorialWorkflow.TransitionRule
            {
                Id = rule.Id != Guid.Empty ? rule.Id : Guid.NewGuid(),
                Created = DateTime.Now
            };
            rule.Id = model.Id;
            await _db.Set<Data.EditorialWorkflow.TransitionRule>().AddAsync(model).ConfigureAwait(false);
        }

        model.Description = rule.Description;
        model.CommentTemplate = rule.CommentTemplate;
        model.RequiresComment = rule.RequiresComment;
        model.AllowedRoles = rule.AllowedRoles;
        model.SortOrder = rule.SortOrder;
        model.IsActive = rule.IsActive;
        model.FromStateId = rule.FromStateId;
        model.ToStateId = rule.ToStateId;

        await _db.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task Delete(Guid id)
    {
        var model = await _db.Set<Data.EditorialWorkflow.TransitionRule>()
            .FirstOrDefaultAsync(t => t.Id == id)
            .ConfigureAwait(false);

        if (model != null)
        {
            _db.Set<Data.EditorialWorkflow.TransitionRule>().Remove(model);
            await _db.SaveChangesAsync().ConfigureAwait(false);
        }
    }

    public async Task<bool> Exists(Guid id)
    {
        return await _db.Set<Data.EditorialWorkflow.TransitionRule>()
            .AnyAsync(t => t.Id == id)
            .ConfigureAwait(false);
    }

    public async Task<bool> TransitionExists(Guid fromStateId, Guid toStateId)
    {
        return await _db.Set<Data.EditorialWorkflow.TransitionRule>()
            .AnyAsync(t => t.FromStateId == fromStateId && t.ToStateId == toStateId)
            .ConfigureAwait(false);
    }

    private Piranha.EditorialWorkflow.Models.TransitionRule Transform(Data.EditorialWorkflow.TransitionRule model)
    {
        return new Piranha.EditorialWorkflow.Models.TransitionRule
        {
            Id = model.Id,
            Description = model.Description,
            CommentTemplate = model.CommentTemplate,
            RequiresComment = model.RequiresComment,
            AllowedRoles = model.AllowedRoles,
            SortOrder = model.SortOrder,
            IsActive = model.IsActive,
            Created = model.Created,
            FromStateId = model.FromStateId,
            ToStateId = model.ToStateId
        };
    }
}
