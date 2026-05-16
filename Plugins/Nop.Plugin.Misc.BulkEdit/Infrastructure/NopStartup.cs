using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;

namespace Nop.Plugin.Misc.BulkEdit.Infrastructure;

/// <summary>
/// Represents the startup configuration for the Bulk Edit plugin.
/// Registers services and configures middleware for the plugin.
/// </summary>
public class NopStartup : INopStartup
{
    #region Methods
    /// <summary>
    /// Configures the middleware pipeline for the application.
    /// </summary>
    /// <param name="application">The builder for configuring the application's request pipeline.</param>
    public void Configure(IApplicationBuilder application)
    {
    }

    /// <summary>
    /// Adds and configures services for the BulkEdit plugin during application startup.
    /// </summary>
    /// <param name="services">The collection of service descriptors to configure.</param>
    /// <param name="configuration">The configuration for the application.</param>
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Configure Razor view engine options to add custom view location expanders
        services.Configure<RazorViewEngineOptions>(options =>
        {
            options.ViewLocationExpanders.Add(new ViewLocationExpander());
        });
    }
    #endregion

    #region Properties
    /// <summary>
    /// Gets the order of this startup configuration implementation.
    /// </summary>
    public int Order => int.MaxValue;
    #endregion
}
