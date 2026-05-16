using Nop.Core;

namespace Nop.Plugin.Misc.Inventory.Order.Domain
{
    /// <summary>
    /// Represents the Po order item domain
    /// </summary>
    public partial class PoOrderItem : BaseEntity
    {
        /// <summary>
        /// Gets or sets the PO order identifier
        /// </summary>
        public int PoOrderId { get; set; }

        /// <summary>
        /// Gets or sets the manufacturer identifier
        /// </summary>
        public int ManufacturerId { get; set; }

        /// <summary>
        /// Gets or sets product identifier
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Gets or sets ordered qty
        /// </summary>
        public int OrderedQty { get; set; }
    }
}
