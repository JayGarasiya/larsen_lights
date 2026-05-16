using Nop.Web.Framework.Models;

namespace Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Models.PriceImports
{
    /// <summary>
    /// Represents a price range search model
    /// </summary>
    public partial record PriceRangeSearchModel : BaseSearchModel
    {
        public int PriceImportId { get; set; }
    }
}
