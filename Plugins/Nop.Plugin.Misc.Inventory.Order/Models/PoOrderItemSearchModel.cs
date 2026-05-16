using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Inventory.Order.Models
{
    /// <summary>
    /// Represents the po order item search model
    /// </summary>
    public partial record PoOrderItemSearchModel : BaseSearchModel
    {
        public int PoOrderId { get; set; }
    }
}
