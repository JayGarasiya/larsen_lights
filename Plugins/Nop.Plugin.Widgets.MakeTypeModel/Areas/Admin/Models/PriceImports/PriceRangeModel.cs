using Nop.Web.Framework.Models;

namespace Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Models.PriceImports
{
    /// <summary>
    /// Represents a price range model
    /// </summary>
    public partial record PriceRangeModel : BaseNopEntityModel
    {
        #region Properties

        public string PricePercentage { get; set; }
        public string PriceAmount { get; set; }
        public string FromPrice { get; set; }
        public string ToPrice { get; set; }

        #endregion
    }
}
