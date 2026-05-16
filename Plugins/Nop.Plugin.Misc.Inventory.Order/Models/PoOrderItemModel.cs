using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Inventory.Order.Models
{
    /// <summary>
    /// Represents po order item model
    /// </summary>
    public partial record PoOrderItemModel : BaseNopEntityModel
    {
        #region Properties

        [NopResourceDisplayName("Plugins.InventoryOrder.Fields.ManufacturerName")]
        public string ManufacturerName { get; set; }

        [NopResourceDisplayName("Plugins.InventoryOrder.Fields.PictureThumbnailUrl")]
        public string PictureThumbnailUrl { get; set; }

        [NopResourceDisplayName("Plugins.InventoryOrder.Fields.ProductName")]
        public string ProductName { get; set; }

        [NopResourceDisplayName("Plugins.InventoryOrder.Fields.SKU")]
        public string Sku { get; set; }

        [NopResourceDisplayName("Plugins.InventoryOrder.Fields.orderedqty")]
        public int OrderedQty { get; set; }

        [NopResourceDisplayName("Plugins.InventoryOrder.Fields.ProductCost")]
        public decimal ProductCost { get; set; }

        public bool Published { get; set; }
        #endregion 
    }
}
