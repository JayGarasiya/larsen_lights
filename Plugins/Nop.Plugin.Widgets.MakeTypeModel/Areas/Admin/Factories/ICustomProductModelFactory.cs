using Nop.Core.Domain.Catalog;
using Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Models.Catalog;

namespace Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Factories
{
    /// <summary>
    /// Represent custom product model factory
    /// </summary>
    public interface ICustomProductModelFactory
    {

        /// <summary>
        /// Prepare product model
        /// </summary>
        /// <param name="model">Product model</param>
        /// <param name="product">Product</param>
        /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product model
        /// </returns>
        Task<OverrideProductModel> PrepareCustomProductModelAsync(OverrideProductModel model, Product product, bool excludeProperties = false);
    }
}
