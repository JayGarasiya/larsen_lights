using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Reports.ChartsAndTables.Models
{
    /// <summary>
    /// Represents an aggrerator model
    /// </summary>
    public partial record AggreratorModel : BaseNopModel
    {
        #region Properties

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Aggrerator.TotalSalesValue")]
        public string TotalSalesValue { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Aggrerator.AverageSalesValue")]
        public string AverageSalesValue { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Aggrerator.MinSalesValue")]
        public string MinSalesValue { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Aggrerator.MaxSalesValue")]
        public string MaxSalesValue { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Aggrerator.TotalQuantity")]
        public int TotalQuantity { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Aggrerator.AverageQuantity")]
        public decimal AverageQuantity { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Aggrerator.MinQuantity")]
        public int MinQuantity { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Aggrerator.MaxQuantity")]
        public int MaxQuantity { get; set; }

        #endregion
    }
}
