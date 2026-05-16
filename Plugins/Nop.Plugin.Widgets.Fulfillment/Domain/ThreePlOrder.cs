using Nop.Core;

namespace Nop.Plugin.Widgets.Fulfillment.Domain
{
    /// <summary>
    /// Represents a 3PL central record
    /// </summary>
    public partial class ThreePlOrder : BaseEntity
    {
        #region Properties

        /// <summary>
        /// Gets or sets the 3Pl identifier
        /// </summary>
        public int ThreePlId { get; set; }

        /// <summary>
        /// Gets or sets the external 3PL order identifier
        /// </summary>
        public int ThreePlOrderId { get; set; }

        /// <summary>
        /// Gets or sets the 3PL Stutus
        /// </summary>
        public int ThreePlStutusId { get; set; }

        /// <summary>
        /// Gets or sets the creation date
        /// </summary>
        public DateTime? CreationDate { get; set; }

        /// <summary>
        /// Gets or sets the cancelled date
        /// </summary>
        public DateTime? CancelledDate { get; set; }

        /// <summary>
        /// Gets or sets the fulfill invoice total
        /// </summary>
        public decimal FulfillInvTotal { get; set; }

        #endregion

        #region Custom properties

        /// <summary>
        /// Gets or sets the order status
        /// </summary>
        public ThreePlStutus ThreePlStutus
        {
            get => (ThreePlStutus)ThreePlStutusId;
            set => ThreePlStutusId = (int)value;
        }

        #endregion
    }
}
