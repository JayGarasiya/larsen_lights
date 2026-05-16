using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Plugin.Widgets.ProductExtension.Areas.Admin.Controllers;
using Nop.Plugin.Widgets.ProductExtension.Areas.Admin.Factories;
using Nop.Plugin.Widgets.ProductExtension.Contollers;
using Nop.Plugin.Widgets.ProductExtension.Factories;
using Nop.Plugin.Widgets.ProductExtension.Services;
using Nop.Services.Catalog;
using Nop.Services.Media;
using Nop.Web.Controllers;
using Nop.Web.Factories;

namespace Nop.Plugin.Widgets.ProductExtension.Infrastructure
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
            services.AddScoped<IProductExtendService, ProductExtendService>();
            services.AddScoped<IQuoteService, QuoteService>();

            //override services
            services.AddScoped<IProductService, OverrideProductService>();
            services.AddScoped<IProductAttributeParser, OverrideProductAttributeParser>();
            services.AddScoped<IPictureService, OverridePictureService>();

            //override factory services
            services.AddScoped<IProductModelFactory, ProductPriceModelFactory>();
            services.AddScoped<Web.Areas.Admin.Factories.IProductModelFactory, OverrideProductModelFactory>();
            services.AddScoped<ICustomProductModelFactory, CustomProductModelFactory>();
            services.AddScoped<ICustomProductService, CustomProductService>();

            services.AddScoped<IProductExtensionFactory, ProductExtensionFactory>();

            //override controller
            services.AddScoped<ShoppingCartController, OverrideShoppingCartController>();
            services.AddScoped<Web.Areas.Admin.Controllers.ProductController, OverrideProductController>();
            services.AddScoped<Web.Areas.Admin.Controllers.ReportController, OverrideReportController>();

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
}
