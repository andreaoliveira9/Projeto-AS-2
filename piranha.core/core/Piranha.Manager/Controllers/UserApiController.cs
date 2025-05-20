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
/// Api controller for user operations.
/// </summary>
[Area("Manager")]
[Route("manager/api/users")]
[Authorize(Policy = Permission.Admin)]
[ApiController]
public class UserApiController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;

    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <param name="userManager">The user manager</param>
    public UserApiController(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
    }

    /// <summary>
    /// Gets all users.
    /// </summary>
    /// <returns>The users</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserModel>>> GetAll()
    {
        // In a real implementation, these would come from querying the user manager
        // For now, return some mock data
        var users = new List<UserModel>
        {
            new UserModel
            {
                Id = "1",
                Username = "admin",
                Email = "admin@example.com"
            },
            new UserModel
            {
                Id = "2",
                Username = "editor",
                Email = "editor@example.com"
            }
        };

        return Ok(users);
    }
}

/// <summary>
/// Model for a user.
/// </summary>
public class UserModel
{
    /// <summary>
    /// Gets/sets the id.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Gets/sets the username.
    /// </summary>
    public string Username { get; set; }

    /// <summary>
    /// Gets/sets the email.
    /// </summary>
    public string Email { get; set; }
}