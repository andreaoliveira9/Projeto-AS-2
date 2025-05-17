using System;

namespace Piranha.Manager.Models
{
    /// <summary>
    /// API model for a workflow transition.
    /// </summary>
    public class TransitionModel
    {
        /// <summary>
        /// Gets/sets the transition id.
        /// </summary>
        public Guid TransitionId { get; set; }

        /// <summary>
        /// Gets/sets the optional comment.
        /// </summary>
        public string Comment { get; set; }
    }
} 