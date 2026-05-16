using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Reports.ChartsAndTables.Models
{
    /// <summary>
    /// Represents a charts and tables data model
    /// </summary>
    public partial record ChartsAndTablesDataModel : BaseNopModel
    {
        #region Properties

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.ChartsAndTablesData.Fields.Title")]
        public string Title { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.ChartsAndTablesData.Fields.Quantity")]
        public int Quantity { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.ChartsAndTablesData.Fields.TotalSales")]
        public string TotalSales { get; set; }

        #endregion
    }
}
