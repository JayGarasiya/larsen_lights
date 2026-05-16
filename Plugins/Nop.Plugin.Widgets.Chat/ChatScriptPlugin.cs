using Nop.Core;
using Nop.Core.Domain.Cms;
using Nop.Plugin.Widgets.Chat.Components;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Web.Framework.Infrastructure;

namespace Nop.Plugin.Widgets.Chat;

/// <summary>
/// Represents the plugin implementation
/// </summary>
public class ChatScriptPlugin : BasePlugin, IWidgetPlugin
{
    #region Fields
    private readonly ILocalizationService _localizationService;
    private readonly ISettingService _settingService;
    private readonly IWebHelper _webHelper;
    private readonly WidgetSettings _widgetSettings;
    #endregion

    #region Ctor
    public ChatScriptPlugin(ILocalizationService localizationService,
        ISettingService settingService,
        IWebHelper webHelper,
        WidgetSettings widgetSettings)
    {
        _localizationService = localizationService;
        _settingService = settingService;
        _webHelper = webHelper;
        _widgetSettings = widgetSettings;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Gets a configuration page URL
    /// </summary>
    public override string GetConfigurationPageUrl()
    {
        return $"{_webHelper.GetStoreLocation()}Admin/ChatScript/Configure";
    }

    /// <summary>
    /// Gets widget zones where this widget should be rendered
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the widget zones
    /// </returns>
    public Task<IList<string>> GetWidgetZonesAsync()
    {
        return Task.FromResult<IList<string>>(new List<string> { PublicWidgetZones.HeadHtmlTag, PublicWidgetZones.BodyEndHtmlTagBefore });
    }

    /// <summary>
    /// Gets a name of a view component for displaying widget
    /// </summary>
    /// <param name="widgetZone">Name of the widget zone</param>
    /// <returns>View component name</returns>
    public Type GetWidgetViewComponent(string widgetZone)
    {
        return typeof(ChatScriptViewComponent);
    }

    /// <summary>
    /// Install plugin
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    public override async Task InstallAsync()
    {
        // Save the default settings for the plugin
        await _settingService.SaveSettingAsync(new ChatScriptSettings
        {
            HeadMetaTag = @"<meta name=""referrer"" content=""no-referrer-when-downgrade"">"
        });

        // Add or update localization resources (translations for UI strings)
        await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
        {
            ["Plugins.Widgets.Chat.Fields.Enabled"] = "Enable",
            ["Plugins.Widgets.Chat.Fields.Enabled.Hint"] = "Check to activate this widget.",
            ["Plugins.Widgets.Chat.Fields.Script"] = "Widget script",
            ["Plugins.Widgets.Chat.Fields.Script.Hint"] = "Find your unique widget script on the Administration tab in your account and then copy it into this field.",
            ["Plugins.Widgets.Chat.Fields.Script.Required"] = "Widget script is required",
            ["Plugins.Widgets.Chat.Fields.HeadMetaTag"] = "<meta> tag",
            ["Plugins.Widgets.Chat.Fields.HeadMetaTag.Hint"] = "Meta tag to allow loading script.",
        });

        // Call the base class installation logic
        await base.InstallAsync();
    }

    /// <summary>
    /// Uninstall plugin
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    public override async Task UninstallAsync()
    {
        // Remove plugin settings
        await _settingService.DeleteSettingAsync<ChatScriptSettings>();

        // Remove widget system name from active widgets
        _widgetSettings.ActiveWidgetSystemNames.Remove(ChatScriptDefaults.SystemName);

        // Delete localization resources associated with this plugin
        await _localizationService.DeleteLocaleResourcesAsync("Plugins.Widgets.Chat");

        // Call the base class uninstall logic
        await base.UninstallAsync();
    }
    #endregion

    #region Properties
    /// <summary>
    /// Gets a value indicating whether to hide this plugin on the widget list page in the admin area
    /// </summary>
    public bool HideInWidgetList => false;
    #endregion
}
