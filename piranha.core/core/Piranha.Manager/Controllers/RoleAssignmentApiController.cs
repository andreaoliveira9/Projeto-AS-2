/*
 * Copyright (c) .NET Foundation and Contributors
 *
 * This software may be modified and distributed under the terms
 * of the MIT license. See the LICENSE file for details.
 *
 * https://github.com/piranhacms/piranha.core
 *
 */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Piranha.Manager.Models;
using Piranha.Security;
using Piranha.Models;

namespace Piranha.Manager.Controllers;

/// <summary>
/// Api controller for role assignments.
/// </summary>
[Area("Manager")]
[Route("manager/api/workflow/roleassignments")]
[Authorize(Policy = WorkflowPermission.ManageWorkflows)]
[ApiController]
public class RoleAssignmentApiController : Controller
{
    private readonly IApi _api;
    private readonly UserManager<IdentityUser> _userManager;

    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <param name="api">The current api</param>
    /// <param name="userManager">The user manager</param>
    public RoleAssignmentApiController(
        IApi api,
        UserManager<IdentityUser> userManager)
    {
        _api = api;
        _userManager = userManager;
    }

    /// <summary>
    /// Gets all role assignments.
    /// </summary>
    /// <returns>The role assignments</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<RoleAssignmentModel>>> GetAll()
    {
        // In a real implementation, these would come from the database
        // For now, return some mock data
        var assignments = new List<RoleAssignmentModel>
        {
            new RoleAssignmentModel
            {
                Id = Guid.Parse("6c8b9d0d-8e7f-4c5c-9e3d-86d3e3a95c9b"),
                Username = "admin",
                Role = "SysAdmin",
                AssignedBy = "system",
                AssignedDate = DateTime.Now.AddDays(-30)
            },
            new RoleAssignmentModel
            {
                Id = Guid.Parse("7f2e9b3a-5d1c-4b8a-9f6e-8c7d2e1a3b4c"),
                Username = "editor",
                Role = "Editor",
                AssignedBy = "admin",
                AssignedDate = DateTime.Now.AddDays(-20)
            }
        };

        return Ok(assignments);
    }

    /// <summary>
    /// Adds a new role assignment.
    /// </summary>
    /// <param name="model">The role assignment model</param>
    /// <returns>The created role assignment</returns>
    [HttpPost]
    public async Task<ActionResult<RoleAssignmentModel>> Add([FromBody] RoleAssignmentModel model)
    {
        // In a real implementation, the role would be saved to the database
        // For now, just return the model with an ID
        model.Id = Guid.NewGuid();
        model.AssignedBy = User.Identity.Name;
        model.AssignedDate = DateTime.Now;

        return Ok(model);
    }

    /// <summary>
    /// Deletes a role assignment.
    /// </summary>
    /// <param name="id">The role assignment id</param>
    /// <returns>The result</returns>
    [HttpDelete("{id:Guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        // In a real implementation, the role would be deleted from the database
        return Ok();
    }
}

/// <summary>
/// Model for a role assignment.
/// </summary>
public class RoleAssignmentModel
{
    /// <summary>
    /// Gets/sets the id.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets/sets the username.
    /// </summary>
    public string Username { get; set; }

    /// <summary>
    /// Gets/sets the role.
    /// </summary>
    public string Role { get; set; }

    /// <summary>
    /// Gets/sets who assigned the role.
    /// </summary>
    public string AssignedBy { get; set; }

    /// <summary>
    /// Gets/sets when the role was assigned.
    /// </summary>
    public DateTime AssignedDate { get; set; }
}