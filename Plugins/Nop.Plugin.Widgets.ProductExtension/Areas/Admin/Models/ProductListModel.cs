using Nop.Web.Framework.Models;

namespace Nop.Plugin.Widgets.ProductExtension.Areas.Admin.Models
{
    /// <summary>
    /// Represents a product list model
    /// </summary>
    public partial record ProductListModel : BasePagedListModel<ProductModel>
    {
    }
}
