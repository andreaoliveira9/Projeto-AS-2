using Microsoft.AspNetCore.Mvc;

namespace MvcWeb.Controllers;

[Route("dashboard")]
public class DashboardController : Controller
{
    [HttpGet("metrics")]
    public IActionResult MetricsDashboard()
    {
        return View("~/Views/Shared/_MetricsDashboard.cshtml");
    }
}