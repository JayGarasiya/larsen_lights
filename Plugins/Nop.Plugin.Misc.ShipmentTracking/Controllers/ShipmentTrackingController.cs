using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Misc.ShipmentTracking.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Controllers;

namespace Nop.Plugin.Misc.ShipmentTracking.Controllers
{
    public class ShipmentTrackingController : BaseAdminController
    {
        #region Fields 
        
        protected readonly ILocalizationService _localizationService;
        protected readonly IPermissionService _permissionService;
        protected readonly IStoreContext _storeContext;
        protected readonly ISettingService _settingService;
        protected readonly INotificationService _notificationService;

        #endregion

        #region Ctor

        public ShipmentTrackingController(ILocalizationService localizationService,
            IPermissionService permissionService,
            IStoreContext storeContext,
            ISettingService settingService,
            INotificationService notificationService)
        {
            _localizationService = localizationService;
            _permissionService = permissionService;
            _storeContext = storeContext;
            _settingService = settingService;
            _notificationService = notificationService;
        }

        #endregion

        #region Methods

        public async Task<IActionResult> Configure()
        {
            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var settings = await _settingService.LoadSettingAsync<ShipmentTrackingSettings>(storeScope);

            var model = new ConfigurationModel()
            {
                EnableUPSMethod = settings.EnableUPSMethod,
                UPSTrackingApiUrl = settings.UPSTrackingApiUrl,
                EnableUSPSMethod = settings.EnableUSPSMethod,
                USPSTrackingApiUrl = settings.USPSTrackingApiUrl,
                USPSUsername = settings.USPSUsername,
                USPSPassword = settings.USPSPassword,
                EnableFedExMethod = settings.EnableFedExMethod,
                FedExTrackingApiUrl = settings.FedExTrackingApiUrl,
                FedExClientId = settings.FedExClientId,
                FedExClientSecret = settings.FedExClientSecret,
                EnableSpeedeeMethod = settings.EnableSpeedeeMethod,
                SpeedeeTrackingApiUrl = settings.SpeedeeTrackingApiUrl,
                SpeedeeAccount = settings.SpeedeeAccount,
                SpeedeePassword = settings.SpeedeePassword,
                EnableDHLMethod = settings.EnableDHLMethod,
                DHLTrackingApiUrl = settings.DHLTrackingApiUrl,
                DHLConsumerKey = settings.DHLConsumerKey,
                UPSAuthApiUrl = settings.UPSAuthApiUrl,
                UPSClientId = settings.UPSClientId,
                UPSClientSecret = settings.UPSClientSecret,
            };

            return View("~/Plugins/Misc.ShipmentTracking/Views/Configure.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var settings = await _settingService.LoadSettingAsync<ShipmentTrackingSettings>(storeScope);

            settings.EnableUPSMethod = model.EnableUPSMethod;
            settings.UPSTrackingApiUrl = model.UPSTrackingApiUrl;
            settings.EnableUSPSMethod = model.EnableUSPSMethod;
            settings.USPSTrackingApiUrl = model.USPSTrackingApiUrl;
            settings.USPSUsername = model.USPSUsername;
            settings.USPSPassword = model.USPSPassword;
            settings.EnableFedExMethod = model.EnableFedExMethod;
            settings.FedExTrackingApiUrl = model.FedExTrackingApiUrl;
            settings.FedExClientId = model.FedExClientId;
            settings.FedExClientSecret = model.FedExClientSecret;
            settings.EnableSpeedeeMethod = model.EnableSpeedeeMethod;
            settings.SpeedeeTrackingApiUrl = model.SpeedeeTrackingApiUrl;
            settings.SpeedeeAccount = model.SpeedeeAccount;
            settings.SpeedeePassword = model.SpeedeePassword;
            settings.EnableDHLMethod = model.EnableDHLMethod;
            settings.DHLTrackingApiUrl = model.DHLTrackingApiUrl;
            settings.DHLConsumerKey = model.DHLConsumerKey;
            settings.UPSAuthApiUrl = model.UPSAuthApiUrl;
            settings.UPSClientSecret = model.UPSClientSecret;
            settings.UPSClientId = model.UPSClientId;

            await _settingService.SaveSettingAsync(settings);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

            return await Configure();
        }

        #endregion
    }
}
