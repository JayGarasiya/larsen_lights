using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Models.MakeProduct
{
    /// <summary>
    /// Represents a make product search model
    /// </summary>
    public partial record MakeProductSearchModel : BaseSearchModel
    {
        #region Ctor

        public MakeProductSearchModel()
        {
            AddMakeProduct = new MakeProductModel();
        }

        #endregion

        #region Properties

        [NopResourceDisplayName("Plugins.Widgets.MakeTypeModel.MakeProduct.Fields.SearchName")]
        public string SearchName { get; set; }

        public MakeProductModel AddMakeProduct { get; set; }

        #endregion
    }
}