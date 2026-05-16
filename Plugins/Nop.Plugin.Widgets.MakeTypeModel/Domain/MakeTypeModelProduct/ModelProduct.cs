using Nop.Core;

namespace Nop.Plugin.Widgets.MakeTypeModel.Domain.MakeTypeModelProduct
{
    /// <summary>
    /// Represents a product model
    /// </summary>
    public partial class ModelProduct : BaseEntity
    {
        /// <summary>
        /// Gets or sets the product model name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the product model description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the product make name
        /// </summary>
        public string MakeName { get; set; }

        /// <summary>
        /// Gets or sets the product type name
        /// </summary>
        public string TypeName { get; set; }

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
