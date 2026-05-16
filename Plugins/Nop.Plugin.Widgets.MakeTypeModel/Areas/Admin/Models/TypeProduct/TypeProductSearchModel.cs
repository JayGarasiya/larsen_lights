using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Models.TypeProduct
{
    /// <summary>
    /// Represents a type product search model
    /// </summary>
    public partial record TypeProductSearchModel : BaseSearchModel
    {
        #region Ctor

        public TypeProductSearchModel()
        {
            AddTypeProduct = new TypeProductModel();
        }

        #endregion

        #region Properties

        [NopResourceDisplayName("Plugins.Widgets.MakeTypeModel.TypeProduct.Fields.SearchName")]
        public string SearchName { get; set; }

        public TypeProductModel AddTypeProduct { get; set; }

        #endregion
    }
}