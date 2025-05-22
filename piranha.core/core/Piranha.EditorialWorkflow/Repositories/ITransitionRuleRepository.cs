/*
 * Copyright (c) .NET Foundation and Contributors
 *
 * This software may be modified and distributed under the terms
 * of the MIT license. See the LICENSE file for details.
 *
 * https://github.com/piranhacms/piranha.core
 *
 */

using Piranha.EditorialWorkflow.Models;

namespace Piranha.EditorialWorkflow.Repositories;

/// <summary>
/// Repository interface for TransitionRule operations
/// </summary>
public interface ITransitionRuleRepository
{
    /// <summary>
    /// Gets all transition rules from the specified state.
    /// </summary>
    /// <param name="fromStateId">The source state id</param>
    /// <returns>The transition rules</returns>
    Task<IEnumerable<TransitionRule>> GetByFromState(Guid fromStateId);

    /// <summary>
    /// Gets all transition rules to the specified state.
    /// </summary>
    /// <param name="toStateId">The target state id</param>
    /// <returns>The transition rules</returns>
    Task<IEnumerable<TransitionRule>> GetByToState(Guid toStateId);

    /// <summary>
    /// Gets the transition rule with the specified id.
    /// </summary>
    /// <param name="id">The unique id</param>
    /// <returns>The transition rule</returns>
    Task<TransitionRule> GetById(Guid id);

    /// <summary>
    /// Gets the transition rule between two states.
    /// </summary>
    /// <param name="fromStateId">The source state id</param>
    /// <param name="toStateId">The target state id</param>
    /// <returns>The transition rule</returns>
    Task<TransitionRule> GetTransition(Guid fromStateId, Guid toStateId);

    /// <summary>
    /// Gets all active transition rules from the specified state.
    /// </summary>
    /// <param name="fromStateId">The source state id</param>
    /// <returns>The active transition rules</returns>
    Task<IEnumerable<TransitionRule>> GetActiveTransitions(Guid fromStateId);

    /// <summary>
    /// Saves the given transition rule.
    /// </summary>
    /// <param name="rule">The transition rule</param>
    Task Save(TransitionRule rule);

    /// <summary>
    /// Deletes the transition rule with the specified id.
    /// </summary>
    /// <param name="id">The unique id</param>
    Task Delete(Guid id);

    /// <summary>
    /// Checks if a transition rule with the specified id exists.
    /// </summary>
    /// <param name="id">The unique id</param>
    /// <returns>True if the rule exists</returns>
    Task<bool> Exists(Guid id);

    /// <summary>
    /// Checks if a transition between two states exists.
    /// </summary>
    /// <param name="fromStateId">The source state id</param>
    /// <param name="toStateId">The target state id</param>
    /// <returns>True if the transition exists</returns>
    Task<bool> TransitionExists(Guid fromStateId, Guid toStateId);
}
