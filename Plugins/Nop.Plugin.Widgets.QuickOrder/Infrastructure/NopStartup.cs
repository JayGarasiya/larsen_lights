using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Plugin.Widgets.QuickOrder.Factories;
using Nop.Plugin.Widgets.QuickOrder.Filters;
using Nop.Plugin.Widgets.QuickOrder.Services;

namespace Nop.Plugin.Widgets.QuickOrder.Infrastructure;

/// <summary>
/// Represents the configuration of services and middleware during the application's startup.
/// </summary>
public class NopStartup : INopStartup
{
    /// <summary>
    /// Adds and configures the required services for the application.
    /// </summary>
    /// <param name="services">Collection of service descriptors.</param>
    /// <param name="configuration">Application configuration.</param>
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Register action filter to handle customer logout logic
        services.Configure<MvcOptions>(options =>
        {
            options.Filters.Add<CustomerLogoutActionFilter>();
        });

        // Register QuickOrder service for dependency injection
        services.AddScoped<IQuickOrderService, QuickOrderService>();
        // Register QuickOrder factory for dependency injection
        services.AddScoped<IQuickOrderModelFactory, QuickOrderModelFactory>();
    }

    /// <summary>
    /// Configures the application's request pipeline (middleware setup).
    /// </summary>
    /// <param name="application">Builder for configuring the application's request pipeline.</param>
    public void Configure(IApplicationBuilder application)
    {
    }

    /// <summary>
    /// Gets the order in which this startup configuration will be executed.
    /// </summary>
    public int Order => 11;
}
