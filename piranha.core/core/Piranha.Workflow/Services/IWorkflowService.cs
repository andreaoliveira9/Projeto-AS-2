/*
 * Copyright (c) .NET Foundation and Contributors
 *
 * This software may be modified and distributed under the terms
 * of the MIT license. See the LICENSE file for details.
 *
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Piranha.Workflow.Models;

namespace Piranha.Workflow.Services
{
    /// <summary>
    /// Interface for the workflow service.
    /// </summary>
    public interface IWorkflowService
    {
        /// <summary>
        /// Gets all workflow definitions.
        /// </summary>
        /// <returns>All workflow definitions</returns>
        Task<IEnumerable<WorkflowDefinition>> GetAllWorkflowDefinitionsAsync();

        /// <summary>
        /// Gets a workflow definition by id.
        /// </summary>
        /// <param name="id">The workflow definition id</param>
        /// <returns>The workflow definition</returns>
        Task<WorkflowDefinition> GetWorkflowDefinitionByIdAsync(Guid id);

        /// <summary>
        /// Gets workflow definitions applicable to a content type.
        /// </summary>
        /// <param name="contentType">The content type</param>
        /// <returns>The applicable workflow definitions</returns>
        Task<IEnumerable<WorkflowDefinition>> GetWorkflowDefinitionsForContentTypeAsync(string contentType);

        /// <summary>
        /// Saves a workflow definition.
        /// </summary>
        /// <param name="definition">The workflow definition</param>
        /// <returns>The saved workflow definition</returns>
        Task<WorkflowDefinition> SaveWorkflowDefinitionAsync(WorkflowDefinition definition);

        /// <summary>
        /// Deletes a workflow definition.
        /// </summary>
        /// <param name="id">The workflow definition id</param>
        /// <returns>Task</returns>
        Task DeleteWorkflowDefinitionAsync(Guid id);

        /// <summary>
        /// Gets a workflow instance by id.
        /// </summary>
        /// <param name="id">The workflow instance id</param>
        /// <returns>The workflow instance</returns>
        Task<WorkflowInstance> GetWorkflowInstanceByIdAsync(Guid id);

        /// <summary>
        /// Gets a workflow instance for a content item.
        /// </summary>
        /// <param name="contentId">The content id</param>
        /// <returns>The workflow instance</returns>
        Task<WorkflowInstance> GetWorkflowInstanceForContentAsync(Guid contentId);

        /// <summary>
        /// Creates a new workflow instance for a content item.
        /// </summary>
        /// <param name="contentId">The content id</param>
        /// <param name="contentType">The content type</param>
        /// <param name="workflowDefinitionId">The workflow definition id</param>
        /// <returns>The created workflow instance</returns>
        Task<WorkflowInstance> CreateWorkflowInstanceAsync(Guid contentId, string contentType, Guid workflowDefinitionId);

        /// <summary>
        /// Gets available transitions for a content item.
        /// </summary>
        /// <param name="contentId">The content id</param>
        /// <param name="userId">The current user id</param>
        /// <returns>Available transitions</returns>
        Task<IEnumerable<TransitionRule>> GetAvailableTransitionsAsync(Guid contentId, Guid? userId = null);

        /// <summary>
        /// Performs a state transition on a content item.
        /// </summary>
        /// <param name="contentId">The content id</param>
        /// <param name="transitionRuleId">The transition rule id</param>
        /// <param name="userId">The user id performing the transition</param>
        /// <param name="username">The username performing the transition</param>
        /// <param name="comment">Optional comment about the transition</param>
        /// <returns>The updated workflow instance</returns>
        Task<WorkflowInstance> PerformTransitionAsync(Guid contentId, Guid transitionRuleId, Guid? userId, string? username, string? comment = null);

        /// <summary>
        /// Gets the workflow content extension for a content item.
        /// </summary>
        /// <param name="contentId">The content id</param>
        /// <returns>The workflow content extension</returns>
        Task<WorkflowContentExtension> GetContentExtensionAsync(Guid contentId);
    }
} 