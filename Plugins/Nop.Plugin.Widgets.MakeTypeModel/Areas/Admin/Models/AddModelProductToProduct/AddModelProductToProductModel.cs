using Nop.Web.Framework.Models;

namespace Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Models.AddModelProductToProduct
{
    /// <summary>
    /// Represents an add model product to product model
    /// </summary>
    public partial record AddModelProductToProductModel : BaseNopModel
    {
        #region Ctor

        public AddModelProductToProductModel()
        {
            SelectedModelProductIds = new List<int>();
        }

        #endregion

        #region Properties

        public int ProductId { get; set; }

        public IList<int> SelectedModelProductIds { get; set; }

        #endregion
    }
}
