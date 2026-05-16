using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Services.Attributes;
using Nop.Services.Common;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Web.Factories;
using Nop.Web.Models.Common;

namespace Nop.Plugin.Tax.TaxJar.Factories;

/// <summary>
/// Overrides the AddressModelFactory to customize address-related logic for TaxJar.
/// </summary>
public class OverrideAddressModelFactory : AddressModelFactory
{
    #region Fields
    private readonly TaxJarSettings _taxJarSettings;
    #endregion

    #region Ctor
    public OverrideAddressModelFactory(
        AddressSettings addressSettings,
        IAddressService addressService,
        IAttributeFormatter<AddressAttribute, AddressAttributeValue> addressAttributeFormatter,
        IAttributeParser<AddressAttribute, AddressAttributeValue> addressAttributeParser,
        IAttributeService<AddressAttribute, AddressAttributeValue> addressAttributeService,
        ICountryService countryService,
        ILocalizationService localizationService,
        IStateProvinceService stateProvinceService,
        IWorkContext workContext,
        TaxJarSettings taxJarSettings) : base(
            addressSettings,
            addressService,
            addressAttributeFormatter,
            addressAttributeParser,
            addressAttributeService,
            countryService,
            localizationService,
            stateProvinceService,
            workContext)
    {
        _taxJarSettings = taxJarSettings;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Prepares the address model, populating it with values from the address entity or customer.
    /// </summary>
    /// <param name="model">The address model to populate.</param>
    /// <param name="address">The address entity to fetch data from.</param>
    /// <param name="excludeProperties">Flag indicating whether to exclude populating model properties.</param>
    /// <param name="addressSettings">Settings related to address form fields.</param>
    /// <param name="loadCountries">Function to load countries.</param>
    /// <param name="prePopulateWithCustomerFields">Flag to prepopulate with customer fields.</param>
    /// <param name="customer">Customer entity (required if prePopulateWithCustomerFields is true).</param>
    /// <param name="overrideAttributesXml">Overridden address attributes in XML format.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public override async Task PrepareAddressModelAsync(
        AddressModel model,
        Address address, bool excludeProperties,
        AddressSettings addressSettings,
        Func<Task<IList<Country>>> loadCountries = null,
        bool prePopulateWithCustomerFields = false,
        Customer customer = null,
        string overrideAttributesXml = "")
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(addressSettings);

        // Populate model with address data if available
        if (!excludeProperties && address != null)
        {
            model.Id = address.Id;
            model.FirstName = address.FirstName;
            model.LastName = address.LastName;
            model.Email = address.Email;
            model.Company = address.Company;
            model.CountryId = address.CountryId;
            model.CountryName = await _countryService.GetCountryByAddressAsync(address) is Country country ? await _localizationService.GetLocalizedAsync(country, x => x.Name) : null;
            model.StateProvinceId = address.StateProvinceId;
            model.StateProvinceName = await _stateProvinceService.GetStateProvinceByAddressAsync(address) is StateProvince stateProvince ? await _localizationService.GetLocalizedAsync(stateProvince, x => x.Name) : null;
            model.County = address.County;
            model.City = address.City;
            model.Address1 = address.Address1;
            model.Address2 = address.Address2;
            model.ZipPostalCode = address.ZipPostalCode;
            model.PhoneNumber = address.PhoneNumber;
            model.FaxNumber = address.FaxNumber;
        }

        // Prepopulate address model with customer fields if needed
        if (address == null && prePopulateWithCustomerFields)
        {
            if (customer == null)
                throw new Exception("Customer cannot be null when prepopulating an address");
            model.Email = customer.Email;
            model.FirstName = customer.FirstName;
            model.LastName = customer.LastName;
            model.Company = customer.Company;
            model.Address1 = customer.StreetAddress;
            model.Address2 = customer.StreetAddress2;
            model.ZipPostalCode = customer.ZipPostalCode;
            model.City = customer.City;
            model.County = customer.County;
            model.CountryId = customer.CountryId;
            model.StateProvinceId = customer.StateProvinceId;
            model.PhoneNumber = customer.Phone;
            model.FaxNumber = customer.Fax;
        }

        // Load countries and states if country-enabled and addressSettings are available
        if (addressSettings.CountryEnabled && loadCountries != null)
        {
            var countries = await loadCountries();

            // Preselect the country if only one is available
            if (_addressSettings.PreselectCountryIfOnlyOne && countries.Count == 1)
                model.CountryId = countries[0].Id;
            else
            {
                model.AvailableCountries.Add(new SelectListItem { Text = await _localizationService.GetResourceAsync("Address.SelectCountry"), Value = "0" });
                if (address == null && !model.CountryId.HasValue)
                    model.CountryId = _taxJarSettings.CountryId;
            }

            // Populate available countries list
            foreach (var c in countries)
            {
                model.AvailableCountries.Add(new SelectListItem
                {
                    Text = await _localizationService.GetLocalizedAsync(c, x => x.Name),
                    Value = c.Id.ToString(),
                    Selected = c.Id == model.CountryId
                });
            }

            // Load and populate states if StateProvince is enabled
            if (addressSettings.StateProvinceEnabled)
            {
                var languageId = (await _workContext.GetWorkingLanguageAsync()).Id;
                var states = (await _stateProvinceService
                    .GetStateProvincesByCountryIdAsync(model.CountryId ?? 0, languageId))
                    .ToList();

                // Populate available states list if states are found
                if (states.Count != 0)
                {
                    model.AvailableStates.Add(new SelectListItem { Text = await _localizationService.GetResourceAsync("Address.SelectState"), Value = "0" });

                    foreach (var s in states)
                    {
                        model.AvailableStates.Add(new SelectListItem
                        {
                            Text = await _localizationService.GetLocalizedAsync(s, x => x.Name),
                            Value = s.Id.ToString(),
                            Selected = (s.Id == model.StateProvinceId)
                        });
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

        // Set form field visibility and requirement based on address settings
        model.CompanyEnabled = addressSettings.CompanyEnabled;
        model.CompanyRequired = addressSettings.CompanyRequired;
        model.StreetAddressEnabled = addressSettings.StreetAddressEnabled;
        model.StreetAddressRequired = addressSettings.StreetAddressRequired;
        model.StreetAddress2Enabled = addressSettings.StreetAddress2Enabled;
        model.StreetAddress2Required = addressSettings.StreetAddress2Required;
        model.ZipPostalCodeEnabled = addressSettings.ZipPostalCodeEnabled;
        model.ZipPostalCodeRequired = addressSettings.ZipPostalCodeRequired;
        model.CityEnabled = addressSettings.CityEnabled;
        model.CityRequired = addressSettings.CityRequired;
        model.CountyEnabled = addressSettings.CountyEnabled;
        model.CountyRequired = addressSettings.CountyRequired;
        model.CountryEnabled = addressSettings.CountryEnabled;
        model.StateProvinceEnabled = addressSettings.StateProvinceEnabled;
        model.PhoneEnabled = addressSettings.PhoneEnabled;
        model.PhoneRequired = addressSettings.PhoneRequired;
        model.FaxEnabled = addressSettings.FaxEnabled;
        model.FaxRequired = addressSettings.FaxRequired;

        // Handle custom address attributes if available
        if (_addressAttributeService != null && _addressAttributeParser != null)
            await PrepareCustomAddressAttributesAsync(model, address, overrideAttributesXml);

        // Format custom attributes if available
        if (_addressAttributeFormatter != null && address != null)
            model.FormattedCustomAddressAttributes = await _addressAttributeFormatter.FormatAttributesAsync(address.CustomAttributes);
    }
    #endregion
}
