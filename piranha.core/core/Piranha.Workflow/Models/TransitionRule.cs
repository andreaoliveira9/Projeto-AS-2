/*
 * Copyright (c) .NET Foundation and Contributors
 *
 * This software may be modified and distributed under the terms
 * of the MIT license. See the LICENSE file for details.
 *
 */

using System;
using System.Collections.Generic;

namespace Piranha.Workflow.Models
{
    /// <summary>
    /// Represents a rule for transitioning between states in a workflow.
    /// </summary>
    public class TransitionRule
    {
        /// <summary>
        /// Gets or sets the unique identifier for the transition rule.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Gets or sets the ID of the originating state.
        /// </summary>
        public Guid FromStateId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the destination state.
        /// </summary>
        public Guid ToStateId { get; set; }

        /// <summary>
        /// Gets or sets the name of the transition.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the description of the transition.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the list of roles allowed to perform this transition.
        /// </summary>
        public List<string> AllowedRoles { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets whether validation is required for this transition.
        /// </summary>
        public bool RequiresValidation { get; set; }

        /// <summary>
        /// Gets or sets the ordering of the transition in the UI.
        /// </summary>
        public int SortOrder { get; set; }
    }
} 