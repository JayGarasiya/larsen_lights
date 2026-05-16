using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Models.Catalog;

/// <summary>
/// Represents an override product model
/// </summary>
public partial record OverrideProductModel : ProductModel
{
    #region Properties

    [NopResourceDisplayName("Admin.Catalog.Products.Fields.ProductTags")]
    public string ProductTags { get; set; }

    public string InitialProductTags { get; set; }

    #endregion
}

