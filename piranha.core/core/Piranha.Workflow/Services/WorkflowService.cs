/*
 * Copyright (c) .NET Foundation and Contributors
 *
 * This software may be modified and distributed under the terms
 * of the MIT license. See the LICENSE file for details.
 *
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Piranha.Workflow.Models;

namespace Piranha.Workflow.Services
{
    /// <summary>
    /// Service for workflow management.
    /// </summary>
    public class WorkflowService : IWorkflowService
    {
        // For demo/prototype, we'll use in-memory storage
        private readonly List<WorkflowDefinition> _definitions = new List<WorkflowDefinition>();
        private readonly List<WorkflowInstance> _instances = new List<WorkflowInstance>();
        private readonly List<WorkflowContentExtension> _contentExtensions = new List<WorkflowContentExtension>();

        /// <summary>
        /// Gets all workflow definitions.
        /// </summary>
        /// <returns>All workflow definitions</returns>
        public Task<IEnumerable<WorkflowDefinition>> GetAllWorkflowDefinitionsAsync()
        {
            return Task.FromResult<IEnumerable<WorkflowDefinition>>(_definitions);
        }

        /// <summary>
        /// Gets a workflow definition by id.
        /// </summary>
        /// <param name="id">The workflow definition id</param>
        /// <returns>The workflow definition</returns>
        public Task<WorkflowDefinition> GetWorkflowDefinitionByIdAsync(Guid id)
        {
            var definition = _definitions.FirstOrDefault(d => d.Id == id);
            if (definition == null)
            {
                throw new ArgumentException($"Workflow definition with ID {id} not found", nameof(id));
            }
            return Task.FromResult(definition);
        }

        /// <summary>
        /// Gets workflow definitions applicable to a content type.
        /// </summary>
        /// <param name="contentType">The content type</param>
        /// <returns>The applicable workflow definitions</returns>
        public Task<IEnumerable<WorkflowDefinition>> GetWorkflowDefinitionsForContentTypeAsync(string contentType)
        {
            var definitions = _definitions
                .Where(d => d.IsActive && (d.ContentTypes.Contains(contentType) || !d.ContentTypes.Any()));
            return Task.FromResult(definitions);
        }

        /// <summary>
        /// Saves a workflow definition.
        /// </summary>
        /// <param name="definition">The workflow definition</param>
        /// <returns>The saved workflow definition</returns>
        public Task<WorkflowDefinition> SaveWorkflowDefinitionAsync(WorkflowDefinition definition)
        {
            // Check if the workflow is valid before saving
            if (!definition.Validate())
            {
                throw new InvalidOperationException("The workflow definition is invalid. Please check that it has initial and terminal states, and all transitions reference valid states.");
            }

            definition.LastModified = DateTime.Now;

            // Update existing definition
            var existingIndex = _definitions.FindIndex(d => d.Id == definition.Id);
            if (existingIndex >= 0)
            {
                _definitions[existingIndex] = definition;
            }
            else
            {
                // Add new definition
                definition.Created = DateTime.Now;
                _definitions.Add(definition);
            }

            return Task.FromResult(definition);
        }

        /// <summary>
        /// Deletes a workflow definition.
        /// </summary>
        /// <param name="id">The workflow definition id</param>
        public Task DeleteWorkflowDefinitionAsync(Guid id)
        {
            // Check if any instances are using this workflow definition
            if (_instances.Any(i => i.WorkflowDefinitionId == id))
            {
                throw new InvalidOperationException("Cannot delete a workflow definition that has active instances.");
            }

            _definitions.RemoveAll(d => d.Id == id);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Gets a workflow instance by id.
        /// </summary>
        /// <param name="id">The workflow instance id</param>
        /// <returns>The workflow instance</returns>
        public Task<WorkflowInstance> GetWorkflowInstanceByIdAsync(Guid id)
        {
            var instance = _instances.FirstOrDefault(i => i.Id == id);
            if (instance == null)
            {
                throw new ArgumentException($"Workflow instance with ID {id} not found", nameof(id));
            }
            return Task.FromResult(instance);
        }

        /// <summary>
        /// Gets a workflow instance for a content item.
        /// </summary>
        /// <param name="contentId">The content id</param>
        /// <returns>The workflow instance</returns>
        public Task<WorkflowInstance> GetWorkflowInstanceForContentAsync(Guid contentId)
        {
            var instance = _instances.FirstOrDefault(i => i.ContentId == contentId);
            if (instance == null)
            {
                throw new ArgumentException($"No workflow instance found for content ID {contentId}", nameof(contentId));
            }
            return Task.FromResult(instance);
        }

        /// <summary>
        /// Creates a new workflow instance for a content item.
        /// </summary>
        /// <param name="contentId">The content id</param>
        /// <param name="contentType">The content type</param>
        /// <param name="workflowDefinitionId">The workflow definition id</param>
        /// <returns>The created workflow instance</returns>
        public async Task<WorkflowInstance> CreateWorkflowInstanceAsync(Guid contentId, string contentType, Guid workflowDefinitionId)
        {
            // Check if a workflow instance already exists for this content
            if (_instances.Any(i => i.ContentId == contentId))
            {
                throw new InvalidOperationException("A workflow instance already exists for this content item.");
            }

            // Get the workflow definition
            var definition = await GetWorkflowDefinitionByIdAsync(workflowDefinitionId);

            // Get the initial state
            var initialState = definition.GetInitialState();
            if (initialState == null)
            {
                throw new InvalidOperationException("The workflow definition does not have an initial state.");
            }

            // Create new instance
            var instance = new WorkflowInstance
            {
                ContentId = contentId,
                ContentType = contentType,
                WorkflowDefinitionId = workflowDefinitionId,
                CurrentStateId = initialState.Id,
                Created = DateTime.Now,
                LastUpdated = DateTime.Now,
                History = new List<StateChangeRecord>
                {
                    new StateChangeRecord
                    {
                        WorkflowInstanceId = Guid.NewGuid(), // This will be set properly after saving
                        ToStateId = initialState.Id,
                        Timestamp = DateTime.Now,
                        Comment = "Initial state"
                    }
                }
            };

            _instances.Add(instance);

            // Update instance ID in the history record
            foreach (var record in instance.History)
            {
                record.WorkflowInstanceId = instance.Id;
            }

            // Create the content extension
            var extension = new WorkflowContentExtension
            {
                ContentId = contentId,
                WorkflowInstanceId = instance.Id,
                CurrentStateId = initialState.Id,
                CurrentStateName = initialState.Name,
                LastStateUpdated = DateTime.Now
            };

            _contentExtensions.Add(extension);

            return instance;
        }

        /// <summary>
        /// Gets available transitions for a content item.
        /// </summary>
        /// <param name="contentId">The content id</param>
        /// <param name="userId">The current user id</param>
        /// <returns>Available transitions</returns>
        public async Task<IEnumerable<TransitionRule>> GetAvailableTransitionsAsync(Guid contentId, Guid? userId = null)
        {
            try
            {
                // Get the workflow instance for this content
                var instance = await GetWorkflowInstanceForContentAsync(contentId);

                // Get the workflow definition
                var definition = await GetWorkflowDefinitionByIdAsync(instance.WorkflowDefinitionId);

                // Get available transitions from the current state
                var transitions = definition.GetAvailableTransitions(instance.CurrentStateId);

                // Filter by user permissions if a user ID is provided
                // This would normally check against actual user roles, but for now we'll
                // just return all transitions
                return transitions;
            }
            catch (ArgumentException)
            {
                // No instance or definition found
                return Enumerable.Empty<TransitionRule>();
            }
        }

        /// <summary>
        /// Performs a state transition on a content item.
        /// </summary>
        /// <param name="contentId">The content id</param>
        /// <param name="transitionRuleId">The transition rule id</param>
        /// <param name="userId">The user id performing the transition</param>
        /// <param name="username">The username performing the transition</param>
        /// <param name="comment">Optional comment about the transition</param>
        /// <returns>The updated workflow instance</returns>
        public async Task<WorkflowInstance> PerformTransitionAsync(Guid contentId, Guid transitionRuleId, Guid? userId, string? username, string? comment = null)
        {
            // Get the workflow instance for this content
            var instance = await GetWorkflowInstanceForContentAsync(contentId);

            // Get the workflow definition
            var definition = await GetWorkflowDefinitionByIdAsync(instance.WorkflowDefinitionId);

            // Get the transition rule
            var transitionRule = definition.TransitionRules.FirstOrDefault(t => t.Id == transitionRuleId);
            if (transitionRule == null)
            {
                throw new ArgumentException("The specified transition rule does not exist.", nameof(transitionRuleId));
            }

            // Verify that the transition is valid from the current state
            if (transitionRule.FromStateId != instance.CurrentStateId)
            {
                throw new InvalidOperationException("The specified transition is not valid from the current state.");
            }

            // Get the destination state
            var toState = definition.GetState(transitionRule.ToStateId);
            if (toState == null)
            {
                throw new InvalidOperationException("The destination state does not exist.");
            }

            // Update the instance
            instance.CurrentStateId = toState.Id;
            instance.LastUpdated = DateTime.Now;

            // Add to history
            var stateChangeRecord = new StateChangeRecord
            {
                Id = Guid.NewGuid(),
                WorkflowInstanceId = instance.Id,
                TransitionRuleId = transitionRuleId,
                FromStateId = transitionRule.FromStateId,
                ToStateId = transitionRule.ToStateId,
                UserId = userId,
                Username = username,
                Comment = comment,
                Timestamp = DateTime.Now
            };

            instance.History.Add(stateChangeRecord);

            // Update the content extension
            var extension = _contentExtensions.FirstOrDefault(e => e.ContentId == contentId);
            if (extension != null)
            {
                extension.CurrentStateId = toState.Id;
                extension.CurrentStateName = toState.Name;
                extension.LastStateUpdated = DateTime.Now;
                extension.LastUpdatedByUserId = userId;
                extension.LastUpdatedByUsername = username;
            }

            return instance;
        }

        /// <summary>
        /// Gets the workflow content extension for a content item.
        /// </summary>
        /// <param name="contentId">The content id</param>
        /// <returns>The workflow content extension</returns>
        public Task<WorkflowContentExtension> GetContentExtensionAsync(Guid contentId)
        {
            var extension = _contentExtensions.FirstOrDefault(e => e.ContentId == contentId);
            if (extension == null)
            {
                throw new ArgumentException($"No workflow content extension found for content ID {contentId}", nameof(contentId));
            }
            return Task.FromResult(extension);
        }
    }
} 