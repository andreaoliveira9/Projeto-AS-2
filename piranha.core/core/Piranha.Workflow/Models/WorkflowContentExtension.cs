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
    /// Represents an extension to content items to store workflow metadata.
    /// </summary>
    public class WorkflowContentExtension
    {
        /// <summary>
        /// Gets or sets the ID of the content item.
        /// </summary>
        public Guid ContentId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the workflow instance attached to this content item.
        /// </summary>
        public Guid WorkflowInstanceId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the current state in the workflow.
        /// </summary>
        public Guid CurrentStateId { get; set; }

        /// <summary>
        /// Gets or sets the name of the current state in the workflow.
        /// </summary>
        public string CurrentStateName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets when the workflow state was last updated.
        /// </summary>
        public DateTime LastStateUpdated { get; set; } = DateTime.Now;

        /// <summary>
        /// Gets or sets the ID of the user who last updated the workflow state.
        /// </summary>
        public Guid? LastUpdatedByUserId { get; set; }

        /// <summary>
        /// Gets or sets the username of the user who last updated the workflow state.
        /// </summary>
        public string? LastUpdatedByUsername { get; set; }
    }
} 