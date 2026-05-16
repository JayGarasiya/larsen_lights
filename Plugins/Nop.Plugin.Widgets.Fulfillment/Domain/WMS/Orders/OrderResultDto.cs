using Newtonsoft.Json;

namespace Nop.Plugin.Widgets.Fulfillment.Domain.WMS.Orders
{
    /// <summary>
    /// Represents the complete result returned from WMS for an order.
    /// </summary>
    public class OrderResultDto
    {
        /// <summary>
        /// Read-only properties populated by WMS.
        /// </summary>
        public ReadOnlyDto ReadOnly { get; set; }

        /// <summary>
        /// Routing information for the order.
        /// </summary>
        public RoutingDto RoutingInfo { get; set; }


        /// <summary>
        /// Embedded object containing ordered items.
        /// </summary>
        [JsonProperty("_embedded")]
        public OrderedItems Items { get; set; }

        /// <summary>
        /// Error message returned from WMS, if the request fails.
        /// </summary>
        public string ErrorMessage { get; set; }

        #region Nested Classes

        /// <summary>
        /// Read-only order data provided by WMS.
        /// </summary>
        public class ReadOnlyDto
        {
            /// <summary>
            /// Unique identifier for the order.
            /// </summary>
            public int OrderId { get; set; }
        }

        /// <summary>
        /// Represents routing information for the order.
        /// </summary>
        public class RoutingDto
        {
            /// <summary>
            /// Carrier name.
            /// </summary>
            public string Carrier { get; set; }

            /// <summary>
            /// Tracking number for the shipment.
            /// </summary>
            public string TrackingNumber { get; set; }
        }



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