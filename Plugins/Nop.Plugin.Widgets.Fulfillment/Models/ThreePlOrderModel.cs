using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Widgets.Fulfillment.Models
{
    /// <summary>
    /// Represents a 3PL Order model
    /// </summary>
    public partial record ThreePlOrderModel : BaseNopEntityModel
    {
        #region Properties

        public int ThreePlId { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.Fulfillment.Fields.ThreePlOrderId")]
        public int ThreePlOrderId { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.Fulfillment.Fields.ThreePlStutus")]
        public string ThreePlStutus { get; set; }
        public int ThreePlStutusId { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.Fulfillment.Fields.CreationDate")]
        public string CreationDate { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.Fulfillment.Fields.CancelledDate")]
        public string CancelledDate { get; set; }

        #endregion
    }
}
