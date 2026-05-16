using Nop.Core;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;

namespace Nop.Plugin.OnePage.Checkout
{
    /// <summary>
    /// Represents the one page checkout plugin
    /// </summary>
    public class OnePageCheckoutPlugin : BasePlugin, IMiscPlugin
    {
        #region Fields

        protected readonly ISettingService _settingService;
        protected readonly ILocalizationService _localizationService;
        protected readonly IWebHelper _webHelper;

        #endregion

        #region Ctor

        public OnePageCheckoutPlugin(ISettingService settingService,
            ILocalizationService localizationService,
            IWebHelper webHelper)
        {
            _settingService = settingService;
            _localizationService = localizationService;
            _webHelper = webHelper;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return _webHelper.GetStoreLocation() + "Admin/OnePageCheckout/Configure";
        }

        /// <summary>
        /// Install plugin
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task InstallAsync()
        {
            //settings
            var settings = new OnePageCheckoutSettings
            {
                EnableOnePageCheckout = true,
                LoginRegisterPopUp = false
            };
            await _settingService.SaveSettingAsync(settings);

            await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
            {
                ["Plugins.OnePage.Checkout.Fields.EnableOnePageCheckout"] = "Enable One Page Checkout",
                ["Plugins.OnePage.Checkout.Fields.EnableOnePageCheckout.Hint"] = "If you check this option you will enable the one page checkout plugin.",
                ["Plugins.OnePage.Checkout.Fields.LoginRegisterPopUp"] = "Enable Login/Register Popup",
                ["Plugins.OnePage.Checkout.Fields.LoginRegisterPopUp.Hint"] = "If you check this option you will enable the login/register popup.",
                ["Plugins.OnePage.Checkout.LoginRegister.AlreadyRegistered"] = "A user has been already registered.",
                ["Plugins.OnePage.Checkout.LoginRegister.AlreadyLogin"] = "Your are already logged in.",
                ["Plugins.OnePage.Checkout.LoginRegister.CreateAccountHint"] = "Don't have an account?",
                ["Plugins.OnePage.Checkout.LoginRegister.AlreadyAccountHint"] = "Already have an account?",
                ["Plugins.OnePage.Checkout.OrderSummary.Show"] = "Show order summary",
                ["Plugins.OnePage.Checkout.OrderSummary.Hide"] = "Hide order summary",
                ["Plugins.OnePage.Checkout.BackToCart"] = "Back to cart",
                ["Plugins.OnePage.Checkout.BackToBillingAddress"] = "Back to bill to",
                ["Plugins.OnePage.Checkout.BackToShippingAddress"] = "Back to ship to",
                ["Plugins.OnePage.Checkout.BackToShipping"] = "Back to shipping",
                ["Plugins.OnePage.Checkout.BackToPayment"] = "Back to payment",
                ["Plugins.OnePage.Checkout.BackToPaymentInfo"] = "Back to payment info",
                ["Plugins.OnePage.Checkout.Change"] = "Change",
                ["Plugins.OnePage.Checkout.BillTo"] = "Bill to",
                ["Plugins.OnePage.Checkout.ShipTo"] = "Ship to",
                ["Plugins.OnePage.Checkout.Pickup"] = "Pickup",
                ["Plugins.OnePage.Checkout.Shipping"] = "Shipping",
                ["Plugins.OnePage.Checkout.Payment"] = "Payment",
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
            await _settingService.DeleteSettingAsync<OnePageCheckoutSettings>();

            //locales
            await _localizationService.DeleteLocaleResourcesAsync("Plugins.OnePage.Checkout");

            await base.UninstallAsync();
        }

        #endregion
    }
}
