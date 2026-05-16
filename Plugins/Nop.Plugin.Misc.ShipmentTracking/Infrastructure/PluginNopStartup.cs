using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Plugin.Misc.ShipmentTracking.Areas.Admin.Factories;
using Nop.Plugin.Misc.ShipmentTracking.Factories;
using Nop.Plugin.Misc.ShipmentTracking.Services;
using Nop.Web.Factories;

namespace Nop.Plugin.Misc.ShipmentTracking.Infrastructure
{
    /// <summary>
    /// Represents object for the configuring plugin DB context on application startup
    /// </summary>
    public class PluginNopStartup : INopStartup
    {
        /// <summary>
        /// Add and configure any of the middleware
        /// </summary>
        /// <param name="services">Collection of service descriptors</param>
        /// <param name="configuration">Configuration of the application</param>
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            //services
            services.AddScoped<IShipmentTrackingService, ShipmentTrackingService>();
            services.AddScoped<IAdminNoteService, AdminNoteService>();

            //override factory services
            services.AddScoped<IOrderModelFactory, OrderShipmentModelFactory>();

            //override admin factory services
            services.AddScoped<Web.Areas.Admin.Factories.IOrderModelFactory, OverrideOrderModelFactory>();

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
        public int Order => 2003;
    }
}
