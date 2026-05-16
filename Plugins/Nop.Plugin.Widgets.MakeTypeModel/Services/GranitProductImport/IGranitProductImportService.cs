using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Widgets.MakeTypeModel.Domain.GranitImport;

namespace Nop.Plugin.Widgets.MakeTypeModel.Services.GranitProductImport
{
    /// <summary>
    /// Granit product import service interface
    /// </summary>
    public interface IGranitProductImportService
    {
        /// <summary>
        /// Delete a granit product import
        /// </summary>
        /// <param name="graniteProductImport">Granite Product import</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task DeleteGranitProductImportAsync(Domain.GranitImport.GranitProductImport graniteProductImport);

        /// <summary>
        /// Gets all temp data records
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the temp data records.
        /// </returns>
        Task<IList<GranitDataImport>> GetAllTempDataRecordsAsync();

        /// <summary>
        /// Delete data records
        /// </summary>
        /// <param name="dataRecords">Temp1</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task DeleteDataRecordsFromTempAsync(IList<GranitDataImport> dataRecords);

        /// <summary>
        /// Gets all granit attr import records
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the temp attr records.
        /// </returns>
        Task<IList<GranitAttrImport>> GetAllGranitAttrImportRecordsAsync();

        /// <summary>
        /// Delete attr records
        /// </summary>
        /// <param name="dataRecords">Temp2</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task DeleteAttrRecordsFromTempAsync(IList<GranitAttrImport> attrRecords);

        /// <summary>
        /// Delete all granit product import complete files
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task DeleteAllGranitProductImportCompleteFilesAsync();

        /// <summary>
        /// Gets a granit product import by product import identifier
        /// </summary>
        /// <param name="graniteProductImportId">GraniteProduct import identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product import
        /// </returns>
        Task<Domain.GranitImport.GranitProductImport> GetGranitProductImportByIdAsync(int graniteProductImportId);

        /// <summary>
        /// Inserts a granit product import
        /// </summary>
        /// <param name="graniteProductImport">Granite Product import</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task InsertGranitProductImportAsync(Domain.GranitImport.GranitProductImport graniteProductImport);

        /// <summary>
        /// Updates the granit product import
        /// </summary>
        /// <param name="graniteProductImport">Granite Product import</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task UpdateGranitProductImportAsync(Domain.GranitImport.GranitProductImport graniteProductImport);

        /// <summary>
        /// Gets a value indicating whether is import file already exists
        /// </summary>
        /// <returns>The task result contains a value indicating whether is import file already exists</returns>
        bool IsImportFileExists();

        /// <summary>
        /// Gets amanufacturer by name
        /// </summary>
        /// <param name="manufacturerName">Manufacturer</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the manufacturer
        /// </returns>
        Task<Manufacturer> GetManufacturerByNameAsync(string manufacturerName);

        /// <summary>
        /// Gets category by name
        /// </summary>
        /// <param name="categoryName">Category</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the category
        /// </returns>
        Task<Category> GetCategoryByNameAsync(string categoryName);

        /// <summary>
        /// Gets specification attribute by name
        /// </summary>
        /// <param name="specificationAttributeName">SpecificationAttribute</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the category
        /// </returns>
        Task<SpecificationAttribute> GetSpecificationAttributeByNameAsync(string specificationAttributeName);

        /// <summary>
        /// Gets all Granit product imports
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <param name="importTypeIds">Import type identifiers; null to load all product imports</param>
        /// <param name="importStatusIds">Import status identifiers; null to load all product imports</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product imports
        /// </returns>
        Task<IPagedList<Domain.GranitImport.GranitProductImport>> GetAllGranitProductImportAsync(string fileName = null,
           List<int> importStatusIds = null, int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Gets a list of granit product imports
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        Task<Domain.GranitImport.GranitProductImport> GetGranitProductImportAsync();

        /// <summary>
        /// Gets a list of day=ta records from temp table
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        Task<IList<GranitDataImport>> GetDataRecordsFromTempAsync(int startRow = 0, int endRow = 0);

        /// <summary>
        /// Gets a list of attr records from temp table
        /// </summary>
        /// <param name="sku">SKU</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product
        /// </returns>
        Task<IList<GranitAttrImport>> GetAttrRecordsBySKUAsync(string sku);

        /// <summary>
        /// Import granit products
        /// </summary>
        /// <param name="graniteProductImport">Product import</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task ImportGranitProductsAsync(IList<GranitDataImport> dataRecords, Domain.GranitImport.GranitProductImport importFile, int firstDataRecord, int maxDataRecord);

        /// <summary>
        /// Gets a products by SKU array
        /// </summary>
        /// <param name="skuArray">SKU array</param>
        /// <param name="vendorId">Vendor ID; 0 to load all records</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the products
        /// </returns>
        Task<IList<Product>> GetProductsBySkuAsync(string[] skuArray);
    }
}
