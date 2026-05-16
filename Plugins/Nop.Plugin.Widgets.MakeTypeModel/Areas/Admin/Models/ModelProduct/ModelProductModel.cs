using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Models.ModelProductMapping;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Models.ModelProduct
{
    /// <summary>
    /// Represents a model product model
    /// </summary>
    public partial record ModelProductModel : BaseNopEntityModel
    {
        #region Ctor

        public ModelProductModel()
        {
            AvailableMakes = new List<SelectListItem>();
            AvailableTypes = new List<SelectListItem>();
            ModelProductMappingSearchModel = new ModelProductMappingSearchModel();
        }

        #endregion

        #region Properties

        [NopResourceDisplayName("Plugins.Widgets.MakeTypeModel.ModelProduct.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.MakeTypeModel.ModelProduct.Fields.Description")]
        public string Description { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.MakeTypeModel.ModelProduct.Fields.MakeName")]
        public string MakeName { get; set; }
        public IList<SelectListItem> AvailableMakes { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.MakeTypeModel.ModelProduct.Fields.TypeName")]
        public string TypeName { get; set; }
        public IList<SelectListItem> AvailableTypes { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.MakeTypeModel.ModelProduct.Fields.Published")]
        public bool Published { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.MakeTypeModel.ModelProduct.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        public int ProductModelId { get; set; }

        public bool HasMoreProducts { get; set; }

        public ModelProductMappingSearchModel ModelProductMappingSearchModel { get; set; }

        #endregion
    }
}
