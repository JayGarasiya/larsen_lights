using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Models.AddModelProductToProduct
{
    /// <summary>
    /// Represents a product list model to add to the model product
    /// </summary>
    public partial record AddProductToModelProductListModel : BasePagedListModel<ProductModel>
    {
    }
}