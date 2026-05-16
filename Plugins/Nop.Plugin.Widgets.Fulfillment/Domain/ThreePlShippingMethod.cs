using Nop.Core;

namespace Nop.Plugin.Widgets.Fulfillment.Domain
{
    /// <summary>
    /// Represents a 3PL central shipping method record
    /// </summary>
    public partial class ThreePlShippingMethod : BaseEntity
    {
        #region Properties

        /// <summary>
        /// Gets or sets the shipping method name
        /// </summary>
        public string ShippingMethod { get; set; }

        /// <summary>
        /// Gets or sets the 3PL carrier identifier
        /// </summary>
        public string ThreePlCarrier { get; set; }

        /// <summary>
        /// Gets or sets the 3PL service identifier
        /// </summary>
        public string ThreePlService { get; set; }

        #endregion
    }
}
