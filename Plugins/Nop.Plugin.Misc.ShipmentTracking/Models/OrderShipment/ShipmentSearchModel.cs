using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.ShipmentTracking.Models.OrderShipment
{
    /// <summary>
    /// Represents an extended shipment search model
    /// </summary>
    public record ShipmentSearchModel : Web.Areas.Admin.Models.Orders.ShipmentSearchModel
    {
        #region Ctor

        public ShipmentSearchModel()
        {
            AvailableShippingMethods = new List<SelectListItem>();
        }

        #endregion

        #region Properties

        [NopResourceDisplayName("Plugins.Misc.ShipmentTracking.Fields.ShippingMethod")]
        public string ShippingMethod { get; set; }

        public IList<SelectListItem> AvailableShippingMethods { get; set; }

        #endregion
    }
}
