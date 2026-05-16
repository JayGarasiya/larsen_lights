using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Inventory.Order.Models
{
    /// <summary>
    /// Represents po order item list model
    /// </summary>
    public partial record PoOrderItemListModel : BasePagedListModel<PoOrderItemModel>
    {
    }
}
