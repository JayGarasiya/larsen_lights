using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Inventory.Order.Models
{
    /// <summary>
    /// Represents the po order list model
    /// </summary>
    public partial record PoOrderListModel : BasePagedListModel<PoOrderModel>
    {
    }
}
