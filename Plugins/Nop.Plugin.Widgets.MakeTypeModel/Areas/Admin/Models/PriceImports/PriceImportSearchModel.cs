using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Models.PriceImports
{
    /// <summary>
    /// Represents a price impoprt search model
    /// </summary>
    public partial record PriceImportSearchModel : BaseSearchModel
    {
        #region Ctor
        public PriceImportSearchModel()
        {
            AvailableImportStatuses = new List<SelectListItem>();
            AvailableVendors = new List<SelectListItem>();
            PriceRanges = new List<PriceRangeJson>();
            priceRangeSearchModel = new PriceRangeSearchModel();
        }
        #endregion

        #region Properties
       
        [NopResourceDisplayName("Plugins.Widgets.MakeTypeModel.PriceImport.Fields.SearchImportStatus")]
        public int SearchImportStatus { get; set; }
        public IList<SelectListItem> AvailableImportStatuses { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.List.SearchVendor")]
        public int VendorId { get; set; }

        public IList<SelectListItem> AvailableVendors { get; set; }

        public List<PriceRangeJson> PriceRanges { get; set; }

        public PriceRangeSearchModel priceRangeSearchModel { get; set; }

        #endregion
    }
}
