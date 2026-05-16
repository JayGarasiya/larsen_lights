namespace Nop.Plugin.Widgets.Fulfillment.Domain.WMS.Orders
{
    /// <summary>
    /// Represents a order item to sync 3PL central order item data transfer object
    /// </summary>
    public class SyncOrderItem
    {
        /// <summary>
        /// Gets or sets the OrderItemId
        /// </summary>
        public int OrderItemId { get; set; }

        /// <summary>
        /// Gets or sets the Note
        /// </summary>
        public string Note { get; set; }

        /// <summary>
        /// Gets or sets the InStockItems
        /// </summary>
        public List<OrdersItemDto> InStockItems { get; set; }

        /// <summary>
        /// Gets or sets the OutOfStockItems
        /// </summary>
        public List<OrdersItemDto> OutOfStockItems { get; set; }
    }
}
