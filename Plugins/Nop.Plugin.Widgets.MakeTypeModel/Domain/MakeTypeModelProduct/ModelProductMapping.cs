using Nop.Core;

namespace Nop.Plugin.Widgets.MakeTypeModel.Domain.MakeTypeModelProduct
{
    /// <summary>
    /// Represents a product mapping with model
    /// </summary>
    public partial class ModelProductMapping : BaseEntity
    {
        /// <summary>
        /// Gets or sets the product identifier
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Gets or sets the product model identifier
        /// </summary>
        public int ModelId { get; set; }
    }
}
