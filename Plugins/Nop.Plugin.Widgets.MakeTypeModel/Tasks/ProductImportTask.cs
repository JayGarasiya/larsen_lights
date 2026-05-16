using Nop.Plugin.Widgets.MakeTypeModel.Domain.ProductImport;
using Nop.Plugin.Widgets.MakeTypeModel.Services.ProductImport;
using Nop.Services.Logging;
using Nop.Services.ScheduleTasks;

namespace Nop.Plugin.Widgets.MakeTypeModel.Tasks
{
    /// <summary>
    /// Represents a schedule task of product import
    /// </summary>
    public partial class ProductImportTask : IScheduleTask
    {
        #region Fields

        protected readonly IProductImportService _productImportService;
        protected readonly ILogger _logger;

        #endregion

        #region Ctor

        public ProductImportTask(IProductImportService productImportService,
            ILogger logger)
        {
            _productImportService = productImportService;
            _logger = logger;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Execute task
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task ExecuteAsync()
        {
            //delete all completed files
            await _productImportService.DeleteAllProductImportCompleteFilesAsync();

            //import new files
            var importFiles = await _productImportService.GetProductImportByStatusAsync();
            foreach (var importFile in importFiles)
            {
                try
                {
                    switch(importFile.ImportType)
                    {
                        case ProductImportTypeEnum.Products:
                            await _productImportService.ImportProductsFromXlsxAsync(importFile);
                            break;
                        case ProductImportTypeEnum.ModelFit:
                            await _productImportService.ImportModelFitProductsFromXlsxAsync(importFile);
                            break;
                        case ProductImportTypeEnum.Stock:
                            await _productImportService.ImportProductStockFromXlsxAsync(importFile);
                            break;
                        default:
                            await _logger.ErrorAsync($"Hy-capacity imports : {nameof(importFile.ImportType)} cannot be import");
                            break;
                    }
                }
                catch(Exception ex)
                {
                    await _logger.ErrorAsync($"Hy-capacity imports : {ex.Message}", ex);
                }

                //check current file status is completed
                var importFileTemp = await _productImportService.GetProductImportByIdAsync(importFile.Id);
                if (importFileTemp.ImportStatus != ProductImportStatusEnum.Complete)
                    break;
            }
        }

        #endregion
    }
}