using Nop.Core;

namespace Nop.Plugin.Widgets.MakeTypeModel.Domain.MakeTypeModelProduct
{
    /// <summary>
    /// Represents a product make
    /// </summary>
    public partial class MakeProduct : BaseEntity
    {
        /// <summary>
        /// Gets or sets the product make name
        /// </summary>
        public string Name { get; set; }

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
