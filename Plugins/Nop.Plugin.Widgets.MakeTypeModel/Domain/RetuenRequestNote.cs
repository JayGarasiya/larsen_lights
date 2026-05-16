using Nop.Core;

namespace Nop.Plugin.Widgets.MakeTypeModel.Domain
{
    /// <summary>
    /// Represents a return request note.
    /// </summary>
    public class RetuenRequestNote : BaseEntity
    {
        /// <summary>
        /// Gets or sets the return request identifier.
        /// </summary>
        public int RetuenRequestId { get; set; }

        /// <summary>
        /// Gets or sets the note text.
        /// </summary>
        public string Notes { get; set; }
    }
}
