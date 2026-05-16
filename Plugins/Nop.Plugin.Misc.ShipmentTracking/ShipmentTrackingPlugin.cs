using Nop.Core;
using Nop.Core.Domain.ScheduleTasks;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.ScheduleTasks;

namespace Nop.Plugin.Misc.ShipmentTracking
{
    /// <summary>
    /// Represents the shipment tracking plugin
    /// </summary>
    public class ShipmentTrackingPlugin : BasePlugin, IMiscPlugin
    {
        #region Fields

       protected readonly ISettingService _settingService;
       protected readonly ILocalizationService _localizationService;
       protected readonly IWebHelper _webHelper;
       protected readonly IScheduleTaskService _scheduleTaskService;

        #endregion

        #region Ctor

        public ShipmentTrackingPlugin(ISettingService settingService,
            ILocalizationService localizationService,
            IWebHelper webHelper,
            IScheduleTaskService scheduleTaskService)
        {
            _settingService = settingService;
            _localizationService = localizationService;
            _webHelper = webHelper;
            _scheduleTaskService = scheduleTaskService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return _webHelper.GetStoreLocation() + "Admin/ShipmentTracking/Configure";
        }

        /// <summary>
        /// Install plugin
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task InstallAsync()
        {
            //delete privouse task
            var trackingTask = await _scheduleTaskService.GetTaskByTypeAsync("Nop.Plugin.Execula.ShipmentTracking.Tasks.ShipmentTrackingTask, Nop.Plugin.Execula.ShipmentTracking, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            if (trackingTask != null)
                await _scheduleTaskService.DeleteTaskAsync(trackingTask);

            //install 3PL FullFilment Process task
            if (await _scheduleTaskService.GetTaskByTypeAsync("Nop.Plugin.Misc.ShipmentTracking.Services.ShipmentTrackingTask, Nop.Plugin.Misc.ShipmentTracking") == null)
            {
                await _scheduleTaskService.InsertTaskAsync(new ScheduleTask
                {
                    Seconds = 60,
                    Name = "Shipment Tracking Task",
                    Type = "Nop.Plugin.Misc.ShipmentTracking.Services.ShipmentTrackingTask, Nop.Plugin.Misc.ShipmentTracking",
                });
            }

            //settings
            var settings = new ShipmentTrackingSettings
            {
                UPSTrackingApiUrl = "https://onlinetools.ups.com/rest/Track",
                USPSTrackingApiUrl = "https://production.shippingapis.com",
                FedExTrackingApiUrl = "https://apis.fedex.com",
                SpeedeeTrackingApiUrl = "http://packages.speedeedelivery.com/rest/index.php",
                DHLTrackingApiUrl = "https://api-eu.dhl.com/track/shipments",
                CurrentPageIndex = 0,
            };
            await _settingService.SaveSettingAsync(settings);

            await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
            {
                ["Plugins.Misc.ShipmentTracking.UPSMethod"] = "UPS Shipment Tracking API Configuration",
                ["Plugins.Misc.ShipmentTracking.EnableUPSMethod"] = "Enable UPS Shipment Tracking",
                ["Plugins.Misc.ShipmentTracking.EnableUPSMethod.Hint"] = "Check to enable UPS shipment tracking.",
                ["Plugins.Misc.ShipmentTracking.UPSTrackingApiUrl"] = "Api URL",
                ["Plugins.Misc.ShipmentTracking.UPSTrackingApiUrl.Hint"] = "Specify USPS Api URL.",
                ["Plugins.Misc.ShipmentTracking.UPSUsername"] = "Username",
                ["Plugins.Misc.ShipmentTracking.UPSUsername.Hint"] = "Specify UPS username.",
                ["Plugins.Misc.ShipmentTracking.UPSPassword"] = "Password",
                ["Plugins.Misc.ShipmentTracking.UPSPassword.Hint"] = "Specify UPS password.",
                ["Plugins.Misc.ShipmentTracking.UPSAccessKey"] = "Access Key",
                ["Plugins.Misc.ShipmentTracking.UPSAccessKey.Hint"] = "Specify UPS access key.",
                ["Plugins.Misc.ShipmentTracking.USPSMethod"] = " USPS Shipment Tracking API Configuration",
                ["Plugins.Misc.ShipmentTracking.EnableUSPSMethod"] = "Enable USPS Shipment Tracking",
                ["Plugins.Misc.ShipmentTracking.EnableUSPSMethod.Hint"] = "Check to enable USPS shipment tracking.",
                ["Plugins.Misc.ShipmentTracking.USPSTrackingApiUrl"] = "Api URL",
                ["Plugins.Misc.ShipmentTracking.USPSTrackingApiUrl.Hint"] = "Specify USPS Api URL.",
                ["Plugins.Misc.ShipmentTracking.USPSUsername"] = "Username",
                ["Plugins.Misc.ShipmentTracking.USPSUsername.Hint"] = "Specify USPS username.",
                ["Plugins.Misc.ShipmentTracking.USPSPassword"] = "Password",
                ["Plugins.Misc.ShipmentTracking.USPSPassword.Hint"] = "Specify USPS password.",
                ["Plugins.Misc.ShipmentTracking.FedExMethod"] = " FedEx Shipment Tracking API Configuration",
                ["Plugins.Misc.ShipmentTracking.EnableFedExMethod"] = "Enable FedEx Shipment Tracking",
                ["Plugins.Misc.ShipmentTracking.EnableFedExMethod.Hint"] = "Check to enable FedEx shipment tracking.",
                ["Plugins.Misc.ShipmentTracking.FedExTrackingApiUrl"] = "Api URL",
                ["Plugins.Misc.ShipmentTracking.FedExTrackingApiUrl.Hint"] = "Specify FedEx Api URL.",
                ["Plugins.Misc.ShipmentTracking.FedExClientId"] = "Client ID",
                ["Plugins.Misc.ShipmentTracking.FedExClientId.Hint"] = "Specify the Client ID also known as API Key received during FedEx Developer portal registration.",
                ["Plugins.Misc.ShipmentTracking.FedExClientSecret"] = "Client secret",
                ["Plugins.Misc.ShipmentTracking.FedExClientSecret.Hint"] = "Specify the Client secret also known as Secret Key received during FedEx Developer portal registration.",
                ["Plugins.Misc.ShipmentTracking.SpeedeeMethod"] = "Speedee Delivery Shipment Tracking API Configuration",
                ["Plugins.Misc.ShipmentTracking.EnableSpeedeeMethod"] = "Enable Speedee Delivery Shipment Tracking",
                ["Plugins.Misc.ShipmentTracking.EnableSpeedeeMethod.Hint"] = "Check to enable Speedee Delivery shipment tracking.",
                ["Plugins.Misc.ShipmentTracking.SpeedeeTrackingApiUrl"] = "Api URL",
                ["Plugins.Misc.ShipmentTracking.SpeedeeTrackingApiUrl.Hint"] = "Specify Speedee Delivery Api URL.",
                ["Plugins.Misc.ShipmentTracking.SpeedeeAccount"] = "Account number",
                ["Plugins.Misc.ShipmentTracking.SpeedeeAccount.Hint"] = "Specify Speedee Delivery account number.",
                ["Plugins.Misc.ShipmentTracking.SpeedeePassword"] = "Password",
                ["Plugins.Misc.ShipmentTracking.SpeedeePassword.Hint"] = "Specify Speedee Delivery password.",
                ["Plugins.Misc.ShipmentTracking.DHLMethod"] = " DHL Shipment Tracking API Configuration",
                ["Plugins.Misc.ShipmentTracking.EnableDHLMethod"] = "Enable DHL Shipment Tracking",
                ["Plugins.Misc.ShipmentTracking.EnableDHLMethod.Hint"] = "Check to enable DHL shipment tracking.",
                ["Plugins.Misc.ShipmentTracking.DHLTrackingApiUrl"] = "Api URL",
                ["Plugins.Misc.ShipmentTracking.DHLTrackingApiUrl.Hint"] = "Specify DHL Api URL.",
                ["Plugins.Misc.ShipmentTracking.DHLConsumerKey"] = "Consumer Key",
                ["Plugins.Misc.ShipmentTracking.DHLConsumerKey.Hint"] = "Specify DHL consumer key.",
                ["Plugins.Misc.ShipmentTracking.PageSize"] = "Page size.",
                ["Plugins.Misc.ShipmentTracking.PageSize.Hint"] = "Enter page size for shipment tracking API.",

                ["Plugins.Misc.ShipmentTracking.Orders.PurchaseOrderNumber"] = "PO Number #",
                ["Plugins.Misc.ShipmentTracking.Orders.PurchaseOrderNumber.Hint"] = "Enter to filter the orders by purchase order number.",
                ["Plugins.Misc.ShipmentTracking.Orders.BillingZipPostalCode"] = "Billing zip / postal code",
                ["Plugins.Misc.ShipmentTracking.Orders.BillingZipPostalCode.Hint"] = "Enter to filter the orders by billing zip / postal code.",
                
                ["Plugins.Misc.ShipmentTracking.Orders.TrackOrder"] = "Track order #",
                ["Plugins.Misc.ShipmentTracking.Orders.TrackOrder.Hint"] = "Enter order # to add a new shipment to order.",
                ["Plugins.Misc.ShipmentTracking.Orders.Track"] = "Track",
                ["Plugins.Misc.ShipmentTracking.Orders.OrderNotFound"] = "The provided order # {0} could not be found.",
                
                ["Plugins.Misc.ShipmentTracking.Orders.AdminNote"] = "Note",

                ["Plugins.Misc.ShipmentTracking.Fields.ShippingMethod"] = "Shipment method",
                ["Plugins.Misc.ShipmentTracking.Fields.ShippingMethod.Hint"] = "Choose from drop down list to filter the shipments by shipment method.",
                ["Plugins.Misc.ShipmentTracking.Fields.ShippingMethod.Button"] = "Set shipment method",
                ["Plugins.Misc.ShipmentTracking.Fields.SendShippedEmail"] = "Send shipped notification email",
                ["Plugins.Misc.ShipmentTracking.Fields.SendShippedEmail.Hint"] = "Check to send shipped notification email to customer.",
                ["Plugins.Misc.ShipmentTracking.Fields.SendDeliveredEmail"] = "Send delivered notification email",
                ["Plugins.Misc.ShipmentTracking.Fields.SendDeliveredEmail.Hint"] = "Check to send shipped notification email to customer.",

                ["Enums.Nop.Plugin.Misc.ShipmentTracking.Domain.ShipmentMethod.UPS"] = "UPS - United Parcel Service",
                ["Enums.Nop.Plugin.Misc.ShipmentTracking.Domain.ShipmentMethod.USPS"] = "USPS - United States Postal Service",
                ["Enums.Nop.Plugin.Misc.ShipmentTracking.Domain.ShipmentMethod.FEDEX"] = "FedEx - Federal Express",
                ["Enums.Nop.Plugin.Misc.ShipmentTracking.Domain.ShipmentMethod.SPEEDEE"] = "Spee-Dee Delivery",
                ["Enums.Nop.Plugin.Misc.ShipmentTracking.Domain.ShipmentMethod.DHL"] = "DHL - Dalsey, Hillblom and Lynn",

                ["Plugins.Misc.ShipmentTracking.OrderItem.Fields.PictureThumbnailUrl"] = "Picture",
                ["Plugins.Misc.ShipmentTracking.OrderItem.Fields.Product"] = "Product",
                ["Plugins.Misc.ShipmentTracking.OrderItem.Fields.UnitPriceInclTax"] = "Price",
                ["Plugins.Misc.ShipmentTracking.OrderItem.Fields.Quantity"] = "Quantity",
                ["Plugins.Misc.ShipmentTracking.OrderItem.Fields.DiscountInclTax"] = "Discount",
                ["Plugins.Misc.ShipmentTracking.OrderItem.Fields.SubTotalInclTax"] = "Total",

                ["Plugins.Misc.ShipmentTracking.AdminNotes"] = "Notes",
                ["Plugins.Misc.ShipmentTracking.AdminNote.Fields.SearchNote"] = "Note",
                ["Plugins.Misc.ShipmentTracking.AdminNote.Fields.SearchNote.Hint"] = "Search in admin notes. Leave empty to load all admin notes for selected order.",


                ["Plugins.Misc.ShipmentTracking.UPSClientId"] = "UPS client Id",
                ["Plugins.Misc.ShipmentTracking.UPSClientId.Hint"] = "Enter UPS client Id",
                ["Plugins.Misc.ShipmentTracking.UPSClientSecret"] = "UPS client secret",
                ["Plugins.Misc.ShipmentTracking.UPSClientSecret.Hint"] = "Enter UPS client secret",
                ["Plugins.Misc.ShipmentTracking.UPSAuthApiUrl"] = "Authentication Api URL",
                ["Plugins.Misc.ShipmentTracking.UPSAuthApiUrl.Hint"] = "Enter authentication Api URL",
                ["Plugins.Misc.ShipmentTracking.Orders.List.SearchCompany"] = "Company",
                ["Plugins.Misc.ShipmentTracking.Orders.List.SearchCompany.Hint"] = "Search by company.",

            });

            await base.InstallAsync();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task UninstallAsync()
        {
            //settings
            await _settingService.DeleteSettingAsync<ShipmentTrackingSettings>();

            //schedule task
            var trackingTask = await _scheduleTaskService.GetTaskByTypeAsync("Nop.Plugin.Misc.ShipmentTracking.Services.ShipmentTrackingTask, Nop.Plugin.Misc.ShipmentTracking");
            if (trackingTask != null)
                await _scheduleTaskService.DeleteTaskAsync(trackingTask);

            //locales
            await _localizationService.DeleteLocaleResourcesAsync("Plugins.Misc.ShipmentTracking");

            await _localizationService.DeleteLocaleResourcesAsync("Enums.Nop.Plugin.Misc.ShipmentTracking");

            await base.UninstallAsync();
        }

        #endregion
    }
}
