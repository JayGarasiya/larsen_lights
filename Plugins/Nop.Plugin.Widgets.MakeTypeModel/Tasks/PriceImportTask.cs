using Nop.Plugin.Widgets.MakeTypeModel.Domain.ProductImport;
using Nop.Plugin.Widgets.MakeTypeModel.Services.PriceImport;
using Nop.Services.Logging;
using Nop.Services.ScheduleTasks;

namespace Nop.Plugin.Widgets.MakeTypeModel.Tasks
{
    /// <summary>
    /// Represents a schedule task of price import
    /// </summary>
    public class PriceImportTask : IScheduleTask
    {
        #region Fields

        protected readonly IPriceImportService _priceImportService;
        protected readonly ILogger _logger;

        #endregion

        #region Ctor

        public PriceImportTask(IPriceImportService priceImportService,
            ILogger logger)
        {
            _priceImportService = priceImportService;
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
            //import new files
            var importFiles = await _priceImportService.GetPriceImportByStatusAsync();
            foreach (var importFile in importFiles)
            {
                try
                {
                    await _priceImportService.ImportPricesFromXlsxAsync(importFile);
                }
                catch (Exception ex)
                {
                    await _logger.ErrorAsync($"Price imports : {ex.Message}", ex);
                }

                //check current file status is completed
                var importFileTemp = await _priceImportService.GetPriceImportByIdAsync(importFile.Id);
                if (importFileTemp.ImportStatus != ProductImportStatusEnum.Complete)
                    break;
            }
        }
        #endregion
    }
}
