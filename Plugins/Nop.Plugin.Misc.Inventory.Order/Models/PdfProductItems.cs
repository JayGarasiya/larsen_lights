using System.ComponentModel;

namespace Nop.Plugin.Misc.Inventory.Order.Models
{
    /// <summary>
    /// Represents pdf product item
    /// </summary>
    public partial class PdfProductItems 
    {
        [DisplayName("poorderitem.manufacturer")]
        public string ManufacturerName { get; set; }
        
        [DisplayName("poorderitem.productname")]
        public string ProductName { get; set; }
        
        [DisplayName("poorderitem.sku")]
        public string SKU { get; set; }
        
        [DisplayName("poorderitem.productcost")]
        public string ProductCost { get; set; }
        
        [DisplayName("poorderitem.qty")]
        public string QTY { get; set; }
    }
}
