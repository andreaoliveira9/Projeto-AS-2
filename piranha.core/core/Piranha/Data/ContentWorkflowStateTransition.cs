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
/// Entity for storing content workflow state transitions.
/// </summary>
[Table("Piranha_ContentWorkflowStateTransitions")]
public class ContentWorkflowStateTransition
{
    /// <summary>
    /// Gets/sets the unique id.
    /// </summary>
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets/sets the content id.
    /// </summary>
    public Guid ContentId { get; set; }

    /// <summary>
    /// Gets/sets the from state id.
    /// </summary>
    [StringLength(64)]
    public string FromStateId { get; set; }

    /// <summary>
    /// Gets/sets the to state id.
    /// </summary>
    [Required]
    [StringLength(64)]
    public string ToStateId { get; set; }

    /// <summary>
    /// Gets/sets when the transition occurred.
    /// </summary>
    public DateTime TransitionedAt { get; set; }

    /// <summary>
    /// Gets/sets who performed the transition.
    /// </summary>
    [StringLength(128)]
    public string TransitionedBy { get; set; }

    /// <summary>
    /// Gets/sets any comment provided with the transition.
    /// </summary>
    public string Comment { get; set; }
}
