using Nop.Core;

namespace Nop.Plugin.Widgets.Fulfillment.Domain
{
    /// <summary>
    /// Represents a 3PL central record
    /// </summary>
    public partial class ThreePlRecord : BaseEntity
    {
        #region Properties

        /// <summary>
        /// Gets or sets the order identifier
        /// </summary>
        public int OrderId { get; set; }

        /// <summary>
        /// Gets or sets the 3PL Stutus
        /// </summary>
        public int ThreePlStutusId { get; set; }

        /// <summary>
        /// Gets or sets the paid by check order
        /// </summary>
        public bool PaidByCheck { get; set; }

        /// <summary>
        /// Gets or sets the note if error while sync order
        /// </summary>
        public string Note { get; set; }

        /// <summary>
        /// Gets or sets the cancelled date
        /// </summary>
        public DateTime? CancelledDate { get; set; }

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
