namespace Nop.Plugin.Reports.ChartsAndTables.Domain
{
    /// <summary>
    /// Represents a table report line
    /// </summary>
    [Serializable]
    public partial class TableReport
    {
        /// <summary>
        /// Gets or sets the order identifier
        /// </summary>
        public int OrderId { get; set; }

        /// <summary>
        /// Gets or sets the customer identifier
        /// </summary>
        public int CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the customer identifier
        /// </summary>
        public string CustomerEmail { get; set; }

        /// <summary>
        /// Gets or sets the product identifier
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Gets or sets the order item attributes
        /// </summary>
        public string Attributes { get; set; }

        /// <summary>
        /// Gets or sets the order item qty
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Gets or sets the unit price excluding tax
        /// </summary>
        public decimal UnitPriceExclTax { get; set; }

        /// <summary>
        /// Gets or sets the order item price excluding tax
        /// </summary>
        public decimal LineTotalExclTax { get; set; }

        /// <summary>
        /// Gets or sets the payment method
        /// </summary>
        public string PaymentMethod { get; set; }

        /// <summary>
        /// Gets or sets the shipping method
        /// </summary>
        public string ShippingMethod { get; set; }

        /// <summary>
        /// Gets or sets the billing country
        /// </summary>
        public string BillingCountry { get; set; }

        /// <summary>
        /// Gets or sets the billing city
        /// </summary>
        public string BillingCity { get; set; }

        /// <summary>
        /// Gets or sets the order year month (YYYY-MM)
        /// </summary>
        public string YearMonth { get; set; }
    }
}
