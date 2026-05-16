using Nop.Plugin.Widgets.PreviouslyPurchased.Models;
using Nop.Web.Models.Catalog;

namespace Nop.Plugin.Widgets.PreviouslyPurchased.Factories;

/// <summary>
/// Factory to create models for previously purchased products.
/// </summary>
public interface IPreviouslyPurchasedProductsModelFactory
{
    /// <summary>
    /// Prepares the model for the previously purchased products.
    /// This includes setting the title, URL, and retrieving catalog products.
    /// </summary>
    /// <param name="command">Model containing the catalog command parameters.</param>
    /// <returns>
    /// A task representing the asynchronous operation. 
    /// The task result contains the previously purchased products model.
    /// </returns>
    Task<PreviouslyPurchasedModel> PreparePreviouslyPurchasedProductsModelAsync(CatalogProductsCommand command);

    /// <summary>
    /// Prepares the catalog products model based on the previously purchased products.
    /// </summary>
    /// <param name="command">Model containing the catalog command parameters.</param>
    /// <returns>
    /// A task representing the asynchronous operation. 
    /// The task result contains the catalog products model.
    /// </returns>
    Task<CatalogProductsModel> PrepareProductsModelAsync(CatalogProductsCommand command);
}
