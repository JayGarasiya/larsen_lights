using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Widgets.ProductExtension.Services;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Widgets.ProductExtension.Components
{
    /// <summary>
    /// Represents the view component to place a widget into pages
    /// </summary>
    public class CartQuoteViewComponent : NopViewComponent
    {
        #region Fields

        protected readonly IProductExtendService _productExtendService;
        protected readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public CartQuoteViewComponent(IProductExtendService productExtendService,
            IWorkContext workContext)
        {
            _productExtendService = productExtendService;
            _workContext = workContext;
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

            if (!Request.RouteValues.ContainsKey("action") ? true : !(Request.RouteValues["action"]?.ToString() ?? string.Empty).Equals("Cart", StringComparison.InvariantCultureIgnoreCase))
                return Content(string.Empty);

            var customer = await _workContext.GetCurrentCustomerAsync();

            return View("~/Plugins/Widgets.ProductExtension/Views/ShoppingCart/QuoteInfo.cshtml", customer.CustomerGuid);
        }

        #endregion
    }
}