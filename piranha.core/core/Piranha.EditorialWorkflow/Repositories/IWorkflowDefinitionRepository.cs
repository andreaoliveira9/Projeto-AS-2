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
/// Repository interface for WorkflowDefinition operations
/// </summary>
public interface IWorkflowDefinitionRepository
{
    /// <summary>
    /// Gets all available workflow definitions.
    /// </summary>
    /// <returns>The available workflow definitions</returns>
    Task<IEnumerable<WorkflowDefinition>> GetAll();

    /// <summary>
    /// Gets all active workflow definitions.
    /// </summary>
    /// <returns>The active workflow definitions</returns>
    Task<IEnumerable<WorkflowDefinition>> GetActive();

    /// <summary>
    /// Gets the workflow definition with the specified id.
    /// </summary>
    /// <param name="id">The unique id</param>
    /// <returns>The workflow definition</returns>
    Task<WorkflowDefinition> GetById(Guid id);

    /// <summary>
    /// Gets the workflow definition with the specified name.
    /// </summary>
    /// <param name="name">The workflow name</param>
    /// <returns>The workflow definition</returns>
    Task<WorkflowDefinition> GetByName(string name);

    /// <summary>
    /// Gets the workflow definition with its states.
    /// </summary>
    /// <param name="id">The unique id</param>
    /// <returns>The workflow definition with states</returns>
    Task<WorkflowDefinition> GetWithStates(Guid id);

    /// <summary>
    /// Gets the workflow definition with its states and transitions.
    /// </summary>
    /// <param name="id">The unique id</param>
    /// <returns>The workflow definition with states and transitions</returns>
    Task<WorkflowDefinition> GetWithStatesAndTransitions(Guid id);

    /// <summary>
    /// Saves the given workflow definition.
    /// </summary>
    /// <param name="definition">The workflow definition</param>
    Task Save(WorkflowDefinition definition);

    /// <summary>
    /// Deletes the workflow definition with the specified id.
    /// </summary>
    /// <param name="id">The unique id</param>
    Task Delete(Guid id);

    /// <summary>
    /// Checks if a workflow definition with the specified id exists.
    /// </summary>
    /// <param name="id">The unique id</param>
    /// <returns>True if the workflow exists</returns>
    Task<bool> Exists(Guid id);

    /// <summary>
    /// Checks if a workflow definition with the specified name exists.
    /// </summary>
    /// <param name="name">The workflow name</param>
    /// <param name="excludeId">Optional id to exclude from the check</param>
    /// <returns>True if a workflow with the name exists</returns>
    Task<bool> ExistsByName(string name, Guid? excludeId = null);
}
