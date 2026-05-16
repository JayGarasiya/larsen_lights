using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Web.Framework.Events;
using Nop.Web.Framework.Menu;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Misc.BulkEdit.Event;

/// <summary>
/// Handles events related to the admin menu, adding Bulk Edit menu items to the admin panel.
/// </summary>
public class EventConsumer : IConsumer<AdminMenuCreatedEvent>
{
    #region Fields
    private readonly ILocalizationService _localizationService;
    #endregion

    #region Ctor
    public EventConsumer(ILocalizationService localizationService)
    {
        _localizationService = localizationService;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Handles the event when the admin menu is created. 
    /// Adds the Bulk Edit plugin menu item under the "Catalog" menu.
    /// </summary>
    /// <param name="eventMessage">The event data containing the root menu item to be modified.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]  // Ensures the user has the required permission
    public async Task HandleEventAsync(AdminMenuCreatedEvent eventMessage)
    {
        // Create a new admin menu item for Bulk Edit plugin
        var adminMenuItem = new AdminMenuItem
        {
            SystemName = BulkEditDefaults.SystemName,
            Title = await _localizationService.GetResourceAsync("Plugins.Misc.BulkEdit.Title"),
            Url = eventMessage.GetMenuItemUrl("Product", "BulkEdit"),
            IconClass = "far fa-dot-circle",
            Visible = true,
        };

        // Find the "Catalog" menu to add the Bulk Edit menu item to it
        var customerMenu = eventMessage.RootMenuItem.GetItemBySystemName("Catalog");

        // Add the Bulk Edit menu item to the "Catalog" menu if found, otherwise add it directly to the root
        if (customerMenu != null)
            customerMenu.ChildNodes.Add(adminMenuItem);
        else
            eventMessage.RootMenuItem.ChildNodes.Add(adminMenuItem);
    }
    #endregion
}
