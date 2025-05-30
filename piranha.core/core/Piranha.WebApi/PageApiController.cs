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
using Piranha.AspNetCore.Telemetry;
using Piranha.Models;
using Piranha.Telemetry;

namespace Piranha.WebApi;

[ApiController]
[Route("api/page")]
public class PageApiController : Controller
{
    private readonly IApi _api;
    private readonly IAuthorizationService _auth;

    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <param name="api">The current api</param>
    /// <param name="auth">The authorization service</param>
    public PageApiController(IApi api, IAuthorizationService auth)
    {
        _api = api;
        _auth = auth;
    }

    /// <summary>
    /// Gets the full page model for the page with
    /// the specified id.
    /// </summary>
    /// <param name="id">The page id</param>
    /// <returns>The page model</returns>
    [HttpGet]
    [Route("{id:Guid}")]
    public virtual async Task<IActionResult> GetById(Guid id)
    {
        using var activity = PiranhaTelemetry.StartActivity(PiranhaTelemetry.ActivityNames.ApiOperation, "GetPageById");
        activity?.EnrichWithHttpContext(HttpContext);
        activity?.SetTag(PiranhaTelemetry.AttributeNames.ContentId, id.ToString());
        activity?.SetTag("operation.name", AspNetCoreTracingExtensions.CreateOperationName("PageAPI", "GetById", "Page"));
        if (!Module.AllowAnonymousAccess)
        {
            if (!(await _auth.AuthorizeAsync(User, Permissions.Pages)).Succeeded)
            {
                return Unauthorized();
            }
        }
        var page = await _api.Pages.GetByIdAsync<PageBase>(id);
        if (page != null)
        {
            activity?.SetTag(PiranhaTelemetry.AttributeNames.ContentType, page.TypeId);
            activity?.SetOperationStatus(true, "Page retrieved successfully");
        }
        else
        {
            activity?.SetOperationStatus(false, "Page not found");
        }
        return Json(page);
    }

    /// <summary>
    /// Gets the full page model for the page with
    /// the specified slug in the default site.
    /// </summary>
    /// <param name="slug">The slug</param>
    /// <returns>The page model</returns>
    [HttpGet]
    [Route("{slug}")]
    public virtual async Task<IActionResult> GetBySlug(string slug)
    {
        using var activity = PiranhaTelemetry.StartActivity(PiranhaTelemetry.ActivityNames.ApiOperation, "GetPageBySlug");
        activity?.EnrichWithHttpContext(HttpContext);
        activity?.SetTag("page.slug", slug);
        activity?.SetTag("operation.name", AspNetCoreTracingExtensions.CreateOperationName("PageAPI", "GetBySlug", "Page"));
        if (!Module.AllowAnonymousAccess)
        {
            if (!(await _auth.AuthorizeAsync(User, Permissions.Pages)).Succeeded)
            {
                return Unauthorized();
            }
        }
        var page = await _api.Pages.GetBySlugAsync<PageBase>(slug);
        if (page != null)
        {
            activity?.SetTag(PiranhaTelemetry.AttributeNames.ContentId, page.Id.ToString());
            activity?.SetTag(PiranhaTelemetry.AttributeNames.ContentType, page.TypeId);
            activity?.SetOperationStatus(true, "Page retrieved successfully");
        }
        else
        {
            activity?.SetOperationStatus(false, "Page not found");
        }
        return Json(page);
    }

    /// <summary>
    /// Gets the full page model for the page with
    /// the specified slug and site.
    /// </summary>
    /// <param name="siteId">The site id</param>
    /// <param name="slug">The slug</param>
    /// <returns>The page model</returns>
    [HttpGet]
    [Route("{siteId}/{slug}")]
    public virtual async Task<IActionResult> GetBySlugAndSite(Guid siteId, string slug)
    {
        if (!Module.AllowAnonymousAccess)
        {
            if (!(await _auth.AuthorizeAsync(User, Permissions.Pages)).Succeeded)
            {
                return Unauthorized();
            }
        }
        return Json(await _api.Pages.GetBySlugAsync<PageBase>(slug, siteId));
    }

    /// <summary>
    /// Gets the page info model for the page with
    /// the specified id.
    /// </summary>
    /// <param name="id">The page id</param>
    /// <returns>The page model</returns>
    [HttpGet]
    [Route("info/{id:Guid}")]
    public virtual async Task<IActionResult> GetInfoById(Guid id)
    {
        if (!Module.AllowAnonymousAccess)
        {
            if (!(await _auth.AuthorizeAsync(User, Permissions.Pages)).Succeeded)
            {
                return Unauthorized();
            }
        }
        return Json(await _api.Pages.GetByIdAsync<PageInfo>(id));
    }

    /// <summary>
    /// Gets the page info model for the page with
    /// the specified slug in the default site.
    /// </summary>
    /// <param name="slug">The slug</param>
    /// <returns>The page model</returns>
    [HttpGet]
    [Route("info/{slug}")]
    public virtual async Task<IActionResult> GetInfoBySlug(string slug)
    {
        if (!Module.AllowAnonymousAccess)
        {
            if (!(await _auth.AuthorizeAsync(User, Permissions.Pages)).Succeeded)
            {
                return Unauthorized();
            }
        }
        return Json(await _api.Pages.GetBySlugAsync<PageInfo>(slug));
    }

    /// <summary>
    /// Gets the page info model for the page with
    /// the specified slug and site.
    /// </summary>
    /// <param name="siteId">The site id</param>
    /// <param name="slug">The slug</param>
    /// <returns>The page model</returns>
    [HttpGet]
    [Route("info/{siteId}/{slug}")]
    public virtual async Task<IActionResult> GetInfoBySlugAndSite(Guid siteId, string slug)
    {
        if (!Module.AllowAnonymousAccess)
        {
            if (!(await _auth.AuthorizeAsync(User, Permissions.Pages)).Succeeded)
            {
                return Unauthorized();
            }
        }
        return Json(await _api.Pages.GetBySlugAsync<PageInfo>(slug, siteId));
    }
}
