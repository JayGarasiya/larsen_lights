using Microsoft.AspNetCore.Hosting;
using Nop.Core;
using Nop.Core.Domain.Stores;
using Nop.Core.Infrastructure;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Plugins;
using Nop.Services.ScheduleTasks;
using Nop.Services.Stores;

namespace Nop.Plugin.Feed.GoogleShopping.ScheduleTask;

public partial class GoogleFeedGenerate : IScheduleTask
{
    #region Fields
    private readonly IStoreContext _storeContext;
    private readonly IPluginService _pluginService;
    private readonly ILocalizationService _localizationService;
    private readonly IStoreService _storeService;
    private readonly INotificationService _notificationService;
    private readonly ILogger _logger;
    private readonly INopFileProvider _nopFileProvider;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly GoogleShoppingSettings _googleShoppingSettings;
    #endregion

    #region Ctor
    public GoogleFeedGenerate(IStoreContext storeContext, IPluginService pluginService, ILocalizationService localizationService,
        IStoreService storeService, INotificationService notificationService,
        ILogger logger, INopFileProvider nopFileProvider, GoogleShoppingSettings googleShoppingSettings, IWebHostEnvironment webHostEnvironment)
    {
        _storeContext = storeContext;
        _pluginService = pluginService;
        _localizationService = localizationService;
        _storeService = storeService;
        _notificationService = notificationService;
        _logger = logger;
        _nopFileProvider = nopFileProvider;
        _googleShoppingSettings = googleShoppingSettings;
        _webHostEnvironment = webHostEnvironment;
    }
    #endregion

    #region Methods
    public async Task ExecuteAsync()
    {
        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();

        try
        {
            // Plugin
            var pluginDescriptor = await _pluginService.GetPluginDescriptorBySystemNameAsync<IPlugin>("PromotionFeed.GoogleShopping");
            if (pluginDescriptor == null || pluginDescriptor.Instance<IPlugin>() is not GoogleShoppingService plugin)
                throw new Exception(await _localizationService.GetResourceAsync("Plugins.Feed.GoogleShopping.ExceptionLoadPlugin"));

            var stores = new List<Store>();
            var storeById = await _storeService.GetStoreByIdAsync(storeScope);
            if (storeScope > 0)
                stores.Add(storeById);
            else
                stores.AddRange(await _storeService.GetAllStoresAsync());

            var result = false;
            foreach (var store in stores)
                result = await plugin.GenerateStaticFileAsync(store);

            if (result)
                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugins.Feed.GoogleShopping.SuccessResult"));
        }
        catch (Exception exc)
        {
            var stores = new List<Store>();
            var storeById = await _storeService.GetStoreByIdAsync(storeScope);
            if (storeScope > 0)
                stores.Add(storeById);
            else
                stores.AddRange(await _storeService.GetAllStoresAsync());

            var fileCounter = _googleShoppingSettings.FileCounter;
            foreach (var store in stores)
            {
                var baseFilePath = _nopFileProvider.Combine(_webHostEnvironment.WebRootPath, "files", "exportimport", "googleFeedXmls", $"{store.Name}");
                var filePath = $"{baseFilePath}_part_{fileCounter}_{_googleShoppingSettings.StaticFileName}";

                if (File.Exists(filePath))
                    File.Delete(filePath);
            }

            _notificationService.ErrorNotification(exc.Message);
            await _logger.ErrorAsync(exc.Message, exc);
        }
    }
    #endregion
}
