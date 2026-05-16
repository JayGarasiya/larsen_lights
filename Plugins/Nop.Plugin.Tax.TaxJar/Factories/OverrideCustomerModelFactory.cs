using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Gdpr;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Vendors;
using Nop.Services.Attributes;
using Nop.Services.Authentication.External;
using Nop.Services.Authentication.MultiFactor;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Gdpr;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Stores;
using Nop.Web.Factories;
using Nop.Web.Models.Customer;

namespace Nop.Plugin.Tax.TaxJar.Factories;

/// <summary>
/// Customizes the customer registration model to integrate with TaxJar settings.
/// </summary>
public class OverrideCustomerModelFactory : CustomerModelFactory
{
    #region Fields
    private readonly TaxJarSettings _taxJarSettings;
    #endregion

    #region Ctor
    public OverrideCustomerModelFactory(
        AddressSettings addressSettings,
        CaptchaSettings captchaSettings,
        CatalogSettings catalogSettings,
        CommonSettings commonSettings,
        CustomerSettings customerSettings,
        DateTimeSettings dateTimeSettings,
        ExternalAuthenticationSettings externalAuthenticationSettings,
        ForumSettings forumSettings,
        GdprSettings gdprSettings,
        IAddressModelFactory addressModelFactory,
        IAttributeParser<CustomerAttribute, CustomerAttributeValue> customerAttributeParser,
        IAttributeService<CustomerAttribute, CustomerAttributeValue> customerAttributeService,
        IAuthenticationPluginManager authenticationPluginManager,
        ICountryService countryService,
        ICustomerService customerService,
        IDateTimeHelper dateTimeHelper,
        IExternalAuthenticationModelFactory externalAuthenticationModelFactory,
        IExternalAuthenticationService externalAuthenticationService,
        IGdprService gdprService,
        IGenericAttributeService genericAttributeService,
        ILocalizationService localizationService,
        IMultiFactorAuthenticationPluginManager multiFactorAuthenticationPluginManager,
        INewsLetterSubscriptionService newsLetterSubscriptionService,
        INewsLetterSubscriptionTypeService newsLetterSubscriptionTypeService,
        IOrderService orderService,
        IPermissionService permissionService,
        IPictureService pictureService,
        IProductService productService,
        IReturnRequestService returnRequestService,
        IStateProvinceService stateProvinceService,
        IStoreContext storeContext,
        IStoreMappingService storeMappingService,
        IUrlRecordService urlRecordService,
        IWorkContext workContext,
        MediaSettings mediaSettings,
        OrderSettings orderSettings,
        RewardPointsSettings rewardPointsSettings,
        SecuritySettings securitySettings,
        TaxSettings taxSettings,
        VendorSettings vendorSettings,
        TaxJarSettings taxJarSettings) : base(
            addressSettings,
            captchaSettings,
            catalogSettings,
            commonSettings,
            customerSettings,
            dateTimeSettings,
            externalAuthenticationSettings,
            forumSettings,
            gdprSettings,
            addressModelFactory,
            customerAttributeParser,
            customerAttributeService,
            authenticationPluginManager,
            countryService,
            customerService,
            dateTimeHelper,
            externalAuthenticationModelFactory,
            externalAuthenticationService,
            gdprService,
            genericAttributeService,
            localizationService,
            multiFactorAuthenticationPluginManager,
            newsLetterSubscriptionService,
            newsLetterSubscriptionTypeService,
            orderService,
            permissionService,
            pictureService,
            productService,
            returnRequestService,
            stateProvinceService,
            storeContext,
            storeMappingService,
            urlRecordService,
            workContext,
            mediaSettings,
            orderSettings,
            rewardPointsSettings,
            securitySettings,
            taxSettings,
            vendorSettings)
    {
        _taxJarSettings = taxJarSettings;
    }
    #endregion

    #region Methods

    /// <summary>
    /// Prepare the customer register model
    /// </summary>
    /// <param name="model">Customer register model</param>
    /// <param name="excludeProperties">Whether to exclude populating of model properties from the entity</param>
    /// <param name="overrideCustomCustomerAttributesXml">Overridden customer attributes in XML format; pass null to use CustomCustomerAttributes of customer</param>
    /// <param name="setDefaultValues">Whether to populate model properties by default values</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the customer register model
    /// </returns>
    public override async Task<RegisterModel> PrepareRegisterModelAsync(
        RegisterModel model,
        bool excludeProperties,
        string overrideCustomCustomerAttributesXml = "",
        bool setDefaultValues = false)
    {
        ArgumentNullException.ThrowIfNull(model);

        var customer = await _workContext.GetCurrentCustomerAsync();

        // Time zone settings
        model.AllowCustomersToSetTimeZone = _dateTimeSettings.AllowCustomersToSetTimeZone;
        foreach (var tzi in _dateTimeHelper.GetSystemTimeZones())
            model.AvailableTimeZones.Add(new SelectListItem
            {
                Text = tzi.DisplayName,
                Value = tzi.Id,
                Selected = excludeProperties ? tzi.Id == model.TimeZoneId : tzi.Id == (await _dateTimeHelper.GetCurrentTimeZoneAsync()).Id
            });

        // VAT settings
        model.DisplayVatNumber = _taxSettings.EuVatEnabled;
        if (_taxSettings.EuVatEnabled && _taxSettings.EuVatEnabledForGuests)
            model.VatNumber = customer.VatNumber;

        // Form field visibility and requirements
        model.FirstNameEnabled = _customerSettings.FirstNameEnabled;
        model.LastNameEnabled = _customerSettings.LastNameEnabled;
        model.FirstNameRequired = _customerSettings.FirstNameRequired;
        model.LastNameRequired = _customerSettings.LastNameRequired;
        model.GenderEnabled = _customerSettings.GenderEnabled;
        model.DateOfBirthEnabled = _customerSettings.DateOfBirthEnabled;
        model.DateOfBirthRequired = _customerSettings.DateOfBirthRequired;
        model.CompanyEnabled = _customerSettings.CompanyEnabled;
        model.CompanyRequired = _customerSettings.CompanyRequired;
        model.StreetAddressEnabled = _customerSettings.StreetAddressEnabled;
        model.StreetAddressRequired = _customerSettings.StreetAddressRequired;
        model.StreetAddress2Enabled = _customerSettings.StreetAddress2Enabled;
        model.StreetAddress2Required = _customerSettings.StreetAddress2Required;
        model.ZipPostalCodeEnabled = _customerSettings.ZipPostalCodeEnabled;
        model.ZipPostalCodeRequired = _customerSettings.ZipPostalCodeRequired;
        model.CityEnabled = _customerSettings.CityEnabled;
        model.CityRequired = _customerSettings.CityRequired;
        model.CountyEnabled = _customerSettings.CountyEnabled;
        model.CountyRequired = _customerSettings.CountyRequired;
        model.CountryEnabled = _customerSettings.CountryEnabled;
        model.CountryRequired = _customerSettings.CountryRequired;
        model.StateProvinceEnabled = _customerSettings.StateProvinceEnabled;
        model.StateProvinceRequired = _customerSettings.StateProvinceRequired;
        model.PhoneEnabled = _customerSettings.PhoneEnabled;
        model.PhoneRequired = _customerSettings.PhoneRequired;
        model.FaxEnabled = _customerSettings.FaxEnabled;
        model.FaxRequired = _customerSettings.FaxRequired;
        model.NewsletterEnabled = _customerSettings.NewsletterEnabled;
        model.AcceptPrivacyPolicyEnabled = _customerSettings.AcceptPrivacyPolicyEnabled;
        model.AcceptPrivacyPolicyPopup = _commonSettings.PopupForTermsOfServiceLinks;
        model.UsernamesEnabled = _customerSettings.UsernamesEnabled;
        model.CheckUsernameAvailabilityEnabled = _customerSettings.CheckUsernameAvailabilityEnabled;
        model.HoneypotEnabled = _securitySettings.HoneypotEnabled;
        model.DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnRegistrationPage;
        model.EnteringEmailTwice = _customerSettings.EnteringEmailTwice;

        // Set default values if required
        if (setDefaultValues)
        {
            model.VatNumber = customer.VatNumber;
            model.Gender = customer.Gender;
            var dateOfBirth =customer.DateOfBirth;
            if (dateOfBirth.HasValue)
            {
                model.DateOfBirthDay = dateOfBirth.Value.Day;
                model.DateOfBirthMonth = dateOfBirth.Value.Month;
                model.DateOfBirthYear = dateOfBirth.Value.Year;
            }
            var billingAddress = await _customerService.GetCustomerBillingAddressAsync(customer);
            if (billingAddress != null)
            {
                model.FirstName = billingAddress.FirstName;
                model.LastName = billingAddress.LastName;
                model.Email = billingAddress.Email;
                model.Company = billingAddress.Company;
                model.CountryId = billingAddress.CountryId ?? 0;
                model.StateProvinceId = billingAddress.StateProvinceId ?? 0;
                model.County = billingAddress.County;
                model.City = billingAddress.City;
                model.StreetAddress = billingAddress.Address1;
                model.StreetAddress2 = billingAddress.Address2;
                model.ZipPostalCode = billingAddress.ZipPostalCode;
                model.Phone = billingAddress.PhoneNumber;
                model.Fax = billingAddress.FaxNumber;
            }
            else
            {
                model.FirstName = customer.FirstName;
                model.LastName = customer.LastName;
                model.Company = customer.Company;
                model.StreetAddress = customer.StreetAddress;
                model.StreetAddress2 = customer.StreetAddress2; 
                model.ZipPostalCode = customer.ZipPostalCode;   
                model.City = customer.City;
                model.County = customer.County;
                model.CountryId = customer.CountryId;
                model.StateProvinceId = customer.StateProvinceId;
                model.Phone = customer.Phone;
                model.Fax = customer.Fax;
            }

            //enable newsletter by default
            model.NewsletterEnabled = _customerSettings.NewsletterEnabled;
        }

        // Countries and States settings
        if (_customerSettings.CountryEnabled)
        {
            model.AvailableCountries.Add(new SelectListItem { Text = await _localizationService.GetResourceAsync("Address.SelectCountry"), Value = "0" });

            if (model.CountryId == 0)
                model.CountryId = _taxJarSettings.CountryId;

            var currentLanguage = await _workContext.GetWorkingLanguageAsync();
            foreach (var c in await _countryService.GetAllCountriesAsync(currentLanguage.Id))
            {
                model.AvailableCountries.Add(new SelectListItem
                {
                    Text = await _localizationService.GetLocalizedAsync(c, x => x.Name),
                    Value = c.Id.ToString(),
                    Selected = c.Id == model.CountryId
                });
            }

            if (_customerSettings.StateProvinceEnabled)
            {
                var states = (await _stateProvinceService.GetStateProvincesByCountryIdAsync(model.CountryId, currentLanguage.Id)).ToList();
                if (states.Count != 0)
                {
                    model.AvailableStates.Add(new SelectListItem { Text = await _localizationService.GetResourceAsync("Address.SelectState"), Value = "0" });

                    foreach (var s in states)
                    {
                        model.AvailableStates.Add(new SelectListItem {
                            Text = await _localizationService.GetLocalizedAsync(s, x => x.Name),
                            Value = s.Id.ToString(),
                            Selected = (s.Id == model.StateProvinceId) });
                    }
                }
                else
                {
                    var anyCountrySelected = model.AvailableCountries.Any(x => x.Selected);

                    model.AvailableStates.Add(new SelectListItem
                    {
                        Text = await _localizationService.GetResourceAsync(anyCountrySelected ? "Address.Other" : "Address.SelectState"),
                        Value = "0"
                    });
                }

            }
        }

        // Custom customer attributes
        var customAttributes = await PrepareCustomCustomerAttributesAsync(customer, overrideCustomCustomerAttributesXml);
        foreach (var attribute in customAttributes)
            model.CustomerAttributes.Add(attribute);

        // GDPR consents if enabled
        if (_gdprSettings.GdprEnabled)
        {
            var consents = (await _gdprService.GetAllConsentsAsync()).Where(consent => consent.DisplayDuringRegistration).ToList();
            foreach (var consent in consents)
            {
                model.GdprConsents.Add(await PrepareGdprConsentModelAsync(consent, false));
            }
        }

        return model;
    }
    #endregion
}
