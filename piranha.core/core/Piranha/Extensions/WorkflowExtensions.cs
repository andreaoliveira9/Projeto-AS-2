/*
 * Copyright (c) .NET Foundation and Contributors
 *
 * This software may be modified and distributed under the terms
 * of the MIT license. See the LICENSE file for details.
 *
 * https://github.com/piranhacms/piranha.core
 *
 */

using Piranha.Workflow;

namespace Piranha.Extensions;

/// <summary>
/// Extensions methods for adding workflow services.
/// </summary>
public static class WorkflowExtensions
{
    /// <summary>
    /// Adds the workflow service to the service builder.
    /// </summary>
    /// <param name="serviceBuilder">The service builder</param>
    /// <param name="editorialWorkflowPath">Optional path to the editorial workflow JSON file</param>
    /// <returns>The updated service builder</returns>
    public static PiranhaServiceBuilder AddWorkflow(this PiranhaServiceBuilder serviceBuilder, string editorialWorkflowPath = null)
    {
        serviceBuilder.Services.AddPiranhaWorkflow(editorialWorkflowPath);

        return serviceBuilder;
    }
}
