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

namespace Piranha.Workflow.Models
{
    /// <summary>
    /// Represents the definition of a workflow, including states and transition rules.
    /// </summary>
    public class WorkflowDefinition
    {
        /// <summary>
        /// Gets or sets the unique identifier for the workflow definition.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Gets or sets the name of the workflow.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the description of the workflow.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the states in this workflow.
        /// </summary>
        public List<WorkflowState> States { get; set; } = new List<WorkflowState>();

        /// <summary>
        /// Gets or sets the transition rules in this workflow.
        /// </summary>
        public List<TransitionRule> TransitionRules { get; set; } = new List<TransitionRule>();

        /// <summary>
        /// Gets or sets whether this workflow is active.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Gets or sets the content types this workflow applies to.
        /// </summary>
        public List<string> ContentTypes { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets when this workflow was created.
        /// </summary>
        public DateTime Created { get; set; } = DateTime.Now;

        /// <summary>
        /// Gets or sets when this workflow was last modified.
        /// </summary>
        public DateTime LastModified { get; set; } = DateTime.Now;

        /// <summary>
        /// Gets the initial state of the workflow.
        /// </summary>
        /// <returns>The initial state or null if not found</returns>
        public WorkflowState? GetInitialState()
        {
            return States.FirstOrDefault(s => s.IsInitial);
        }

        /// <summary>
        /// Gets the available transitions from the specified state.
        /// </summary>
        /// <param name="stateId">The state ID</param>
        /// <returns>The list of available transitions</returns>
        public IEnumerable<TransitionRule> GetAvailableTransitions(Guid stateId)
        {
            return TransitionRules.Where(t => t.FromStateId == stateId);
        }

        /// <summary>
        /// Gets the state by its ID.
        /// </summary>
        /// <param name="stateId">The state ID</param>
        /// <returns>The state or null if not found</returns>
        public WorkflowState? GetState(Guid stateId)
        {
            return States.FirstOrDefault(s => s.Id == stateId);
        }

        /// <summary>
        /// Validates if this workflow definition is valid.
        /// </summary>
        /// <returns>True if valid, otherwise false</returns>
        public bool Validate()
        {
            // A workflow must have at least one initial state
            if (!States.Any(s => s.IsInitial))
            {
                return false;
            }

            // A workflow must have at least one terminal state
            if (!States.Any(s => s.IsTerminal))
            {
                return false;
            }

            // Validate that transition rules reference valid states
            foreach (var rule in TransitionRules)
            {
                if (!States.Any(s => s.Id == rule.FromStateId) || !States.Any(s => s.Id == rule.ToStateId))
                {
                    return false;
                }
            }

            return true;
        }
    }
} 