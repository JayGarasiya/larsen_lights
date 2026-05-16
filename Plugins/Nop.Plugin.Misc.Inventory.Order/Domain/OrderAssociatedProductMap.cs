using Nop.Core;

namespace Nop.Plugin.Misc.Inventory.Order.Domain
{
    /// <summary>
    /// Represents the Order Associated Product Map domain
    /// </summary>
    public partial class OrderAssociatedProductMap : BaseEntity
    {
        /// <summary>
        /// Gets or sets the order identifier
        /// </summary>
        public int OrderId { get; set; }

        /// <summary>
        /// Gets or sets the product identifier
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Gets or sets the quantity
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Gets or sets the order item identifier
        /// </summary>
        public int OrderItemId { get; set; }

        /// <summary>
        /// Gets or sets the created on utc
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }
    }
}
