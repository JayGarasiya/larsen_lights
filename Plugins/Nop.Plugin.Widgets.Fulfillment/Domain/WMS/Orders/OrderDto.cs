using Newtonsoft.Json;

namespace Nop.Plugin.Widgets.Fulfillment.Domain.WMS.Orders
{
    /// <summary>
    /// Represents a 3PL central order data transfer object
    /// </summary>
    public class OrderDto
    {
        /// <summary>
        /// Gets or sets the CustomerIdentifier
        /// </summary>
        public CustomerDto CustomerIdentifier { get; set; }

        /// <summary>
        /// Gets or sets the FacilityIdentifier
        /// </summary>
        public FacilityDto FacilityIdentifier { get; set; }

        /// <summary>
        /// Gets or sets the ReferenceNum
        /// </summary>
        public string ReferenceNum { get; set; }

        /// <summary>
        /// Gets or sets the PoNum
        /// </summary>
        public string PoNum { get; set; }

        /// <summary>
        /// Gets or sets the BillingCode
        /// </summary>
        public string BillingCode { get; set; }

        /// <summary>
        /// Gets or sets the FulfillInvInfo
        /// </summary>
        public FulfillInvInfoDto FulfillInvInfo { get; set; }

        /// <summary>
        /// Gets or sets the RoutingInfo
        /// </summary>
        public RoutingDto RoutingInfo { get; set; }

        /// <summary>
        /// Gets or sets the ShipTo
        /// </summary>
        public AddressDto ShipTo { get; set; }

        /// <summary>
        /// Gets or sets the BillTo
        /// </summary>
        public AddressDto BillTo { get; set; }

        /// <summary>
        /// Gets or sets the Items
        /// </summary>
        [JsonProperty("_embedded")]
        public OrderItems Items { get; set; }

        #region Nested Classes

        /// <summary>
        /// Represents a 3PL central fulfill invoice information data transfer object
        /// </summary>
        public class FulfillInvInfoDto
        {
            /// <summary>
            /// Gets or sets the FulfillInvDiscountAmount
            /// </summary>
            public decimal FulfillInvDiscountAmount { get; set; }
        }

        /// <summary>
        /// Represents a 3PL central customer data transfer object
        /// </summary>
        public class CustomerDto
        {
            /// <summary>
            /// Gets or sets the Id
            /// </summary>
            public string Id { get; set; }
        }

        /// <summary>
        /// Represents a 3PL central facility data transfer object
        /// </summary>
        public class FacilityDto
        {
            /// <summary>
            /// Gets or sets the Id
            /// </summary>
            public string Id { get; set; }
        }

        /// <summary>
        /// Represents a 3PL central routing data transfer object
        /// </summary>
        public class RoutingDto
        {
            /// <summary>
            /// Gets or sets the Carrier
            /// </summary>
            public string Carrier { get; set; }

            /// <summary>
            /// Gets or sets the Mode
            /// </summary>
            public string Mode { get; set; }
        }

        /// <summary>
        /// Represents a 3PL central address data transfer object
        /// </summary>
        public class AddressDto
        {
            /// <summary>
            /// Gets or sets the CompanyName
            /// </summary>
            public string CompanyName { get; set; }

            /// <summary>
            /// Gets or sets the Name
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the Address1
            /// </summary>
            public string Address1 { get; set; }

            /// <summary>
            /// Gets or sets the Address2
            /// </summary>
            public string Address2 { get; set; }

            /// <summary>
            /// Gets or sets the City
            /// </summary>
            public string City { get; set; }

            /// <summary>
            /// Gets or sets the State
            /// </summary>
            public string State { get; set; }

            /// <summary>
            /// Gets or sets the Zip
            /// </summary>
            public string Zip { get; set; }

            /// <summary>
            /// Gets or sets the Country
            /// </summary>
            public string Country { get; set; }

            /// <summary>
            /// Gets or sets the PhoneNumber
            /// </summary>
            public string PhoneNumber { get; set; }

            /// <summary>
            /// Gets or sets the EmailAddress
            /// </summary>
            public string EmailAddress { get; set; }

            /// <summary>
            /// Gets or sets the Fax
            /// </summary>
            public string Fax { get; set; }

        }

        /// <summary>
        /// Represents a 3PL central order items
        /// </summary>
        public class OrderItems
        {
            /// <summary>
            /// Gets or sets the Items
            /// </summary>
            [JsonProperty("http://api.3plCentral.com/rels/orders/item")]
            public OrdersItemDto[] Items { get; set; }
        }

        #endregion
    }
}
