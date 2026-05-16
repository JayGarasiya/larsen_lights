using Nop.Plugin.Widgets.ProductExtension.Domain;
using Nop.Plugin.Widgets.ProductExtension.Models;

namespace Nop.Plugin.Widgets.ProductExtension.Factories
{
    /// <summary>
    /// Represents the product extension factory implementation interface
    /// </summary>
    public partial interface IProductExtensionFactory
    {
        /// <summary>
        /// Prepare product note model
        /// </summary>
        /// <param name="model">Product Note Model</param>
        /// <param name="productNote">Product Note</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the store model
        /// </returns>
        Task<ProductNoteModel> PrepareProductNoteModelAsync(ProductNoteModel model, ProductNote productNote);

        /// <summary>
        /// Prepare product notes search model async
        /// </summary>
        /// <param name="searchModel">searchModel</param>
        /// <returns></returns>
        ProductNoteSearchModel PrepareProductNotesSearchModelAsync(ProductNoteSearchModel searchModel);

        /// <summary>
        /// Prepare product notes list model async
        /// </summary>
        /// <param name="searchModel"></param>
        /// <returns></returns>
        Task<ProductNoteListModel> PrepareProductNotesListModelAsync(ProductNoteSearchModel searchModel);
    }
}
