using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Plugin.Tax.TaxJar.Factories;
using Nop.Plugin.Tax.TaxJar.Filters;
using Nop.Plugin.Tax.TaxJar.Services;
using Nop.Services.Security;
using Nop.Services.Tax;
using Nop.Web.Factories;

namespace Nop.Plugin.Tax.TaxJar.Infrastructure;

/// <summary>
/// Represents object for the configuring plugin DB context on application startup
/// </summary>
public class PluginDbStartup : INopStartup
{
    /// <summary>
    /// Add and configure any of the middleware
    /// </summary>
    /// <param name="services">Collection of service descriptors</param>
    /// <param name="configuration">Configuration of the application</param>
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Services
        services.AddScoped<TaxJarTaxManager>();
        services.AddScoped<TaxJarRequestLogService>();
        services.AddScoped<ITaxJarService, TaxJarService>();

        // Override services
        services.AddScoped<ITaxService, OverrideTaxService>();
        services.AddScoped<IAclService, OverrideAclService>();

        // Factories
        services.AddScoped<ITaxJarModelFactory, TaxJarModelFactory>();

        // Override factory services
        services.AddScoped<IAddressModelFactory, OverrideAddressModelFactory>();
        services.AddScoped<ICustomerModelFactory, OverrideCustomerModelFactory>();

        // Register action filter 
        services.Configure<MvcOptions>(options =>
        {
            options.Filters.Add<CustomerCreateAndUpdateActionFilter>();
        });

        services.Configure<RazorViewEngineOptions>(options =>
        {
            options.ViewLocationExpanders.Add(new ViewLocationExpander());
        });
    }

    /// <summary>
    /// Configure the using of added middleware
    /// </summary>
    /// <param name="application">Builder for configuring an application's request pipeline</param>
    public void Configure(IApplicationBuilder application)
    {
    }

    /// <summary>
    /// Gets order of this startup configuration implementation
    /// </summary>
    public int Order => 3000;
}
