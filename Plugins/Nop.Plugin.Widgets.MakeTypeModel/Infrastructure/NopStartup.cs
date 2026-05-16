using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Controllers;
using Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Factories;
using Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Services;
using Nop.Plugin.Widgets.MakeTypeModel.Controllers;
using Nop.Plugin.Widgets.MakeTypeModel.Factories;
using Nop.Plugin.Widgets.MakeTypeModel.Services.CustomGenericAttribute;
using Nop.Plugin.Widgets.MakeTypeModel.Services.GranitProductImport;
using Nop.Plugin.Widgets.MakeTypeModel.Services.MakeTypeModel;
using Nop.Plugin.Widgets.MakeTypeModel.Services.Orders;
using Nop.Plugin.Widgets.MakeTypeModel.Services.PriceImport;
using Nop.Plugin.Widgets.MakeTypeModel.Services.ProductImport;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Controllers;

namespace Nop.Plugin.Widgets.MakeTypeModel.Infrastructure
{
    /// <summary>
    /// Represents a plugin dependency registrar
    /// </summary>
    public class NopStartup : INopStartup
    {
        #region Methods

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
            services.AddScoped<IMakeTypeModelService, MakeTypeModelService>();
            services.AddScoped<IProductImportService, ProductImportService>();
            services.AddScoped<IGranitProductImportService, GranitProductImportService>();
            services.AddScoped<ReturnRequestController, OverrideReturnRequestController>();
            services.AddScoped<IReturnRequestModelFactory, OverrideReturnRequestModelFactory>();
            services.AddScoped<IPriceImportService, PriceImportService>();
            services.AddScoped<Web.Areas.Admin.Controllers.ReturnRequestController, OverrideAdminReturnRequestController>();
            services.AddScoped<ICustomAdminReturnRequestModelFactory, CustomAdminReturnRequestModelFactory>();
            services.AddScoped<ICustomReturnRequestService, CustomReturnRequestService>();
            services.AddScoped<IFindPartsModelFactory, FindPartsModelFactory>();
            services.AddScoped<ICustomReturnRequestModelFactory, CustomReturnRequestModelFactory>();
            services.AddScoped<Web.Factories.IReturnRequestModelFactory, OverrideWebReturnRequestModelFactory>();
            services.AddScoped<IWorkflowMessageService, OverrideWorkflowMessageService>();
            services.AddScoped<ICustomGenericAttributeService, CustomGenericAttributeService>();
            services.AddScoped<IReturnRequestService, OverrideReturnRequestService>();
            services.AddScoped<IMakeTypeModelFactory, MakeTypeModelFactory>();
            services.AddScoped<ICustomProductModelFactory, CustomProductModelFactory>();

        }

        /// <summary>
        /// Register services and interfaces
        /// </summary>
        /// <param name="services">Collection of service descriptors</param>
        /// <param name="typeFinder">Type finder</param>
        /// <param name="appSettings">App settings</param>
        public void Register(IServiceCollection services, ITypeFinder typeFinder, AppSettings appSettings)
        {
            //services
        }

        /// <summary>
        /// Configure the using of added middleware
        /// </summary>
        /// <param name="application">Builder for configuring an application's request pipeline</param>
        public void Configure(IApplicationBuilder application)
        {
            
        }
        #endregion

        #region Properties
        /// <summary>
        /// Order of this dependency registrar implementation
        /// </summary>
        public int Order => int.MaxValue;

        #endregion
    }
}