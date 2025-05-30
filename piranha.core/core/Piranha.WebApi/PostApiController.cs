/*
 * Copyright (c) .NET Foundation and Contributors
 *
 * This software may be modified and distributed under the terms
 * of the MIT license. See the LICENSE file for details.
 *
 * https://github.com/piranhacms/piranha.core
 *
 */

using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Piranha.Models;
using Piranha.Telemetry;

namespace Piranha.WebApi;

[ApiController]
[Route("api/post")]
public class PostApiController : Controller
{
    private readonly IApi _api;
    private readonly IAuthorizationService _auth;

    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <param name="api">The current api</param>
    /// <param name="auth">The authorization service</param>
    public PostApiController(IApi api, IAuthorizationService auth)
    {
        _api = api;
        _auth = auth;
    }

    /// <summary>
    /// Gets the full post model for the post with
    /// the specified id.
    /// </summary>
    /// <param name="id">The post id</param>
    /// <returns>The post model</returns>
    [HttpGet]
    [Route("{id:Guid}")]
    public virtual async Task<IActionResult> GetById(Guid id)
    {
        using var activity = PiranhaTelemetry.StartActivity(PiranhaTelemetry.ActivityNames.ApiOperation, "GetPostById");
        activity?.EnrichWithHttp("GET", $"/api/post/{id}", User?.Identity?.Name);
        activity?.SetTag(PiranhaTelemetry.AttributeNames.ContentId, id.ToString());
        if (!Module.AllowAnonymousAccess)
        {
            if (!(await _auth.AuthorizeAsync(User, Permissions.Posts)).Succeeded)
            {
                return Unauthorized();
            }
        }
        var post = await _api.Posts.GetByIdAsync<PostBase>(id);
        if (post != null)
        {
            activity?.SetTag(PiranhaTelemetry.AttributeNames.ContentType, post.TypeId);
            activity?.SetOperationStatus(true, "Post retrieved successfully");
        }
        else
        {
            activity?.SetOperationStatus(false, "Post not found");
        }
        return Json(post);
    }

    /// <summary>
    /// Gets the full post model for the post with
    /// the specified archive and slug.
    /// </summary>
    /// <param name="archiveId">The archive id</param>
    /// <param name="slug">The slug</param>
    /// <returns>The post model</returns>
    [HttpGet]
    [Route("{archiveId}/{slug}")]
    public virtual async Task<IActionResult> GetBySlugAndArchive(Guid archiveId, string slug)
    {
        using var activity = PiranhaTelemetry.StartActivity(PiranhaTelemetry.ActivityNames.ApiOperation, "GetPostBySlug");
        activity?.EnrichWithHttp("GET", $"/api/post/{archiveId}/{slug}", User?.Identity?.Name);
        activity?.SetTag("post.archive_id", archiveId.ToString());
        activity?.SetTag("post.slug", slug);
        if (!Module.AllowAnonymousAccess)
        {
            if (!(await _auth.AuthorizeAsync(User, Permissions.Posts)).Succeeded)
            {
                return Unauthorized();
            }
        }
        var post = await _api.Posts.GetBySlugAsync<PostBase>(archiveId, slug);
        if (post != null)
        {
            activity?.SetTag(PiranhaTelemetry.AttributeNames.ContentId, post.Id.ToString());
            activity?.SetTag(PiranhaTelemetry.AttributeNames.ContentType, post.TypeId);
            activity?.SetOperationStatus(true, "Post retrieved successfully");
        }
        else
        {
            activity?.SetOperationStatus(false, "Post not found");
        }
        return Json(post);
    }

    /// <summary>
    /// Gets the post info model for the post with
    /// the specified id.
    /// </summary>
    /// <param name="id">The post id</param>
    /// <returns>The post model</returns>
    [HttpGet]
    [Route("info/{id:Guid}")]
    public virtual async Task<IActionResult> GetInfoById(Guid id)
    {
        if (!Module.AllowAnonymousAccess)
        {
            if (!(await _auth.AuthorizeAsync(User, Permissions.Posts)).Succeeded)
            {
                return Unauthorized();
            }
        }
        return Json(await _api.Posts.GetByIdAsync<PostInfo>(id));
    }

    /// <summary>
    /// Gets the post info model for the post with
    /// the specified archive and slug.
    /// </summary>
    /// <param name="archiveId">The archive id</param>
    /// <param name="slug">The slug</param>
    /// <returns>The post model</returns>
    [HttpGet]
    [Route("info/{archiveId}/{slug}")]
    public virtual async Task<IActionResult> GetInfoBySlugAndSite(Guid archiveId, string slug)
    {
        if (!Module.AllowAnonymousAccess)
        {
            if (!(await _auth.AuthorizeAsync(User, Permissions.Posts)).Succeeded)
            {
                return Unauthorized();
            }
        }
        return Json(await _api.Posts.GetBySlugAsync<PostInfo>(archiveId, slug));
    }
}
