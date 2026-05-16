using Nop.Core;

namespace Nop.Plugin.Widgets.MakeTypeModel.Domain.MakeTypeModelProduct
{
    /// <summary>
    /// Represents a product model category
    /// </summary>
    public partial class ModelCategory : BaseEntity
    {
        /// <summary>
        /// Gets or sets the category identifier
        /// </summary>
        public int CategoryId { get; set; }

        /// <summary>
        /// Gets or sets the published
        /// </summary>
        public bool Published { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
        public int DisplayOrder { get; set; }
    }
}
