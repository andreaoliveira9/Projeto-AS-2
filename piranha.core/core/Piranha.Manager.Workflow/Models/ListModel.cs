using Piranha.Workflow.Models;
using System.Collections.Generic;

namespace Piranha.Manager.Models
{
    /// <summary>
    /// View model for the workflow list view.
    /// </summary>
    public class ListModel
    {
        /// <summary>
        /// Gets/sets the available workflow definitions.
        /// </summary>
        public IEnumerable<WorkflowDefinition> Definitions { get; set; } = new List<WorkflowDefinition>();
    }
} 