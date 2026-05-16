using Newtonsoft.Json;

namespace Nop.Plugin.Widgets.Fulfillment.Domain.WMS.Orders
{
    /// <summary>
    /// Represents the result returned from WMS for order items.
    /// </summary>
    public class OrderItemsResultDto
    {
        /// <summary>
        /// Embedded object containing ordered items.
        /// </summary>
        [JsonProperty("_embedded")]
        public OrderedItems Items { get; set; }

        #region Nested Classes

        /// <summary>
        /// Container class for order item resources.
        /// </summary>
        public class OrderedItems
        {
            /// <summary>
            /// List of order items returned by WMS.
            /// </summary>
            [JsonProperty("http://api.3plCentral.com/rels/orders/item")]
            public OrdersItemDto[] Items { get; set; }

            #region Nested Classes

            /// <summary>
            /// Represents a single order item.
            /// </summary>
            public class OrdersItemDto
            {
                /// <summary>
                /// Identifier information for the item.
                /// </summary>
                public ItemDto ItemIdentifier { get; set; }

                /// <summary>
                /// Primary quantity of the item.
                /// </summary>
                public decimal Qty { get; set; }

                #region Nested Classes
                /// <summary>
                /// Represents item SKU information.
                /// </summary>
                public class ItemDto
                {
                    /// <summary>
                    /// Stock keeping unit (SKU).
                    /// </summary>
                    public string Sku { get; set; }
                }

                #endregion
            }

            #endregion
        }

        #endregion
    }
}
