using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Piranha.Manager.Models;
using Piranha.Security;

namespace Piranha.Manager.Areas.Manager.Pages;

[Authorize(Policy = WorkflowPermission.ManageWorkflows)]
public class WorkflowAdminModel : PageModel
{
    private readonly IApi _api;
    private readonly Services.WorkflowService _workflowService;

    public WorkflowAdminModel(IApi api, Services.WorkflowService workflowService)
    {
        _api = api;
        _workflowService = workflowService;
    }

    public IActionResult OnGet()
    {
        return Page();
    }
}
