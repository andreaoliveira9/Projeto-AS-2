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

namespace Piranha.Manager.Controllers;

/// <summary>
/// Api controller for workflow role assignments.
/// </summary>
[Area("Manager")]
[Route("manager/api/workflow/roleassignments")]
[Authorize(Policy = Permission.Admin)]
[ApiController]
public class WorkflowRoleApiController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ManagerLocalizer _localizer;

    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <param name="userManager">The user manager</param>
    /// <param name="roleManager">The role manager</param>
    /// <param name="localizer">The localizer</param>
    public WorkflowRoleApiController(
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ManagerLocalizer localizer)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _localizer = localizer;
    }

    /// <summary>
    /// Gets all role assignments.
    /// </summary>
    /// <returns>The role assignments</returns>
    [HttpGet]
    [Authorize(Policy = WorkflowPermission.ManageWorkflows)]
    public async Task<ActionResult<IEnumerable<WorkflowRoleAssignmentModel>>> GetAll()
    {
        var result = new List<WorkflowRoleAssignmentModel>();

        foreach (var user in _userManager.Users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            
            foreach (var role in roles.Where(r => IsWorkflowRole(r)))
            {
                result.Add(new WorkflowRoleAssignmentModel
                {
                    Id = Guid.NewGuid(), // In a real system, this would be a database ID
                    Username = user.UserName,
                    Role = role,
                    AssignedBy = User.Identity.Name,
                    AssignedDate = DateTime.Now // In a real system, this would be from the database
                });
            }
        }

        return Ok(result);
    }

    /// <summary>
    /// Adds a role assignment.
    /// </summary>
    /// <param name="model">The role assignment model</param>
    /// <returns>The result</returns>
    [HttpPost]
    [Authorize(Policy = WorkflowPermission.ManageWorkflows)]
    public async Task<ActionResult<WorkflowRoleAssignmentModel>> Add([FromBody] WorkflowRoleAssignmentModel model)
    {
        if (string.IsNullOrEmpty(model.Username) || string.IsNullOrEmpty(model.Role))
        {
            return BadRequest("Username and role are required");
        }

        if (!IsWorkflowRole(model.Role))
        {
            return BadRequest($"Role '{model.Role}' is not a valid workflow role");
        }

        var user = await _userManager.FindByNameAsync(model.Username);
        if (user == null)
        {
            return NotFound($"User '{model.Username}' not found");
        }

        // Ensure the role exists
        if (!await _roleManager.RoleExistsAsync(model.Role))
        {
            await _roleManager.CreateAsync(new IdentityRole(model.Role));
        }

        // Assign the role to the user
        var result = await _userManager.AddToRoleAsync(user, model.Role);
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors.First().Description);
        }

        model.Id = Guid.NewGuid(); // In a real system, this would be a database ID
        model.AssignedBy = User.Identity.Name;
        model.AssignedDate = DateTime.Now;

        return Ok(model);
    }

    /// <summary>
    /// Removes a role assignment.
    /// </summary>
    /// <param name="id">The assignment id</param>
    /// <returns>The result</returns>
    [HttpDelete("{id:Guid}")]
    [Authorize(Policy = WorkflowPermission.ManageWorkflows)]
    public ActionResult Remove(Guid id)
    {
        // In a real implementation, we would get the assignment from a database
        // For this example, we'll just return a success
        return Ok();
    }

    /// <summary>
    /// Gets all workflow roles.
    /// </summary>
    /// <returns>The workflow roles</returns>
    [HttpGet("roles")]
    [Authorize(Policy = WorkflowPermission.ManageWorkflows)]
    public ActionResult<IEnumerable<string>> GetRoles()
    {
        return Ok(new[] { "Admin", "Editor", "LegalReviewer" });
    }

    /// <summary>
    /// Gets all users.
    /// </summary>
    /// <returns>The users</returns>
    [HttpGet("users")]
    [Authorize(Policy = WorkflowPermission.ManageWorkflows)]
    public ActionResult<IEnumerable<object>> GetUsers()
    {
        return Ok(_userManager.Users.Select(u => new
        {
            username = u.UserName,
            email = u.Email
        }));
    }

    /// <summary>
    /// Checks if the given role is a workflow role.
    /// </summary>
    /// <param name="role">The role name</param>
    /// <returns>If the role is a workflow role</returns>
    private bool IsWorkflowRole(string role)
    {
        return role switch
        {
            "Admin" => true,
            "Editor" => true,
            "LegalReviewer" => true,
            _ => false
        };
    }
}

/// <summary>
/// Model for a workflow role assignment.
/// </summary>
public class WorkflowRoleAssignmentModel
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
