using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Models.ModelCategory
{
    /// <summary>
    /// Represents a model category search model
    /// </summary>
    public partial record ModelCategorySearchModel : BaseSearchModel
    {
        #region Properties

        [NopResourceDisplayName("Admin.Catalog.Categories.List.SearchCategoryName")]
        public string SearchCategoryName { get; set; }

        #endregion
    }
}