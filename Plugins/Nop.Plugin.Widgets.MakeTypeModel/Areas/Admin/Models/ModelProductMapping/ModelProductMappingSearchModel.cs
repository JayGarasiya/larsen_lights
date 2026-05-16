using Nop.Web.Framework.Models;

namespace Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Models.ModelProductMapping
{
    /// <summary>
    /// Represents a model product mapping search model
    /// </summary>
    public partial record ModelProductMappingSearchModel : BaseSearchModel
    {
        #region Properties

        public int ModelId { get; set; }

        #endregion
    }
}