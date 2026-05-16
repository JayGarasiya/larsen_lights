using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Models.ProductImport
{
    /// <summary>
    /// Represents product import model 
    /// </summary>
    public partial record ProductImportModel : BaseNopEntityModel
    {
        #region Properties

        [NopResourceDisplayName("Plugins.Widgets.MakeTypeModel.ProductImport.Fields.ImportType")]
        public int ImportTypeId { get; set; }
        public string ImportType { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.MakeTypeModel.ProductImport.Fields.FileName")]
        public string FileName { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.MakeTypeModel.ProductImport.Fields.RowNumber")]
        public int RowNumber { get; set; }

        public string ImportStatus { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.MakeTypeModel.ProductImport.Fields.DeleteAll")]
        public bool DeleteAll { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.MakeTypeModel.ProductImport.Fields.CreatedOnUtc")]
        public DateTime CreatedOnUtc { get; set; }

        #endregion
    }
}
