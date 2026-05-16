namespace Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Models.PriceImports
{
    /// <summary>
    /// Represents a price range json
    /// </summary>
    public class PriceRangeJson
    {
        #region Properties

        public bool UsePercentage { get; set; }
        public decimal PricePercentage { get; set; }
        public decimal PriceAmount { get; set; }
        public decimal FromPrice { get; set; }
        public decimal ToPrice { get; set; }

        #endregion
    }
}
