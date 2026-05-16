using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Cms;
using Nop.Plugin.Widgets.PreviouslyPurchased.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Widgets.PreviouslyPurchased.Controllers;

public class PreviouslyPurchasedController : BaseAdminController
{
    #region Fields
    private readonly ILocalizationService _localizationService;
    private readonly INotificationService _notificationService;
    private readonly ISettingService _settingService;
    private readonly IStoreContext _storeContext;
    #endregion

    #region Ctor
    public PreviouslyPurchasedController(
        ILocalizationService localizationService,
        INotificationService notificationService,
        ISettingService settingService,
        IStoreContext storeContext)
    {
        _localizationService = localizationService;
        _notificationService = notificationService;
        _settingService = settingService;
        _storeContext = storeContext;
    }
    #endregion

    #region Methods
    [CheckPermission(StandardPermission.Configuration.MANAGE_WIDGETS)]
    public async Task<IActionResult> Configure()
    {
        // Get the active store's configuration scope
        var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();

        // Load current settings for PreviouslyPurchased and Widget
        var settings = await _settingService.LoadSettingAsync<PreviouslyPurchasedSettings>(storeId);
        var widgetSettings = await _settingService.LoadSettingAsync<WidgetSettings>(storeId);

        // Create a model with the current settings to display in the configuration page
        var model = new ConfigurationModel
        {
            Enabled = widgetSettings.ActiveWidgetSystemNames.Contains(PreviouslyPurchasedDefaults.SystemName),
            ProductsType = settings.ProductsType,
            PreviouslyPurchasedProductsNumber = settings.PreviouslyPurchasedProductsNumber,
            MyAccountNavigation = settings.MyAccountNavigation,
            ActiveStoreScopeConfiguration = storeId
        };

        // Check if any settings are overridden for the current store
        if (storeId > 0)
        {
            model.Enabled_OverrideForStore = await _settingService.SettingExistsAsync(widgetSettings, setting => setting.ActiveWidgetSystemNames, storeId);
            model.ProductsType_OverrideForStore = await _settingService.SettingExistsAsync(settings, setting => setting.ProductsType, storeId);
            model.PreviouslyPurchasedProductsNumber_OverrideForStore = await _settingService.SettingExistsAsync(settings, setting => setting.PreviouslyPurchasedProductsNumber, storeId);
            model.MyAccountNavigation_OverrideForStore = await _settingService.SettingExistsAsync(settings, setting => setting.MyAccountNavigation, storeId);
        }

        return View("~/Plugins/Widgets.PreviouslyPurchased/Views/Configure.cshtml", model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Configuration.MANAGE_WIDGETS)]
    public async Task<IActionResult> Configure(ConfigurationModel model)
    {
        // If the model state is invalid, reload the configuration page with current values
        if (!ModelState.IsValid)
            return await Configure();

        // Get the active store's configuration scope
        var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();

        // Load current settings for PreviouslyPurchased and Widget
        var settings = await _settingService.LoadSettingAsync<PreviouslyPurchasedSettings>(storeId);
        var widgetSettings = await _settingService.LoadSettingAsync<WidgetSettings>(storeId);

        // Update settings with the values from the form
        settings.ProductsType = model.ProductsType;
        settings.PreviouslyPurchasedProductsNumber = model.PreviouslyPurchasedProductsNumber;
        settings.MyAccountNavigation = model.MyAccountNavigation;

        // Save the updated settings for the store, considering overrides
        await _settingService.SaveSettingOverridablePerStoreAsync(settings, setting => setting.ProductsType, model.ProductsType_OverrideForStore, storeId, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(settings, setting => setting.PreviouslyPurchasedProductsNumber, model.PreviouslyPurchasedProductsNumber_OverrideForStore, storeId, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(settings, setting => setting.MyAccountNavigation, model.MyAccountNavigation_OverrideForStore, storeId, false);

        // Enable/Disable the widget based on the form input
        if (model.Enabled && !widgetSettings.ActiveWidgetSystemNames.Contains(PreviouslyPurchasedDefaults.SystemName))
            widgetSettings.ActiveWidgetSystemNames.Add(PreviouslyPurchasedDefaults.SystemName);
        if (!model.Enabled && widgetSettings.ActiveWidgetSystemNames.Contains(PreviouslyPurchasedDefaults.SystemName))
            widgetSettings.ActiveWidgetSystemNames.Remove(PreviouslyPurchasedDefaults.SystemName);

        // Save the updated widget settings, considering overrides
        await _settingService.SaveSettingOverridablePerStoreAsync(widgetSettings, setting => setting.ActiveWidgetSystemNames, model.Enabled_OverrideForStore, storeId, false);

        // Clear the cache to apply changes
        await _settingService.ClearCacheAsync();

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

        return await Configure();
    }
    #endregion
}
