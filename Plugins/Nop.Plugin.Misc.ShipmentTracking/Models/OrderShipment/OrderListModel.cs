using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.ShipmentTracking.Models.OrderShipment
{
    /// <summary>
    /// Represents an order list model
    /// </summary>
    public record OrderListModel: BasePagedListModel<OrderModel>
    {
    }
}
