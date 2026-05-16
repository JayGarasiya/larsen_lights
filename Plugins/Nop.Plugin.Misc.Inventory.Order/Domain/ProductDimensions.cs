using Nop.Core;

namespace Nop.Plugin.Misc.Inventory.Order.Domain
{
    /// <summary>
    /// Represents the product dimensions domain
    /// </summary>
    public partial class ProductDimensions : BaseEntity
    {
        /// <summary>
        /// Gets or sets product identifier
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Gets or sets qty cartoon
        /// </summary>
        public int QtyCartoon { get; set; }

        /// <summary>
        /// Gets or sets dimensions height
        /// </summary>
        public decimal DimensionsHeight { get; set; }

        /// <summary>
        /// Gets or sets dimensions length
        /// </summary>
        public decimal DimensionsLength { get; set; }

        /// <summary>
        /// Gets or sets dimensions width
        /// </summary>
        public decimal DimensionsWidth { get; set; }
    }
}
