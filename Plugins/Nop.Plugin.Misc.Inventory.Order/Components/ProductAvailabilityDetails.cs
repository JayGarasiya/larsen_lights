using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Cms;
using Nop.Plugin.Misc.Inventory.Order.Services;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Web.Framework.Components;
using Nop.Web.Models.Catalog;

namespace Nop.Plugin.Misc.Inventory.Order.Components
{
    /// <summary>
    /// Represents the view component for displaying the "Product Availability" message.
    /// </summary>
    public class ProductAvailabilityDetails : NopViewComponent
    {
        #region Fields
        protected readonly IProductService _productService;
        protected readonly IPoOrderService _poOrderService;
        protected readonly ILocalizationService _localizationService;
        protected readonly WidgetSettings _widgetSettings;
        #endregion

        #region Ctor
        public ProductAvailabilityDetails(IProductService productService,
            IPoOrderService poOrderService,
            ILocalizationService localizationService,
            WidgetSettings widgetSettings)
        {
            _productService = productService;
            _poOrderService = poOrderService;
            _localizationService = localizationService;
            _widgetSettings = widgetSettings;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Invokes the view component to display the "Product Availability" message.
        /// </summary>
        /// <param name="widgetZone">The name of the widget zone where this component will be rendered.</param>
        /// <param name="additionalData">Additional data passed from the widget zone (not used in this case).</param>
        /// <returns>
        /// A task representing the asynchronous operation. 
        /// The task result contains the view component result.
        /// </returns>
        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            if (!_widgetSettings.ActiveWidgetSystemNames.Contains(InventoryOrderDefault.SystemName))
                return Content(string.Empty);

            if (additionalData is not ProductDetailsModel productModel)
                return Content(string.Empty);

            if (productModel.Id == 0)
                return Content(string.Empty);

            var product = await _productService.GetProductByIdAsync(productModel.Id);
            var poOrderItemDetalis = await _poOrderService.GetPoOrderItemByProductId(productModel.Id);
            if (poOrderItemDetalis != null)
            {
                var poDetalis = await _poOrderService.GetPoOrderById(poOrderItemDetalis.PoOrderId);
                var totalStock = await _productService.GetTotalStockQuantityAsync(product);
                if (poOrderItemDetalis.OrderedQty > 0 && poDetalis.AvailableDateOnUTC != null && totalStock < product.NotifyAdminForQuantityBelow)
                {
                    var message = string.Format(await _localizationService.GetResourceAsync("Plugins.InventoryOrder.Product.Availability.Message"), poOrderItemDetalis.OrderedQty, Convert.ToDateTime(poDetalis.AvailableDateOnUTC).ToShortDateString());
                    return View("~/Plugins/Inventory.Order/Views/ProductAvailabilityDetails.cshtml", message);
                }
            }

            return Content(string.Empty);
        }
        #endregion
    }
}
