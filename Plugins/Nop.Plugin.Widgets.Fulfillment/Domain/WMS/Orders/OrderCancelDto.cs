namespace Nop.Plugin.Widgets.Fulfillment.Domain.WMS.Orders
{
    /// <summary>
    /// Represents order cancel dto
    /// </summary>
    public class OrderCancelDto
    {
        /// <summary>
        /// Gets or sets the Reason
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// Gets or sets the Charge
        /// </summary>
        public decimal Charge { get; set; }

        /// <summary>
        /// Gets or sets the InvoiceCreationInfo
        /// </summary>
        public InvoiceCreationDto InvoiceCreationInfo { get; set; }

        #region Nested Classes

        /// <summary>
        /// Represents invoice creation dto
        /// </summary>
        public class InvoiceCreationDto
        {
            /// <summary>
            /// Gets or sets the SetInvoiceDate
            /// </summary>
            public bool SetInvoiceDate { get; set; }

            /// <summary>
            /// Gets or sets the UtcOffset
            /// </summary>
            public int UtcOffset { get; set; }
        }

        #endregion
    }
}
