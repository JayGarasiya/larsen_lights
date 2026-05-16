using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Models.TypeProduct
{
    /// <summary>
    /// Represents a type product model
    /// </summary>
    public partial record TypeProductModel : BaseNopEntityModel
    {
        #region Properties

        [NopResourceDisplayName("Plugins.Widgets.MakeTypeModel.TypeProduct.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.MakeTypeModel.TypeProduct.Fields.Published")]
        public bool Published { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.MakeTypeModel.TypeProduct.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        #endregion
    }
}
