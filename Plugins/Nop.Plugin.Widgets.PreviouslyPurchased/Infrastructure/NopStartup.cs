using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Plugin.Widgets.PreviouslyPurchased.Factories;
using Nop.Plugin.Widgets.PreviouslyPurchased.Services;

namespace Nop.Plugin.Widgets.PreviouslyPurchased.Infrastructure;

/// <summary>
/// Represents the startup configuration for the PreviouslyPurchased plugin.
/// Registers services and configures middleware for the plugin.
/// </summary>
public class NopStartup : INopStartup
{
    #region Methods
    /// <summary>
    /// Adds and configures services for the PreviouslyPurchased plugin during application startup.
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

        // Register the services used by the PreviouslyPurchased plugin
        services.AddScoped<IPreviouslyPurchasedService, PreviouslyPurchasedService>();
        services.AddScoped<IPreviouslyPurchasedProductsModelFactory, PreviouslyPurchasedProductsModelFactory>();
    }

    /// <summary>
    /// Configures the middleware pipeline for the application.
    /// </summary>
    /// <param name="application">The builder for configuring the application's request pipeline.</param>
    public void Configure(IApplicationBuilder application)
    {
    }
    #endregion

    #region Properties
    /// <summary>
    /// Gets the order of this startup configuration implementation.
    /// </summary>
    public int Order => 11;
    #endregion
}