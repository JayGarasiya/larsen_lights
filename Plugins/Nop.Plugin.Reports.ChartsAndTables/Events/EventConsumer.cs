using Nop.Core;
using Nop.Core.Domain.Cms;
using Nop.Plugin.Reports.ChartsAndTables.Services;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Web.Framework.Events;
using Nop.Web.Framework.Menu;

namespace Nop.Plugin.Reports.ChartsAndTables.Events
{
    public class EventConsumer : BaseAdminMenuCreatedEventConsumer
    {
        protected readonly IStoreContext _storeContext;
        protected readonly ISettingService _settingService;
        protected readonly ICustomerService _customerService;
        protected readonly IWorkContext _workContext;
        protected readonly ILocalizationService _localizationService;
        protected readonly IAdminMenu _adminMenu;
        protected readonly IChartsAndTablesServices _chartsAndTablesServices;
        protected readonly WidgetSettings _widgetSettings;

        public EventConsumer(IPluginManager<IPlugin> pluginManager,
            IStoreContext storeContext,
            ISettingService settingService,
            ICustomerService customerService,
            IWorkContext workContext,
            ILocalizationService localizationService,
            IAdminMenu adminMenu,
            IChartsAndTablesServices chartsAndTablesServices,
            WidgetSettings widgetSettings) : base(pluginManager)
        {
            _storeContext = storeContext;
            _settingService = settingService;
            _customerService = customerService;
            _workContext = workContext;
            _localizationService = localizationService;
            _adminMenu = adminMenu;
            _chartsAndTablesServices = chartsAndTablesServices;
            _widgetSettings = widgetSettings;
        }

        protected override string PluginSystemName => ChartsAndTablesDefaults.SystemName;

        /// <summary>
        /// The system name of the menu item after with need to insert the current one
        /// </summary>
        protected override string AfterMenuSystemName => "Third party plugins";

        /// <summary>
        /// The system name of the menu item before with need to insert the current one
        /// </summary>
        protected override string BeforeMenuSystemName => "Third party plugins";

        /// <summary>
        /// Menu item insertion type (by default: <see cref="MenuItemInsertType.TryAfterThanBefore"/>)
        /// </summary>
        protected override MenuItemInsertType InsertType => MenuItemInsertType.TryAfterThanBefore;

        public override async Task HandleEventAsync(AdminMenuCreatedEvent eventMessage)
        {
            var plugin = await _pluginManager.LoadPluginBySystemNameAsync(PluginSystemName);
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var chartsAndTablesSettings = await _settingService.LoadSettingAsync<ChartsAndTablesSettings>(storeScope);
            var responseCode = await _chartsAndTablesServices.CheckLicenseKeyValidAsync(chartsAndTablesSettings.LicenseKey);
            if (responseCode == 100 && _widgetSettings.ActiveWidgetSystemNames.Contains(ChartsAndTablesDefaults.SystemName))
            {
                if (string.IsNullOrEmpty(chartsAndTablesSettings.LicenseKey))
                    return;

                var newItem = await GetAdminMenuItemAsync(plugin);

                if (newItem == null)
                    return;

                var parentMenuItem = eventMessage.RootMenuItem.ChildNodes.FirstOrDefault(x => x.ContainsSystemName(AfterMenuSystemName));
                if (parentMenuItem == null)
                {
                    parentMenuItem = new AdminMenuItem()
                    {
                        SystemName = AfterMenuSystemName,
                        Title = await _localizationService.GetResourceAsync(AfterMenuSystemName),
                        Visible = true,
                        IconClass = "fa fa-bars",
                    };
                    if (!string.IsNullOrEmpty(chartsAndTablesSettings.LicenseKey))
                        parentMenuItem.ChildNodes.Add(newItem);

                    switch (InsertType)
                    {
                        case MenuItemInsertType.After:
                            eventMessage.RootMenuItem.InsertAfter(AfterMenuSystemName, parentMenuItem);
                            break;
                        case MenuItemInsertType.Before:
                            eventMessage.RootMenuItem.InsertBefore(BeforeMenuSystemName, parentMenuItem);
                            break;
                        case MenuItemInsertType.TryAfterThanBefore:
                            if (!eventMessage.RootMenuItem.InsertAfter(AfterMenuSystemName, parentMenuItem))
                                eventMessage.RootMenuItem.InsertBefore(BeforeMenuSystemName, parentMenuItem);
                            break;
                        case MenuItemInsertType.TryBeforeThanAfter:
                            if (!eventMessage.RootMenuItem.InsertBefore(BeforeMenuSystemName, parentMenuItem))
                                eventMessage.RootMenuItem.InsertAfter(AfterMenuSystemName, parentMenuItem);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(chartsAndTablesSettings.LicenseKey))
                        parentMenuItem.ChildNodes.Add(newItem);
                }
                newItem = parentMenuItem;

            }
        }
        protected override async Task<AdminMenuItem> GetAdminMenuItemAsync(IPlugin plugin)
        {           
             var menuItem = new AdminMenuItem()
             {
                 SystemName = "nopCommercePlus",
                 Title = "nopCommercePlus",
                 IconClass = "fas fa-signature",
                 Visible = true
             };
             menuItem.ChildNodes.Add(new AdminMenuItem()
             {
                 SystemName = "Plugins.Reports.ChartsAndTables",
                 Title = await _localizationService.GetResourceAsync("Plugins.Reports.ChartsAndTables"),
                 IconClass = "fas fa-chart-bar",
                 Visible = true
             });

             menuItem.ChildNodes.Add(new AdminMenuItem()
             {
                 SystemName = "Plugins.Reports.ChartsAndTables.Configuration",
                 Title = await _localizationService.GetResourceAsync("Plugins.Reports.ChartsAndTables.Configuration"),
                 Visible = true,
                 IconClass = "fas fa-cogs",
                 Url = _adminMenu.GetMenuItemUrl("ChartsAndTables", "Configure"),
             });

             menuItem.ChildNodes.Add(new AdminMenuItem()
             {
                 SystemName = "Plugins.Reports.ChartsAndTables.Dashboard",
                 Title = await _localizationService.GetResourceAsync("Plugins.Reports.ChartsAndTables.Dashboard"),
                 Visible = true,
                 IconClass = "fas fa-tachometer-alt",
                 Url = _adminMenu.GetMenuItemUrl("ChartsAndTablesMenu", "ReportMenuDashboard"),
             });

             menuItem.ChildNodes.Add(new AdminMenuItem()
             {
                 SystemName = "Plugins.Reports.ChartsAndTables.Products",
                 Title = await _localizationService.GetResourceAsync("Plugins.Reports.ChartsAndTables.Products"),
                 Visible = true,
                 IconClass = "fab fa-product-hunt",
                 Url = _adminMenu.GetMenuItemUrl("ChartsAndTablesMenu", "ReportMenuProducts"),
             });
             menuItem.ChildNodes.Add(new AdminMenuItem()
             {
                 SystemName = "Plugins.Reports.ChartsAndTables.Customers",
                 Title = await _localizationService.GetResourceAsync("Plugins.Reports.ChartsAndTables.Customers"),
                 Visible = true,
                 IconClass = "far fa-user",
                 Url = _adminMenu.GetMenuItemUrl("ChartsAndTablesMenu", "ReportMenuCustomers"),
             });
             menuItem.ChildNodes.Add(new AdminMenuItem()
             {
                  SystemName = "Plugins.Reports.ChartsAndTables.Orders",
                  Title = await _localizationService.GetResourceAsync("Plugins.Reports.ChartsAndTables.Orders"),
                  Visible = true,
                  IconClass = "fas fa-shopping-cart",
                  Url = _adminMenu.GetMenuItemUrl("ChartsAndTablesMenu", "ReportMenuOrders"),
             });
             menuItem.ChildNodes.Add(new AdminMenuItem()
             {
                  SystemName = "Plugins.Reports.ChartsAndTables.Table",
                  Title = await _localizationService.GetResourceAsync("Plugins.Reports.ChartsAndTables.Table"),
                  Visible = true,
                  IconClass = "fas fa-table",
                  Url = _adminMenu.GetMenuItemUrl("ChartsAndTablesMenu", "ReportMenuTable"),
             });
            return menuItem;
        }
    }
}
