using Nop.Web.Framework.Models;

namespace Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Models.TypeProduct
{
    /// <summary>
    /// Represents a type product list model
    /// </summary>
    public record TypeProductListModel : BasePagedListModel<TypeProductModel>
    {
    }
}