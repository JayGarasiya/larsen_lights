using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace Nop.Plugin.Misc.Inventory.Order.Models
{
    /// <summary>
    /// Represents manage inventory search model
    /// </summary>
    public record ManageInventorySearchModel : BaseSearchModel
    {
        #region Ctor
        public ManageInventorySearchModel()
        {
            AvailableCategories = new List<SelectListItem>();
            AvailableManufacturers = new List<SelectListItem>();
        }
        #endregion

        #region Properties
        [NopResourceDisplayName("Plugins.InventoryOrder.Fields.StartDate")]
        [UIHint("DateNullable")]
        public DateTime? StartDate { get; set; }

        [NopResourceDisplayName("Plugins.InventoryOrder.Fields.EndDate")]
        [UIHint("DateNullable")]
        public DateTime? EndDate { get; set; }

        [NopResourceDisplayName("Plugins.InventoryOrder.Fields.Category")]
        public int SearchCategoryId { get; set; }

        [NopResourceDisplayName("Plugins.InventoryOrder.Fields.Manufacture")]
        public int SearchManufacturerId { get; set; }

        [NopResourceDisplayName("Plugins.InventoryOrder.Fields.SearchIncreOrDecre")]
        public int SearchPercentage { get; set; }

        [NopResourceDisplayName("Plugins.InventoryOrder.Fields.TotalBoxVolume")]
        public decimal TotalBoxVolume { get; set; }

        public IList<SelectListItem> AvailableCategories { get; set; }

        public IList<SelectListItem> AvailableManufacturers { get; set; }

        public string SelectedIds { get; set; }

        public int PoOrderId { get; set; }

        public string PONumber { get; set; }
        #endregion
    }
}
