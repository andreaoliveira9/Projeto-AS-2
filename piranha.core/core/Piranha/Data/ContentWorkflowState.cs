/*
 * Copyright (c) .NET Foundation and Contributors
 *
 * This software may be modified and distributed under the terms
 * of the MIT license. See the LICENSE file for details.
 *
 * https://github.com/piranhacms/piranha.core
 *
 */

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Piranha.Data;

/// <summary>
/// Entity for storing content workflow states.
/// </summary>
[Table("Piranha_ContentWorkflowStates")]
public class ContentWorkflowState
{
    /// <summary>
    /// Gets/sets the content id.
    /// </summary>
    [Key]
    public Guid ContentId { get; set; }

    /// <summary>
    /// Gets/sets the workflow name.
    /// </summary>
    [Required]
    [StringLength(64)]
    public string WorkflowName { get; set; }

    /// <summary>
    /// Gets/sets the current state id.
    /// </summary>
    [Required]
    [StringLength(64)]
    public string CurrentStateId { get; set; }

    /// <summary>
    /// Gets/sets when the state was last changed.
    /// </summary>
    public DateTime StateChangedAt { get; set; }

    /// <summary>
    /// Gets/sets who last changed the state.
    /// </summary>
    [StringLength(128)]
    public string StateChangedBy { get; set; }

    /// <summary>
    /// Gets/sets any comment provided with the last state change.
    /// </summary>
    public string StateChangeComment { get; set; }
}
