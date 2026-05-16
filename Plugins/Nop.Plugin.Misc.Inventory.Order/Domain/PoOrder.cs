using Nop.Core;

namespace Nop.Plugin.Misc.Inventory.Order.Domain
{
    /// <summary>
    /// Represents the Po Order domain
    /// </summary>
    public partial class PoOrder : BaseEntity
    {
        /// <summary>
        /// Gets or sets the PO Number
        /// </summary>
        public string PONumber { get; set; }

        /// <summary>
        /// Gets or sets the comment
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// Gets or sets the percent adjusted
        /// </summary>
        public int PercentAdjusted { get; set; }

        /// <summary>
        /// Gets or sets hasreceived
        /// </summary>
        public bool HasReceived { get; set; }

        /// <summary>
        /// Gets or sets total volume
        /// </summary>
        public decimal TotalVolume { get; set; }

        /// <summary>
        /// Gets or sets created on utc
        /// </summary>
        public DateTime CreatedOnUTC { get; set; }

        /// <summary>
        /// Gets or sets recevied on utc
        /// </summary>
        public DateTime? ReceivedOnUTC { get; set; }

        /// <summary>
        /// Gets or sets available date on utc
        /// </summary>
        public DateTime? AvailableDateOnUTC { get; set; }
    }
}
