using Nop.Web.Framework.Models;

namespace Nop.Web.Areas.Admin.Models.Orders
{
    /// <summary>
    /// Represents an order item list model
    /// </summary>
    public partial record OrderItemListModel : BasePagedListModel<OrderItemModel>
    {
    }
}