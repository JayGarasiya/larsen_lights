using Nop.Core;

namespace Nop.Plugin.Widgets.ProductExtension.Domain
{
    /// <summary>
    /// Represents a product notes record
    /// </summary>
    public partial class ProductNote : BaseEntity
    {
        /// <summary>
        /// Gets or sets product note name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets product detail widget zone to show note
        /// </summary>
        public string WidgetZone { get; set; }

        /// <summary>
        /// Gets or sets product note description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets display order
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Gets or sets published
        /// </summary>
        public bool Published { get; set; }

        /// <summary>
        /// Gets or sets allow to show display at front side
        /// </summary>
        public bool ShowDisplayName { get; set; }
    }
}
