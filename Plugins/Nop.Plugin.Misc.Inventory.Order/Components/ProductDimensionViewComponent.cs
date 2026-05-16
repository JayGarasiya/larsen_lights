using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Cms;
using Nop.Plugin.Misc.Inventory.Order.Models;
using Nop.Plugin.Misc.Inventory.Order.Services;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Misc.Inventory.Order.Components
{
    /// <summary>
    /// Represents the view component for displaying the "Product Dimensions" tab in product details.
    /// </summary>
    public class ProductDimensionViewComponent : NopViewComponent
    {
        #region Fields
        private readonly IPoOrderService _poorderService;
        private readonly WidgetSettings _widgetSettings;
        #endregion

        #region Ctor
        public ProductDimensionViewComponent(IPoOrderService poorderService, 
            WidgetSettings widgetSettings)
        {
            _poorderService = poorderService;
            _widgetSettings = widgetSettings;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Invokes the view component to display the product dimensions tab in product details.
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

            if (additionalData is not ProductModel productModel)
                return Content(string.Empty);

            if (productModel.Id == 0)
                return Content(string.Empty);

            var productDimensions = await _poorderService.GetProductDimensionsByProductId(productModel.Id);
            var model = new ProductDimensionsModel();

            if (productDimensions != null)
                model = productDimensions.ToModel<ProductDimensionsModel>();

            model.ProductId = productModel.Id;
            return View("~/Plugins/Inventory.Order/Views/_CreateOrUpdate.PanelProductDimension.cshtml", model);
        }
        #endregion
    }
}
