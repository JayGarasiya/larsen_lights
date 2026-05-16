using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Payments.Evance.BaseApiCall;
using Nop.Plugin.Payments.Evance.Domain;
using Nop.Plugin.Payments.Evance.Models;
using Nop.Plugin.Payments.Evance.Service;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Orders;
using Nop.Web.Framework.Components;
using System.Net;
using System.Xml.Linq;

namespace Nop.Plugin.Payments.Evance.Components
{
    /// <summary>
    /// Represents payment info view component
    /// </summary>
    public class PaymentEvanceViewComponent : NopViewComponent
    {
        #region Fields

        private readonly IWorkContext _workContext;
        private readonly EvanceSettings _evanceSettings;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly ICustomerService _customerService;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly ICustomerVaultService _customerVaultService;

        #endregion

        #region Ctor

        public PaymentEvanceViewComponent(IWorkContext workContext,
            EvanceSettings evanceSettings,
            IShoppingCartService shoppingCartService,
            ICustomerService customerService,
            ICountryService countryService,
            IStateProvinceService stateProvinceService,
            IOrderTotalCalculationService orderTotalCalculationService,
            ICustomerVaultService customerVaultService)
        {
            _workContext = workContext;
            _evanceSettings = evanceSettings;
            _shoppingCartService = shoppingCartService;
            _customerService = customerService;
            _countryService = countryService;
            _stateProvinceService = stateProvinceService;
            _orderTotalCalculationService = orderTotalCalculationService;
            _customerVaultService = customerVaultService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Invoke view component
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the view component result
        /// </returns>
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = new PaymentInfoModel();
            var browser = string.Empty;
            var customerName = string.Empty;
            var form = Request?.Form;
            var billingAddress = new Core.Domain.Common.Address();
            var userAgent = Request?.Headers["User-Agent"].ToString();

            if (!string.IsNullOrEmpty(userAgent))
            {
                if (userAgent.Contains("Edg"))
                    browser = "Edge";
                else if (userAgent.Contains("Chrome") && !userAgent.Contains("Chromium"))
                    browser = "Chrome";
                else if (userAgent.Contains("Safari") && !userAgent.Contains("Chrome"))
                    browser = "Safari";
                else if (userAgent.Contains("Firefox"))
                    browser = "Firefox";
            }
            
            if (Request?.Method != WebRequestMethods.Http.Get)
            {
                // Define payment oprions logos
                var paymentLogos = new Dictionary<PaymentOptions, string>
                {
                    { PaymentOptions.Card_Payment, "/Plugins/Payments.Evance/Content/Images/Card-payment.png" },
                    { PaymentOptions.Google_Pay, "/Plugins/Payments.Evance/Content/Images/GPay.png" },
                    { PaymentOptions.Apple_Pay, "/Plugins/Payments.Evance/Content/Images/Apple-icon.png" },
                    { PaymentOptions.ACH_Payment, "/Plugins/Payments.Evance/Content/Images/ach.png" }
                };

                model.PaymentMethods = _evanceSettings.PaymentOptions
                    .Where(id => !((browser == "Chrome" || browser == "Firefox" || browser == "Edge") && id == (int)PaymentOptions.Apple_Pay)) // Exclude GPay for Safari, firefox and Edge
                    .Select(id => new PaymentMethodModel
                    {
                        Name = ((PaymentOptions)id).ToString().Replace("_", " "),
                        LogoUrl = paymentLogos.TryGetValue((PaymentOptions)id, out var logoUrl) ? logoUrl : "",
                        PaymentMethodSystemName = ((PaymentOptions)id).ToString(),
                        Selected = true
                    })
                    .ToList();

                var customer = await _workContext.GetCurrentCustomerAsync();
                billingAddress = await _customerService.GetCustomerBillingAddressAsync(customer);

                if (await _customerService.IsGuestAsync(customer))
                    customerName = $"{billingAddress.FirstName} {billingAddress.LastName}";
                else
                    customerName = $"{customer?.FirstName ?? string.Empty} {customer?.LastName ?? string.Empty}".Trim();
                
                var currency = await _workContext.GetWorkingCurrencyAsync();
                var cart = await _shoppingCartService.GetShoppingCartAsync(customer);
                var shoppingCartTotal = (await _orderTotalCalculationService.GetShoppingCartTotalAsync(cart)).shoppingCartTotal;

                var shippingAddress = await _customerService.GetCustomerShippingAddressAsync(customer);
                var shippingCountry = await _countryService.GetCountryByIdAsync(shippingAddress.CountryId?? 0);
                var shippingStateAbbreviation = shippingAddress.StateProvinceId.HasValue
                        ? (await _stateProvinceService.GetStateProvinceByIdAsync(shippingAddress.StateProvinceId.Value))?.Abbreviation ?? string.Empty
                        : string.Empty;
                
                var billingCountry = await _countryService.GetCountryByIdAsync(billingAddress.CountryId ?? 0);
                var billingStateAbbreviation = billingAddress.StateProvinceId.HasValue
                        ? (await _stateProvinceService.GetStateProvinceByIdAsync(billingAddress.StateProvinceId.Value))?.Abbreviation ?? string.Empty
                        : string.Empty;

                if (customer != null)
                {
                    var customerVaults = await _customerVaultService.GetCustomerVaultsByCustomerIdAsync(customer.Id);
                    var securityKey = _evanceSettings.SecurityKey;
                    var queryAPIBaseUrl = _evanceSettings.QueryAPIBaseUrl;

                    foreach (var customerVault in customerVaults)
                    {
                        var requestModel = new BaseRequestModel();

                        requestModel.AddParameter("security_key", securityKey);
                        requestModel.AddParameter("customer_vault_id", customerVault.CustomerVaultId);

                        // Call API (ensure URL-encoded request)
                        var (response, isSuccess, requst) = await IntegratePaymentApi.WebApiRequestAsync<PaymentApiResponseModel>(url: queryAPIBaseUrl, content: requestModel, method: HttpMethod.Post);

                        var parsedDocument = XDocument.Parse(response);
                        var customerCardNumber = parsedDocument.Descendants("cc_number").FirstOrDefault()?.Value;
                        var accountType = parsedDocument.Descendants("account_type").FirstOrDefault()?.Value;
                        var expirationDate = parsedDocument.Descendants("cc_exp").FirstOrDefault()?.Value;
                        var accountNumber = parsedDocument.Descendants("check_account").FirstOrDefault()?.Value;
                        var cardType = parsedDocument.Descendants("cc_type").FirstOrDefault()?.Value.Trim().Replace(" ", "");

                        if (string.IsNullOrEmpty(accountType))
                        {
                            if (!string.IsNullOrEmpty(expirationDate) && !string.IsNullOrEmpty(customerCardNumber) && !string.IsNullOrEmpty(cardType))
                            {
                                var formattedDate = expirationDate.Insert(2, "/");

                                model.CardInformation.Add(new CustomerCardInfo
                                {
                                    CustomerCardNumber = customerCardNumber,
                                    CustomerExpirationDate = formattedDate,
                                    CardType = cardType,
                                    CustomerVaultId = customerVault.CustomerVaultId,
                                });
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(accountNumber))
                            {
                                model.ACHInformation.Add(new CustomerACHInfo
                                {
                                    CustomerAccountNumber = accountNumber,
                                    CustomerVaultId = customerVault.CustomerVaultId,
                                });
                            }
                        }

                    }
                }

                if (_evanceSettings.CustomerVaultEnabled)
                {
                    model.AllowSaveCard = _evanceSettings.AllowSaveCard;
                    model.AllowSaveACH = _evanceSettings.AllowSaveACH;
                }

                if (_evanceSettings.ThreedSEnable)
                {
                    if(_evanceSettings.ExcludeThreeDSToCustomerRoles.Count() > 0)
                    {
                        var customerRoles = (await _customerService.GetCustomerRolesAsync(customer)).Select(x=>x.Id).ToList();

                        if (_evanceSettings.ExcludeThreeDSToCustomerRoles.Any(id => customerRoles.Contains(id)))
                            model.ThreedSJSEnable = "false";
                        else
                            model.ThreedSJSEnable = "true";
                    }
                    else
                        model.ThreedSJSEnable = "true";
                }
                else
                    model.ThreedSJSEnable = "false";

                model.CardholderName = customerName;
                model.Amount = shoppingCartTotal.ToString() ?? string.Empty;
                model.IsPaymentOptions = _evanceSettings.PaymentOptions.Count > 0;
                model.SelectedPaymentOptionIds = _evanceSettings.PaymentOptions;
                model.Email = customer?.Email ?? string.Empty;
                model.CurrencyCode = currency.CurrencyCode;
                model.CollectCheckoutKey = _evanceSettings.CollectCheckoutKey;

                //billing address details
                model.BillingFirstName = billingAddress.FirstName;
                model.BillingLastName = billingAddress.LastName;
                model.BillingPhoneNumber = billingAddress.PhoneNumber;
                model.BillingCity = billingAddress.City;
                model.BillingZip = billingAddress.ZipPostalCode;
                model.BillingAddress1 = billingAddress.Address1;
                model.BillingCountryCode = billingCountry.TwoLetterIsoCode;
                model.BillingStateAbbreviation = billingStateAbbreviation;
                model.BillingAddress2 = billingAddress.Address2;

            }

            return View("~/Plugins/Payments.Evance/Views/PaymentInfo.cshtml", model);
        }

        #endregion
    }
}
