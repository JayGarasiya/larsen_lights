using Nop.Web.Models.Catalog;

namespace Nop.Plugin.Widgets.PreviouslyPurchased.Models;

/// <summary>
/// Represents the model for displaying previously purchased products.
/// </summary>
public partial record PreviouslyPurchasedModel
{
    public PreviouslyPurchasedModel()
    {
        CatalogProductsModel = new CatalogProductsModel();
    }

    public string Title { get; set; }
    public string Url { get; set; }

    public CatalogProductsModel CatalogProductsModel { get; set; }
}
