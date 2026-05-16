using Nop.Core;

namespace Nop.Plugin.Widgets.MakeTypeModel.Services.ProductImport
{
    /// <summary>
    /// Product import service 
    /// </summary>
    public partial interface IProductImportService
    {
        /// <summary>
        /// Delete a product import
        /// </summary>
        /// <param name="productImport">Product import</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task DeleteProductImportAsync(Domain.ProductImport.ProductImport productImport);

        /// <summary>
        /// Delete all product import complete files
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task DeleteAllProductImportCompleteFilesAsync();

        /// <summary>
        /// Gets a product import by product import identifier
        /// </summary>
        /// <param name="productImportId">Product import identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product import
        /// </returns>
        Task<Domain.ProductImport.ProductImport> GetProductImportByIdAsync(int productImportId);

        /// <summary>
        /// Inserts a product import
        /// </summary>
        /// <param name="productImport">Product import</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task InsertProductImportAsync(Domain.ProductImport.ProductImport productImport);

        /// <summary>
        /// Updates the product import
        /// </summary>
        /// <param name="productImport">Product import</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task UpdateProductImportAsync(Domain.ProductImport.ProductImport productImport);

        /// <summary>
        /// Gets a value indicating whether is import file name already exists
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <returns>The task result contains a value indicating whether is import file name already exists</returns>
        bool IsImportFileNameAlreadyExists(string filename);

        /// <summary>
        /// Gets all product imports 
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
        Task<IPagedList<Domain.ProductImport.ProductImport>> GetAllProductImportAsync(string fileName = null,
            List<int> importTypeIds = null, List<int> importStatusIds = null, int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Gets a list of product imports by status
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        Task<IList<Domain.ProductImport.ProductImport>> GetProductImportByStatusAsync();

        /// <summary>
        /// Import model fit products from XLSX file
        /// </summary>
        /// <param name="productImport">Product import</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task ImportModelFitProductsFromXlsxAsync(Domain.ProductImport.ProductImport productImport);

        /// <summary>
        /// Import products from XLSX file
        /// </summary>
        /// <param name="productImport">Product import</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task ImportProductsFromXlsxAsync(Domain.ProductImport.ProductImport productImport);

        /// <summary>
        /// Import products stock from XLSX file
        /// </summary>
        /// <param name="productImport">Product stock import</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task ImportProductStockFromXlsxAsync(Domain.ProductImport.ProductImport productImport);
    }
}
