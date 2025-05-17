/*
 * Copyright (c) .NET Foundation and Contributors
 *
 * This software may be modified and distributed under the terms
 * of the MIT license. See the LICENSE file for details.
 *
 */

using System;

namespace Piranha.Workflow.Models
{
    /// <summary>
    /// Represents a state in a workflow definition.
    /// </summary>
    public class WorkflowState
    {
        /// <summary>
        /// Gets or sets the unique identifier for the state.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Gets or sets the name of the state.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the description of the state.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets whether this is the initial state of the workflow.
        /// </summary>
        public bool IsInitial { get; set; }

        /// <summary>
        /// Gets or sets whether this is a terminal state (end state) of the workflow.
        /// </summary>
        public bool IsTerminal { get; set; }

        /// <summary>
        /// Gets or sets the ordering of the state in the workflow.
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// Gets or sets the color associated with this state for UI representation.
        /// </summary>
        public string Color { get; set; } = "#007bff";
    }
} 