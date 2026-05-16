using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Models.ModelProductMapping
{
    /// <summary>
    /// Represents a model product mapping model
    /// </summary>
    public partial record ModelProductMappingModel : BaseNopEntityModel
    {
        #region Properties

        public int ModelId { get; set; }

        public int ProductId { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.MakeTypeModel.ModelProductMapping.Fields.Product")]
        public string ProductName { get; set; }

        #endregion
    }
}