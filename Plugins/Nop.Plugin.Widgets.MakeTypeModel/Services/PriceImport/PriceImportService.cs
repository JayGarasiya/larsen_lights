using ClosedXML.Excel;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Models.PriceImports;
using Nop.Plugin.Widgets.MakeTypeModel.Domain.ProductImport;
using Nop.Services.Catalog;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Vendors;
using System.Globalization;

namespace Nop.Plugin.Widgets.MakeTypeModel.Services.PriceImport
{
    /// <summary>
    /// Price import service
    /// </summary>
    public class PriceImportService : IPriceImportService
    {
        #region Fields

        protected readonly IRepository<Domain.PriceImport.PriceImport> _priceImportRepository;
        protected readonly INopFileProvider _fileProvider;
        protected readonly MakeTypeModelSettings _makeTypeModelSettings;
        protected readonly IProductService _productService;
        protected readonly ILogger _logger;
        protected readonly IVendorService _vendorService;
        protected readonly IDownloadService _downloadService;

        #endregion

        #region Ctor

        public PriceImportService(IRepository<Domain.PriceImport.PriceImport> priceImportRepository,
            INopFileProvider fileProvider,
            MakeTypeModelSettings makeTypeModelSettings,
            IProductService productService,
            ILogger logger,
            IVendorService vendorService,
            IDownloadService downloadService)
        {
            _priceImportRepository = priceImportRepository;
            _fileProvider = fileProvider;
            _makeTypeModelSettings = makeTypeModelSettings;
            _productService = productService;
            _logger = logger;
            _vendorService = vendorService;
            _downloadService = downloadService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Delete a price import
        /// </summary>
        /// <param name="priceImport">Price import</param>
        /// <returns>A task that represents the asynchronous operation</returns> 
        public virtual async Task DeletePriceImportAsync(Domain.PriceImport.PriceImport priceImport)
        {
            ArgumentNullException.ThrowIfNull(priceImport);

            try
            {
                var filePath = _fileProvider.Combine(_fileProvider.MapPath("~/wwwroot/files/priceimport/" + priceImport.FileName));

                if (_fileProvider.FileExists(filePath))
                    _fileProvider.DeleteFile(filePath);
            }
            catch
            {
                // ignored
            }

            await _priceImportRepository.DeleteAsync(priceImport);
        }

        /// <summary>
        /// Get All Price Import
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="importTypeIds"></param>
        /// <param name="importStatusIds"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public virtual async Task<IPagedList<Domain.PriceImport.PriceImport>> GetAllPriceImportAsync(List<int> importStatusIds = null, int pageIndex = 0, int pageSize = int.MaxValue, int vendorId = 0)
        {
            var query = _priceImportRepository.Table;

            if (importStatusIds != null && importStatusIds.Any())
                query = query.Where(o => importStatusIds.Contains(o.ImportStatusId));

            if (vendorId > 0)
                query = query.Where(o => o.VendorId == vendorId);

            query = query.OrderByDescending(o => o.CreatedOnUtc);

            //database layer paging
            return await query.ToPagedListAsync(pageIndex, pageSize);
        }

        /// <summary>
        /// Inserts a price import
        /// </summary>
        /// <param name="priceImport">price import</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task InsertPriceImportAsync(Domain.PriceImport.PriceImport priceImport)
        {
            ArgumentNullException.ThrowIfNull(priceImport);

            await _priceImportRepository.InsertAsync(priceImport);
        }

        /// <summary>
        /// Gets a list of price imports
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns> 
        public virtual async Task<IList<Domain.PriceImport.PriceImport>> GetPriceImportByStatusAsync()
        {
            var importStatusIds = new List<int> { (int)ProductImportStatusEnum.Processing, (int)ProductImportStatusEnum.Pending };
            var query = _priceImportRepository.Table;

            query = query.Where(p => importStatusIds.Contains(p.ImportStatusId));

            query = query.OrderByDescending(o => o.ImportStatusId).ThenBy(o => o.CreatedOnUtc);

            return await query.ToListAsync();
        }

        /// <summary>
        /// Gets a product import by price import identifier
        /// </summary>
        /// <param name="priceImportId">Price import identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product import
        /// </returns>
        public virtual async Task<Domain.PriceImport.PriceImport> GetPriceImportByIdAsync(int priceImportId)
        {
            return await _priceImportRepository.GetByIdAsync(priceImportId, cache => default);
        }

        /// <summary>
        /// Updates the price import
        /// </summary>
        /// <param name="priceImport">Price import</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task UpdatePriceImportAsync(Domain.PriceImport.PriceImport priceImport)
        {
            ArgumentNullException.ThrowIfNull(priceImport);

            await _priceImportRepository.UpdateAsync(priceImport);
        }

        /// <summary>
        /// Import price from XLSX file
        /// </summary>
        /// <param name="priceImport">price import</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task ImportPricesFromXlsxAsync(Domain.PriceImport.PriceImport priceImport)
        {
            var download = await _downloadService.GetDownloadByIdAsync(priceImport.DownloadId);
            if (download != null)
            {
                using var stream = new MemoryStream(download.DownloadBinary);
                using var xlPackage = new XLWorkbook(stream);
                var dataWorkSheet = xlPackage.Worksheet(1);

                if (dataWorkSheet == null)
                    return;

                var firstDataRow = priceImport.RowNumber == 0 ? 2 : priceImport.RowNumber;
                var lastRow = dataWorkSheet.LastRowUsed().RowNumber();
                var maxRow = priceImport.RowNumber + _makeTypeModelSettings.PriceImportRecords;

                for (var iRow = firstDataRow; iRow <= lastRow; iRow++)
                {
                    var row = dataWorkSheet.Row(iRow);
                    var productSKU = row.Cell(1).Value.ToString();

                    if (string.IsNullOrEmpty(productSKU))
                    {
                        firstDataRow++;
                        await _logger.InformationAsync($"Not found SKU '{productSKU}'.");
                        continue;
                    }

                    var product = await _productService.GetProductBySkuAsync(productSKU);
                    if (product != null)
                    {
                        if (product.VendorId == priceImport.VendorId)
                        {
                            var priceRangeList = string.IsNullOrEmpty(priceImport.PriceRange) ? new List<PriceRangeJson>() : JsonConvert.DeserializeObject<List<PriceRangeJson>>(priceImport.PriceRange);

                            foreach (var priceRange in priceRangeList)
                            {
                                var priceString = row.Cell(2).Value.ToString();
                                if (decimal.TryParse(priceString, NumberStyles.Any, CultureInfo.InvariantCulture, out var originalCost))
                                {
                                    // Check price range
                                    if ((priceRange.FromPrice == 0 || originalCost >= priceRange.FromPrice) && (priceRange.ToPrice == 0 || originalCost <= priceRange.ToPrice))
                                    {
                                        decimal newCost;

                                        if (priceRange.UsePercentage)
                                        {
                                            // Percentage-based adjustment
                                            newCost = originalCost + originalCost * priceRange.PricePercentage / 100m;
                                        }
                                        else
                                        {
                                            // Fixed amount adjustment
                                            newCost = originalCost + priceRange.PriceAmount;
                                        }
                                        newCost = Math.Round(newCost, 2);
                                        product.Price = newCost;
                                        product.ProductCost = originalCost;
                                        product.Published = true;

                                        await _productService.UpdateProductAsync(product);
                                    }
                                }
                                else
                                {
                                    await _logger.InformationAsync($"Invalid price format for SKU '{productSKU}'. Value received: '{priceString}' at row {iRow}.");
                                }
                            }
                        }
                        else
                        {
                            var vendor = await _vendorService.GetVendorByIdAsync(priceImport.VendorId);
                            await _logger.InformationAsync($"{vendor.Name} Vendor is not mapped for product SKU '{productSKU}'.");
                        }
                    }

                    //update row processed 
                    firstDataRow++;
                    priceImport.RowNumber = firstDataRow;
                    priceImport.ImportStatus = ProductImportStatusEnum.Processing;
                    await UpdatePriceImportAsync(priceImport);

                    if (firstDataRow >= maxRow)
                        break;
                }

                if (firstDataRow >= lastRow)
                    priceImport.ImportStatusId = (int)ProductImportStatusEnum.Complete;

                await UpdatePriceImportAsync(priceImport);

            }
        }

        #endregion
    }
}
