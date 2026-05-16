using LinqToDB.Data;
using Newtonsoft.Json;
using Nop.Data;
using Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Models.PriceImports;
using Nop.Plugin.Widgets.MakeTypeModel.BaseApiCall;
using Nop.Plugin.Widgets.MakeTypeModel.Services.PriceImport;
using Nop.Services.Configuration;
using Nop.Services.ScheduleTasks;

namespace Nop.Plugin.Widgets.MakeTypeModel.Tasks
{
    /// <summary>
    /// Represents a schedule task of hy-cap stock API 
    /// </summary>
    public class HyCapStockAPITask : IScheduleTask
    {
        #region Fields

        protected readonly MakeTypeModelSettings _makeTypeModelSettings;
        protected readonly INopDataProvider _nopDataProvider;
        protected readonly ISettingService _settingService;
        protected readonly IPriceImportService _priceImportService;

        #endregion

        #region Ctor

        public HyCapStockAPITask(MakeTypeModelSettings makeTypeModelSettings,
            INopDataProvider nopDataProvider,
            ISettingService settingService,
            IPriceImportService priceImportService)
        {
            _makeTypeModelSettings = makeTypeModelSettings;
            _nopDataProvider = nopDataProvider;
            _settingService = settingService;
            _priceImportService = priceImportService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Execute task
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task ExecuteAsync()
        {
            var authAPIUrl = _makeTypeModelSettings.AuthorizationAPIURL;
            var inventoryAPIUrl = _makeTypeModelSettings.InventoryAPIURL;
            var userName = _makeTypeModelSettings.Username;
            var passWord = _makeTypeModelSettings.Password;
            var tokenGuidId = _makeTypeModelSettings.TokenGuidId;
            var lastStockAPIExecutedDate = _makeTypeModelSettings.LastExecutedStockAPI;

            var executeAPI = true;

            if (lastStockAPIExecutedDate.HasValue)
            {
                var lastExecutedDate = lastStockAPIExecutedDate?.ToString("MM/dd/yyyy");
                var currentDate = DateTime.UtcNow.ToString("MM/dd/yyyy");

                executeAPI = lastExecutedDate == currentDate;

            }

            if (!_makeTypeModelSettings.LastExecutedProductAPI.HasValue || (!executeAPI))
            {
                if (!string.IsNullOrEmpty(authAPIUrl) && !string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(passWord)
                    && !string.IsNullOrEmpty(inventoryAPIUrl) && !string.IsNullOrEmpty(tokenGuidId))
                {
                    var token = await HyCapacityAPI.GetAuthorizationTokenAsync(authAPIUrl, userName, passWord);
                    if (token != null)
                    {
                        var inventoryAPIResponse = await HyCapacityAPI.InventoryAPIAsync(authorizationToken: token,
                                           tokenGuidId: tokenGuidId, apiUrl: inventoryAPIUrl);

                        if (!string.IsNullOrEmpty(inventoryAPIResponse))
                        {
                            _makeTypeModelSettings.LastExecutedStockAPI = DateTime.UtcNow;

                            await _settingService.SaveSettingAsync(_makeTypeModelSettings);

                            var priceImport = (await _priceImportService.GetAllPriceImportAsync(vendorId: _makeTypeModelSettings.HyCapacityVendorId)).FirstOrDefault();

                            var priceRangeList = new List<PriceRangeJson>();

                            if (priceImport != null)
                                priceRangeList = string.IsNullOrEmpty(priceImport.PriceRange) ? new List<PriceRangeJson>() : JsonConvert.DeserializeObject<List<PriceRangeJson>>(priceImport.PriceRange);

                            // Prepare parameters for stored procedure
                            var parameters = new[]
                                {
                                    new DataParameter("@JsonData", inventoryAPIResponse, LinqToDB.DataType.NVarChar),
                                    new DataParameter("@PriceRangeJson", JsonConvert.SerializeObject(priceRangeList), LinqToDB.DataType.NVarChar)
                                };

                            // Call stored procedure using NopDataProvider
                            await _nopDataProvider.QueryProcAsync<int>("UpdateProductStockFromJson", parameters);
                        }
                    }
                }
            }
        }

        #endregion
    }
}
