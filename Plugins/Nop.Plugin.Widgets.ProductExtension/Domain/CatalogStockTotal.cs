using System.ComponentModel;

namespace Nop.Plugin.Widgets.ProductExtension.Domain
{
    /// <summary>
    /// Represents totals for an catalog stock
    /// </summary>
    public class CatalogStockTotal
    {
        /// <summary>
        /// Gets or sets the order subtotal (include tax)
        /// </summary>
        [DisplayName("Plugins.Widgets.ProductExtension.CatalogStockPdf.TotalCost")]
        public string TotalProductCost { get; set; }

        /// <summary>
        /// Gets or sets the order subtotal (include tax)
        /// </summary>
        [DisplayName("Plugins.Widgets.ProductExtension.CatalogStockPdf.TotalPrice")]
        public string TotalPrice { get; set; }
    }
}
