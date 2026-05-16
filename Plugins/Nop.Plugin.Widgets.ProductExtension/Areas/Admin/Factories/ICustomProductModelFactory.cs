using Nop.Plugin.Widgets.ProductExtension.Areas.Admin.Models;

namespace Nop.Plugin.Widgets.ProductExtension.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the Custom product model factory
    /// </summary>
    public interface ICustomProductModelFactory
    {
        /// <summary>
        /// Prepare product search model
        /// </summary>
        /// <param name="searchModel">Product search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product search model
        /// </returns>
        Task<ProductSearchModel> PrepareProductSearchModelAsync(ProductSearchModel searchModel);

        /// <summary>
        /// Prepare paged product list model
        /// </summary>
        /// <param name="searchModel">Product search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product list model
        /// </returns>
        Task<Nop.Web.Areas.Admin.Models.Catalog.ProductListModel> PrepareProductListModelAsync(ProductSearchModel searchModel);
    }
}