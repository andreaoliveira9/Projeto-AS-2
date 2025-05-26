/*
 * Copyright (c) .NET Foundation and Contributors
 *
 * This software may be modified and distributed under the terms
 * of the MIT license. See the LICENSE file for details.
 *
 * https://github.com/piranhacms/piranha.core
 *
 */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Piranha.Manager.Models;

/// <summary>
/// Page model for the workflow states configuration page.
/// </summary>
[Authorize(Policy = Permission.Admin)]
public class WorkflowStatesModel : PageModel
{
    /// <summary>
    /// Default constructor.
    /// </summary>
    public WorkflowStatesModel()
    {
    }
}
