using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Widgets.PreviouslyPurchased.Models;

/// <summary>
/// Represents configuration model for previously purchased products.
/// </summary>
public record ConfigurationModel : BaseNopModel
{
    #region Properties
    public int ActiveStoreScopeConfiguration { get; set; }

    [NopResourceDisplayName("Plugins.Widgets.PreviouslyPurchased.Fields.Enable")]
    public bool Enabled { get; set; }
    public bool Enabled_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.Widgets.PreviouslyPurchased.Fields.ProductsType")]
    public int ProductsType { get; set; }
    public bool ProductsType_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.Widgets.PreviouslyPurchased.Fields.PreviouslyPurchasedProductsNumber")]
    public int PreviouslyPurchasedProductsNumber { get; set; }
    public bool PreviouslyPurchasedProductsNumber_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.Widgets.PreviouslyPurchased.Fields.MyAccountNavigation")]
    public bool MyAccountNavigation { get; set; }
    public bool MyAccountNavigation_OverrideForStore { get; set; }
    #endregion
}