using System.ComponentModel;
using Nop.Services.Common.Pdf;

namespace Nop.Plugin.Widgets.ProductExtension.Domain
{
    /// <summary>
    /// Represents a PDF catalog entry
    /// </summary>
    public partial class CatalogProductItem : ProductItem
    {
        /// <summary>
        /// Gets or sets the stock quantity
        /// </summary>
        [DisplayName("Plugins.Widgets.ProductExtension.CatalogStockPdf.Stock")]
        public string Stock { get; set; }

        /// <summary>
        /// Gets or sets the stock quantity
        /// </summary>
        [DisplayName("Plugins.Widgets.ProductExtension.CatalogStockPdf.ProductCost")]
        public string ProductCost { get; set; }

        /// <summary>
        /// Gets or sets the stock quantity
        /// </summary>
        [DisplayName("Plugins.Widgets.ProductExtension.CatalogStockPdf.TotalProductCost")]
        public string TotalProductCost { get; set; }

        /// <summary>
        /// Gets or sets the stock quantity
        /// </summary>
        [DisplayName("Plugins.Widgets.ProductExtension.CatalogStockPdf.PicturePath")]
        public byte[] PicturePath { get; set; }
    }
}
