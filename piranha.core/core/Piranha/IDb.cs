/*
 * Copyright (c) .NET Foundation and Contributors
 *
 * This software may be modified and distributed under the terms
 * of the MIT license. See the LICENSE file for details.
 *
 * https://github.com/piranhacms/piranha.core
 *
 */

using System.Collections.Generic;
using System.Threading.Tasks;
using Piranha.Data;

namespace Piranha;

/// <summary>
/// Interface for the Piranha database context.
/// </summary>
public interface IDb
{
    /// <summary>
    /// Gets/sets the content workflow states.
    /// </summary>
    IEnumerable<ContentWorkflowState> ContentWorkflowStates { get; set; }

    /// <summary>
    /// Gets/sets the content workflow state transitions.
    /// </summary>
    IEnumerable<ContentWorkflowStateTransition> ContentWorkflowStateTransitions { get; set; }

    /// <summary>
    /// Saves changes to the database.
    /// </summary>
    /// <returns>The number of rows affected</returns>
    Task<int> SaveChangesAsync();
}
