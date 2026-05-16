using Microsoft.Extensions.DependencyInjection;
using Nop.Plugin.Widgets.MakeTypeModel.Domain.GranitImport;
using Nop.Plugin.Widgets.MakeTypeModel.Services.GranitProductImport;
using Nop.Services.Logging;
using Nop.Services.ScheduleTasks;

namespace Nop.Plugin.Widgets.MakeTypeModel.Tasks
{
    /// <summary>
    /// Represents a schedule task to granite product import 
    /// </summary>
    public class GraniteProductImportTask : IScheduleTask
    {
        #region Field

        private readonly IGranitProductImportService _granitProductImportService;
        private readonly ILogger _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly MakeTypeModelSettings _makeTypeModelSettings;

        #endregion

        #region Ctor
        public GraniteProductImportTask(IGranitProductImportService granitProductImportService,
            ILogger logger,
            IServiceProvider serviceProvider,
            MakeTypeModelSettings makeTypeModelSettings)
        {
            _granitProductImportService = granitProductImportService;
            _logger = logger;
            _serviceProvider = serviceProvider;
            _makeTypeModelSettings = makeTypeModelSettings;
        }

        #endregion

        #region Method

        /// <summary>
        /// Execute task
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task ExecuteAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var granitProductImportService = scope.ServiceProvider.GetRequiredService<IGranitProductImportService>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger>();

            // Delete all completed files
            await granitProductImportService.DeleteAllGranitProductImportCompleteFilesAsync();

            // Import new files
            var granitProductImport = await granitProductImportService.GetGranitProductImportAsync();
            if(granitProductImport != null)
            { 
                try
                {
                    var firstDataRow = granitProductImport.DataRowNumber == 0 ? granitProductImport.DataRowNumber + 1 : granitProductImport.DataRowNumber;
                    var maxRow = firstDataRow + _makeTypeModelSettings.GraintProductsRecords;

                    var records = await _granitProductImportService.GetDataRecordsFromTempAsync(firstDataRow, maxRow);

                    if (records.Count == 0)
                    {
                        if(granitProductImport.DataRowNumber != 0 && granitProductImport.ImportStatus != GranitProductImportStatusEnum.Pending)
                        {
                            granitProductImport.ImportStatus = GranitProductImportStatusEnum.Complete;
                            await granitProductImportService.UpdateGranitProductImportAsync(granitProductImport);
                        }
                    }
                    else
                    {
                        await _granitProductImportService.ImportGranitProductsAsync(records, granitProductImport, firstDataRow, maxRow);
                    }
                }
                catch (Exception ex)
                {
                    await _logger.ErrorAsync($"Granit imports : {ex.Message}", ex);
                }

                //check current file status is completed
                var importFileTemp = await _granitProductImportService.GetGranitProductImportByIdAsync(granitProductImport.Id);
                if (importFileTemp.ImportStatus != GranitProductImportStatusEnum.Complete)
                    return;
            }
        }

        #endregion
    }
}
