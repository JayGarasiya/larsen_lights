using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Web.Framework.Events;
using Nop.Web.Framework.Menu;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Widgets.QuickOrder.Services;

/// <summary>
/// Handles events related to the admin menu, adding Quick Order menu items to the admin panel.
/// </summary>
public class EventConsumer : IConsumer<AdminMenuCreatedEvent>
{
    #region Fields

    protected readonly ILocalizationService _localizationService;

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
    /// Adds the Quick Order plugin menu item under the "Customers" menu.
    /// </summary>
    /// <param name="eventMessage">The event data containing the root menu item to be modified.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
    public async Task HandleEventAsync(AdminMenuCreatedEvent eventMessage)
    {
        // Create a new admin menu item for Quick Order plugin
        var adminMenuItem = new AdminMenuItem
        {
            SystemName = "Widgets.QuickOrder",
            Title = await _localizationService.GetResourceAsync("Plugins.Widgets.QuickOrder.Title"),
            Url = eventMessage.GetMenuItemUrl("QuickOrder", "List"),
            IconClass = "far fa-dot-circle",
            Visible = true,
        };

        // Find the "Customers" menu to add the Quick Order menu item to it
        var customerMenu = eventMessage.RootMenuItem.GetItemBySystemName("Customers");

        // Add the Quick Order menu item to the "Customers" menu if found, otherwise add it directly to the root
        if (customerMenu != null)
            customerMenu.ChildNodes.Add(adminMenuItem);
        else
            eventMessage.RootMenuItem.ChildNodes.Add(adminMenuItem);
    }

    #endregion
}
