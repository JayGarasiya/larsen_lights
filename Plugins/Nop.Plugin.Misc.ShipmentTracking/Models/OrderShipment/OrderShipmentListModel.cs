using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.ShipmentTracking.Models.OrderShipment
{
    /// <summary>
    /// Represents an order shipment list model
    /// </summary>
    public partial record OrderShipmentListModel : BasePagedListModel<ShipmentModel>
    {
    }
}
