using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Models.ModelProduct
{
    /// <summary>
    /// Represents a model product search model
    /// </summary>
    public partial record ModelProductSearchModel : BaseSearchModel
    {
        #region Ctor

        public ModelProductSearchModel()
        {
            AvailableMakes = new List<SelectListItem>();
            AvailableTypes = new List<SelectListItem>();
        }

        #endregion

        #region Properties

        public int ProductId { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.MakeTypeModel.ModelProduct.Fields.SearchName")]
        public string SearchName { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.MakeTypeModel.ModelProduct.Fields.SearchInDescription")]
        public bool SearchInDescription { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.MakeTypeModel.ModelProduct.Fields.SearchMakeName")]
        public string SearchMakeName { get; set; }
        public IList<SelectListItem> AvailableMakes { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.MakeTypeModel.ModelProduct.Fields.SearchTypeName")]
        public string SearchTypeName { get; set; }
        public IList<SelectListItem> AvailableTypes { get; set; }

        #endregion
    }
}