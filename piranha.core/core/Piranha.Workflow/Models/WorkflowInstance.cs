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
    /// Represents an instance of a workflow applied to a specific content item.
    /// </summary>
    public class WorkflowInstance
    {
        /// <summary>
        /// Gets or sets the unique identifier for the workflow instance.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Gets or sets the ID of the workflow definition this instance is based on.
        /// </summary>
        public Guid WorkflowDefinitionId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the content item this workflow instance is attached to.
        /// </summary>
        public Guid ContentId { get; set; }

        /// <summary>
        /// Gets or sets the type of the content.
        /// </summary>
        public string ContentType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the ID of the current state of the workflow.
        /// </summary>
        public Guid CurrentStateId { get; set; }

        /// <summary>
        /// Gets or sets when this workflow instance was created.
        /// </summary>
        public DateTime Created { get; set; } = DateTime.Now;

        /// <summary>
        /// Gets or sets when this workflow instance was last updated.
        /// </summary>
        public DateTime LastUpdated { get; set; } = DateTime.Now;

        /// <summary>
        /// Gets or sets the history of state transitions for this workflow instance.
        /// </summary>
        public List<StateChangeRecord> History { get; set; } = new List<StateChangeRecord>();
    }

    /// <summary>
    /// Represents a record of a state change in a workflow instance.
    /// </summary>
    public class StateChangeRecord
    {
        /// <summary>
        /// Gets or sets the ID of the state change record.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Gets or sets the ID of the workflow instance this record belongs to.
        /// </summary>
        public Guid WorkflowInstanceId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the transition rule that was applied.
        /// </summary>
        public Guid TransitionRuleId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the state before the transition.
        /// </summary>
        public Guid FromStateId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the state after the transition.
        /// </summary>
        public Guid ToStateId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the user who performed the transition.
        /// </summary>
        public Guid? UserId { get; set; }

        /// <summary>
        /// Gets or sets the username of the user who performed the transition.
        /// </summary>
        public string? Username { get; set; }

        /// <summary>
        /// Gets or sets the comment associated with this state change.
        /// </summary>
        public string? Comment { get; set; }

        /// <summary>
        /// Gets or sets when this state change occurred.
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
} 