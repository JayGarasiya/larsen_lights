using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Plugin.Widgets.Fulfillment.Factories;
using Nop.Plugin.Widgets.Fulfillment.Services.ShoppingCart;
using Nop.Plugin.Widgets.Fulfillment.Services.ThreePl;
using Nop.Plugin.Widgets.Fulfillment.Services.WMS;
using Nop.Services.Orders;

namespace Nop.Plugin.Widgets.Fulfillment.Infrastructure
{
    /// <summary>
    /// Represents object for the configuring services on application startup
    /// </summary>
    public class NopStartup : INopStartup
    {
        /// <summary>
        /// Add and configure any of the middleware
        /// </summary>
        /// <param name="services">Collection of service descriptors</param>
        /// <param name="configuration">Configuration of the application</param>
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            //override services
            services.AddScoped<IShoppingCartService, OverrideShoppingCartService>();

            //register service
            services.AddScoped<IThreePlService, ThreePlService>();
            services.AddScoped<IThreePlWarehouseService, ThreePlWarehouseService>();

            //register Factories
            services.AddScoped<IFulfillmentModelFactory, FulfillmentModelFactory>();
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
}
