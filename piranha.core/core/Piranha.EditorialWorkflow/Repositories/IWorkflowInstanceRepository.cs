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
/// Repository interface for WorkflowInstance operations
/// </summary>
public interface IWorkflowInstanceRepository
{
    /// <summary>
    /// Gets all workflow instances.
    /// </summary>
    /// <returns>The workflow instances</returns>
    Task<IEnumerable<WorkflowInstance>> GetAll();

    /// <summary>
    /// Gets all active workflow instances.
    /// </summary>
    /// <returns>The active workflow instances</returns>
    Task<IEnumerable<WorkflowInstance>> GetActive();

    /// <summary>
    /// Gets all workflow instances for the specified workflow definition.
    /// </summary>
    /// <param name="workflowDefinitionId">The workflow definition id</param>
    /// <returns>The workflow instances</returns>
    Task<IEnumerable<WorkflowInstance>> GetByWorkflow(Guid workflowDefinitionId);

    /// <summary>
    /// Gets all workflow instances in the specified state.
    /// </summary>
    /// <param name="stateId">The state id</param>
    /// <returns>The workflow instances</returns>
    Task<IEnumerable<WorkflowInstance>> GetByState(Guid stateId);

    /// <summary>
    /// Gets all workflow instances created by the specified user.
    /// </summary>
    /// <param name="userId">The user id</param>
    /// <returns>The workflow instances</returns>
    Task<IEnumerable<WorkflowInstance>> GetByUser(string userId);

    /// <summary>
    /// Gets the workflow instance with the specified id.
    /// </summary>
    /// <param name="id">The unique id</param>
    /// <returns>The workflow instance</returns>
    Task<WorkflowInstance> GetById(Guid id);

    /// <summary>
    /// Gets the active workflow instance for the specified content.
    /// </summary>
    /// <param name="contentId">The content id</param>
    /// <returns>The workflow instance</returns>
    Task<WorkflowInstance> GetByContent(string contentId);

    /// <summary>
    /// Saves the given workflow instance.
    /// </summary>
    /// <param name="instance">The workflow instance</param>
    Task Save(WorkflowInstance instance);

    /// <summary>
    /// Deletes the workflow instance with the specified id.
    /// </summary>
    /// <param name="id">The unique id</param>
    Task Delete(Guid id);

    /// <summary>
    /// Checks if a workflow instance with the specified id exists.
    /// </summary>
    /// <param name="id">The unique id</param>
    /// <returns>True if the instance exists</returns>
    Task<bool> Exists(Guid id);

    /// <summary>
    /// Checks if an active workflow instance exists for the specified content.
    /// </summary>
    /// <param name="contentId">The content id</param>
    /// <returns>True if an active instance exists</returns>
    Task<bool> ExistsByContent(string contentId);
}
