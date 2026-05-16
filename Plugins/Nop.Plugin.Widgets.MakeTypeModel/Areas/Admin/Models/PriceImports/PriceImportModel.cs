using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Models.PriceImports
{
    /// <summary>
    /// Represents a price impoprt model
    /// </summary>
    public partial record PriceImportModel : BaseNopEntityModel
    {
        #region Ctor

        public PriceImportModel() 
        {
            AvailableVendors = new List<SelectListItem>();
            PriceRanges = new List<PriceRangeJson>();
        }
        #endregion

        #region Properties

        [NopResourceDisplayName("Plugins.Widgets.MakeTypeModel.PriceImport.Fields.FileName")]
        public string FileName { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.MakeTypeModel.PriceImport.Fields.RowNumber")]
        public int RowNumber { get; set; }

        public string ImportStatus { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.MakeTypeModel.PriceImport.Fields.CreatedOnUtc")]
        public DateTime CreatedOnUtc { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.MakeTypeModel.PriceImport.Fields.VendorId")]
        public int VendorId { get; set; }

        public string VendorName { get; set; }

        public IList<SelectListItem> AvailableVendors { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.MakeTypeModel.PriceImport.Fields.IsMulitplePriceRange")]
        public bool IsMulitplePriceRange { get; set; }

        public List<PriceRangeJson> PriceRanges { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.MakeTypeModel.PriceImport.Fields.ExcelFile")]
        [UIHint("Download")]
        public int DownloadId { get; set; }

        #endregion
    }
}
