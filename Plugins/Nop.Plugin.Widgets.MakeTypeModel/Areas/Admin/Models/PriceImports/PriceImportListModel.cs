using Nop.Web.Framework.Models;

namespace Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Models.PriceImports
{
    /// <summary>
    /// Represents a price impoprt list model
    /// </summary>
    public partial record PriceImportListModel : BasePagedListModel<PriceImportModel>
    {
    }
}
