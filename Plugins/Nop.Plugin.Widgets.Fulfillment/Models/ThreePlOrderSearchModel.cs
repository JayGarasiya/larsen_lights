using Nop.Web.Framework.Models;

namespace Nop.Plugin.Widgets.Fulfillment.Models
{
    /// <summary>
    /// Represents a 3PL order search model
    /// </summary>
    public partial record ThreePlOrderSearchModel : BaseSearchModel
    {
        #region Properties

        public int ThreePlId { get; set; }
        public int SearchOrderId { get; set; }
        public int SearchThreePlOrderId { get; set; }

        #endregion
    }
}
