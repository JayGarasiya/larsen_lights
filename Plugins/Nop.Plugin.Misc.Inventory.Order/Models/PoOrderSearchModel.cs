using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace Nop.Plugin.Misc.Inventory.Order.Models
{
    /// <summary>
    /// Represents po order search model
    /// </summary>
    public partial record PoOrderSearchModel : BaseSearchModel
    {
        #region Ctor
        public PoOrderSearchModel()
        {
            PoOrderItemSearchModel = new PoOrderItemSearchModel();
            PoorderModel = new PoOrderModel();
        }
        #endregion

        #region Properties

        [NopResourceDisplayName("Plugins.InventoryOrder.Fields.SearchPoNumber")]
        public string SearchPoNumber { get; set; }

        [NopResourceDisplayName("Plugins.InventoryOrder.Fields.SearchAdminComment")]
        public string SearchAdminComment { get; set; }

        [NopResourceDisplayName("Plugins.InventoryOrder.Fields.StartDate")]
        [UIHint("DateNullable")]
        public DateTime? StartDate { get; set; }

        [NopResourceDisplayName("Plugins.InventoryOrder.Fields.EndDate")]
        [UIHint("DateNullable")]
        public DateTime? EndDate { get; set; }

        public PoOrderItemSearchModel PoOrderItemSearchModel { get; set; }

        public PoOrderModel PoorderModel { get; set; }
        #endregion 
    }
}
