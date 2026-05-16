using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.ShipmentTracking.Models.OrderShipment
{
    /// <summary>
    /// Represents an order item search model
    /// </summary>
    public partial record OrderItemSearchModel : BaseSearchModel
    {
        #region Properties

        public int OrderId { get; set; }

        #endregion
    }
}