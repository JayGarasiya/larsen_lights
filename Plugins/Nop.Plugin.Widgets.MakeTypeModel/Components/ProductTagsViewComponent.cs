using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Models.Catalog;
using Nop.Services.Catalog;

namespace Nop.Plugin.Widgets.MakeTypeModel.Components
{
    /// <summary>
    /// Represents the view component of product tags
    /// </summary>
    public class ProductTagsViewComponent : ViewComponent
    {
        #region Fields

        protected readonly IProductTagService _productTagService;

        #endregion

        #region Ctor

        public ProductTagsViewComponent(IProductTagService productTagService)
        {
            _productTagService = productTagService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Invoke view component
        /// </summary>
        /// <param name="productId">Product Indentifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the view component result
        /// </returns>
        public async Task<IViewComponentResult> InvokeAsync(int productId)
        {
            var model = new OverrideProductModel();

            if (productId > 0)
            {
                var tags = await _productTagService.GetAllProductTagsByProductIdAsync(productId);
                model.ProductTags = string.Join(", ", tags.Select(x => x.Name));
            }

            return View("~/Plugins/Widgets.MakeTypeModel/Areas/Admin/Views/Product/ProductTag.cshtml", model);
        }

        #endregion
    }
}
