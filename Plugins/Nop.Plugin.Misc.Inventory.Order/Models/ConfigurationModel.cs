using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Inventory.Order.Models
{
    /// <summary>
    /// Represents configuration model
    /// </summary>
    public record ConfigurationModel : BaseNopModel
    {
        #region Ctor
        public ConfigurationModel()
        {
            AllowedManufacturers = new List<int>();
            AvailableManufacturers = new List<SelectListItem>();
        }
        #endregion

        #region Properties
        [NopResourceDisplayName("Plugins.InventoryOrder.Fields.AllowedManufacturers")]
        public IList<int> AllowedManufacturers { get; set; }

        public IList<SelectListItem> AvailableManufacturers { get; set; }
        #endregion
    }
}
