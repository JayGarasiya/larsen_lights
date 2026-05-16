using Nop.Web.Framework.Models;

namespace Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Models.AddModelProductToProduct
{
    /// <summary>
    /// Represents a product model to add to the model product
    /// </summary>
    public partial record AddProductToModelProductModel : BaseNopModel
    {
        #region Ctor

        public AddProductToModelProductModel()
        {
            SelectedProductIds = new List<int>();
        }

        #endregion

        #region Properties

        public int ModelId { get; set; }

        public IList<int> SelectedProductIds { get; set; }

        #endregion
    }
}