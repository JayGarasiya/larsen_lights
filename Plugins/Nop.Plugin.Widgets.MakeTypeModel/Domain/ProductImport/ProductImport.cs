using Nop.Core;

namespace Nop.Plugin.Widgets.MakeTypeModel.Domain.ProductImport
{
    /// <summary>
    /// Represents a product import record.
    /// </summary>
    public partial class ProductImport : BaseEntity
    {
        #region Properties

        /// <summary>
        /// Gets or sets the import type identifier.
        /// </summary>
        public int ImportTypeId { get; set; }

        /// <summary>
        /// Gets or sets the import file name.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the processed row number.
        /// </summary>
        public int RowNumber { get; set; }

        /// <summary>
        /// Gets or sets the import status identifier.
        /// </summary>
        public int ImportStatusId { get; set; }

        /// <summary>
        /// Gets or sets a delete all.
        /// </summary>
        public bool DeleteAll { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the import was created.
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

        #endregion

        #region Custom properties

        /// <summary>
        /// Gets or sets the order status
        /// </summary>
        public ProductImportTypeEnum ImportType
        {
            get => (ProductImportTypeEnum)ImportTypeId;
            set => ImportTypeId = (int)value;
        }

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