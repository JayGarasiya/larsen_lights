using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Models.MakeProduct
{
    /// <summary>
    /// Represents a make product model
    /// </summary>
    public partial record MakeProductModel : BaseNopEntityModel
    {
        #region Properties

        [NopResourceDisplayName("Plugins.Widgets.MakeTypeModel.MakeProduct.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.MakeTypeModel.MakeProduct.Fields.Published")]
        public bool Published { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.MakeTypeModel.MakeProduct.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        #endregion
    }
}
