using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Models.GranitProduct
{
    /// <summary>
    /// Represents a granit product import model 
    /// </summary>
    public partial record GranitProductImportModel : BaseNopEntityModel
    {
        #region Properties
        
        [NopResourceDisplayName("Plugins.Widgets.MakeTypeModel.GranitProductImport.Fields.FileName")]
        public string FileName { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.MakeTypeModel.GranitProductImport.Fields.DataRowNumber")]
        public int DataRowNumber { get; set; }

        public string ImportStatus { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.MakeTypeModel.GranitProductImport.Fields.CreatedOnUtc")]
        public DateTime CreatedOnUtc { get; set; }
        #endregion
    }
}
