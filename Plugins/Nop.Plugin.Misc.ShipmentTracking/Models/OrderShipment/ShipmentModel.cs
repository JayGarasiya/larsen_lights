using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.ShipmentTracking.Models.OrderShipment
{
    /// <summary>
    /// Represents an extended shipment model
    /// </summary>
    public record ShipmentModel : Web.Areas.Admin.Models.Orders.ShipmentModel
    {
        #region Ctor

        public ShipmentModel()
        {
            AvailableShippingMethods = new List<SelectListItem>();
        }

        #endregion

        #region Properties

        [NopResourceDisplayName("Plugins.Misc.ShipmentTracking.Fields.ShippingMethod")]
        public string ShippingMethod { get; set; }
        public IList<SelectListItem> AvailableShippingMethods { get; set; }

        public bool BulkTrackOrder { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ShipmentTracking.Fields.SendShippedEmail")]
        public bool SendShippedEmail { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ShipmentTracking.Fields.SendDeliveredEmail")]
        public bool SendDeliveredEmail { get; set; }

        #endregion
    }
}
