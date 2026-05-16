using Nop.Core.Configuration;

namespace Nop.Plugin.Misc.InvoicePDF
{
    /// <summary>
    /// Represents the Invoice PDF Setting
    /// </summary>
    public class InvoicePDFSettings : ISettings
    {
        public InvoicePDFSettings()
        {
            ExcludedCustomerIds = new List<int>();
        }

        /// <summary>
        /// Gets or sets a AttachOrder Id
        /// </summary>
        public bool AttachOrderId { get; set; }
        /// <summary>
        /// Gets or sets a Use Order Mask
        /// </summary>
        public bool UseOrderMask { get; set; }
        /// <summary>
        /// Gets or sets a Vendor Cost
        /// </summary>
        public bool VendorCost { get; set; }
        /// <summary>
        /// Gets or sets a Render Attribute Value Prices
        /// </summary>
        public bool RenderAttributeValuePrices { get; set; }
        /// <summary>
        /// Gets or sets a Return Address
        /// </summary>
        public string ReturnAddress { get; set; }
        /// <summary>
        /// Gets or sets a Excluded Customer Ids
        /// </summary>
        public List<int> ExcludedCustomerIds { get; set; }
    }
}