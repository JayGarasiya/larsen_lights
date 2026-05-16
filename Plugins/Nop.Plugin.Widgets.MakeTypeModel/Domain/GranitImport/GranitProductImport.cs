using Nop.Core;

namespace Nop.Plugin.Widgets.MakeTypeModel.Domain.GranitImport
{
    /// <summary>
    /// Represents a granite product import
    /// </summary>
    public class GranitProductImport : BaseEntity
    {
        #region Properties

        /// <summary>
        /// Gets or sets file name
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets data sheet row number
        /// </summary>
        public int DataRowNumber { get; set; }

        /// <summary>
        /// Gets or sets import status id
        /// </summary>
        public int ImportStatusId { get; set; }

        /// <summary>
        /// Gets or sets created on utc
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }
        #endregion

        #region Custom properties
       
        /// <summary>
        /// Gets or sets the payment status
        /// </summary>
        public GranitProductImportStatusEnum ImportStatus
        {
            get => (GranitProductImportStatusEnum)ImportStatusId;
            set => ImportStatusId = (int)value;
        }
        #endregion
    }
}
