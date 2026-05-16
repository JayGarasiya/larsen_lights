namespace Nop.Plugin.Widgets.Fulfillment.Domain.WMS.Orders
{
    /// <summary>
    /// Represents a 3PL central order item data transfer object
    /// </summary>
    public class OrdersItemDto
    {
        /// <summary>
        /// Gets or sets the ItemIdentifier
        /// </summary>
        public ItemDto ItemIdentifier { get; set; }

        /// <summary>
        /// Gets or sets the ExternalId
        /// </summary>
        public string ExternalId { get; set; }

        /// <summary>
        /// Gets or sets the Qty
        /// </summary>
        public decimal Qty { get; set; }

        /// <summary>
        /// Gets or sets the FulfillInvSalePrice
        /// </summary>
        public decimal FulfillInvSalePrice { get; set; }

        #region Nested Classes

        /// <summary>
        /// Represents item SKU information.
        /// </summary>
        public class ItemDto
        {
            /// <summary>
            /// Gets or sets the Sku
            /// </summary>
            public string Sku { get; set; }
        }

        #endregion
    }
}
