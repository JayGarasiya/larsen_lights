using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Widgets.Fulfillment.Models
{
    /// <summary>
    /// Represents a 3PL Shipping Method model
    /// </summary>
    public partial record ThreePlShippingMethodModel : BaseNopEntityModel
    {
        #region Properties

        [NopResourceDisplayName("Plugins.Widgets.Fulfillment.Fields.ShippingMethod")]
        public string ShippingMethod { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.Fulfillment.Fields.ThreePlCarrier")]
        public string ThreePlCarrier { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.Fulfillment.Fields.ThreePlService")]
        public string ThreePlService { get; set; }

        #endregion
    }
}
