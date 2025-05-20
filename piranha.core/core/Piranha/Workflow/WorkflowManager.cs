/*
 * Copyright (c) .NET Foundation and Contributors
 *
 * This software may be modified and distributed under the terms
 * of the MIT license. See the LICENSE file for details.
 *
 * https://github.com/piranhacms/piranha.core
 *
 */

using System.Text.Json;
using Piranha.Workflow.Models;

namespace Piranha.Workflow;

/// <summary>
/// The workflow manager class.
/// </summary>
public class WorkflowManager
{
    private readonly Dictionary<string, WorkflowDefinition> _workflows = new();
    private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Loads a workflow definition from a JSON file.
    /// </summary>
    /// <param name="path">The file path</param>
    public void LoadFromFile(string path)
    {
        if (File.Exists(path))
        {
            var json = File.ReadAllText(path);
            var workflow = JsonSerializer.Deserialize<WorkflowDefinition>(json, _jsonOptions);

            if (workflow != null && !string.IsNullOrEmpty(workflow.WorkflowName))
            {
                _workflows[workflow.WorkflowName] = workflow;
            }
        }
    }

    /// <summary>
    /// Gets all available workflow definitions.
    /// </summary>
    /// <returns>All workflows</returns>
    public IEnumerable<WorkflowDefinition> GetWorkflows()
    {
        return _workflows.Values;
    }

    /// <summary>
    /// Gets the workflow with the specified name.
    /// </summary>
    /// <param name="name">The workflow name</param>
    /// <returns>The workflow</returns>
    public WorkflowDefinition GetWorkflow(string name)
    {
        if (_workflows.TryGetValue(name, out var workflow))
        {
            return workflow;
        }
        return null;
    }
}
