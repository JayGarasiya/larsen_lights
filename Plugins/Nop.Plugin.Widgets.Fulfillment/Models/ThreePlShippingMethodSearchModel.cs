using Nop.Web.Framework.Models;

namespace Nop.Plugin.Widgets.Fulfillment.Models
{
    /// <summary>
    /// Represents a 3PL Shipping Method search model
    /// </summary>
    public partial record ThreePlShippingMethodSearchModel : BaseSearchModel
    {
        #region Ctor

        public ThreePlShippingMethodSearchModel()
        {
            AddThreePlShippingMethod = new ThreePlShippingMethodModel();
        }

        #endregion

        #region Properties

        public ThreePlShippingMethodModel AddThreePlShippingMethod { get; set; }

        #endregion
    }
}
