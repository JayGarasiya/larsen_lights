using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Widgets.Fulfillment.Models
{
    /// <summary>
    /// Represents a 3PL Record model
    /// </summary>
    public partial record ThreePlRecordModel : BaseNopEntityModel
    {
        #region Properties

        [NopResourceDisplayName("Plugins.Widgets.Fulfillment.Fields.OrderId")]
        public int OrderId { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.Fulfillment.Fields.ThreePlStutus")]
        public string ThreePlStutus { get; set; }
        public int ThreePlStutusId { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.Fulfillment.Fields.PaidByCheck")]
        public bool PaidByCheck { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.Fulfillment.Fields.Note")]
        public string Note { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.Fulfillment.Fields.CancelledDate")]
        public string CancelledDate { get; set; }

        #endregion
    }
}
