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
/// Repository interface for WorkflowState operations
/// </summary>
public interface IWorkflowStateRepository
{
    /// <summary>
    /// Gets all states for the specified workflow.
    /// </summary>
    /// <param name="workflowDefinitionId">The workflow definition id</param>
    /// <returns>The workflow states</returns>
    Task<IEnumerable<WorkflowState>> GetByWorkflow(Guid workflowDefinitionId);

    /// <summary>
    /// Gets the workflow state with the specified id.
    /// </summary>
    /// <param name="id">The unique id</param>
    /// <returns>The workflow state</returns>
    Task<WorkflowState> GetById(Guid id);

    /// <summary>
    /// Gets the workflow state with the specified state id within a workflow.
    /// </summary>
    /// <param name="workflowDefinitionId">The workflow definition id</param>
    /// <param name="stateId">The state identifier</param>
    /// <returns>The workflow state</returns>
    Task<WorkflowState> GetByStateId(Guid workflowDefinitionId, string stateId);

    /// <summary>
    /// Gets the initial state for the specified workflow.
    /// </summary>
    /// <param name="workflowDefinitionId">The workflow definition id</param>  
    /// <returns>The initial workflow state</returns>
    Task<WorkflowState> GetInitialState(Guid workflowDefinitionId);

    /// <summary>
    /// Gets all published states for the specified workflow.
    /// </summary>
    /// <param name="workflowDefinitionId">The workflow definition id</param>
    /// <returns>The published workflow states</returns>
    Task<IEnumerable<WorkflowState>> GetPublishedStates(Guid workflowDefinitionId);

    /// <summary>
    /// Saves the given workflow state.
    /// </summary>
    /// <param name="state">The workflow state</param>
    Task Save(WorkflowState state);

    /// <summary>
    /// Deletes the workflow state with the specified id.
    /// </summary>
    /// <param name="id">The unique id</param>
    Task Delete(Guid id);

    /// <summary>
    /// Checks if a workflow state with the specified id exists.
    /// </summary>
    /// <param name="id">The unique id</param>
    /// <returns>True if the state exists</returns>
    Task<bool> Exists(Guid id);
}
