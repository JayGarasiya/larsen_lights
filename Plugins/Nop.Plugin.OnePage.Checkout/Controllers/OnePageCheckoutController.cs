using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Plugin.OnePage.Checkout.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Controllers;

namespace Nop.Plugin.OnePage.Checkout.Controllers
{
    public class OnePageCheckoutController : BaseAdminController
    {
        #region Fields

        protected readonly ILocalizationService _localizationService;
        protected readonly IPermissionService _permissionService;
        protected readonly ISettingService _settingService;
        protected readonly IStoreContext _storeContext;
        protected readonly INotificationService _notificationService;
        
        #endregion

        #region Ctor

        public OnePageCheckoutController(ILocalizationService localizationService,
            IPermissionService permissionService,
            ISettingService settingService,
            IStoreContext storeContext,
            INotificationService notificationService)
        {
            _localizationService = localizationService;
            _permissionService = permissionService;
            _settingService = settingService;
            _storeContext = storeContext;
            _notificationService = notificationService;
        }

        #endregion

        #region Methods

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var onePageCheckoutSettings = await _settingService.LoadSettingAsync<OnePageCheckoutSettings>(storeScope);

            var model = new ConfigurationModel()
            {
                EnableOnePageCheckout = onePageCheckoutSettings.EnableOnePageCheckout,
                LoginRegisterPopUp = onePageCheckoutSettings.LoginRegisterPopUp,
                ActiveStoreScopeConfiguration = storeScope
            };

            if (storeScope > 0)
            {
                model.EnableOnePageCheckout_OverrideForStore = await _settingService.SettingExistsAsync(onePageCheckoutSettings, x => x.EnableOnePageCheckout, storeScope);
                model.LoginRegisterPopUp_OverrideForStore = await _settingService.SettingExistsAsync(onePageCheckoutSettings, x => x.LoginRegisterPopUp, storeScope);
            }

            return View("~/Plugins/OnePage.Checkout/Views/Configure.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var onePageCheckoutSettings = await _settingService.LoadSettingAsync<OnePageCheckoutSettings>(storeScope);

            onePageCheckoutSettings.EnableOnePageCheckout = model.EnableOnePageCheckout;
            onePageCheckoutSettings.LoginRegisterPopUp = model.LoginRegisterPopUp;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            await _settingService.SaveSettingOverridablePerStoreAsync(onePageCheckoutSettings, x => x.EnableOnePageCheckout, model.EnableOnePageCheckout_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(onePageCheckoutSettings, x => x.LoginRegisterPopUp, model.LoginRegisterPopUp_OverrideForStore, storeScope, false);

            if(model.EnableOnePageCheckout)
            {
                //stop showing order summary at payment info page
                var orderSettings = await _settingService.LoadSettingAsync<OrderSettings>(storeScope);
                orderSettings.OnePageCheckoutDisplayOrderTotalsOnPaymentInfoTab = false;
                await _settingService.SaveSettingOverridablePerStoreAsync(orderSettings, x => x.OnePageCheckoutDisplayOrderTotalsOnPaymentInfoTab, model.EnableOnePageCheckout_OverrideForStore, storeScope, false);
            }

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

            return await Configure();
        }

        #endregion
    }
}
