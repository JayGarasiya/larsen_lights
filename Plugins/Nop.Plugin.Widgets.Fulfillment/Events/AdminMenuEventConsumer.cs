using Nop.Core.Domain.Cms;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Web.Framework.Events;
using Nop.Web.Framework.Menu;

namespace Nop.Plugin.Widgets.Fulfillment.Events
{
    /// <summary>
    /// Represents plugin event consumer
    /// </summary>
    public class AdminMenuEventConsumer : IConsumer<AdminMenuCreatedEvent>
    {
        #region Fields

        protected readonly ILocalizationService _localizationService;
        protected readonly IAdminMenu _adminMenu;
        protected readonly WidgetSettings _widgetSettings;

        #endregion

        #region Ctor

        public AdminMenuEventConsumer(
            ILocalizationService localizationService,
            IAdminMenu adminMenu,
            WidgetSettings widgetSettings)
        {
            _localizationService = localizationService;
            _adminMenu = adminMenu;
            _widgetSettings = widgetSettings;
        }

        #endregion

        #region Sitemap

        /// <summary>
        /// Handle admin menu created event
        /// </summary>
        /// <param name="eventMessage">Event message</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(AdminMenuCreatedEvent eventMessage)
        {
            if (_widgetSettings.ActiveWidgetSystemNames.Contains(FulfillmentDefaults.SystemName))
            {
                var pluginNode = new AdminMenuItem()
                {
                    SystemName = "Fulfillment.3PL",
                    Title = await _localizationService.GetResourceAsync("Plugins.Widgets.Fulfillment"),
                    IconClass = "far fa-dot-circle",
                    Visible = true,
                    PermissionNames = new List<string> { StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE }
                };

                var shippingMethodNode = new AdminMenuItem()
                {
                    SystemName = "Fulfillment.3PLCarriers",
                    Title = await _localizationService.GetResourceAsync("Plugins.Widgets.Fulfillment.Fields.Carriers"),
                    Url = _adminMenu.GetMenuItemUrl("Fulfillment", "ThreePlShippingMethods"),
                    IconClass = "far fa-circle",
                    Visible = true,
                    PermissionNames = new List<string> { StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE }
                };

                var logNode = new AdminMenuItem()
                {
                    SystemName = "Fulfillment.ThreePlRecords",
                    Title = await _localizationService.GetResourceAsync("Plugins.Widgets.Fulfillment.Fields.Logs"),
                    Url = _adminMenu.GetMenuItemUrl("Fulfillment", "ThreePlRecords"),
                    IconClass = "far fa-circle",
                    Visible = true,
                    PermissionNames = new List<string> { StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE }
                };

                pluginNode.ChildNodes.Add(shippingMethodNode);
                pluginNode.ChildNodes.Add(logNode);

                var mainMenuNode = eventMessage.RootMenuItem.ChildNodes.FirstOrDefault(x => x.SystemName == "Fulfillment.3PL");
                if (mainMenuNode != null)
                {
                    mainMenuNode.ChildNodes.Add(shippingMethodNode);
                    mainMenuNode.ChildNodes.Add(logNode);
                }
                else
                {
                    var salesMap = eventMessage.RootMenuItem.ChildNodes.FirstOrDefault(x => x.SystemName.Equals("Sales"));
                    var orderMap = salesMap.ChildNodes.FirstOrDefault(x => x.SystemName.Equals("Orders"));
                    var orderIndex = salesMap.ChildNodes.IndexOf(orderMap);
                    salesMap.ChildNodes.Insert(orderIndex + 1, pluginNode);
                }
            }
        }
        #endregion
    }
}
