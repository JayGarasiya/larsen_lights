using Nop.Core;

namespace Nop.Plugin.Widgets.MakeTypeModel.Domain.GranitImport
{
    /// <summary>
    /// Represents a granit data import
    /// </summary>
    public class GranitDataImport : BaseEntity
    {
        /// <summary>
        /// Gets or sets the sub category level 1 name.
        /// </summary>
        public string Sub1CategoryName { get; set; }

        /// <summary>
        /// Gets or sets the sub category level 1 image.
        /// </summary>
        public string Sub1CategoryImage { get; set; }

        /// <summary>
        /// Gets or sets the sub category level 2 name.
        /// </summary>
        public string Sub2CategoryName { get; set; }

        /// <summary>
        /// Gets or sets the sub category level 2 image.
        /// </summary>
        public string Sub2CategoryImage { get; set; }

        /// <summary>
        /// Gets or sets the sub category level 3 name.
        /// </summary>
        public string Sub3CategoryName { get; set; }

        /// <summary>
        /// Gets or sets the sub category level 3 picture.
        /// </summary>
        public string Sub3CategoryPicture { get; set; }

        /// <summary>
        /// Gets or sets the sub category level 4 name.
        /// </summary>
        public string Sub4CategoryName { get; set; }

        /// <summary>
        /// Gets or sets the sub category level 4 picture.
        /// </summary>
        public string Sub4CategoryPicture { get; set; }

        /// <summary>
        /// Gets or sets the sub category level 5 name.
        /// </summary>
        public string Sub5CategoryName { get; set; }

        /// <summary>
        /// Gets or sets the sub category level 5 picture.
        /// </summary>
        public string Sub5CategoryPicture { get; set; }

        /// <summary>
        /// Gets or sets the category display order.
        /// </summary>
        public string CategoryDisplayOrder { get; set; }

        /// <summary>
        /// Gets or sets the product SKU.
        /// </summary>
        public string ProductSKU { get; set; }

        /// <summary>
        /// Gets or sets the product name.
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// Gets or sets the full product description.
        /// </summary>
        public string FullDescription { get; set; }

        /// <summary>
        /// Gets or sets the product tags.
        /// </summary>
        public string ProductTags { get; set; }

        /// <summary>
        /// Gets or sets the pack of quantity.
        /// </summary>
        public string PackOfQuantity { get; set; }

        /// <summary>
        /// Gets or sets the main product image.
        /// </summary>
        public string ProductImage { get; set; }

        /// <summary>
        /// Gets or sets the manufacturer name.
        /// </summary>
        public string Manufacturer { get; set; }

        /// <summary>
        /// Gets or sets the additional product images.
        /// </summary>
        public string ProductImages { get; set; }

        /// <summary>
        /// Gets or sets the related products.
        /// </summary>
        public string RelatedProducts { get; set; }

        /// <summary>
        /// Gets or sets the product weight.
        /// </summary>
        public string Weight { get; set; }
    }
}
