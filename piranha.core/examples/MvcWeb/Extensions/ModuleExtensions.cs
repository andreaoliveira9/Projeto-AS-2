using Piranha;
using Piranha.Workflow;

namespace MvcWeb.Extensions
{
    /// <summary>
    /// Module extensions for the application.
    /// </summary>
    public static class ModuleExtensions
    {
        /// <summary>
        /// Checks if the modules collection contains a module of the specified type.
        /// </summary>
        /// <typeparam name="T">The module type</typeparam>
        /// <returns>If the module exists</returns>
        public static bool Contains<T>(this Piranha.Runtime.AppModuleList modules) where T : Piranha.Extend.IModule
        {
            return modules.GetByType(typeof(T)) != null;
        }
    }
}
