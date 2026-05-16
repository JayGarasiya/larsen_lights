using Nop.Core;

namespace Nop.Plugin.Widgets.ProductExtension.Domain
{
    /// <summary>
    /// Represents a product attribute value condition record
    /// </summary>
    public partial class ProductAttributeValueCondition : BaseEntity
    {
        /// <summary>
        /// Gets or sets the first product attribute value identifier
        /// </summary>
        public int ProductAttributeValueId1 { get; set; }

        /// <summary>
        /// Gets or sets the second product attribute value identifier
        /// </summary>
        public int ProductAttributeValueId2 { get; set; }
    }
}
