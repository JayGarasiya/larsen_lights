using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Models.ProductImport
{
    /// <summary>
    /// Represents product import search model 
    /// </summary>
    public partial record ProductImportSearchModel : BaseSearchModel
    {
        #region Ctor

        public ProductImportSearchModel()
        {
            AddProductImport = new ProductImportModel();
            AvailableImportTypes = new List<SelectListItem>();
            AvailableImportStatuses = new List<SelectListItem>();
        }

        #endregion

        #region Properties

        public ProductImportModel AddProductImport { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.MakeTypeModel.ProductImport.Fields.SearchImportType")]
        public int SearchImportType { get; set; }
        public IList<SelectListItem> AvailableImportTypes { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.MakeTypeModel.ProductImport.Fields.SearchImportStatus")]
        public int SearchImportStatus { get; set; }
        public IList<SelectListItem> AvailableImportStatuses { get; set; }

        #endregion
    }
}
