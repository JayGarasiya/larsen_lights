using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Inventory.Order.Models
{
    /// <summary>
    /// Represents the product dimensions model
    /// </summary>
    public record ProductDimensionsModel : BaseNopEntityModel
    {
        public int ProductId { get; set; }

        [NopResourceDisplayName("Plugins.InventoryOrder.Fields.QtyCartoon")]
        public int QtyCartoon { get; set; }

        [NopResourceDisplayName("Plugins.InventoryOrder.Fields.DimensionsHeight")]
        public decimal DimensionsHeight { get; set; }

        [NopResourceDisplayName("Plugins.InventoryOrder.Fields.DimensionsLength")]
        public decimal DimensionsLength { get; set; }

        [NopResourceDisplayName("Plugins.InventoryOrder.Fields.DimensionsWidth")]
        public decimal DimensionsWidth { get; set; }
    }
}
