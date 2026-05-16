using Nop.Core;
using Nop.Core.Domain.Cms;
using Nop.Core.Domain.Localization;
using Nop.Data;
using Nop.Plugin.Widgets.Fulfillment.Components;
using Nop.Services.Cms;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Plugins;
using Nop.Services.ScheduleTasks;
using Nop.Services.Security;
using Nop.Web.Framework.Infrastructure;

namespace Nop.Plugin.Widgets.Fulfillment
{
    /// <summary>
    /// Represents the 3PL Central plugin
    /// </summary>
    public class FulfillmentProcessor : BasePlugin, IWidgetPlugin
    {
        #region Fields

        protected readonly ISettingService _settingService;
        protected readonly ILocalizationService _localizationService;
        protected readonly IWebHelper _webHelper;
        protected readonly WidgetSettings _widgetSettings;
        protected readonly IScheduleTaskService _scheduleTaskService;
        protected readonly IPermissionService _permissionService;
        protected readonly IRepository<LocaleStringResource> _lsrRepository;
        protected readonly INopDataProvider _dataProvider;
        protected readonly IGenericAttributeService _genericAttributeService;
        protected readonly IOrderService _orderService;

        #endregion

        #region Ctor

        public FulfillmentProcessor(ISettingService settingService,
            ILocalizationService localizationService,
            IWebHelper webHelper,
            WidgetSettings widgetSettings,
            IScheduleTaskService scheduleTaskService,
            IPermissionService permissionService,
            IRepository<LocaleStringResource> lsrRepository,
            INopDataProvider dataProvider,
            IGenericAttributeService genericAttributeService,
            IOrderService orderService)
        {
            _settingService = settingService;
            _localizationService = localizationService;
            _webHelper = webHelper;
            _widgetSettings = widgetSettings;
            _scheduleTaskService = scheduleTaskService;
            _permissionService = permissionService;
            _lsrRepository = lsrRepository;
            _dataProvider = dataProvider;
            _genericAttributeService = genericAttributeService;
            _orderService = orderService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets widget zones where this widget should be rendered
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the widget zones
        /// </returns>
        public Task<IList<string>> GetWidgetZonesAsync()
        {
            return Task.FromResult<IList<string>>(new List<string> { AdminWidgetZones.OrderDetailsButtons });
        }

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/Fulfillment/Configure";
        }

        /// <summary>
        /// Gets a name of a view component for displaying widget
        /// </summary>
        /// <param name="widgetZone">Name of the widget zone</param>
        /// <returns>View component name</returns>
        public Type GetWidgetViewComponent(string widgetZone)
        {
            return typeof(Send3plViewComponents);
        }

        /// <summary>
        /// Install plugin
        /// </summary>
        public override async Task InstallAsync()
        {
            //settings
            await _settingService.SaveSettingAsync(new Fulfillmen3PLtSettings
            {
                ClientId = "51f0ecf7-c78b-41c4-8c99-e2f3ec865349",
                ClientSecret = "FvdDjb3+djDz7I+3TXSKSoKYqDA42oz8",
                ThreePlKey = "{fd0cd3c3-94bd-4f1d-a1bf-d9c93bee1fff}",
                CustomerId = "3",
                FacilityId = "1",
                UserId = "1",
                SendOrderHoursInterval = 1
            });

            if (!_widgetSettings.ActiveWidgetSystemNames.Contains(FulfillmentDefaults.SystemName))
            {
                _widgetSettings.ActiveWidgetSystemNames.Add(FulfillmentDefaults.SystemName);
                await _settingService.SaveSettingAsync(_widgetSettings);
            }

            if (await _scheduleTaskService.GetTaskByTypeAsync(FulfillmentDefaults.FulfillmentSendTask.Type) is null)
            {
                await _scheduleTaskService.InsertTaskAsync(new()
                {
                    Enabled = false,
                    StopOnError = false,
                    LastEnabledUtc = DateTime.UtcNow,
                    Name = FulfillmentDefaults.FulfillmentSendTask.Name,
                    Type = FulfillmentDefaults.FulfillmentSendTask.Type,
                    Seconds = FulfillmentDefaults.FulfillmentSendTask.Period
                });
            }

            if (await _scheduleTaskService.GetTaskByTypeAsync(FulfillmentDefaults.FulfillmentInventoryTask.Type) is null)
            {
                await _scheduleTaskService.InsertTaskAsync(new()
                {
                    Enabled = false,
                    StopOnError = false,
                    LastEnabledUtc = DateTime.UtcNow,
                    Name = FulfillmentDefaults.FulfillmentInventoryTask.Name,
                    Type = FulfillmentDefaults.FulfillmentInventoryTask.Type,
                    Seconds = FulfillmentDefaults.FulfillmentInventoryTask.Period
                });
            }

            if (await _scheduleTaskService.GetTaskByTypeAsync(FulfillmentDefaults.FulfillmentTrackingTask.Type) is null)
            {
                await _scheduleTaskService.InsertTaskAsync(new()
                {
                    Enabled = false,
                    StopOnError = false,
                    LastEnabledUtc = DateTime.UtcNow,
                    Name = FulfillmentDefaults.FulfillmentTrackingTask.Name,
                    Type = FulfillmentDefaults.FulfillmentTrackingTask.Type,
                    Seconds = FulfillmentDefaults.FulfillmentTrackingTask.Period
                });
            }

            //locales
            await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
            {
                ["Plugins.Widgets.Fulfillment"] = "3PL WMS",
                ["Plugins.Widgets.Fulfillment.Fields.ThreePlEnable"] = "Enable",
                ["Plugins.Widgets.Fulfillment.Fields.ThreePlEnable.Hint"] = "Check for enable sync data with 3PL central warehouse",
                ["Plugins.Widgets.Fulfillment.Fields.ClientId"] = "Client Id",
                ["Plugins.Widgets.Fulfillment.Fields.ClientId.Hint"] = "Enter API Client ID.",
                ["Plugins.Widgets.Fulfillment.Fields.ClientSecret"] = "Client Secret",
                ["Plugins.Widgets.Fulfillment.Fields.ClientSecret.Hint"] = "Enter API Client Secret.",
                ["Plugins.Widgets.Fulfillment.Fields.ThreePlKey"] = "3PL key",
                ["Plugins.Widgets.Fulfillment.Fields.ThreePlKey.Hint"] = "Enter 3PL Key/GUID",
                ["Plugins.Widgets.Fulfillment.Fields.UserId"] = "User Login Id",
                ["Plugins.Widgets.Fulfillment.Fields.UserId.Hint"] = "Enter User Login Id",
                ["Plugins.Widgets.Fulfillment.Fields.CustomerId"] = "Customer Id",
                ["Plugins.Widgets.Fulfillment.Fields.CustomerId.Hint"] = "Enter Customer Id",
                ["Plugins.Widgets.Fulfillment.Fields.FacilityId"] = "Facility Id",
                ["Plugins.Widgets.Fulfillment.Fields.FacilityId.Hint"] = "Enter Facility Id",
                ["Plugins.Widgets.Fulfillment.Fields.SendOrderHoursInterval"] = "Send order interval",
                ["Plugins.Widgets.Fulfillment.Fields.SendOrderHoursInterval.Hint"] = "Configure send order interval in hour to sync order.",
                ["Plugins.Widgets.Fulfillment.Fields.MultiPackageForCountries"] = "Multi-packages for countries",
                ["Plugins.Widgets.Fulfillment.Fields.MultiPackageForCountries.Hint"] = "Choose one or several countries i.e. United States, Canada, etc., for which shipping country order like to split while sync to 3PL WMS.",
                ["Plugins.Widgets.Fulfillment.Fields.MultiPackageOrderAmountOver"] = "Multi-packages an order amount over",
                ["Plugins.Widgets.Fulfillment.Fields.MultiPackageOrderAmountOver.Hint"] = "Split an order to multi-packages when the order is sent amount over xxx.xx.",
                ["Plugins.Widgets.Fulfillment.Fields.NoSplitAmountLess"] = "Don't split a package amount less",
                ["Plugins.Widgets.Fulfillment.Fields.NoSplitAmountLess.Hint"] = "Don't split a package amount less then marge with smallest package.",

                ["Plugins.Widgets.Fulfillment.Fields.Settings.Common.BlockTitle"] = "Credentials",

                ["Plugins.Widgets.Fulfillment.Fields.Carriers"] = "3PL carriers",
                ["Plugins.Widgets.Fulfillment.Fields.ManageCarriers"] = "Manage 3PL carriers",
                ["Plugins.Widgets.Fulfillment.Fields.ManageCarriers.Title"] = "3PL carriers",
                ["Plugins.Widgets.Fulfillment.Fields.ShippingMethod"] = "Shipping method",
                ["Plugins.Widgets.Fulfillment.Fields.ShippingMethod.Hint"] = "Order shipping method name where you change carrier and service",
                ["Plugins.Widgets.Fulfillment.Fields.ShippingMethod.Required"] = "Shipping method name is required.",
                ["Plugins.Widgets.Fulfillment.Fields.ShippingMethod.NameAlreadyExists"] = "Shipping method exists with the name: {0}.",
                ["Plugins.Widgets.Fulfillment.Fields.ThreePlCarrier"] = "3PL Carrier",
                ["Plugins.Widgets.Fulfillment.Fields.ThreePlCarrier.Hint"] = "Enter 3PL Carrier name.",
                ["Plugins.Widgets.Fulfillment.Fields.ThreePlCarrier.Required"] = "3PL Carrier name is required.",
                ["Plugins.Widgets.Fulfillment.Fields.ThreePlService"] = "3PL Service",
                ["Plugins.Widgets.Fulfillment.Fields.ThreePlService.Hint"] = "Enter 3PL Service name",
                ["Plugins.Widgets.Fulfillment.Fields.ThreePlService.Required"] = "3PL Service name is required.",

                ["Plugins.Widgets.Fulfillment.Fields.Logs"] = "3PL logs",
                ["Plugins.Widgets.Fulfillment.Fields.ManageLogs"] = "Manage 3PL logs",
                ["Plugins.Widgets.Fulfillment.Fields.ManageLogs.Title"] = "3PL logs",
                ["Plugins.Widgets.Fulfillment.Fields.SearchOrderId"] = "Order #",
                ["Plugins.Widgets.Fulfillment.Fields.SearchOrderId.Hint"] = "Search by order number",
                ["Plugins.Widgets.Fulfillment.Fields.SearchThreePlOrderId"] = "3PL #",
                ["Plugins.Widgets.Fulfillment.Fields.SearchThreePlOrderId.Hint"] = "Search by 3PL order number",
                ["Plugins.Widgets.Fulfillment.Fields.SearchStatusId"] = "3PL Status",
                ["Plugins.Widgets.Fulfillment.Fields.SearchStatusId.Hint"] = "Search by 3PL status",
                ["Plugins.Widgets.Fulfillment.Fields.SearchPaidByCheck"] = "only paid by Check / Money order",
                ["Plugins.Widgets.Fulfillment.Fields.SearchPaidByCheck.Hint"] = "Search only paid by Check / Money order.",

                ["Plugins.Widgets.Fulfillment.Fields.OrderId"] = "Order #",
                ["Plugins.Widgets.Fulfillment.Fields.ThreePlOrderId"] = "3PL #",
                ["Plugins.Widgets.Fulfillment.Fields.CreationDate"] = "Created On",
                ["Plugins.Widgets.Fulfillment.Fields.CancelledDate"] = "Cancelled On",
                ["Plugins.Widgets.Fulfillment.Fields.ThreePlStutus"] = "3PL Status",
                ["Plugins.Widgets.Fulfillment.Fields.PaidByCheck"] = "Paid by Check / Money",
                ["Plugins.Widgets.Fulfillment.Fields.Note"] = "Note",
                ["Plugins.Widgets.Fulfillment.Fields.SyncRequeue"] = "Requeue / Cancel",
                ["Plugins.Widgets.Fulfillment.Fields.Sync"] = "Sync",
                ["Plugins.Widgets.Fulfillment.Fields.Requeue"] = "Requeue",

                ["Plugins.Widgets.Fulfillment.Fields.SendTo3PL"] = "Send to 3PL",
                ["Plugins.Widgets.Fulfillment.Fields.AlreadySendTo3PL"] = "Synced to 3PL",
                ["Plugins.Widgets.Fulfillment.Fields.SendTo3PLInfo"] = "Please confirm order# {0} to sync with 3PL central manually.",
                ["Plugins.Widgets.Fulfillment.Fields.SendTo3PL.Success"] = "Sync to 3PL central successfully.",
                ["Plugins.Widgets.Fulfillment.Fields.SendTo3PL.Error"] = "Failed to sync with 3PL central. For please check order notes detail.",
                ["Plugins.Widgets.Fulfillment.Fields.SendTo3PL.Error.Payment"] = "Failed to sync because only order with status processing and payment status paid allow to sync with 3PL central.",

                ["Plugins.Widgets.Fulfillment.Fields.Instructions"] = @"
                    <div class=""bg-gray-light no-margin"" style=""border-radius: .25rem;box-shadow: 0 1px 3px rgba(0,0,0,.12),0 1px 2px rgba(0,0,0,.24);padding: 15px 30px 15px 15px;border-left: 5px solid #e9ecef;margin-bottom: 1rem;"">
                        <p>For plugin configuration follow these steps:</p>
                        <ul style=""list-style-type:none;"">
                            <li>
                                1. For each client/merchant, the warehouse must contact 3PL Central support (support@3plcentral.com) and ask for the following information (can take 1-2 business days):
                                <div>
                                    <p>Can you please provide us Customer level REST API credentials for the following client(s):</p>
                                    <ul style=""list-style-type:none;"">
                                        <li><b>API credentials needed:</b></li>
                                        <li>i. Client Id</li>
                                        <li>ii. Client Secret</li>
                                        <li>iii. TPL(3PL Key/GUID)</li>
                                        <li>iv. UserLogin Id</li>
                                    </ul>
                                </div>
                            </li>
                            <li>
                                2. Fill in the remaining fields and save to complete the configuration
                            </li>
                        </ul>
                    </div>   
                <br />",
            });

            await base.InstallAsync();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override async Task UninstallAsync()
        {
            //settings
            await _settingService.DeleteSettingAsync<Fulfillmen3PLtSettings>();

            if (_widgetSettings.ActiveWidgetSystemNames.Contains(FulfillmentDefaults.SystemName))
            {
                _widgetSettings.ActiveWidgetSystemNames.Remove(FulfillmentDefaults.SystemName);
                await _settingService.SaveSettingAsync(_widgetSettings);
            }

            //schedule task
            var sendTask = await _scheduleTaskService.GetTaskByTypeAsync(FulfillmentDefaults.FulfillmentSendTask.Type);
            if (sendTask is not null)
                await _scheduleTaskService.DeleteTaskAsync(sendTask);

            var inventoryTask = await _scheduleTaskService.GetTaskByTypeAsync(FulfillmentDefaults.FulfillmentInventoryTask.Type);
            if (inventoryTask is not null)
                await _scheduleTaskService.DeleteTaskAsync(inventoryTask);

            var trackingTask = await _scheduleTaskService.GetTaskByTypeAsync(FulfillmentDefaults.FulfillmentTrackingTask.Type);
            if (trackingTask is not null)
                await _scheduleTaskService.DeleteTaskAsync(trackingTask);

            //locales
            await _localizationService.DeleteLocaleResourcesAsync("Plugins.Widgets.Fulfillment");

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
}
