using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Data;
using Nop.Plugin.Payments.Evance.BaseApiCall;
using Nop.Plugin.Payments.Evance.Components;
using Nop.Plugin.Payments.Evance.Domain;
using Nop.Plugin.Payments.Evance.Models;
using Nop.Plugin.Payments.Evance.Service;
using Nop.Services.Cms;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Plugins;
using Nop.Web.Components;
using Nop.Web.Framework.Infrastructure;
using System.Web;
using System.Xml.Linq;

namespace Nop.Plugin.Payments.Evance
{
    /// <summary>
    /// Represents evance
    /// </summary>
    public class EvancePlugin : BasePlugin, IPaymentMethod, IWidgetPlugin
    {
        #region Fields

        private readonly IWebHelper _webHelper;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly EvanceSettings _evanceSettings;
        private readonly ICustomerService _customerService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWorkContext _workContext;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly ICountryService _countryService;
        private readonly INotificationService _notificationService;
        private readonly IOrderService _orderService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ICustomerVaultService _customerVaultService;
        private readonly INopDataProvider _dataProvider;
        private readonly ILogger _logger;

        #endregion

        #region Ctor

        public EvancePlugin(IWebHelper webHelper,
            ISettingService settingService,
            ILocalizationService localizationService,
            EvanceSettings evanceSettings,
            ICustomerService customerService,
            IOrderTotalCalculationService orderTotalCalculationService,
            IHttpContextAccessor httpContextAccessor,
            IWorkContext workContext,
            IStateProvinceService stateProvinceService,
            ICountryService countryService,
            INotificationService notificationService,
            IOrderService orderService,
            IGenericAttributeService genericAttributeService,
            ICustomerVaultService customerVaultService,
            INopDataProvider dataProvider,
            ILogger logger)
        {
            _webHelper = webHelper;
            _settingService = settingService;
            _localizationService = localizationService;
            _evanceSettings = evanceSettings;
            _customerService = customerService;
            _orderTotalCalculationService = orderTotalCalculationService;
            _httpContextAccessor = httpContextAccessor;
            _workContext = workContext;
            _stateProvinceService = stateProvinceService;
            _countryService = countryService;
            _notificationService = notificationService;
            _orderService = orderService;
            _genericAttributeService = genericAttributeService;
            _customerVaultService = customerVaultService;
            _dataProvider = dataProvider;
            _logger = logger;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/Evance/Configure";
        }

        /// <summary>
        /// Gets widget zones where this widget should be rendered
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the widget zones
        /// </returns>
        public Task<IList<string>> GetWidgetZonesAsync()
        {
            return Task.FromResult<IList<string>>(new List<string> {
                PublicWidgetZones.HeaderAfter,
                PublicWidgetZones.AccountNavigationAfter,
                PublicWidgetZones.OpCheckoutConfirmTop,
            });
        }

        /// <summary>
        /// Gets a type of a view component for displaying plugin in public store ("payment info" checkout step)
        /// </summary>
        /// <returns>View component type</returns>
        public Type GetPublicViewComponent()
        {
            return typeof(PaymentEvanceViewComponent);
        }

        /// <summary>
        /// Gets a type of a view component for displaying widget
        /// </summary>
        /// <param name="widgetZone">Name of the widget zone</param>
        /// <returns>View component type</returns>
        public Type GetWidgetViewComponent(string widgetZone)
        {
            if (widgetZone == null)
                throw new ArgumentNullException(nameof(widgetZone));

            if (widgetZone.Equals(PublicWidgetZones.HeaderAfter))
                return typeof(ThreeDSecureViewComponent);

            if (widgetZone.Equals(PublicWidgetZones.AccountNavigationAfter))
                return typeof(CustomerVaultViewComponent);

            if (widgetZone.Equals(PublicWidgetZones.OpCheckoutConfirmTop))
                return typeof(ErrorValidationViewComponent);

            return typeof(WidgetViewComponent);
        }

        /// <summary>
        /// Install plugin
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task InstallAsync()
        {
            await _settingService.SaveSettingAsync(new EvanceSettings
            {
                Enable = true,
            });

            //locals
            await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
            {
                ["Plugins.Payments.Evance.Fields.Enable"] = "Enable",
                ["Plugins.Payments.Evance.Fields.Enable.Hint"] = "Enable/Disable the plugin",
                ["Plugins.Payments.Evance.Fields.SecurityKey"] = "Security Key",
                ["Plugins.Payments.Evance.Fields.SecurityKey.Hint"] = "Enter security key",
                ["Plugins.Payments.Evance.Fields.PaymentMethodDescription"] = "Pay by card, Google pay, Apple pay, or eCheck",
                ["Plugins.Payments.Evance.Fields.AdditionalFee"] = "Additional fee",
                ["Plugins.Payments.Evance.Fields.AdditionalFee.Hint"] = "Enter additional fee to charge your customers.",
                ["Plugins.Payments.Evance.Fields.AdditionalFeePercentage"] = "Additional fee. Use percentage",
                ["Plugins.Payments.Evance.Fields.AdditionalFeePercentage.Hint"] = "Determines whether to apply a percentage additional fee to the order total. If not enabled, a fixed value is used.",
                ["Plugins.Payments.Evance.Fields.Payments"] = "Payment Options",
                ["Plugins.Payments.Evance.Fields.Payments.Hint"] = "Select acceptable payment options",
                ["Plugins.Payments.Evance.Fields.ThreedSEnable"] = "3d Secure Enable",
                ["Plugins.Payments.Evance.Fields.ThreedSEnable.Hint"] = "Enable/Disable the 3d Secure",
                ["Plugins.Payments.Evance.Fields.ExcludeCustomerRoleIds"] = "Exclude Customer Roles For 3DS ",
                ["Plugins.Payments.Evance.Fields.ExcludeCustomerRoleIds.Hint"] = "Select customer roles for which 3DS should not apply",
                ["Plugins.Payments.Evance.Fields.CustomerVaultEnable"] = "Customer Vault Enable",
                ["Plugins.Payments.Evance.Fields.CustomerVaultEnable.Hint"] = "Enable/Disable the Customer Vault",
                ["Plugins.Payments.Evance.Fields.AllowSaveCard"] = "Allow Save Card",
                ["Plugins.Payments.Evance.Fields.AllowSaveCard.Hint"] = "Allow Save Card",
                ["Plugins.Payments.Evance.Fields.AllowSaveACH"] = "Allow Save ACH",
                ["Plugins.Payments.Evance.Fields.AllowSaveACH.Hint"] = "Allow Save ACH",
                ["Plugins.Payments.Evance.Fields.PaymentAPIBaseUrl"] = "Payment API Base Url",
                ["Plugins.Payments.Evance.Fields.PaymentAPIBaseUrl.Hint"] = "Enter Base Url of Payment API",
                ["Plugins.Payments.Evance.Fields.QueryAPIBaseUrl"] = "Query API Base Url",
                ["Plugins.Payments.Evance.Fields.QueryAPIBaseUrl.Hint"] = "Enter Base Url of Query API",
                ["Plugins.Payments.Evance.Fields.TransactionType"] = "Transaction Type",
                ["Plugins.Payments.Evance.Fields.TransactionType.Hint"] = "Select Transaction Type.",
                ["Plugin.Payments.Evance.Fields.CardDetails.Required"] = "Unable to retrive card detail to process payment.",
                ["Plugins.Payments.Evance.Fields.CollectCheckoutKey"] = "Collect Checkout Key",
                ["Plugins.Payments.Evance.Fields.CollectCheckoutKey.Hint"] = "Enter Collect Checkout Key",
                ["Plugins.Payments.Evance.Payment.SaveCard"] = "Save Card",
                ["Plugins.Payments.Evance.Payment.CheckName"] = "Account Holder Name",
                ["Plugins.Payments.Evance.Payment.AccountNumber"] = "Account Number",
                ["Plugins.Payments.Evance.Payment.RoutingNumber"] = "Routing Number",
                ["Plugins.Payments.Evance.Payment.CheckTransit"] = "Check Transit",
                ["Plugins.Payments.Evance.Payment.CheckInstitution"] = "Check Institution",
                ["Plugins.Payments.Evance.Payment.SaveACH"] = "Save Details",
                ["Plugins.Payments.Evance.SavedCard.Title"] = "Saved Cards",
                ["Plugins.Payments.Evance.SavedACH.Title"] = "Saved Bank Accounts",
                ["Plugins.Payments.Evance.CustomerVault.Title"] = "Saved card/ Bank detail",
                ["Plugins.Payments.Evance.NoSavedCardACH.Title"] = "You don't have any Saved card/ ACH bank detail",
                ["Plugins.Payments.Evance.Fields.DisplaySavedDetails"] = "Display saved details",
                ["Plugins.Payments.Evance.Fields.DisplaySavedDetails.Hint"] = "check to wether display saved card / bank details in customer account.",
                ["Plugins.Payments.Evance.Payment.Different.Card"] = "Use a different card",
                ["Plugins.Payments.Evance.Payment.Different.BankAccount"] = "Use a different bank account",
                ["Plugins.Payments.Evance.Payment.Error.InvalidSecretKey"] = "Security Key is not Provided",
                ["Plugins.Payments.Evance.Payment.Error.InvalidApiKey"] = "Invalid End Point",
                ["Plugins.Payments.Evance.Payment.Error.BillingAddress"] = "Billing Address is required",
                ["Plugins.Payments.Evance.Payment.Error.PaymentToken"] = "Payment Token does not exist",
                ["Plugins.Payments.Evance.Payment.Error.CVV.NotValid"] = "The security code (CVV) you entered is incorrect. Please check and try again.",
                ["Plugins.Payments.Evance.Payment.Error.CVV.NotVerified"] = "There was an issue verifying your card's security code. Please try again or use a different card.",
                ["Plugins.Payments.Evance.Payment.Error.CVV.NotExist"] = "Your card does not have a security code. Please check your card details or try a different card.",
                ["Plugins.Payments.Evance.Payment.Error.CVV.User.NotVerified"] = "We couldn't verify your card due to an issue with the card issuer. Please try another card or contact your bank.",
                ["Plugins.Payments.Evance.Payment.Failed"] = "Payment Failed",
                ["Plugins.Payments.Evance.Payment.TransactionId.NotExist"] = "Transaction Id does not exist",
                ["Plugins.Payments.Evance.Fields.DropTable"] = "Drop table",
                ["Plugins.Payments.Evance.Fields.DropTable.Hint"] = "Check to Drop table at plugin uninstall time",
                ["Plugins.Payments.Evance.Payment.Error.AVS.Address.Records"] = "The billing address and ZIP code do not match the card issuer’s records.",
                ["Plugins.Payments.Evance.Payment.Error.AVS.Address.NotMatched"] = "The billing address, ZIP code, and cardholder name do not match.",
                ["Plugins.Payments.Evance.Payment.Error.AVS.Country.NotMatched"] = "The card was issued outside the U.S. and does not support address verification.",
                ["Plugins.Payments.Evance.Payment.Error.AVS.System.NotAvailable"] = "The card issuer’s system was unavailable. Please try again later.",
                ["Plugins.Payments.Evance.Payment.Error.AVS.NotSupported"] = "AVS is not supported for this type of transaction.",
                ["Plugins.Payments.Evance.Payment.Warning.ACH"] = "It can take up to 4 days for the payment to clear.",
                ["Plugins.Payments.Evance.Payment.Response.Error.Declined"] = "Your transaction was declined. Please try a different card or contact your bank.",
                ["Plugins.Payments.Evance.Payment.Response.Error.Bank.Declined"] = "Your bank declined the transaction. Please contact them or try another card.",
                ["Plugins.Payments.Evance.Payment.Response.Error.NotFunds"] = "There are not enough funds available to complete the payment.",
                ["Plugins.Payments.Evance.Payment.Response.Error.ExceedLimit"] = "This card has exceeded its limit. Please use a different payment method.",
                ["Plugins.Payments.Evance.Payment.Response.Error.Transaction.NotAllowed"] = "This type of transaction is not allowed with your card. Contact your bank for details.",
                ["Plugins.Payments.Evance.Payment.Response.Error.PaymentInformation"] = "Some of the payment information entered is incorrect. Please double-check and try again.",
                ["Plugins.Payments.Evance.Payment.Response.Error.CardIssuer.NotFound"] = "This card issuer could not be found. Please verify your card details.",
                ["Plugins.Payments.Evance.Payment.Response.Error.CardNumber.NotVerified"] = "The card number could not be verified. Please check and try again.",
                ["Plugins.Payments.Evance.Payment.Response.Error.Card.Expired"] = "This card has expired. Please use a different card.",
                ["Plugins.Payments.Evance.Payment.Response.Error.CardDate.InValid"] = "The expiration date is invalid. Please correct it and try again.",
                ["Plugins.Payments.Evance.Payment.Response.Error.CardCvv.InValid"] = "The card security code (CVV) is incorrect. Please try again.",
                ["Plugins.Payments.Evance.Payment.Response.Error.PIN.InValid"] = "The PIN entered is invalid. Please try again.",
                ["Plugins.Payments.Evance.Payment.Response.Error.ContactBank"] = "Please contact your bank for more information about this decline.",
                ["Plugins.Payments.Evance.Payment.Response.Error.Card.CanNotUsed"] = "Your card cannot be used. Please contact your bank or use another card.",
                ["Plugins.Payments.Evance.Payment.Response.Error.Card.Lost"] = "This card has been reported lost. Please use a different card.",
                ["Plugins.Payments.Evance.Payment.Response.Error.Card.Stolen"] = "This card has been reported stolen. Please use a different card.",
                ["Plugins.Payments.Evance.Payment.Response.Error.Card.Fraud"] = "The card was flagged for fraud. Please contact your bank.",
                ["Plugins.Payments.Evance.Payment.Response.Error.Card.Declined"] = "Your card was declined. More details may be available from your bank.",
                ["Plugins.Payments.Evance.Payment.Response.Error.Card.Recurring.NotSupported"] = "Recurring payments are blocked on this card. Contact your bank for help.",
                ["Plugins.Payments.Evance.Payment.Response.Error.Card.Recurring.Declined"] = "This subscription or recurring payment was declined by your bank.",
                ["Plugins.Payments.Evance.Payment.Response.Error.Card.Update"] = "Please update your card information. Your bank has provided updated details.",
                ["Plugins.Payments.Evance.Payment.Response.Error.Transaction.Declined"] = "This transaction was declined. Please try again in a few days.",
                ["Plugins.Payments.Evance.Payment.Response.Error.Payment.Rejected"] = "The payment was rejected by our system. Please try again later.",
                ["Plugins.Payments.Evance.Payment.Response.Error.Processing"] = "A processing error occurred. Please try again or use another method.",
                ["Plugins.Payments.Evance.Payment.Response.Error.MerchantSetting.Invalid"] = "There is an issue with our merchant settings. Please contact support.",
                ["Plugins.Payments.Evance.Payment.Response.Error.Evance.PaymentAccount.InActive"] = "Our payment account is currently inactive. Please try again later.",
                ["Plugins.Payments.Evance.Payment.Response.Error.Communication"] = "A communication error occurred. Please retry your payment.",
                ["Plugins.Payments.Evance.Payment.Response.Error.CardIssuer"] = "We couldn't reach your card issuer. Please try again or use another card.",
                ["Plugins.Payments.Evance.Payment.Response.Error.Transaction.Duplicate"] = "This looks like a duplicate transaction. Please check your card activity or try again after a while.",
                ["Plugins.Payments.Evance.Payment.Response.Error.TechnicalIssue"] = "A technical issue occurred. Please try again.",
                ["Plugins.Payments.Evance.Payment.Response.Error.Transaction.Invalid"] = "Invalid transaction details. Please verify your information and try again.",
                ["Plugins.Payments.Evance.Payment.Response.Error.TransactionType.NotSupported"] = "This type of transaction is not supported. Please contact support.",
                ["Plugins.Payments.Evance.Payment.Response.Error.CardType.NotSupported"] = "This card type is not supported. Please use another card.",
                ["Plugins.Payments.Evance.Payment.Response.Error.Card.RecurringPayments.Declined"] = "This recurring payment was declined by your bank.",
                ["Plugins.Payments.Evance.Payment.ThreeDS.Error.Authentication"] = "Your card couldn't be authenticated. Please try a different card or contact your bank for assistance.",
                ["Plugins.Payments.Evance.Payment.ThreeDS.Error.Authentication.NotPerformed"] = "We're having trouble verifying your card at the moment. Please try again later or use a different card.",
                ["Plugins.Payments.Evance.Payment.ThreeDS.Error.InvalidFormatted"] = "Something went wrong while processing your card. Please try again or use a different payment method.",
                ["Plugins.Payments.Evance.Payment.ThreeDS.Error.TimeOut"] = "The authentication process took too long and timed out. Please try again or use another card.",
                ["Plugins.Payments.Evance.Payment.ThreeDS.Error.NotVerified"] = "Your card couldn’t be verified after additional security. Please check your details or try a different card.",
                ["Plugins.Payments.Evance.Payment.ThreeDS.Error"] = "An error occurred during the security check. Please try again or use another card.",
                ["Plugins.Payments.Evance.Payment.ThreeDS.Error.AnotherInstance"] = "Something went wrong while verifying your card. Please refresh the page and try again.",
                ["Plugins.Payments.Evance.Payment.ThreeDS.Error.Common"] = "An unexpected error occurred. Please try again or use a different payment method.",
            });

            await base.InstallAsync();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task UninstallAsync()
        {
            // Load the settings
            var evanceSettings = await _settingService.LoadSettingAsync<EvanceSettings>();

            // Truncate or Drop Table if needed
            if (evanceSettings?.DropTable == true)
            {
                await _dataProvider.ExecuteNonQueryAsync("TRUNCATE TABLE [CustomerVault]");
            }

            //Delete settings
            await _settingService.DeleteSettingAsync<EvanceSettings>();

            //locales
            await _localizationService.DeleteLocaleResourcesAsync("Plugins.Payments.Evance");

            await base.UninstallAsync();
        }

        /// <summary>
        /// Get payment information
        /// </summary>
        /// <param name="form">The parsed form values</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the payment info holder
        /// </returns>
        public Task<ProcessPaymentRequest> GetPaymentInfoAsync(IFormCollection form)
        {
            _httpContextAccessor.HttpContext?.Session.Clear();
            var paymentToken = form["payment_token"];
            var cardHolderAuth = form["CardHolderAuth"];
            var cavv = form["cavv"];
            var threeDsVersion = form["threeDsVersion"];
            var directoryServerId = form["directoryServerId"];
            var checkSaveCard = form["checkSaveCard"];
            var cvvBox = form["cvvBox"];
            var customerVaultId = form["customerVaultId"];
            var checkSaveACH = form["checkSaveACH"];
            var aCHVaultId = form["ACHVaultId"];
            var paymentType = form["paymentType"];

            if (!string.IsNullOrEmpty(paymentToken))
                _httpContextAccessor.HttpContext?.Session.SetString("paymentToken", paymentToken.ToString().Trim('[', ']'));

            if (!string.IsNullOrEmpty(cavv))
                _httpContextAccessor.HttpContext?.Session.SetString("cavv", cavv.ToString().Trim('[', ']'));

            if (!string.IsNullOrEmpty(cardHolderAuth))
                _httpContextAccessor.HttpContext?.Session.SetString("cardHolderAuth", cardHolderAuth.ToString().Trim('[', ']'));

            if (!string.IsNullOrEmpty(directoryServerId))
                _httpContextAccessor.HttpContext?.Session.SetString("directoryServerId", directoryServerId.ToString().Trim('[', ']'));

            if (!string.IsNullOrEmpty(threeDsVersion))
                _httpContextAccessor.HttpContext?.Session.SetString("threeDsVersion", threeDsVersion.ToString().Trim('[', ']'));

            if (!string.IsNullOrEmpty(checkSaveCard))
                _httpContextAccessor.HttpContext?.Session.SetString("checkSaveCard", checkSaveCard.ToString().Trim('[', ']'));

            if (!string.IsNullOrEmpty(cvvBox))
                _httpContextAccessor.HttpContext?.Session.SetString("cvvBox", cvvBox.ToString().Trim('[', ']'));

            if (!string.IsNullOrEmpty(customerVaultId))
                _httpContextAccessor.HttpContext?.Session.SetString("customerVaultId", customerVaultId.ToString().Trim('[', ']'));

            if (!string.IsNullOrEmpty(checkSaveACH))
                _httpContextAccessor.HttpContext?.Session.SetString("checkSaveACH", checkSaveACH.ToString().Trim('[', ']'));

            if (!string.IsNullOrEmpty(aCHVaultId))
                _httpContextAccessor.HttpContext?.Session.SetString("ACHVaultId", aCHVaultId.ToString().Trim('[', ']'));

            if (!string.IsNullOrEmpty(paymentType))
                _httpContextAccessor.HttpContext?.Session.SetString("paymentType", paymentType.ToString().ToLower().Trim('[', ']'));

            return Task.FromResult(new ProcessPaymentRequest());
        }

        /// <summary>
        /// Gets a payment method description that will be displayed on checkout pages in the public store
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<string> GetPaymentMethodDescriptionAsync()
        {
            return await _localizationService.GetResourceAsync("Plugins.Payments.Evance.Fields.PaymentMethodDescription");
        }

        /// <summary>
        /// Returns a value indicating whether payment method should be hidden during checkout
        /// </summary>
        /// <param name="cart">Shoping cart</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the rue - hide; false - display.
        /// </returns>
        public Task<bool> HidePaymentMethodAsync(IList<ShoppingCartItem> cart)
        {
            var isHide = true;

            if (_evanceSettings.Enable && !string.IsNullOrEmpty(_evanceSettings.SecurityKey) && cart.Any())
                isHide = false;

            return Task.FromResult(isHide);
        }

        /// <summary>
        /// Gets additional handling fee
        /// </summary>
        /// <param name="cart">Shoping cart</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the additional handling fee
        /// </returns>
        public async Task<decimal> GetAdditionalHandlingFeeAsync(IList<ShoppingCartItem> cart)
        {
            return await _orderTotalCalculationService.CalculatePaymentAdditionalFeeAsync(cart,
               _evanceSettings.AdditionalFee, _evanceSettings.AdditionalFeePercentage);
        }

        /// <summary>
        /// Validate payment form
        /// </summary>
        /// <param name="form">The parsed form values</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the list of validating errors
        /// </returns>
        public async Task<IList<string>> ValidatePaymentFormAsync(IFormCollection form)
        {
            var warnings = new List<string>();

            if (form.TryGetValue(nameof(PaymentInfoModel.PaymentToken), out var paymentToken) && string.IsNullOrEmpty(paymentToken))
                warnings.Add(await _localizationService.GetResourceAsync("Plugin.Payments.Evance.Fields.CardDetails.Required"));

            return warnings;
        }

        /// <summary>
        /// Process a payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the process payment result
        /// </returns>
        public async Task<ProcessPaymentResult> ProcessPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            var processPaymentResult = new ProcessPaymentResult();
            if (_httpContextAccessor.HttpContext?.Session.Keys.Any() != null &&
                (_httpContextAccessor.HttpContext.Session.Keys.Any(c => c == "paymentToken") || _httpContextAccessor.HttpContext.Session.Keys.Any(c => c == "customerVaultId") || _httpContextAccessor.HttpContext.Session.Keys.Any(c => c == "ACHVaultId")))
            {
                var cavv = string.Empty;
                var cvvBox = string.Empty;
                var cardHolderAuth = string.Empty;
                var directoryServerId = string.Empty;
                var threeDsVersion = string.Empty;
                var eci = string.Empty;
                var checkSaveCard = string.Empty;
                var checkSaveACH = string.Empty;
                var savedCustomerVaultId = string.Empty;
                var customerVaultId = string.Empty;
                var aCHVaultId = string.Empty;
                var paymentType = string.Empty;

                var customer = await _workContext.GetCurrentCustomerAsync();
                var paymentToken = _httpContextAccessor.HttpContext.Session.GetString("paymentToken")?.Trim('"');

                if (_httpContextAccessor.HttpContext.Session.Keys.Any(c => c == "cavv"))
                    cavv = _httpContextAccessor.HttpContext.Session.GetString("cavv")?.Trim('"');

                if (_httpContextAccessor.HttpContext.Session.Keys.Any(c => c == "cardHolderAuth"))
                    cardHolderAuth = _httpContextAccessor.HttpContext.Session.GetString("cardHolderAuth")?.Trim('"');

                if (_httpContextAccessor.HttpContext.Session.Keys.Any(c => c == "directoryServerId"))
                    directoryServerId = _httpContextAccessor.HttpContext.Session.GetString("directoryServerId")?.Trim('"');

                if (_httpContextAccessor.HttpContext.Session.Keys.Any(c => c == "threeDsVersion"))
                    threeDsVersion = _httpContextAccessor.HttpContext.Session.GetString("threeDsVersion")?.Trim('"');

                if (_httpContextAccessor.HttpContext.Session.Keys.Any(c => c == "checkSaveCard"))
                    checkSaveCard = _httpContextAccessor.HttpContext.Session.GetString("checkSaveCard")?.Trim('"');

                if (_httpContextAccessor.HttpContext.Session.Keys.Any(c => c == "cvvBox"))
                    cvvBox = _httpContextAccessor.HttpContext.Session.GetString("cvvBox")?.Trim('"');

                if (_httpContextAccessor.HttpContext.Session.Keys.Any(c => c == "customerVaultId"))
                    savedCustomerVaultId = _httpContextAccessor.HttpContext.Session.GetString("customerVaultId")?.Trim('"');

                if (_httpContextAccessor.HttpContext.Session.Keys.Any(c => c == "checkSaveACH"))
                    checkSaveACH = _httpContextAccessor.HttpContext.Session.GetString("checkSaveACH")?.Trim('"');

                if (_httpContextAccessor.HttpContext.Session.Keys.Any(c => c == "ACHVaultId"))
                    aCHVaultId = _httpContextAccessor.HttpContext.Session.GetString("ACHVaultId")?.Trim('"');

                if (_httpContextAccessor.HttpContext.Session.Keys.Any(c => c == "paymentType"))
                    paymentType = _httpContextAccessor.HttpContext.Session.GetString("paymentType")?.Trim('"');

                if (!string.IsNullOrEmpty(paymentToken) || !string.IsNullOrEmpty(savedCustomerVaultId) || !string.IsNullOrEmpty(aCHVaultId))
                {
                    var paymentAPIBaseUrl = _evanceSettings.PaymentAPIBaseUrl;
                    var securityKey = _evanceSettings.SecurityKey;

                    if (string.IsNullOrEmpty(paymentAPIBaseUrl))
                    {
                        processPaymentResult.Errors.Add(await _localizationService.GetResourceAsync("Plugins.Payments.Evance.Payment.Error.InvalidApiKey"));
                        return processPaymentResult;
                    }

                    if (string.IsNullOrEmpty(securityKey))
                    {
                        processPaymentResult.Errors.Add(await _localizationService.GetResourceAsync("Plugins.Payments.Evance.Payment.Error.InvalidSecretKey"));
                        return processPaymentResult;
                    }

                    var transactiontype = string.Empty;
                    if (_evanceSettings.TransactionType.Equals(TransactionType.AuthorizeOnly))
                        transactiontype = "auth";

                    if (_evanceSettings.TransactionType.Equals(TransactionType.AuthorizeAndCapture))
                        transactiontype = "sale";

                    var address = new Core.Domain.Common.Address();
                    var orderCustomer = await _customerService.GetCustomerByIdAsync(processPaymentRequest.CustomerId);
                    var shippingAddress = await _customerService.GetCustomerShippingAddressAsync(orderCustomer);
                    var billingAddress = await _customerService.GetCustomerBillingAddressAsync(orderCustomer);

                    if (billingAddress != null)
                        address = billingAddress;
                    else if (shippingAddress != null)
                        address = shippingAddress;
                    else
                    {
                        processPaymentResult.Errors.Add(await _localizationService.GetResourceAsync("Plugins.Payments.Evance.Payment.Error.BillingAddress"));
                        return processPaymentResult;
                    }

                    var stateprovinceCode = string.Empty;
                    var stateprovince = await _stateProvinceService.GetStateProvinceByIdAsync(address.StateProvinceId.HasValue ? address.StateProvinceId.Value : 0);
                    if (stateprovinceCode != null)
                        stateprovinceCode = stateprovince.Abbreviation;

                    var countryCode = string.Empty;
                    var country = await _countryService.GetCountryByIdAsync(address.CountryId.HasValue ? address.CountryId.Value : 0);
                    if (country != null)
                        countryCode = country.TwoLetterIsoCode;

                    // Prepare request parameters
                    var requestModel = new BaseRequestModel();

                    requestModel.AddParameter("security_key", securityKey);
                    requestModel.AddParameter("amount", Math.Round(processPaymentRequest.OrderTotal, 2).ToString());
                    requestModel.AddParameter("city", address.City);
                    requestModel.AddParameter("state", stateprovinceCode ?? string.Empty);
                    requestModel.AddParameter("address1", address.Address1);
                    requestModel.AddParameter("country", countryCode);
                    requestModel.AddParameter("zip", address.ZipPostalCode);
                    if (await _customerService.IsGuestAsync(customer))
                    {
                        requestModel.AddParameter("email", address.Email);
                        requestModel.AddParameter("phone", address.PhoneNumber);
                        requestModel.AddParameter("first_name", address.FirstName);
                        requestModel.AddParameter("last_name", address.LastName);
                    }
                    else
                    {
                        requestModel.AddParameter("email", orderCustomer != null ? orderCustomer.Email : customer.Email);
                        requestModel.AddParameter("phone", orderCustomer != null ? orderCustomer.Phone : customer.Phone);
                        requestModel.AddParameter("first_name", orderCustomer?.FirstName ?? customer.FirstName);
                        requestModel.AddParameter("last_name", orderCustomer?.LastName ?? customer.LastName);
                    }

                    if (paymentType == "card payment")
                    {
                        requestModel.AddParameter("payment", "creditcard");
                        requestModel.AddParameter("type", transactiontype);
                    }
                    else if (paymentType == "ach payment")
                    {
                        requestModel.AddParameter("payment", "check");
                        requestModel.AddParameter("type", "sale");
                    }
                    else
                        requestModel.AddParameter("type", transactiontype);


                    if (!string.IsNullOrEmpty(savedCustomerVaultId) && !string.IsNullOrEmpty(cvvBox))
                    {
                        requestModel.AddParameter("cvv", cvvBox);
                        requestModel.AddParameter("customer_vault_id", savedCustomerVaultId);
                    }
                    else if (!string.IsNullOrEmpty(aCHVaultId) && string.IsNullOrEmpty(paymentToken))
                    {
                        requestModel.AddParameter("customer_vault_id", aCHVaultId);
                    }
                    else
                    {
                        if (checkSaveCard == "true" || checkSaveACH == "true")
                        {
                            requestModel.AddParameter("customer_vault", "add_customer");
                        }
                        requestModel.AddParameter("payment_token", paymentToken ?? string.Empty);
                        requestModel.AddParameter("cardholder_auth", cardHolderAuth ?? string.Empty);
                        requestModel.AddParameter("three_ds_version", threeDsVersion ?? string.Empty);
                        requestModel.AddParameter("directory_server_id", directoryServerId ?? string.Empty);
                        requestModel.AddParameter("cavv", cavv ?? string.Empty);
                    }

                    // Call API (ensure URL-encoded request)
                    var (response, isSuccess, request) = await IntegratePaymentApi.WebApiRequestAsync<PaymentApiResponseModel>(url: paymentAPIBaseUrl, content: requestModel, method: HttpMethod.Post);

                    var responseModel = new PaymentApiResponseModel();
                    if (!string.IsNullOrEmpty(response))
                    {
                        var queryParams = HttpUtility.ParseQueryString(response);
                        responseModel.Response = queryParams["response"] ?? string.Empty;
                        responseModel.ResponseMessage = queryParams["responsetext"] ?? string.Empty;
                        responseModel.AuthCode = queryParams["authcode"] ?? string.Empty;
                        responseModel.TransactionId = queryParams["transactionid"] ?? string.Empty;
                        responseModel.AvsResponse = queryParams["avsresponse"] ?? string.Empty;
                        responseModel.CvvResponse = queryParams["cvvresponse"] ?? string.Empty;
                        responseModel.OrderId = queryParams["orderid"] ?? string.Empty;
                        responseModel.Type = queryParams["type"] ?? string.Empty;
                        responseModel.ResponseCode = queryParams["response_code"] ?? string.Empty;
                        customerVaultId = queryParams["customer_vault_id"] ?? string.Empty;

                    }

                    if (isSuccess && response != null && responseModel != null && !string.IsNullOrEmpty(responseModel.TransactionId))
                    {
                        if (!string.IsNullOrEmpty(responseModel.Response))
                        {
                            if (responseModel.Response != "1")
                            {
                                if (!string.IsNullOrEmpty(responseModel.ResponseCode) && !string.IsNullOrEmpty(responseModel.ResponseMessage))
                                {
                                    switch (responseModel.ResponseCode)
                                    {
                                        case "200":
                                            processPaymentResult.AddError(await _localizationService.GetResourceAsync("Plugins.Payments.Evance.Payment.Response.Error.Declined"));
                                            return processPaymentResult;

                                        case "201":
                                            processPaymentResult.AddError(await _localizationService.GetResourceAsync("Plugins.Payments.Evance.Payment.Response.Error.Bank.Declined"));
                                            return processPaymentResult;

                                        case "202":
                                            processPaymentResult.AddError(await _localizationService.GetResourceAsync("Plugins.Payments.Evance.Payment.Response.Error.NotFunds"));
                                            return processPaymentResult;

                                        case "203":
                                            processPaymentResult.AddError(await _localizationService.GetResourceAsync("Plugins.Payments.Evance.Payment.Response.Error.ExceedLimit"));
                                            return processPaymentResult;

                                        case "204":
                                            processPaymentResult.AddError(await _localizationService.GetResourceAsync("Plugins.Payments.Evance.Payment.Response.Error.Transaction.NotAllowed"));
                                            return processPaymentResult;

                                        case "220":
                                            processPaymentResult.AddError(await _localizationService.GetResourceAsync("Plugins.Payments.Evance.Payment.Response.Error.PaymentInformation"));
                                            return processPaymentResult;

                                        case "221":
                                            processPaymentResult.AddError(await _localizationService.GetResourceAsync("Plugins.Payments.Evance.Payment.Response.Error.CardIssuer.NotFound"));
                                            return processPaymentResult;

                                        case "222":
                                            processPaymentResult.AddError(await _localizationService.GetResourceAsync("Plugins.Payments.Evance.Payment.Response.Error.CardNumber.NotVerified"));
                                            return processPaymentResult;

                                        case "223":
                                            processPaymentResult.AddError(await _localizationService.GetResourceAsync("Plugins.Payments.Evance.Payment.Response.Error.Card.Expired"));
                                            return processPaymentResult;

                                        case "224":
                                            processPaymentResult.AddError(await _localizationService.GetResourceAsync("Plugins.Payments.Evance.Payment.Response.Error.CardDate.InValid"));
                                            return processPaymentResult;

                                        case "225":
                                            processPaymentResult.AddError(await _localizationService.GetResourceAsync("Plugins.Payments.Evance.Payment.Response.Error.CardCvv.InValid"));
                                            return processPaymentResult;

                                        case "226":
                                            processPaymentResult.AddError(await _localizationService.GetResourceAsync("Plugins.Payments.Evance.Payment.Response.Error.PIN.InValid"));
                                            return processPaymentResult;

                                        case "240":
                                            processPaymentResult.AddError(await _localizationService.GetResourceAsync("Plugins.Payments.Evance.Payment.Response.Error.ContactBank"));
                                            return processPaymentResult;

                                        case "250":
                                            processPaymentResult.AddError(await _localizationService.GetResourceAsync("Plugins.Payments.Evance.Payment.Response.Error.Card.CanNotUsed"));
                                            return processPaymentResult;

                                        case "251":
                                            processPaymentResult.AddError(await _localizationService.GetResourceAsync("Plugins.Payments.Evance.Payment.Response.Error.Card.Lost"));
                                            return processPaymentResult;

                                        case "252":
                                            processPaymentResult.AddError(await _localizationService.GetResourceAsync("Plugins.Payments.Evance.Payment.Response.Error.Card.Stolen"));
                                            return processPaymentResult;

                                        case "253":
                                            processPaymentResult.AddError(await _localizationService.GetResourceAsync("Plugins.Payments.Evance.Payment.Response.Error.Card.Fraud"));
                                            return processPaymentResult;

                                        case "260":
                                            processPaymentResult.AddError(await _localizationService.GetResourceAsync("Plugins.Payments.Evance.Payment.Response.Error.Card.Recurring.NotSupported"));
                                            return processPaymentResult;

                                        case "261":
                                            processPaymentResult.AddError(await _localizationService.GetResourceAsync("Plugins.Payments.Evance.Payment.Response.Error.Card.RecurringPayments.Declined"));
                                            return processPaymentResult;

                                        case "262":
                                            processPaymentResult.AddError(await _localizationService.GetResourceAsync("Plugins.Payments.Evance.Payment.Response.Error.Card.RecurringPayments.Declined"));
                                            return processPaymentResult;

                                        case "263":
                                            processPaymentResult.AddError(await _localizationService.GetResourceAsync("Plugins.Payments.Evance.Payment.Response.Error.Card.Update"));
                                            return processPaymentResult;

                                        case "264":
                                            processPaymentResult.AddError(await _localizationService.GetResourceAsync("Plugins.Payments.Evance.Payment.Response.Error.Transaction.Declined"));
                                            return processPaymentResult;

                                        case "300":
                                            processPaymentResult.AddError(await _localizationService.GetResourceAsync("Plugins.Payments.Evance.Payment.Response.Error.Payment.Rejected"));
                                            return processPaymentResult;

                                        case "400":
                                            processPaymentResult.AddError(await _localizationService.GetResourceAsync("Plugins.Payments.Evance.Payment.Response.Error.Processing"));
                                            return processPaymentResult;

                                        case "410":
                                            processPaymentResult.AddError(await _localizationService.GetResourceAsync("Plugins.Payments.Evance.Payment.Response.Error.MerchantSetting.Invalid"));
                                            return processPaymentResult;

                                        case "411":
                                            processPaymentResult.AddError(await _localizationService.GetResourceAsync("Plugins.Payments.Evance.Payment.Response.Error.Evance.PaymentAccount.InActive"));
                                            return processPaymentResult;

                                        case "420":
                                            processPaymentResult.AddError(await _localizationService.GetResourceAsync("Plugins.Payments.Evance.Payment.Response.Error.Communication"));
                                            return processPaymentResult;

                                        case "421":
                                            processPaymentResult.AddError(await _localizationService.GetResourceAsync("Plugins.Payments.Evance.Payment.Response.Error.Communication"));
                                            return processPaymentResult;

                                        case "430":
                                            processPaymentResult.AddError(await _localizationService.GetResourceAsync("Plugins.Payments.Evance.Payment.Response.Error.Transaction.Duplicate"));
                                            return processPaymentResult;

                                        case "440":
                                            processPaymentResult.AddError(await _localizationService.GetResourceAsync("Plugins.Payments.Evance.Payment.Response.Error.TechnicalIssue"));
                                            return processPaymentResult;

                                        case "441":
                                            processPaymentResult.AddError(await _localizationService.GetResourceAsync("Plugins.Payments.Evance.Payment.Response.Error.Transaction.Invalid"));
                                            return processPaymentResult;

                                        case "460":
                                            processPaymentResult.AddError(await _localizationService.GetResourceAsync("Plugins.Payments.Evance.Payment.Response.Error.TransactionType.NotSupported"));
                                            return processPaymentResult;

                                        case "461":
                                            processPaymentResult.AddError(await _localizationService.GetResourceAsync("Plugins.Payments.Evance.Payment.Response.Error.CardType.NotSupported"));
                                            return processPaymentResult;

                                        default:
                                            break;
                                    }
                                }
                            }

                        }
                        if (!string.IsNullOrEmpty(checkSaveCard) || !string.IsNullOrEmpty(cvvBox))
                        {
                            if (responseModel.AvsResponse != null && responseModel.Response != "1" && responseModel.ResponseCode != "100")
                            {
                                switch (responseModel.AvsResponse)
                                {
                                    case "C":
                                        processPaymentResult.AddError(await _localizationService.GetResourceAsync("Plugins.Payments.Evance.Payment.Error.AVS.Address.Records"));
                                        return processPaymentResult;

                                    case "N":
                                        processPaymentResult.AddError(await _localizationService.GetResourceAsync("Plugins.Payments.Evance.Payment.Error.AVS.Address.Records"));
                                        return processPaymentResult;

                                    case "4":
                                        processPaymentResult.AddError(await _localizationService.GetResourceAsync("Plugins.Payments.Evance.Payment.Error.AVS.Address.NotMatched"));
                                        return processPaymentResult;

                                    case "U":
                                        processPaymentResult.AddError(await _localizationService.GetResourceAsync("Plugins.Payments.Evance.Payment.Error.AVS.Address.NotMatched"));
                                        return processPaymentResult;

                                    case "G":
                                        processPaymentResult.AddError(await _localizationService.GetResourceAsync("Plugins.Payments.Evance.Payment.Error.AVS.Country.NotMatched"));
                                        return processPaymentResult;

                                    case "I":
                                        processPaymentResult.AddError(await _localizationService.GetResourceAsync("Plugins.Payments.Evance.Payment.Error.AVS.Country.NotMatched"));
                                        return processPaymentResult;

                                    case "R":
                                        processPaymentResult.AddError(await _localizationService.GetResourceAsync("Plugins.Payments.Evance.Payment.Error.AVS.System.NotAvailable"));
                                        return processPaymentResult;

                                    case "E":
                                        processPaymentResult.AddError(await _localizationService.GetResourceAsync("Plugins.Payments.Evance.Payment.Error.AVS.NotSupported"));
                                        return processPaymentResult;

                                    default:
                                        break;
                                }
                            }
                            if (responseModel.CvvResponse != null && responseModel.Response != "1" && responseModel.ResponseCode != "100")
                            {
                                switch (responseModel.CvvResponse)
                                {
                                    case "M":
                                        break;

                                    case "N":
                                        processPaymentResult.AddError(await _localizationService.GetResourceAsync("Plugins.Payments.Evance.Payment.Error.CVV.NotValid"));
                                        return processPaymentResult;

                                    case "P":
                                        processPaymentResult.AddError(await _localizationService.GetResourceAsync("Plugins.Payments.Evance.Payment.Error.CVV.NotVerified"));
                                        return processPaymentResult;

                                    case "S":
                                        processPaymentResult.AddError(await _localizationService.GetResourceAsync("Plugins.Payments.Evance.Payment.Error.CVV.NotExist"));
                                        return processPaymentResult;

                                    case "U":
                                        processPaymentResult.AddError(await _localizationService.GetResourceAsync("Plugins.Payments.Evance.Payment.Error.CVV.User.NotVerified"));
                                        return processPaymentResult;

                                    default:
                                        break;
                                }
                            }
                        }

                        var ordeGuild = processPaymentRequest.OrderGuid.ToString();
                        await _genericAttributeService.SaveAttributeAsync(customer, $"{ordeGuild}- request", request);
                        await _genericAttributeService.SaveAttributeAsync(customer, processPaymentRequest.OrderGuid.ToString(), response);

                        if (checkSaveCard == "true" || checkSaveACH == "true")
                        {
                            if (!string.IsNullOrEmpty(customerVaultId))
                            {
                                var customerVault = new CustomerVault()
                                {
                                    CustomerId = customer.Id,
                                    CustomerVaultId = customerVaultId
                                };
                                await _customerVaultService.InsertCustomerVaultAsync(customerVault);
                            }
                        }
                        if (paymentType == "ach payment")
                        {
                            processPaymentResult.CaptureTransactionId = responseModel.TransactionId;
                            processPaymentResult.CaptureTransactionResult =
                                $"{responseModel.TransactionId},{responseModel.AuthCode}";
                            processPaymentResult.NewPaymentStatus = PaymentStatus.Pending;
                        }
                        else
                        {
                            switch (_evanceSettings.TransactionType)
                            {
                                case TransactionType.AuthorizeOnly:
                                    processPaymentResult.AuthorizationTransactionId = responseModel.TransactionId;
                                    processPaymentResult.AuthorizationTransactionCode =
                                        $"{responseModel.TransactionId},{responseModel.AuthCode}";
                                    processPaymentResult.NewPaymentStatus = PaymentStatus.Authorized;
                                    break;
                                case TransactionType.AuthorizeAndCapture:
                                    processPaymentResult.CaptureTransactionId = responseModel.TransactionId;
                                    processPaymentResult.CaptureTransactionResult =
                                        $"{responseModel.TransactionId},{responseModel.AuthCode}";
                                    processPaymentResult.NewPaymentStatus = PaymentStatus.Paid;
                                    break;
                            }
                        }

                        processPaymentResult.AvsResult = responseModel.AvsResponse;
                    }
                    else
                    {
                        processPaymentResult.AddError(responseModel?.ResponseMessage ?? await _localizationService.GetResourceAsync("Plugins.Payments.Evance.Payment.Failed"));
                    }
                }
                else
                {
                    processPaymentResult.Errors.Add(await _localizationService.GetResourceAsync("Plugins.Payments.Evance.Payment.Error.PaymentToken"));
                }
            }
            else
            {
                processPaymentResult.Errors.Add(await _localizationService.GetResourceAsync("Plugins.Payments.Evance.Payment.Error.PaymentToken"));
            }
            _httpContextAccessor.HttpContext?.Session.Clear();
            return processPaymentResult;
        }

        /// <summary>
        /// Post process payment (used by payment gateways that require redirecting to a third-party URL)
        /// </summary>
        /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public Task PostProcessPaymentAsync(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Captures payment
        /// </summary>
        /// <param name="capturePaymentRequest">Capture payment request</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the capture payment result
        /// </returns>
        public async Task<CapturePaymentResult> CaptureAsync(CapturePaymentRequest capturePaymentRequest)
        {
            var capturePaymentResult = new CapturePaymentResult();
            var transactionId = capturePaymentRequest.Order.AuthorizationTransactionId;
            if (capturePaymentRequest != null && !string.IsNullOrEmpty(transactionId))
            {
                var paymentAPIBaseUrl = _evanceSettings.PaymentAPIBaseUrl;
                if (string.IsNullOrEmpty(paymentAPIBaseUrl))
                {
                    capturePaymentResult.Errors.Add(await _localizationService.GetResourceAsync("Plugins.Payments.Evance.Payment.Error.InvalidApiKey"));
                    return capturePaymentResult;
                }

                if (string.IsNullOrEmpty(_evanceSettings.SecurityKey))
                {
                    capturePaymentResult.Errors.Add(await _localizationService.GetResourceAsync("Plugins.Payments.Evance.Payment.Error.InvalidSecretKey"));
                    return capturePaymentResult;
                }

                // Prepare request parameters
                var requestModel = new BaseRequestModel();
                requestModel.AddParameter("type", "capture");
                requestModel.AddParameter("security_key", _evanceSettings.SecurityKey);
                requestModel.AddParameter("transactionid", transactionId);
                requestModel.AddParameter("amount", Math.Round(capturePaymentRequest.Order.OrderTotal, 2).ToString());

                // Call API (ensure URL-encoded request)
                var (response, isSuccess, request) = await IntegratePaymentApi.WebApiRequestAsync<PaymentApiResponseModel>(url: paymentAPIBaseUrl, content: requestModel, method: HttpMethod.Post);

                var responseModel = new PaymentApiResponseModel();
                if (!string.IsNullOrEmpty(response))
                {
                    var queryParams = HttpUtility.ParseQueryString(response);
                    responseModel.Response = queryParams["response"] ?? string.Empty;
                    responseModel.ResponseMessage = queryParams["responsetext"] ?? string.Empty;
                    responseModel.AuthCode = queryParams["authcode"] ?? string.Empty;
                    responseModel.TransactionId = queryParams["transactionid"] ?? string.Empty;
                    responseModel.AvsResponse = queryParams["avsresponse"] ?? string.Empty;
                    responseModel.CvvResponse = queryParams["cvvresponse"] ?? string.Empty;
                    responseModel.OrderId = queryParams["orderid"] ?? string.Empty;
                    responseModel.Type = queryParams["type"] ?? string.Empty;
                    responseModel.ResponseCode = queryParams["response_code"] ?? string.Empty;
                }

                if (isSuccess && response != null && responseModel != null && !string.IsNullOrEmpty(responseModel.AuthCode) && !string.IsNullOrEmpty(responseModel.TransactionId))
                {
                    var note = new OrderNote()
                    {
                        CreatedOnUtc = DateTime.UtcNow,
                        OrderId = capturePaymentRequest.Order.Id,
                        Note = response
                    };
                    await _orderService.InsertOrderNoteAsync(note);

                    capturePaymentResult.CaptureTransactionId =
                                $"{responseModel.TransactionId}";
                    capturePaymentResult.CaptureTransactionResult =
                                $"{responseModel.TransactionId} {responseModel.AuthCode}";
                    capturePaymentResult.NewPaymentStatus = PaymentStatus.Paid;
                }

                if (isSuccess)
                    _notificationService.SuccessNotification($"{responseModel?.ResponseMessage}");
                else
                    _notificationService.ErrorNotification($"{responseModel?.ResponseMessage}");
            }
            else
            {
                capturePaymentResult.Errors.Add(await _localizationService.GetResourceAsync("Plugins.Payments.Evance.Payment.TransactionId.NotExist"));
            }
            return capturePaymentResult;
        }

        /// <summary>
        /// Refunds a payment
        /// </summary>
        /// <param name="refundPaymentRequest">Request</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public async Task<RefundPaymentResult> RefundAsync(RefundPaymentRequest refundPaymentRequest)
        {
            var refundPaymentResult = new RefundPaymentResult();
            var authorizationTransactionId = refundPaymentRequest.Order.AuthorizationTransactionId;
            var captureTransactionId = refundPaymentRequest.Order.CaptureTransactionId;

            var transactiontype = string.Empty;
            if (refundPaymentResult != null && (!string.IsNullOrEmpty(authorizationTransactionId) || !string.IsNullOrEmpty(captureTransactionId)))
            {
                var securityKey = _evanceSettings.SecurityKey;
                var paymentAPIBaseUrl = _evanceSettings.PaymentAPIBaseUrl;
                if (string.IsNullOrEmpty(paymentAPIBaseUrl))
                {
                    refundPaymentResult.Errors.Add(await _localizationService.GetResourceAsync("Plugins.Payments.Evance.Payment.Error.InvalidApiKey"));
                    return refundPaymentResult;
                }

                if (string.IsNullOrEmpty(securityKey))
                {
                    refundPaymentResult.Errors.Add(await _localizationService.GetResourceAsync("Plugins.Payments.Evance.Payment.Error.InvalidSecretKey"));
                    return refundPaymentResult;
                }
                var requestModel = new BaseRequestModel();
                var queryAPIBaseUrl = _evanceSettings.QueryAPIBaseUrl;
                var transactionId = refundPaymentRequest.Order.AuthorizationTransactionId ?? refundPaymentRequest.Order.CaptureTransactionId;

                requestModel.AddParameter("security_key", securityKey);
                requestModel.AddParameter("transaction_id", transactionId);

                // Call API (ensure URL-encoded request)
                var (queryResponse, queryIsSuccess, queryRequest) = await IntegratePaymentApi.WebApiRequestAsync<PaymentApiResponseModel>(url: queryAPIBaseUrl, content: requestModel, method: HttpMethod.Post);

                var parsedDocument = XDocument.Parse(queryResponse);
                var transactionType = parsedDocument.Descendants("transaction_type").FirstOrDefault()?.Value;
                var responseCode = parsedDocument.Descendants("response_code").FirstOrDefault()?.Value;
                var responseText = parsedDocument.Descendants("response_text").FirstOrDefault()?.Value;

                if (queryIsSuccess && responseCode == "100")
                {
                    if (transactionType == "ck")
                    {
                        refundPaymentResult.Errors.Add(await _localizationService.GetResourceAsync("Plugins.Payments.Evance.Refund.ACH.ErrorMessage"));
                        return refundPaymentResult;
                    }
                }
                else
                {
                    refundPaymentResult.Errors.Add(responseText);
                    return refundPaymentResult;
                }

                var isSettleTransaction = await _genericAttributeService.GetAttributeAsync<string>(refundPaymentRequest.Order, EvanceDefaults.IsTransactionSettleAttributeKey, refundPaymentRequest.Order.StoreId);

                var orderPaidDate = refundPaymentRequest.Order.PaidDateUtc?.ToString("MM/dd/yyyy");
                var amountToRefund = Math.Round(refundPaymentRequest.AmountToRefund, 2);
                var isVoid = false;

                var currentDate = DateTime.UtcNow.ToString("MM/dd/yyyy");
                bool isPaidToday = orderPaidDate == currentDate;

                if (!string.IsNullOrEmpty(isSettleTransaction) && isSettleTransaction.ToLower() == "true")
                {
                    transactiontype = "refund";
                    requestModel.AddParameter("amount", amountToRefund.ToString());
                }
                else
                {
                    if (isPaidToday && (amountToRefund == refundPaymentRequest.Order.OrderTotal))
                    {
                        transactiontype = "void";
                        isVoid = true;
                    }
                    else
                    {
                        transactiontype = "refund";
                        requestModel.AddParameter("amount", amountToRefund.ToString());
                    }
                }
                
                var transactionid = authorizationTransactionId ?? captureTransactionId;

                // Prepare request parameters
                requestModel.AddParameter("type", transactiontype);
                requestModel.AddParameter("security_key", securityKey);
                requestModel.AddParameter("transactionid", transactionid);

                // Call API (ensure URL-encoded request)
                var (response, isSuccess, request) = await IntegratePaymentApi.WebApiRequestAsync<PaymentApiResponseModel>(url: paymentAPIBaseUrl, content: requestModel, method: HttpMethod.Post);

                var responseModel = new PaymentApiResponseModel();
                if (!string.IsNullOrEmpty(response))
                {
                    var queryParams = HttpUtility.ParseQueryString(response);
                    responseModel.Response = queryParams["response"] ?? string.Empty;
                    responseModel.ResponseMessage = queryParams["responsetext"] ?? string.Empty;
                    responseModel.AuthCode = queryParams["authcode"] ?? string.Empty;
                    responseModel.TransactionId = queryParams["transactionid"] ?? string.Empty;
                    responseModel.AvsResponse = queryParams["avsresponse"] ?? string.Empty;
                    responseModel.CvvResponse = queryParams["cvvresponse"] ?? string.Empty;
                    responseModel.OrderId = queryParams["orderid"] ?? string.Empty;
                    responseModel.Type = queryParams["type"] ?? string.Empty;
                    responseModel.ResponseCode = queryParams["response_code"] ?? string.Empty;
                }

                if (isSuccess && response != null && responseModel != null && !string.IsNullOrEmpty(responseModel.TransactionId)
                        && responseModel.ResponseCode != null && responseModel.ResponseCode == "100" && responseModel.Response == "1")
                {
                    if (isVoid)
                    {
                        _logger.Information($"Refund final void: {refundPaymentRequest.IsPartialRefund}");
                        refundPaymentResult.NewPaymentStatus = PaymentStatus.Voided;
                    }
                    else
                    {
                        _logger.Information($"Refund final: {refundPaymentRequest.IsPartialRefund}");
                        refundPaymentResult.NewPaymentStatus = refundPaymentRequest.IsPartialRefund ? PaymentStatus.PartiallyRefunded : PaymentStatus.Refunded;

                    }
                }
                else
                {
                    refundPaymentResult.Errors.Add(responseModel?.ResponseMessage);
                    return refundPaymentResult;
                }

                if (isSuccess && responseModel.ResponseCode == "100" && responseModel.Response == "1")
                {
                    var note = new OrderNote()
                    {
                        CreatedOnUtc = DateTime.UtcNow,
                        OrderId = refundPaymentRequest.Order.Id,
                        Note = response ?? responseModel.ResponseMessage
                    };
                    await _orderService.InsertOrderNoteAsync(note);

                    _notificationService.SuccessNotification($"{responseModel?.ResponseMessage}");
                }
                else
                    _notificationService.ErrorNotification($"{responseModel?.ResponseMessage}");
            }
            else
            {
                refundPaymentResult.Errors.Add(await _localizationService.GetResourceAsync("Plugins.Payments.Evance.Payment.TransactionId.NotExist"));
            }
            return refundPaymentResult;
        }

        /// <summary>
        /// Voids a payment
        /// </summary>
        /// <param name="voidPaymentRequest">Request</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public async Task<VoidPaymentResult> VoidAsync(VoidPaymentRequest voidPaymentRequest)
        {
            var voidPaymentResult = new VoidPaymentResult();
            var transactionId = voidPaymentRequest.Order.AuthorizationTransactionId;

            if (!string.IsNullOrEmpty(transactionId))
            {
                var baseUrl = _evanceSettings.PaymentAPIBaseUrl;
                if (string.IsNullOrEmpty(baseUrl))
                {
                    voidPaymentResult.Errors.Add(await _localizationService.GetResourceAsync("Plugins.Payments.Evance.Payment.Error.InvalidApiKey"));
                    return voidPaymentResult;
                }
                if (string.IsNullOrEmpty(_evanceSettings.SecurityKey))
                {
                    voidPaymentResult.Errors.Add(await _localizationService.GetResourceAsync("Plugins.Payments.Evance.Payment.Error.InvalidSecretKey"));
                    return voidPaymentResult;
                }

                // Prepare request parameters
                var requestModel = new BaseRequestModel();
                requestModel.AddParameter("type", "void");
                requestModel.AddParameter("security_key", _evanceSettings.SecurityKey);
                requestModel.AddParameter("transactionid", transactionId);

                // Call API (ensure URL-encoded request)
                var (response, isSuccess, request) = await IntegratePaymentApi.WebApiRequestAsync<PaymentApiResponseModel>(url: baseUrl, content: requestModel, method: HttpMethod.Post);

                if (!string.IsNullOrEmpty(response))
                {
                    var queryParams = HttpUtility.ParseQueryString(response);
                    var responseModel = new PaymentApiResponseModel
                    {
                        Response = queryParams["response"] ?? string.Empty,
                        ResponseMessage = queryParams["responsetext"] ?? string.Empty,
                        AuthCode = queryParams["authcode"] ?? string.Empty,
                        TransactionId = queryParams["transactionid"] ?? string.Empty,
                        AvsResponse = queryParams["avsresponse"] ?? string.Empty,
                        CvvResponse = queryParams["cvvresponse"] ?? string.Empty,
                        OrderId = queryParams["orderid"] ?? string.Empty,
                        Type = queryParams["type"] ?? string.Empty,
                        ResponseCode = queryParams["response_code"] ?? string.Empty
                    };

                    if (isSuccess && !string.IsNullOrEmpty(responseModel.AuthCode) && !string.IsNullOrEmpty(responseModel.TransactionId))
                        voidPaymentResult.NewPaymentStatus = PaymentStatus.Voided;

                    if (isSuccess && responseModel.Response == "1" && responseModel.ResponseCode == "100")
                    {
                        var note = new OrderNote()
                        {
                            CreatedOnUtc = DateTime.UtcNow,
                            OrderId = voidPaymentRequest.Order.Id,
                            Note = response ?? responseModel.ResponseMessage,
                        };
                        await _orderService.InsertOrderNoteAsync(note);

                        _notificationService.SuccessNotification(responseModel.ResponseMessage);
                    }
                    else
                    {
                        _notificationService.ErrorNotification(responseModel.ResponseMessage);
                    }
                }
            }
            else
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Plugins.Payments.Evance.Payment.TransactionId.NotExist"));
            }

            return voidPaymentResult;
        }

        /// <summary>
        /// Process recurring payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the process payment result
        /// </returns>
        public Task<ProcessPaymentResult> ProcessRecurringPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            return Task.FromResult(new ProcessPaymentResult { });
        }

        /// <summary>
        /// Cancels a recurring payment
        /// </summary>
        /// <param name="cancelPaymentRequest">Request</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public Task<CancelRecurringPaymentResult> CancelRecurringPaymentAsync(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            return Task.FromResult(new CancelRecurringPaymentResult());
        }

        /// <summary>
        /// Gets a value indicating whether customers can complete a payment after order is placed but not completed (for redirection payment methods)
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public Task<bool> CanRePostProcessPaymentAsync(Order order)
        {
            //it's not a redirection payment method. So we always return false
            return Task.FromResult(false);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether capture is supported
        /// </summary>
        public bool SupportCapture => true;

        /// <summary>
        /// Gets a value indicating whether partial refund is supported
        /// </summary>
        public bool SupportPartiallyRefund => true;

        /// <summary>
        /// Gets a value indicating whether refund is supported
        /// </summary>
        public bool SupportRefund => true;

        /// <summary>
        /// Gets a value indicating whether void is supported
        /// </summary>
        public bool SupportVoid => true;

        /// <summary>
        /// Gets a recurring payment type of payment method
        /// </summary>
        public RecurringPaymentType RecurringPaymentType => RecurringPaymentType.NotSupported; 

        /// <summary>
        /// Gets a payment method type
        /// </summary>
        public PaymentMethodType PaymentMethodType => PaymentMethodType.Standard; 

        /// <summary>
        /// Gets a value indicating whether we should display a payment information page for this plugin
        /// </summary>
        public bool SkipPaymentInfo => false;

        /// <summary>
        /// Gets a value indicating whether to hide this plugin on the widget list page in the admin area
        /// </summary>
        public bool HideInWidgetList => false;

        #endregion
    }
}
