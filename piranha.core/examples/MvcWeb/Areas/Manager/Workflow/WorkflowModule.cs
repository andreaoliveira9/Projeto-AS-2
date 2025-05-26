using Piranha.Manager;
using Piranha.Manager.Menu;

namespace MvcWeb.Areas.Manager.Workflow;

public class WorkflowModule : IModule
{
    private readonly MenuItemList _menu;

    public WorkflowModule(IMenuItemList menu)
    {
        _menu = menu;
    }

    public void Init()
    {
        // Add the workflow menu item
        _menu.Add(new MenuItem
        {
            InternalId = "Workflow",
            Name = "Workflows",
            Css = "fas fa-project-diagram",
            Route = "/workflow",
            Policy = Permission.Admin,
            SortOrder = 110
        });
    }
} 