using Nop.Core;

namespace Nop.Plugin.Widgets.MakeTypeModel.Services.PriceImport
{
    /// <summary>
    /// Price import service 
    /// </summary>
    public interface IPriceImportService
    {
        /// <summary>
        /// Get All Price Import
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="importTypeIds"></param>
        /// <param name="importStatusIds"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        Task<IPagedList<Domain.PriceImport.PriceImport>> GetAllPriceImportAsync(List<int> importStatusIds = null, int pageIndex = 0, int pageSize = int.MaxValue, int vendorId = 0);

        /// <summary>
        /// Insert a price import
        /// </summary>
        /// <param name="priceImport">price import</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task InsertPriceImportAsync(Domain.PriceImport.PriceImport priceImport);

        /// <summary>
        /// Delete a product import
        /// </summary>
        /// <param name="productImport">Product import</param>
        /// <returns>A task that represents the asynchronous operation</returns> 
        Task DeletePriceImportAsync(Domain.PriceImport.PriceImport priceImport);

        /// <summary>
        /// Gets a list of price imports by status
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns> 
        Task<IList<Domain.PriceImport.PriceImport>> GetPriceImportByStatusAsync();

        /// <summary>
        /// Gets a product import by price import identifier
        /// </summary>
        /// <param name="priceImportId">Price import identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product import 
        /// </returns>
        Task<Domain.PriceImport.PriceImport> GetPriceImportByIdAsync(int priceImportId);

        /// <summary>
        /// Import price from XLSX file
        /// </summary>
        /// <param name="priceImport">price import</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task ImportPricesFromXlsxAsync(Domain.PriceImport.PriceImport priceImport);

        /// <summary>
        /// Updates the price import
        /// </summary>
        /// <param name="priceImport">Price import</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task UpdatePriceImportAsync(Domain.PriceImport.PriceImport priceImport);
    }
}
