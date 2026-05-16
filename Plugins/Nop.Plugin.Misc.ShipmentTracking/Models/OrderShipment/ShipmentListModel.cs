using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.ShipmentTracking.Models.OrderShipment
{
    /// <summary>
    /// Represents a shipment list model
    /// </summary>
    public partial record ShipmentListModel : BasePagedListModel<ShipmentModel>
    {
    }
}
