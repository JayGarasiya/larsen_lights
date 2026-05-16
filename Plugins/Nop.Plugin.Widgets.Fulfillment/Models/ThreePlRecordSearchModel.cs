using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Widgets.Fulfillment.Models
{
    /// <summary>
    /// Represents a 3PL record search model
    /// </summary>
    public partial record ThreePlRecordSearchModel : BaseSearchModel
    {
        #region Ctor

        public ThreePlRecordSearchModel()
        {
            AvailableStatus = new List<SelectListItem>();
            ThreePlOrderSearchModel = new ThreePlOrderSearchModel();
        }

        #endregion

        #region Properties

        [NopResourceDisplayName("Plugins.Widgets.Fulfillment.Fields.SearchOrderId")]
        public int SearchOrderId { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.Fulfillment.Fields.SearchThreePlOrderId")]
        public int SearchThreePlOrderId { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.Fulfillment.Fields.SearchStatusId")]
        public int SearchStatusId { get; set; }
        public IList<SelectListItem> AvailableStatus { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.Fulfillment.Fields.SearchPaidByCheck")]
        public bool SearchPaidByCheck { get; set; }

        public ThreePlOrderSearchModel ThreePlOrderSearchModel { get; set; }

        #endregion
    }
}
