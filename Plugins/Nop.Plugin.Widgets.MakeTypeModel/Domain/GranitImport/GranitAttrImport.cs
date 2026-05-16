using Nop.Core;

namespace Nop.Plugin.Widgets.MakeTypeModel.Domain.GranitImport
{
    /// <summary>
    /// Represents a granit attr import
    /// </summary>
    public class GranitAttrImport : BaseEntity
    {
        /// <summary>
        /// Gets or sets product sku
        /// </summary>
        public string ProductSKU { get; set; }

        /// <summary>
        /// Gets or sets specification name 
        /// </summary>
        public string SpecificationName { get; set; }

        /// <summary>
        /// Gets or sets specification value 
        /// </summary>
        public string SpecificationValue { get; set; }
    }
}
