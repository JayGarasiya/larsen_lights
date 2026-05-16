using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Plugin.Misc.InvoicePDF.Areas.Admin.Controllers;
using Nop.Plugin.Misc.InvoicePDF.Areas.Admin.Factories;
using Nop.Plugin.Misc.InvoicePDF.Services;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Areas.Admin.Factories;

namespace Nop.Plugin.Misc.InvoicePDF.Infrastructure
{
    /// <summary>
    /// Represents object for the configuring plugin DB context on application startup
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
            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.ViewLocationExpanders.Add(new ViewLocationExpander());
            });

            services.AddScoped<IProductAttributeFormatter, OverrideProductAttributeFormatter>();
            services.AddScoped<IPdfService, OverridePdfService>();
            services.AddScoped<IOrderService, OverrideOrderService>();
            services.AddScoped<IMessageTokenProvider, OverrideMessageTokenProvider>();
            services.AddScoped<IOrderProcessingService, OverrideOrderProcessingService>();
            services.AddScoped<IMessageTemplateService, OverrideMessageTemplateService>();
            services.AddScoped<IMailTemplateCustomerRoleService, MailTemplateCustomerRoleService>();
            services.AddScoped<IOrderTotalCalculationService, OverrideOrderTotalCalculationService>();

            //override factories
            services.AddScoped<IMessageTemplateModelFactory, OverrideMessageTemplateModelFactory>();

            //override controller
            services.AddScoped<MessageTemplateController, OverrideMessageTemplateController>();
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
        public int Order => 9999;
    }
}
