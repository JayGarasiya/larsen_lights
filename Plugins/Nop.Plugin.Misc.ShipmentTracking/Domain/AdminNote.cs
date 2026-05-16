using Nop.Core;

namespace Nop.Plugin.Misc.ShipmentTracking.Domain
{
    /// <summary>
    /// Represents an admin note
    /// </summary>
    public partial class AdminNote : BaseEntity
    {
        /// <summary>
        /// Gets or sets the order identifier
        /// </summary>
        public int OrderId { get; set; }

        /// <summary>
        /// Gets or sets the order note identifier
        /// </summary>
        public int OrderNoteId { get; set; }
    }
}
