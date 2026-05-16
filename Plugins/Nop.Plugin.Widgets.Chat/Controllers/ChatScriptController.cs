using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Cms;
using Nop.Plugin.Widgets.Chat.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Widgets.Chat.Controllers;

public class ChatScriptController : BaseAdminController
{
    #region Fields
    private readonly ILocalizationService _localizationService;
    private readonly INotificationService _notificationService;
    private readonly ISettingService _settingService;
    private readonly IStoreContext _storeContext;
    #endregion

    #region Ctor
    public ChatScriptController(
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
        // Get the current store's scope for configuration settings
        var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();

        // Load the ChatScriptSettings and WidgetSettings for the current store
        var settings = await _settingService.LoadSettingAsync<ChatScriptSettings>(storeId);
        var widgetSettings = await _settingService.LoadSettingAsync<WidgetSettings>(storeId);

        // Populate the model with the necessary data to configure the chat script
        var model = new ConfigurationModel
        {
            Script = settings.Script,
            Enabled = widgetSettings.ActiveWidgetSystemNames.Contains(ChatScriptDefaults.SystemName),
            HeadMetaTag = settings.HeadMetaTag,
            ActiveStoreScopeConfiguration = storeId
        };

        // Check if there are overrides for store-specific settings
        if (storeId > 0)
        {
            model.Script_OverrideForStore = await _settingService.SettingExistsAsync(settings, setting => setting.Script, storeId);
            model.Enabled_OverrideForStore = await _settingService.SettingExistsAsync(widgetSettings, setting => setting.ActiveWidgetSystemNames, storeId);
        }

        return View("~/Plugins/Widgets.Chat/Views/Configure.cshtml", model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Configuration.MANAGE_WIDGETS)]
    public async Task<IActionResult> Configure(ConfigurationModel model)
    {
        if (!ModelState.IsValid)
            return await Configure();

        // Get the current store's scope for configuration settings
        var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();

        // Load the current settings for the chat script and widgets
        var settings = await _settingService.LoadSettingAsync<ChatScriptSettings>(storeId);
        var widgetSettings = await _settingService.LoadSettingAsync<WidgetSettings>(storeId);

        settings.Script = model.Script;
        await _settingService.SaveSettingOverridablePerStoreAsync(settings, setting => setting.Script, model.Script_OverrideForStore, storeId, false);

        // If the widget is enabled and not already in the active list, add it
        if (model.Enabled && !widgetSettings.ActiveWidgetSystemNames.Contains(ChatScriptDefaults.SystemName))
            widgetSettings.ActiveWidgetSystemNames.Add(ChatScriptDefaults.SystemName);

        // If the widget is disabled and currently active, remove it from the active list
        if (!model.Enabled && widgetSettings.ActiveWidgetSystemNames.Contains(ChatScriptDefaults.SystemName))
            widgetSettings.ActiveWidgetSystemNames.Remove(ChatScriptDefaults.SystemName);

        await _settingService.SaveSettingOverridablePerStoreAsync(widgetSettings, setting => setting.ActiveWidgetSystemNames, model.Enabled_OverrideForStore, storeId, false);

        await _settingService.ClearCacheAsync();

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

        return await Configure();
    }
    #endregion
}
