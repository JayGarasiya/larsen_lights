using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Models.GranitProduct
{
    /// <summary>
    /// Represents a granit product import search model 
    /// </summary>
    public partial record GranitProductImportSearchModel : BaseSearchModel
    {
        #region Ctor
        public GranitProductImportSearchModel()
        {
            AvailableImportStatuses = new List<SelectListItem>();
        }
        #endregion

        #region Properties
        
        [NopResourceDisplayName("Plugins.Widgets.MakeTypeModel.GranitProductImport.Fields.SearchImportStatus")]
        public int SearchImportStatus { get; set; }
        public IList<SelectListItem> AvailableImportStatuses { get; set; }
        #endregion
    }
}
