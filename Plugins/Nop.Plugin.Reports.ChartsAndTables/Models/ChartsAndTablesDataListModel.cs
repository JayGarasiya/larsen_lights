using Nop.Web.Framework.Models;

namespace Nop.Plugin.Reports.ChartsAndTables.Models
{
    /// <summary>
    /// Represents a chart and table data list model
    /// </summary>
    public partial record ChartsAndTablesDataListModel : BasePagedListModel<ChartsAndTablesDataModel>
    {
    }
}
