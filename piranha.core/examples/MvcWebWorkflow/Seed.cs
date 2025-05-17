using Piranha;
using Piranha.Extend;
using Piranha.Extend.Blocks;
using Piranha.Extend.Fields;
using Piranha.Models;
using MvcWebWorkflow.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MvcWebWorkflow;

/// <summary>
/// Static helper class for seeding test content.
/// </summary>
public static class Seed
{
    /// <summary>
    /// Seeds the test content.
    /// </summary>
    /// <param name="api">The current api</param>
    /// <returns>An awaitable task</returns>
    public static async Task RunAsync(IApi api)
    {
        if ((await api.Pages.GetAllAsync()).Count() == 0)
        {
            // Get the default site
            var site = await api.Sites.GetDefaultAsync();

            await SeedArchivePageAsync(api, site);
            var home = await SeedStartpageAsync(api, site);

            // Add the home page as a default page for the site
            site.SiteTypeId = nameof(BlogSite);
            site.Title = "MvcWebWorkflow";

            await api.Sites.SaveAsync(site);
        }
    }

    private static async Task<PageBase> SeedArchivePageAsync(IApi api, Site site)
    {
        var arch = await StandardArchive.CreateAsync(api);
        arch.Id = Guid.NewGuid();
        arch.SiteId = site.Id;
        arch.Title = "Blog Archive";
        arch.MetaKeywords = "Piranha, Piranha CMS, AspNetCore, .NET, .NET Core";
        arch.MetaDescription = "Piranha CMS Blog";
        arch.NavigationTitle = "Blog";
        arch.Published = DateTime.Now;

        await api.Pages.SaveAsync(arch);

        arch = await api.Pages.GetByIdAsync<StandardArchive>(arch.Id);
        return arch;
    }

    private static async Task<PageBase> SeedStartpageAsync(IApi api, Site site)
    {
        var page = await StandardPage.CreateAsync(api);
        page.Id = Guid.NewGuid();
        page.SiteId = site.Id;
        page.SortOrder = 1;
        page.Title = "Welcome to Piranha CMS";
        page.MetaKeywords = "Piranha, Piranha CMS, AspNetCore, .NET, .NET Core";
        page.MetaDescription = "Piranha is the fun, fast and lightweight framework for developing cms-based web applications with AspNetCore.";
        page.NavigationTitle = "Home";
        page.Blocks.Add(new HtmlBlock
        {
            Body = "<h2>Welcome to your Piranha CMS site</h2><p>This is the default page you get after installing a new site with the MVC View application. Now you can do the following:</p>"
        });
        page.Blocks.Add(new ColumnBlock
        {
            Items = new System.Collections.Generic.List<Block>()
            {
                new HtmlBlock
                {
                    Body = "<h3>Create your content model</h3><p>You can either use Piranha's integrated model engine with Page Types, Post Types and Content Types or you can use your own completely seperate data models and just use Piranha to create a powerful and flexible UI for content management. It's up to you!</p>"
                },
                new HtmlBlock
                {
                    Body = "<h3>Create your design</h3><p>Piranha has a built in API for working with your content as strongly typed .NET objects. You can use the included Razor Pages, create an MVC Site or use tools like Vue.js, React or Angular to build your frontend.</p>"
                },
                new HtmlBlock
                {
                    Body = "<h3>Interate with other systems</h3><p>Your application is only one part of the IT ecosystem. With Piranha you get built-in support for syndicating your content to other systems through RSS & Atom feeds.</p>"
                }
            }
        });
        page.Blocks.Add(new HtmlBlock
        {
            Body = "<p>And by using the available NuGet modules you can convert your content to real time WebSockets or app push notifications with <code>Piranha.WebSockets</code>. Or why not add complex permissions with <code>Piranha.Security</code>. Everything is ready for you!</p>"
        });
        page.Published = DateTime.Now;

        await api.Pages.SaveAsync(page);

        page = await api.Pages.GetByIdAsync<StandardPage>(page.Id);

        return page;
    }
}

public class BlogSite : SiteContent<BlogSite> { }
