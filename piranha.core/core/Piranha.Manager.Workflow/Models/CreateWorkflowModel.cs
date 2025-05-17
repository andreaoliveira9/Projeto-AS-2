using System;

namespace Piranha.Manager.Models
{
    /// <summary>
    /// API model for creating a workflow instance.
    /// </summary>
    public class CreateWorkflowModel
    {
        /// <summary>
        /// Gets/sets the content type.
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// Gets/sets the workflow definition id.
        /// </summary>
        public Guid WorkflowDefinitionId { get; set; }
    }
} 