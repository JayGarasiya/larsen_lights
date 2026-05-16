using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Inventory.Order.Models
{
    /// <summary>
    /// Represents manage inventory model
    /// </summary>
    public record ManageInventoryModel : BaseNopEntityModel
    {
        #region Properties
        public string ManufactureProductIds { get; set; }

        [NopResourceDisplayName("Plugins.InventoryOrder.Fields.Manufacture")]
        public string Manufacture { get; set; }

        [NopResourceDisplayName("Plugins.InventoryOrder.Fields.SKU")]
        public string SKU { get; set; }

        [NopResourceDisplayName("Plugins.InventoryOrder.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Plugins.InventoryOrder.Fields.InStock")]
        public int InStock { get; set; }

        [NopResourceDisplayName("Plugins.InventoryOrder.Fields.TimeFrameQty")]
        public int TimeFrameQty { get; set; }

        [NopResourceDisplayName("Plugins.InventoryOrder.Fields.IncOrDesQty")]
        public int IncOrDesQty { get; set; }

        [NopResourceDisplayName("Plugins.InventoryOrder.Fields.TotalQty")]
        public int TotalQty { get; set; }

        public int ProductId { get; set; }

        [NopResourceDisplayName("Plugins.InventoryOrder.Fields.PictureThumbnailUrl")]
        public string PictureThumbnailUrl { get; set; }

        public int ManufacturerId { get; set; }

        public int OrderedQty1 { get; set; }

        public int OrderedQty2 { get; set; }

        public int OrderedQty3 { get; set; }

        public int OrderedQty4 { get; set; }

        public decimal BoxVolume { get; set; }

        public decimal TotalCartoon { get; set; }

        public int TotalProductQtyCartoon { get; set; }

        public decimal TotalBoxVolume { get; set; }

        public bool IsNegative { get; set; }

        public bool IsBoxVolume { get; set; }

        [NopResourceDisplayName("Plugins.InventoryOrder.Fields.ProductCost")]
        public decimal ProductCost { get; set; }

        public bool Published { get; set; }
        #endregion
    }
}
