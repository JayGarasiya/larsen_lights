using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Nop.Core;
using Nop.Core.Domain;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Gdpr;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Core.Events;
using Nop.Core.Http;
using Nop.Core.Http.Extensions;
using Nop.Services.Attributes;
using Nop.Services.Authentication;
using Nop.Services.Authentication.External;
using Nop.Services.Authentication.MultiFactor;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.ExportImport;
using Nop.Services.Gdpr;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Tax;
using Nop.Web.Components;
using Nop.Web.Controllers;
using Nop.Web.Factories;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Models.Checkout;
using Nop.Web.Models.Common;
using Nop.Web.Models.Customer;
using Nop.Web.Models.ShoppingCart;
using System.Text.Encodings.Web;

namespace Nop.Plugin.OnePage.Checkout.Controllers
{
    public class OverrideCustomerController : CustomerController
    {
        #region Fields

        protected readonly OnePageCheckoutSettings _onePageCheckoutSettings;
        protected readonly IWebHelper _webHelper;
        protected readonly IShoppingCartService _shoppingCartService;
        protected readonly ShippingSettings _shippingSettings;
        protected readonly IPaymentPluginManager _paymentPluginManager;
        protected readonly IOrderTotalCalculationService _orderTotalCalculationService;
        protected readonly IQueuedEmailService _queuedEmailService;
        protected readonly IEmailAccountService _emailAccountService;
        protected readonly IEmailSender _emailSender;
        protected readonly IOrderProcessingService _orderProcessingService;

        #endregion

        #region Ctor

        public OverrideCustomerController(AddressSettings addressSettings,
            CaptchaSettings captchaSettings,
            CustomerSettings customerSettings,
            DateTimeSettings dateTimeSettings,
            ForumSettings forumSettings,
            GdprSettings gdprSettings,
            HtmlEncoder htmlEncoder,
            IAddressModelFactory addressModelFactory,
            IAddressService addressService,
            IAttributeParser<AddressAttribute, AddressAttributeValue> addressAttributeParser,
            IAttributeParser<CustomerAttribute, CustomerAttributeValue> customerAttributeParser,
            IAttributeService<CustomerAttribute, CustomerAttributeValue> customerAttributeService,
            IAuthenticationService authenticationService, ICountryService countryService,
            ICurrencyService currencyService,
            ICustomerActivityService customerActivityService,
            ICustomerModelFactory customerModelFactory,
            ICustomerRegistrationService customerRegistrationService,
            ICustomerService customerService,
            IDownloadService downloadService,
            IEventPublisher eventPublisher,
            IExportManager exportManager,
            IExternalAuthenticationService externalAuthenticationService,
            IGdprService gdprService,
            IGenericAttributeService genericAttributeService,
            IGiftCardService giftCardService,
            ILocalizationService localizationService,
            ILogger logger,
            IMultiFactorAuthenticationPluginManager multiFactorAuthenticationPluginManager,
            INewsLetterSubscriptionService newsLetterSubscriptionService,
            INotificationService notificationService,
            IOrderService orderService,
            IPermissionService permissionService,
            IPictureService pictureService,
            IPriceFormatter priceFormatter,
            IProductService productService,
            IStateProvinceService stateProvinceService,
            IStoreContext storeContext,
            ITaxService taxService,
            IWorkContext workContext,
            IWorkflowMessageService workflowMessageService,
            LocalizationSettings localizationSettings,
            MediaSettings mediaSettings,
            MultiFactorAuthenticationSettings multiFactorAuthenticationSettings,
            StoreInformationSettings storeInformationSettings,
            TaxSettings taxSettings, IShoppingCartService shoppingCartService, ShippingSettings shippingSettings, IPaymentPluginManager paymentPluginManager, IOrderTotalCalculationService orderTotalCalculationService, IQueuedEmailService queuedEmailService, IEmailAccountService emailAccountService, IEmailSender emailSender, OnePageCheckoutSettings onePageCheckoutSettings, IWebHelper webHelper, IOrderProcessingService orderProcessingService)
            : base(addressSettings,
                  captchaSettings,
                  customerSettings,
                  dateTimeSettings,
                  forumSettings,
                  gdprSettings,
                  htmlEncoder,
                  addressModelFactory,
                  addressService,
                  addressAttributeParser,
                  customerAttributeParser,
                  customerAttributeService,
                  authenticationService,
                  countryService,
                  currencyService,
                  customerActivityService,
                  customerModelFactory,
                  customerRegistrationService,
                  customerService,
                  downloadService,
                  eventPublisher,
                  exportManager,
                  externalAuthenticationService,
                  gdprService,
                  genericAttributeService,
                  giftCardService,
                  localizationService,
                  logger,
                  multiFactorAuthenticationPluginManager,
                  newsLetterSubscriptionService,
                  notificationService,
                  orderService,
                  permissionService,
                  pictureService,
                  priceFormatter,
                  productService,
                  stateProvinceService,
                  storeContext,
                  taxService,
                  workContext,
                  workflowMessageService,
                  localizationSettings,
                  mediaSettings,
                  multiFactorAuthenticationSettings,
                  storeInformationSettings,
                  taxSettings)
        {
            _onePageCheckoutSettings = onePageCheckoutSettings;
            _webHelper = webHelper;
            _shoppingCartService = shoppingCartService;
            _shippingSettings = shippingSettings;
            _paymentPluginManager = paymentPluginManager;
            _orderTotalCalculationService = orderTotalCalculationService;
            _queuedEmailService = queuedEmailService;
            _emailAccountService = emailAccountService;
            _emailSender = emailSender;
            _orderProcessingService = orderProcessingService;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Reset data required for checkout
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="storeId">Store identifier</param>
        /// <param name="clearBilling">A value indicating whether to clear billing address</param>
        /// <param name="clearShipping">A value indicating whether to clear shipping address</param>
        /// <param name="clearRewardPoints">A value indicating whether to clear "Use reward points" flag</param>
        /// <param name="clearShippingMethod">A value indicating whether to clear selected shipping method</param>
        /// <param name="clearPaymentMethod">A value indicating whether to clear selected payment method</param>
        /// <param name="clearPaymentInfo">A value indicating whether to clear selected payment method info</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        private async Task ResetCheckoutDataAsync(Customer customer, int storeId,
            bool clearBilling = false, bool clearShipping = false,
            bool clearShippingMethod = false, bool clearRewardPoints = false,
            bool clearPaymentMethod = false)
        {
            if (customer == null)
                throw new ArgumentNullException();

            //clear billing address flag
            if (clearBilling)
                customer.BillingAddressId = null;

            //clear shipping address flag
            if (clearShipping)
                customer.ShippingAddressId = null;

            //clear selected shipping method
            if (clearShippingMethod)
            {
                await _genericAttributeService.SaveAttributeAsync<ShippingOption>(customer, NopCustomerDefaults.SelectedShippingOptionAttribute, null, storeId);
                await _genericAttributeService.SaveAttributeAsync<PickupPoint>(customer, NopCustomerDefaults.SelectedPickupPointAttribute, null, storeId);
            }

            //clear reward points flag
            if (clearRewardPoints)
                await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.UseRewardPointsDuringCheckoutAttribute, false, storeId);

            //clear selected payment method
            if (clearPaymentMethod)
                await _genericAttributeService.SaveAttributeAsync<string>(customer, NopCustomerDefaults.SelectedPaymentMethodAttribute, null, storeId);

            await _customerService.UpdateCustomerAsync(customer);
        }

        /// <summary>
        /// Prepare the order review data model
        /// </summary>
        /// <param name="cart">List of the shopping cart item</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the order review data model
        /// </returns>
        protected virtual async Task<ShoppingCartModel.OrderReviewDataModel> PrepareOrderReviewDataModelAsync(IList<ShoppingCartItem> cart)
        {
            ArgumentNullException.ThrowIfNull(cart);

            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();
            var model = new ShoppingCartModel.OrderReviewDataModel
            {
                Display = true
            };

            //billing info
            var billingAddress = await _customerService.GetCustomerBillingAddressAsync(customer);
            if (billingAddress != null)
            {
                await _addressModelFactory.PrepareAddressModelAsync(model.BillingAddress,
                        address: billingAddress,
                        excludeProperties: false,
                        addressSettings: _addressSettings);
            }
            else
                model.BillingAddress = null;

            //shipping info
            if (await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart))
            {
                model.IsShippable = true;

                var pickupPoint = await _genericAttributeService.GetAttributeAsync<PickupPoint>(customer,
                    NopCustomerDefaults.SelectedPickupPointAttribute, store.Id);
                model.SelectedPickupInStore = _shippingSettings.AllowPickupInStore && pickupPoint != null;
                if (!model.SelectedPickupInStore)
                {
                    if (await _customerService.GetCustomerShippingAddressAsync(customer) is Address address)
                    {
                        await _addressModelFactory.PrepareAddressModelAsync(model.ShippingAddress,
                            address: address,
                            excludeProperties: false,
                            addressSettings: _addressSettings);
                    }
                    else
                        model.ShippingAddress = null;
                }
                else
                {
                    var country = await _countryService.GetCountryByTwoLetterIsoCodeAsync(pickupPoint.CountryCode);
                    var state = await _stateProvinceService.GetStateProvinceByAbbreviationAsync(pickupPoint.StateAbbreviation, country?.Id);

                    model.PickupAddress = new AddressModel
                    {
                        Address1 = pickupPoint.Address,
                        City = pickupPoint.City,
                        County = pickupPoint.County,
                        CountryName = country?.Name ?? string.Empty,
                        StateProvinceName = state?.Name ?? string.Empty,
                        ZipPostalCode = pickupPoint.ZipPostalCode,
                        CountryId = country?.Id,
                        StateProvinceId = state?.Id
                    };

                    var address = new Address
                    {
                        CountryId = model.PickupAddress.CountryId,
                        StateProvinceId = model.PickupAddress.StateProvinceId,
                        City = model.PickupAddress.City,
                        County = model.PickupAddress.County,
                        Address1 = model.PickupAddress.Address1,
                        ZipPostalCode = model.PickupAddress.ZipPostalCode
                    };
                }

                //selected shipping method
                var shippingOption = await _genericAttributeService.GetAttributeAsync<ShippingOption>(customer,
                    NopCustomerDefaults.SelectedShippingOptionAttribute, store.Id);
                if (shippingOption != null)
                    model.ShippingMethod = shippingOption.Name;
            }

            //payment info
            var selectedPaymentMethodSystemName = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.SelectedPaymentMethodAttribute, store.Id);
            var paymentMethod = await _paymentPluginManager
                .LoadPluginBySystemNameAsync(selectedPaymentMethodSystemName, customer, store.Id);
            model.PaymentMethod = paymentMethod != null
                ? await _localizationService.GetLocalizedFriendlyNameAsync(paymentMethod, (await _workContext.GetWorkingLanguageAsync()).Id)
                : string.Empty;

            //custom values
            var processPaymentRequest = await _orderProcessingService.GetProcessPaymentRequestAsync();
            model.CustomValues.AddRange(processPaymentRequest?.CustomValues?.Where(value => value.DisplayToCustomer).ToList() ?? new());

            return model;
        }

        #endregion

        #region Methods

        #region Login / Register

        //available even when a store is closed
        [CheckAccessClosedStore(true)]
        //available even when navigation is not allowed
        [CheckAccessPublicStore(true)]
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task<IActionResult> Login(bool? checkoutAsGuest)
        {
            if (!_onePageCheckoutSettings.LoginRegisterPopUp)
            {
                var model = await _customerModelFactory.PrepareLoginModelAsync(checkoutAsGuest);
                return View("~/Views/Customer/Login.cshtml", model); 
            }

            // Otherwise, continue with the custom popup-based login
            var customModel = await _customerModelFactory.PrepareLoginModelAsync(checkoutAsGuest);
            return View("~/Plugins/OnePage.Checkout/Views/Customer/Login.cshtml", customModel); 
        }

        [HttpPost]
        [ValidateCaptcha]
        //available even when a store is close
        [CheckAccessClosedStore(true)]
        //available even when navigation is not allowed
        [CheckAccessPublicStore(true)]
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task<IActionResult> Login(LoginModel model, string returnUrl, bool captchaValid)
        {
            if (!_onePageCheckoutSettings.LoginRegisterPopUp || !_webHelper.IsAjaxRequest(Request))
                return await base.Login(model, returnUrl, captchaValid);

            //validate CAPTCHA
            if (_captchaSettings.Enabled && _captchaSettings.ShowOnLoginPage && !captchaValid)
            {
                ModelState.AddModelError("", await _localizationService.GetResourceAsync("Common.WrongCaptchaMessage"));
            }

            if (ModelState.IsValid)
            {
                var customerUserName = model.Username;
                var customerEmail = model.Email;
                var userNameOrEmail = _customerSettings.UsernamesEnabled ? customerUserName : customerEmail;

                var loginResult = await _customerRegistrationService.ValidateCustomerAsync(userNameOrEmail, model.Password);
                switch (loginResult)
                {
                    case CustomerLoginResults.Successful:
                        {
                            var customer = _customerSettings.UsernamesEnabled
                                ? await _customerService.GetCustomerByUsernameAsync(customerUserName)
                                : await _customerService.GetCustomerByEmailAsync(customerEmail);

                            return Json(new { success = true, returnUrl = await _customerRegistrationService.SignInCustomerAsync(customer, returnUrl, model.RememberMe) });
                        }
                    case CustomerLoginResults.MultiFactorAuthenticationRequired:
                        {
                            var customerMultiFactorAuthenticationInfo = new CustomerMultiFactorAuthenticationInfo
                            {
                                UserName = userNameOrEmail,
                                RememberMe = model.RememberMe,
                                ReturnUrl = returnUrl
                            };
                            await HttpContext.Session.SetAsync(NopCustomerDefaults.CustomerMultiFactorAuthenticationInfo, customerMultiFactorAuthenticationInfo);
                            return Json(new { success = true, returnUrl = Url.RouteUrl(NopRouteNames.Standard.MULTIFACTOR_VERIFICATION) });
                        }
                    case CustomerLoginResults.CustomerNotExist:
                        ModelState.AddModelError("", await _localizationService.GetResourceAsync("Account.Login.WrongCredentials.CustomerNotExist"));
                        break;
                    case CustomerLoginResults.Deleted:
                        ModelState.AddModelError("", await _localizationService.GetResourceAsync("Account.Login.WrongCredentials.Deleted"));
                        break;
                    case CustomerLoginResults.NotActive:
                        ModelState.AddModelError("", await _localizationService.GetResourceAsync("Account.Login.WrongCredentials.NotActive"));
                        break;
                    case CustomerLoginResults.NotRegistered:
                        ModelState.AddModelError("", await _localizationService.GetResourceAsync("Account.Login.WrongCredentials.NotRegistered"));
                        break;
                    case CustomerLoginResults.LockedOut:
                        ModelState.AddModelError("", await _localizationService.GetResourceAsync("Account.Login.WrongCredentials.LockedOut"));
                        break;
                    case CustomerLoginResults.WrongPassword:
                    default:
                        ModelState.AddModelError("", await _localizationService.GetResourceAsync("Account.Login.WrongCredentials"));
                        break;
                }
                if (loginResult == CustomerLoginResults.WrongPassword && _customerSettings.NotifyFailedLoginAttempt)
                {
                    var customer = _customerSettings.UsernamesEnabled
                            ? await _customerService.GetCustomerByUsernameAsync(customerUserName)
                            : await _customerService.GetCustomerByEmailAsync(customerEmail);

                    await _workflowMessageService.SendCustomerFailedLoginAttemptNotificationAsync(customer, customer.LanguageId ?? 0);
                }
                await _customerActivityService.InsertActivityAsync("PublicStore.FailedLogin",
                    string.Format(await _localizationService.GetResourceAsync("ActivityLog.PublicStore.Login.Fail"), _customerSettings.UsernamesEnabled ? customerUserName : customerEmail));
            }

            //If we got this far, something failed, display errors
            return Json(new { success = false, ModelState });
        }

        //available even when navigation is not allowed
        [CheckAccessPublicStore(true)]
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task<IActionResult> Register(string returnUrl)
        {
            if (!_onePageCheckoutSettings.LoginRegisterPopUp)
            {
                if (_customerSettings.UserRegistrationType == UserRegistrationType.Disabled)
                    return RedirectToRoute(NopRouteNames.Standard.REGISTER_RESULT,new { resultId = (int)UserRegistrationType.Disabled, returnUrl });

                var model = new RegisterModel();
                model = await _customerModelFactory.PrepareRegisterModelAsync(model, excludeProperties: false, setDefaultValues: true);

                return View("~/Views/Customer/Register.cshtml", model);
            }

            if (!_webHelper.IsAjaxRequest(Request))
                return await base.Register(returnUrl);

            if (_customerSettings.UserRegistrationType == UserRegistrationType.Disabled)
                return RedirectToRoute(NopRouteNames.Standard.REGISTER_RESULT,
                    new { resultId = (int)UserRegistrationType.Disabled, returnUrl });

            var popupModel = new RegisterModel();
            popupModel = await _customerModelFactory.PrepareRegisterModelAsync(
                popupModel,
                excludeProperties: false,
                setDefaultValues: true
            );

            return View("~/Plugins/OnePage.Checkout/Views/Customer/Register.cshtml", popupModel);
        }

        [HttpPost]
        [ValidateCaptcha]
        [ValidateHoneypot]
        //available even when navigation is not allowed
        [CheckAccessPublicStore(true)]
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task<IActionResult> Register(RegisterModel model, string returnUrl, bool captchaValid, IFormCollection form)
        {
            if (!_onePageCheckoutSettings.LoginRegisterPopUp || !_webHelper.IsAjaxRequest(Request))
            {
                return await base.Register(model, returnUrl, captchaValid, form);
            }

            var language = await _workContext.GetWorkingLanguageAsync();
            var store = await _storeContext.GetCurrentStoreAsync();
            var customer = await _workContext.GetCurrentCustomerAsync();

            //check whether registration is allowed
            if (_customerSettings.UserRegistrationType == UserRegistrationType.Disabled)
                return Json(new { success = true, returnUrl = Url.RouteUrl(NopRouteNames.Standard.REGISTER_RESULT, new { resultId = (int)UserRegistrationType.Disabled, returnUrl }) });

            if (await _customerService.IsRegisteredAsync(customer))
            {
                //Already registered customer. 
                await _authenticationService.SignOutAsync();

                //raise logged out event       
                await _eventPublisher.PublishAsync(new CustomerLoggedOutEvent(customer));
                customer = await _customerService.InsertGuestCustomerAsync();
                //Save a new record
                await _workContext.SetCurrentCustomerAsync(customer);
            }
            customer.RegisteredInStoreId = store.Id;

            //custom customer attributes
            var customerAttributesXml = await ParseCustomCustomerAttributesAsync(form);
            var customerAttributeWarnings = await _customerAttributeParser.GetAttributeWarningsAsync(customerAttributesXml);
            foreach (var error in customerAttributeWarnings)
            {
                ModelState.AddModelError("", error);
            }

            //validate CAPTCHA
            if (_captchaSettings.Enabled && _captchaSettings.ShowOnRegistrationPage && !captchaValid)
            {
                ModelState.AddModelError("", await _localizationService.GetResourceAsync("Common.WrongCaptchaMessage"));
            }

            //GDPR
            if (_gdprSettings.GdprEnabled)
            {
                var consents = (await _gdprService
                    .GetAllConsentsAsync()).Where(consent => consent.DisplayDuringRegistration && consent.IsRequired).ToList();

                ValidateRequiredConsents(consents, form);
            }

            if (ModelState.IsValid)
            {
                var customerUserName = model.Username;
                var customerEmail = model.Email;

                var isApproved = _customerSettings.UserRegistrationType == UserRegistrationType.Standard;
                var registrationRequest = new CustomerRegistrationRequest(customer,
                    customerEmail,
                    _customerSettings.UsernamesEnabled ? customerUserName : customerEmail,
                    model.Password,
                    _customerSettings.DefaultPasswordFormat,
                    store.Id,
                    isApproved);
                var registrationResult = await _customerRegistrationService.RegisterCustomerAsync(registrationRequest);
                if (registrationResult.Success)
                {
                    //properties
                    if (_dateTimeSettings.AllowCustomersToSetTimeZone)
                        customer.TimeZoneId = model.TimeZoneId;
                    //VAT number
                    if (_taxSettings.EuVatEnabled)
                    {
                        customer.VatNumber = model.VatNumber;

                        var (vatNumberStatus, _, vatAddress) = await _taxService.GetVatNumberStatusAsync(model.VatNumber);
                        customer.VatNumberStatusId = (int)vatNumberStatus;
                        //send VAT number admin notification
                        if (!string.IsNullOrEmpty(model.VatNumber) && _taxSettings.EuVatEmailAdminWhenNewVatSubmitted)
                            await _workflowMessageService.SendNewVatSubmittedStoreOwnerNotificationAsync(customer, model.VatNumber, vatAddress, _localizationSettings.DefaultAdminLanguageId);
                    }

                    //form fields
                    if (_customerSettings.GenderEnabled)
                        customer.Gender = model.Gender;
                    if (_customerSettings.FirstNameEnabled)
                        customer.FirstName = model.FirstName;
                    if (_customerSettings.LastNameEnabled)
                        customer.LastName = model.LastName;
                    if (_customerSettings.DateOfBirthEnabled)
                        customer.DateOfBirth = model.ParseDateOfBirth();
                    if (_customerSettings.CompanyEnabled)
                        customer.Company = model.Company;
                    if (_customerSettings.StreetAddressEnabled)
                        customer.StreetAddress = model.StreetAddress;
                    if (_customerSettings.StreetAddress2Enabled)
                        customer.StreetAddress2 = model.StreetAddress2;
                    if (_customerSettings.ZipPostalCodeEnabled)
                        customer.ZipPostalCode = model.ZipPostalCode;
                    if (_customerSettings.CityEnabled)
                        customer.City = model.City;
                    if (_customerSettings.CountyEnabled)
                        customer.County = model.County;
                    if (_customerSettings.CountryEnabled)
                        customer.CountryId = model.CountryId;
                    if (_customerSettings.CountryEnabled && _customerSettings.StateProvinceEnabled)
                        customer.StateProvinceId = model.StateProvinceId;
                    if (_customerSettings.PhoneEnabled)
                        customer.Phone = model.Phone;
                    if (_customerSettings.FaxEnabled)
                        customer.Fax = model.Fax;

                    //newsletter
                    if (_customerSettings.NewsletterEnabled)
                    {
                        var anyNewSubscriptions = false;
                        var isNewsletterActive = _customerSettings.UserRegistrationType != UserRegistrationType.EmailValidation;
                        var activeSubscriptions = model.NewsLetterSubscriptions.Where(subscriptionModel => subscriptionModel.IsActive);
                        var currentSubscriptions = await _newsLetterSubscriptionService
                            .GetNewsLetterSubscriptionsByEmailAsync(customerEmail, storeId: store.Id);
                        if (currentSubscriptions.Any())
                        {
                            var subscriptionGuid = currentSubscriptions.FirstOrDefault().NewsLetterSubscriptionGuid;
                            foreach (var activeSubscription in activeSubscriptions)
                            {
                                var existingSubscription = currentSubscriptions
                                    ?.FirstOrDefault(subscription => subscription.TypeId == activeSubscription.TypeId);
                                if (existingSubscription is not null)
                                {
                                    if (!existingSubscription.Active && isNewsletterActive)
                                    {
                                        existingSubscription.Active = true;
                                        existingSubscription.LanguageId = customer.LanguageId ?? language.Id;
                                        await _newsLetterSubscriptionService.UpdateNewsLetterSubscriptionAsync(existingSubscription);
                                    }
                                }
                                else
                                {
                                    await _newsLetterSubscriptionService.InsertNewsLetterSubscriptionAsync(new()
                                    {
                                        NewsLetterSubscriptionGuid = subscriptionGuid,
                                        Email = customer.Email,
                                        Active = isNewsletterActive,
                                        TypeId = activeSubscription.TypeId,
                                        StoreId = store.Id,
                                        LanguageId = customer.LanguageId ?? language.Id,
                                        CreatedOnUtc = DateTime.UtcNow
                                    });
                                    anyNewSubscriptions = true;
                                }
                            }
                        }
                        else
                        {
                            var subscriptionGuid = Guid.NewGuid();
                            foreach (var activeSubscription in activeSubscriptions)
                            {
                                await _newsLetterSubscriptionService.InsertNewsLetterSubscriptionAsync(new()
                                {
                                    NewsLetterSubscriptionGuid = subscriptionGuid,
                                    Email = customer.Email,
                                    Active = isNewsletterActive,
                                    TypeId = activeSubscription.TypeId,
                                    StoreId = store.Id,
                                    LanguageId = customer.LanguageId ?? language.Id,
                                    CreatedOnUtc = DateTime.UtcNow
                                });
                                anyNewSubscriptions = true;
                            }
                        }

                        //GDPR
                        if (anyNewSubscriptions && _gdprSettings.GdprEnabled && _gdprSettings.LogNewsletterConsent)
                        {
                            var consentMessage = await _localizationService.GetResourceAsync("Gdpr.Consent.Newsletter");
                            await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ConsentAgree, consentMessage);
                        }
                    }

                    if (_customerSettings.AcceptPrivacyPolicyEnabled)
                    {
                        //privacy policy is required
                        //GDPR
                        if (_gdprSettings.GdprEnabled && _gdprSettings.LogPrivacyPolicyConsent)
                        {
                            await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ConsentAgree, await _localizationService.GetResourceAsync("Gdpr.Consent.PrivacyPolicy"));
                        }
                    }

                    //GDPR
                    if (_gdprSettings.GdprEnabled)
                    {
                        var consents = (await _gdprService.GetAllConsentsAsync()).Where(consent => consent.DisplayDuringRegistration).ToList();
                        foreach (var consent in consents)
                        {
                            var controlId = $"consent{consent.Id}";
                            var cbConsent = form[controlId];
                            if (!StringValues.IsNullOrEmpty(cbConsent) && cbConsent.ToString().Equals("on"))
                            {
                                //agree
                                await _gdprService.InsertLogAsync(customer, consent.Id, GdprRequestType.ConsentAgree, consent.Message);
                            }
                            else
                            {
                                //disagree
                                await _gdprService.InsertLogAsync(customer, consent.Id, GdprRequestType.ConsentDisagree, consent.Message);
                            }
                        }
                    }

                    //save customer attributes
                    customer.CustomCustomerAttributesXML = customerAttributesXml;
                    await _customerService.UpdateCustomerAsync(customer);

                    //insert default address (if possible)
                    var defaultAddress = new Address
                    {
                        FirstName = customer.FirstName,
                        LastName = customer.LastName,
                        Email = customer.Email,
                        Company = customer.Company,
                        CountryId = customer.CountryId > 0
                            ? (int?)customer.CountryId
                            : null,
                        StateProvinceId = customer.StateProvinceId > 0
                            ? (int?)customer.StateProvinceId
                            : null,
                        County = customer.County,
                        City = customer.City,
                        Address1 = customer.StreetAddress,
                        Address2 = customer.StreetAddress2,
                        ZipPostalCode = customer.ZipPostalCode,
                        PhoneNumber = customer.Phone,
                        FaxNumber = customer.Fax,
                        CreatedOnUtc = customer.CreatedOnUtc
                    };
                    if (await _addressService.IsAddressValidAsync(defaultAddress))
                    {
                        //some validation
                        if (defaultAddress.CountryId == 0)
                            defaultAddress.CountryId = null;
                        if (defaultAddress.StateProvinceId == 0)
                            defaultAddress.StateProvinceId = null;
                        //set default address
                        //customer.Addresses.Add(defaultAddress);

                        await _addressService.InsertAddressAsync(defaultAddress);

                        await _customerService.InsertCustomerAddressAsync(customer, defaultAddress);

                        customer.BillingAddressId = defaultAddress.Id;
                        customer.ShippingAddressId = defaultAddress.Id;

                        await _customerService.UpdateCustomerAsync(customer);
                    }

                    //notifications
                    if (_customerSettings.NotifyNewCustomerRegistration)
                        await _workflowMessageService.SendCustomerRegisteredStoreOwnerNotificationMessageAsync(customer,
                            _localizationSettings.DefaultAdminLanguageId);

                    //raise event       
                    await _eventPublisher.PublishAsync(new CustomerRegisteredEvent(customer));

                    switch (_customerSettings.UserRegistrationType)
                    {
                        case UserRegistrationType.EmailValidation:
                            //email validation message
                            await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.AccountActivationTokenAttribute, Guid.NewGuid().ToString());
                            await _workflowMessageService.SendCustomerEmailValidationMessageAsync(customer, (await _workContext.GetWorkingLanguageAsync()).Id);

                            //result
                            return Json(new { success = true, returnUrl = Url.RouteUrl(NopRouteNames.Standard.REGISTER_RESULT, new { resultId = (int)UserRegistrationType.EmailValidation, returnUrl }) });

                        case UserRegistrationType.AdminApproval:
                            return Json(new { success = true, returnUrl = Url.RouteUrl(NopRouteNames.Standard.REGISTER_RESULT, new { resultId = (int)UserRegistrationType.AdminApproval, returnUrl }) });

                        case UserRegistrationType.Standard:
                            //send customer welcome message
                            await _workflowMessageService.SendCustomerWelcomeMessageAsync(customer, (await _workContext.GetWorkingLanguageAsync()).Id);

                            //raise event       
                            await _eventPublisher.PublishAsync(new CustomerActivatedEvent(customer));

                            returnUrl = Url.RouteUrl(NopRouteNames.Standard.REGISTER_RESULT, new { resultId = (int)UserRegistrationType.Standard, returnUrl });
                            return Json(new { success = true, returnUrl = await _customerRegistrationService.SignInCustomerAsync(customer, returnUrl, true) });

                        default:
                            return Json(new { success = true, returnUrl = Url.RouteUrl(NopRouteNames.General.HOMEPAGE) });
                    }
                }

                //errors
                foreach (var error in registrationResult.Errors)
                    ModelState.AddModelError("", error);
            }

            //If we got this far, something failed, display errors
            return Json(new { success = false, ModelState });
        }

        #endregion

        #region Password recovery

        [ValidateCaptcha]
        [HttpPost, ActionName("PasswordRecovery")]
        [FormValueRequired("send-email")]
        //available even when navigation is not allowed
        [CheckAccessPublicStore(ignore: true)]
        //available even when a store is closed
        [CheckAccessClosedStore(ignore: true)]
        public override async Task<IActionResult> PasswordRecoverySend(PasswordRecoveryModel model, bool captchaValid)
        {
            // validate CAPTCHA
            if (_captchaSettings.Enabled && _captchaSettings.ShowOnForgotPasswordPage && !captchaValid)
            {
                ModelState.AddModelError("", await _localizationService.GetResourceAsync("Common.WrongCaptchaMessage"));
            }

            if (ModelState.IsValid)
            {
                var customer = await _customerService.GetCustomerByEmailAsync(model.Email);
                if (customer != null && customer.Active && !customer.Deleted)
                {
                    var linkExpired = await _customerService.IsPasswordRecoveryLinkExpiredAsync(customer);
                    var token = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.PasswordRecoveryTokenAttribute);
                    var tokenValid = await _customerService.IsPasswordRecoveryTokenValidAsync(customer, token);
                    if (!tokenValid)
                    {
                        //save token and current date
                        var passwordRecoveryToken = Guid.NewGuid();
                        await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.PasswordRecoveryTokenAttribute,
                            passwordRecoveryToken.ToString());

                        DateTime? generatedDateTime = DateTime.UtcNow;
                        await _genericAttributeService.SaveAttributeAsync(customer,
                            NopCustomerDefaults.PasswordRecoveryTokenDateGeneratedAttribute, generatedDateTime);
                    }

                    if (linkExpired)
                    {
                        DateTime? generatedDateTime = DateTime.UtcNow;
                        await _genericAttributeService.SaveAttributeAsync(customer,
                            NopCustomerDefaults.PasswordRecoveryTokenDateGeneratedAttribute, generatedDateTime);
                    }

                    //send email
                    var queuedEmailsByIds = await _workflowMessageService.SendCustomerPasswordRecoveryMessageAsync(customer,
                        (await _workContext.GetWorkingLanguageAsync()).Id);

                    //send email instant
                    var queuedEmails = await _queuedEmailService.GetQueuedEmailsByIdsAsync(queuedEmailsByIds.ToArray());
                    foreach (var queuedEmail in queuedEmails)
                    {
                        var bcc = string.IsNullOrWhiteSpace(queuedEmail.Bcc)
                                    ? null
                                    : queuedEmail.Bcc.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                        var cc = string.IsNullOrWhiteSpace(queuedEmail.CC)
                                    ? null
                                    : queuedEmail.CC.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                        try
                        {
                            await _emailSender.SendEmailAsync(await _emailAccountService.GetEmailAccountByIdAsync(queuedEmail.EmailAccountId),
                                queuedEmail.Subject,
                                queuedEmail.Body,
                                queuedEmail.From,
                                queuedEmail.FromName,
                                queuedEmail.To,
                                queuedEmail.ToName,
                                queuedEmail.ReplyTo,
                                queuedEmail.ReplyToName,
                                bcc,
                                cc,
                                queuedEmail.AttachmentFilePath,
                                queuedEmail.AttachmentFileName,
                                queuedEmail.AttachedDownloadId);

                            queuedEmail.SentOnUtc = DateTime.UtcNow;
                        }
                        catch (Exception exc)
                        {
                            await _logger.ErrorAsync($"Error sending e-mail. {exc.Message}", exc);
                        }
                        finally
                        {
                            queuedEmail.SentTries += 1;
                            await _queuedEmailService.UpdateQueuedEmailAsync(queuedEmail);
                        }
                    }


                    _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Account.PasswordRecovery.EmailHasBeenSent"));
                }
                else
                {
                    _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Account.PasswordRecovery.EmailNotFound"));
                }
            }

            model = await _customerModelFactory.PreparePasswordRecoveryModelAsync(model);

            return View(model);
        }

        #endregion

        #region Update Checkout order Summary 

        [IgnoreAntiforgeryToken]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> UpdateOrderSummary(string step, bool? prepareData)
        {
            try
            {
                var customer = await _workContext.GetCurrentCustomerAsync();
                var store = await _storeContext.GetCurrentStoreAsync();
                var currency = await _workContext.GetWorkingCurrencyAsync();
                //reset data as per current active step
                switch (step)
                {
                    case "opc-billing":
                        await ResetCheckoutDataAsync(customer, store.Id, true, true, true, true, true);
                        break;
                    case "opc-shipping":
                        await ResetCheckoutDataAsync(customer, store.Id, false, true, true, true, true);
                        break;
                    case "opc-shipping_method":
                        await ResetCheckoutDataAsync(customer, store.Id, false, false, true, true, true);
                        break;
                    case "opc-payment_method":
                        await ResetCheckoutDataAsync(customer, store.Id, false, false, false, true, true);
                        break;
                    default:
                        break;
                }

                //if not passed, then create a new model
                var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);
                var reviewDataHtml = string.Empty;
                if (prepareData.HasValue ? prepareData.Value : false)
                {
                    var reviewDataModel = await PrepareOrderReviewDataModelAsync(cart);
                    reviewDataHtml = await RenderPartialViewToStringAsync("_OrderReviewData", reviewDataModel);
                }

                //order total (and applied discounts, gift cards, reward points)
                var (orderTotal, _, _, _, _, _) = await _orderTotalCalculationService.GetShoppingCartTotalAsync(cart);
                var orderTotalString = string.Empty;
                if (orderTotal.HasValue)
                {
                    var shoppingCartTotal = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(orderTotal.Value, currency);
                    orderTotalString = await _priceFormatter.FormatPriceAsync(shoppingCartTotal, true, false);
                }
                else
                {
                    //subtotal
                    var subTotalIncludingTax = await _workContext.GetTaxDisplayTypeAsync() == TaxDisplayType.IncludingTax && !_taxSettings.ForceTaxExclusionFromOrderSubtotal;
                    var (_, _, _, subTotalWithoutDiscountBase, _) = await _orderTotalCalculationService.GetShoppingCartSubTotalAsync(cart, subTotalIncludingTax);
                    var subtotalBase = subTotalWithoutDiscountBase;
                    var subtotal = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(subtotalBase, currency);
                    orderTotalString = await _priceFormatter.FormatPriceAsync(subtotal, false, currency, (await _workContext.GetWorkingLanguageAsync()).Id, subTotalIncludingTax);
                }

                return Json(new
                {
                    update_section = new UpdateSectionJsonModel
                    {
                        name = "order-summary",
                        html = await RenderViewComponentToStringAsync(typeof(OrderSummaryViewComponent), new { prepareAndDisplayOrderReviewData = false })
                    },
                    name = "checkout-summary",
                    html = reviewDataHtml,
                    ordertotalstring = orderTotalString
                });
            }
            catch (Exception exc)
            {
                await _logger.WarningAsync(exc.Message, exc, await _workContext.GetCurrentCustomerAsync());
                return Json(new { error = 1, message = exc.Message });
            }
        }

        #endregion

        #endregion
    }
}
