using Nop.Core;
using Nop.Core.Domain.Cms;
using Nop.Plugin.Widgets.PreviouslyPurchased.Components;
using Nop.Plugin.Widgets.PreviouslyPurchased.Domain;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Web.Framework.Infrastructure;

namespace Nop.Plugin.Widgets.PreviouslyPurchased;

/// <summary>
/// Represents the 'Previously Purchased Products' plugin
/// </summary>
public class PreviouslyPurchasedPlugin : BasePlugin, IWidgetPlugin
{
    #region Fields
    private readonly ILocalizationService _localizationService;
    private readonly ISettingService _settingService;
    private readonly IWebHelper _webHelper;
    private readonly WidgetSettings _widgetSettings;
    #endregion

    #region Ctor
    public PreviouslyPurchasedPlugin(
        ILocalizationService localizationService,
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
    /// Gets the widget zones where this widget should be rendered.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains a list of widget zones.
    /// </returns>
    public Task<IList<string>> GetWidgetZonesAsync()
    {
        return Task.FromResult<IList<string>>(new List<string>
        {
            PublicWidgetZones.LeftSideColumnAfter,
            PublicWidgetZones.AccountNavigationAfter
        });
    }

    /// <summary>
    /// Gets the URL of the configuration page for this plugin.
    /// </summary>
    public override string GetConfigurationPageUrl()
    {
        return _webHelper.GetStoreLocation() + "Admin/PreviouslyPurchased/Configure";
    }

    /// <summary>
    /// Gets the view component type for rendering the widget.
    /// </summary>
    /// <param name="widgetZone">The name of the widget zone.</param>
    /// <returns>View component type to render the widget in the specified zone.</returns>
    public Type GetWidgetViewComponent(string widgetZone)
    {
        if (widgetZone.Equals(PublicWidgetZones.LeftSideColumnAfter))
            return typeof(PreviouslyPurchasedViewComponent);
        else
            return typeof(PreviouslyPurchasedNavigationViewComponent);
    }

    /// <summary>
    /// Installs the plugin and performs any necessary setup.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public override async Task InstallAsync()
    {
        // Save plugin settings (default values)
        await _settingService.SaveSettingAsync(new PreviouslyPurchasedSettings
        {
            ProductsType = (int)PreviouslyPurchasedType.LastPurchase,
            PreviouslyPurchasedProductsNumber = 5,
            WidgetZone = PublicWidgetZones.LeftSideColumnAfter
        });

        // Ensure the widget is added to active widgets
        if (!_widgetSettings.ActiveWidgetSystemNames.Contains(PreviouslyPurchasedDefaults.SystemName))
        {
            _widgetSettings.ActiveWidgetSystemNames.Add(PreviouslyPurchasedDefaults.SystemName);
            await _settingService.SaveSettingAsync(_widgetSettings);
        }

        // Add or update locale resources for localization support
        await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
        {
            ["Plugins.Widgets.PreviouslyPurchased"] = "Previously bought",
            ["Plugins.Widgets.PreviouslyPurchased.Title"] = "Previously bought products",
            ["Plugins.Widgets.PreviouslyPurchased.Fields.Enable"] = "Enable",
            ["Plugins.Widgets.PreviouslyPurchased.Fields.Enable.Hint"] = "Check to allow customers to use the 'Previously Bought products' feature in your store.",
            ["Plugins.Widgets.PreviouslyPurchased.Fields.ProductsType"] = "Type of 'Previously Bought products'",
            ["Plugins.Widgets.PreviouslyPurchased.Fields.ProductsType.Hint"] = "Select type of products to show.",
            ["Plugins.Widgets.PreviouslyPurchased.Fields.PreviouslyPurchasedProductsNumber"] = "Number of 'Previously Bought products'",
            ["Plugins.Widgets.PreviouslyPurchased.Fields.PreviouslyPurchasedProductsNumber.Hint"] = "The number of 'Previously Bought products' to display when 'Previously Bought products' option is enabled.",
            ["Plugins.Widgets.PreviouslyPurchased.Fields.MyAccountNavigation"] = "Navigation on my account",
            ["Plugins.Widgets.PreviouslyPurchased.Fields.MyAccountNavigation.Hint"] = "Check to allow customers to add link 'Previously Bought products' in My account navigation.",
        });

        await base.InstallAsync();
    }

    /// <summary>
    /// Uninstalls the plugin and cleans up settings.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public override async Task UninstallAsync()
    {
        // Delete plugin settings
        await _settingService.DeleteSettingAsync<PreviouslyPurchasedSettings>();

        // Remove widget from active widgets list
        if (_widgetSettings.ActiveWidgetSystemNames.Contains(PreviouslyPurchasedDefaults.SystemName))
            _widgetSettings.ActiveWidgetSystemNames.Remove(PreviouslyPurchasedDefaults.SystemName);

        // Delete locale resources for the plugin
        await _localizationService.DeleteLocaleResourcesAsync("Plugins.Widgets.PreviouslyPurchased");

        await base.UninstallAsync();
    }
    #endregion

    #region Properties
    /// <summary>
    /// Gets a value indicating whether to hide this plugin 
    /// on the widget list page in the admin area.
    /// </summary>
    public bool HideInWidgetList => false;
    #endregion
}