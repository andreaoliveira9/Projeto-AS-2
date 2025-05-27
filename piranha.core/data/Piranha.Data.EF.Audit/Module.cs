/*
 * Copyright (c) .NET Foundation and Contributors
 *
 * This software may be modified and distributed under the terms
 * of the MIT license. See the LICENSE file for details.
 *
 * https://github.com/piranhacms/piranha.core
 *
 */

using Microsoft.Extensions.DependencyInjection;
using Piranha;
using Piranha.Audit.Repositories;
using Piranha.Data.EF.Audit.Repositories;

namespace Piranha.Data.EF.Audit;

/// <summary>
/// Extension methods for setting up Audit EF services.
/// </summary>
public static class PiranhaServiceCollectionExtensions
{
    /// <summary>
    /// Adds the Audit EF repositories to the service collection.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddAuditRepositories(this IServiceCollection services)
    {
        services.AddScoped<IStateChangeRecordRepository, StateChangeRecordRepository>();

        return services;
    }

    /// <summary>
    /// Uses the Audit EF repositories.
    /// </summary>
    /// <param name="serviceBuilder">The service builder</param>
    /// <returns>The updated builder</returns>
    public static PiranhaServiceBuilder UseAuditEF(this PiranhaServiceBuilder serviceBuilder)
    {
        serviceBuilder.Services.AddAuditRepositories();
        
        return serviceBuilder;
    }
}
