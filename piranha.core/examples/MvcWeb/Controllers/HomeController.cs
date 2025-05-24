using Microsoft.AspNetCore.Mvc;

namespace MvcWeb.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult WorkflowDashboard()
    {
        return View();
    }
}
