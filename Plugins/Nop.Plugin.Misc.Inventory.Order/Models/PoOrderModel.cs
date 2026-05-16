using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace Nop.Plugin.Misc.Inventory.Order.Models
{
    /// <summary>
    /// Represents the po order model
    /// </summary>
    public partial record PoOrderModel : BaseNopEntityModel
    {
        #region Ctor
        public PoOrderModel()
        {
            Poorderitem = new List<PoOrderItemModel>();
        }
        #endregion 

        #region Properties
        public override int Id { get; set; }

        [NopResourceDisplayName("Plugins.InventoryOrder.Fields.Comment")]
        public string Comment { get; set; }

        [NopResourceDisplayName("Plugins.InventoryOrder.Fields.PONumber")]
        public string PONumber { get; set; }

        [NopResourceDisplayName("Plugins.InventoryOrder.Fields.PercentAdjusted")]
        public int PercentAdjusted { get; set; }

        [NopResourceDisplayName("Plugins.InventoryOrder.Fields.HasRecived")]
        public bool HasReceived { get; set; }

        [NopResourceDisplayName("Plugins.InventoryOrder.Fields.CreatedOnUTC")]
        public DateTime CreatedOnUTC { get; set; }

        [NopResourceDisplayName("Plugins.InventoryOrder.Fields.ReceivedOnUTC")]
        [UIHint("DateTimeNullable")]
        public DateTime? ReceivedOnUTC { get; set; }

        [NopResourceDisplayName("Plugins.InventoryOrder.Fields.AvailableDateOnUTC")]
        [UIHint("DateNullable")]
        public DateTime? AvailableDateOnUTC { get; set; }

        [NopResourceDisplayName("Plugins.InventoryOrder.Fields.TotalVolume")]
        public decimal TotalVolume { get; set; }

        public List<PoOrderItemModel> Poorderitem { get; set; }

        public ICollection<string> SelectedIds { get; set; }

        public ICollection<int> SelectedCartoon { get; set; }
        #endregion 
    }
}
