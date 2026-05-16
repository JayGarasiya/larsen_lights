using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.ShipmentTracking.Models.OrderShipment
{
    /// <summary>
    /// Represents an order model
    /// </summary>
    public record OrderModel: Web.Areas.Admin.Models.Orders.OrderModel
    {
        [NopResourceDisplayName("Plugins.Misc.ShipmentTracking.Fields.AdminNote")]
        public string AdminNote { get; set; }
    }
}
