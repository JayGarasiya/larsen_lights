using Nop.Core;
using Nop.Plugin.Widgets.MakeTypeModel.Domain.ProductImport;

namespace Nop.Plugin.Widgets.MakeTypeModel.Domain.PriceImport
{
    /// <summary>
    /// Represents a price import
    /// </summary>
    public class PriceImport : BaseEntity
    {
        #region Properties
        
        /// <summary>
        /// Gets or sets vendor indetifier
        /// </summary>
        public int VendorId { get; set; }
       
        /// <summary>
        /// Gets or sets file name
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets row number
        /// </summary>
        public int RowNumber { get; set; }

        /// <summary>
        /// Gets or sets import status indetifier
        /// </summary>
        public int ImportStatusId { get; set; }

        /// <summary>
        /// Gets or sets created on utc
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets price range
        /// </summary>
        public string PriceRange { get; set; }

        /// <summary>
        /// Gets or sets multiple price range
        /// </summary>
        public bool IsMulitplePriceRange { get; set; }

        /// <summary>
        /// Gets or sets download indetifier
        /// </summary>
        public int DownloadId { get; set; }

        #endregion

        #region Custom properties

        /// <summary>
        /// Gets or sets the payment status
        /// </summary>
        public ProductImportStatusEnum ImportStatus
        {
            get => (ProductImportStatusEnum)ImportStatusId;
            set => ImportStatusId = (int)value;
        }

        #endregion
    }
}
