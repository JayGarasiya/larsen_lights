using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Widgets.ProductExtension.Services;
using Nop.Web.Factories;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Widgets.ProductExtension.Components
{
    /// <summary>
    /// Represents the view component to place a widget into pages
    /// </summary>
    public class HeaderLoginViewComponent : NopViewComponent
    {
        #region Fields

        protected readonly IProductExtendService _productExtendService;
        protected readonly ICommonModelFactory _commonModelFactory;

        #endregion

        #region Ctor

        public HeaderLoginViewComponent(IProductExtendService productExtendService,
            ICommonModelFactory commonModelFactory)
        {
            _productExtendService = productExtendService;
            _commonModelFactory = commonModelFactory;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Invoke view component
        /// </summary>
        /// <param name="widgetZone">Widget zone name</param>
        /// <param name="additionalData">Additional data</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the view component result
        /// </returns>
        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            if (!await _productExtendService.PluginActiveAsync())
                return Content(string.Empty);

            var model = await _commonModelFactory.PrepareHeaderLinksModelAsync();

            return View("~/Plugins/Widgets.ProductExtension/Views/Shared/Components/HeaderLogin/Default.cshtml", model);
        }

        #endregion
    }
}