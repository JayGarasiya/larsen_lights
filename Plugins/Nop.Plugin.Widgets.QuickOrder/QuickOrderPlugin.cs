using Nop.Core;
using Nop.Core.Domain.Cms;
using Nop.Plugin.Widgets.QuickOrder.Components;
using Nop.Services.Cms;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Web.Framework.Infrastructure;

namespace Nop.Plugin.Widgets.QuickOrder;

/// <summary>
/// Represents the quick order plugin
/// </summary>
public class QuickOrderPlugin : BasePlugin, IWidgetPlugin
{
    #region Fields

    protected readonly ILocalizationService _localizationService;
    protected readonly IWebHelper _webHelper;
    protected readonly WidgetSettings _widgetSettings;

    #endregion

    #region Ctor
    public QuickOrderPlugin(
        ILocalizationService localizationService,
        IWebHelper webHelper,
        WidgetSettings widgetSettings)
    {
        _localizationService = localizationService;
        _webHelper = webHelper;
        _widgetSettings = widgetSettings;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Gets widget zones where this widget should be rendered.
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous operation. 
    /// The task result contains the widget zones.
    /// </returns>
    public Task<IList<string>> GetWidgetZonesAsync()
    {
        // Returns the list of widget zones where the widget should be rendered
        return Task.FromResult<IList<string>>(new List<string> { PublicWidgetZones.AdminHeaderLinksAfter });
    }

    /// <summary>
    /// Gets the URL for the configuration page of the plugin.
    /// </summary>
    /// <returns>
    /// The URL of the plugin's configuration page.
    /// </returns>
    public override string GetConfigurationPageUrl()
    {
        return _webHelper.GetStoreLocation() + "Admin/QuickOrder/Configure";
    }

    /// <summary>
    /// Gets the view component type to render the widget.
    /// </summary>
    /// <param name="widgetZone">The widget zone where the widget will be displayed</param>
    /// <returns>View component type</returns>
    public Type GetWidgetViewComponent(string widgetZone)
    {
        // Returns the view component that will be used to render the widget in the specified widget zone
        return typeof(QuickOrderLinkViewComponent);
    }

    /// <summary>
    /// Installs the plugin by adding necessary localization resources.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public override async Task InstallAsync()
    {
        // Add necessary localization resources for the plugin
        await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
        {
            ["Plugins.Widgets.QuickOrder.Title"] = "Quick order",
            ["Plugins.Widgets.QuickOrder.Fields.Enable"] = "Enable",
            ["Plugins.Widgets.QuickOrder.Fields.Enable.Hint"] = "Check to allow store owner to use the 'Quick Order extension' feature in your store.",
            ["Plugins.Widgets.QuickOrder.List.CustomOrderNumber"] = "Order #",
            ["Plugins.Widgets.QuickOrder.List.CustomOrderNumber.Hint"] = "Search by a specific custom order number.",
        });

        // Call base method for installation
        await base.InstallAsync();
    }

    /// <summary>
    /// Uninstalls the plugin by removing widget settings and localization resources.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public override async Task UninstallAsync()
    {
        // Remove the widget system name from active widget settings
        _widgetSettings.ActiveWidgetSystemNames.Remove("Widgets.QuickOrder");

        // Delete all localization resources associated with the plugin
        await _localizationService.DeleteLocaleResourcesAsync("Plugins.Widgets.QuickOrder");

        // Call base method for uninstallation
        await base.UninstallAsync();
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets a value indicating whether this plugin should be hidden from the widget list page in the admin area.
    /// </summary>
    public bool HideInWidgetList => false;

    #endregion
}
