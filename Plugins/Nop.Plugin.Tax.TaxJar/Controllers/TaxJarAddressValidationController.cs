using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Directory;
using Nop.Plugin.Tax.TaxJar.Models;
using Nop.Plugin.Tax.TaxJar.Services;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Web.Factories;
using Nop.Web.Framework.Controllers;
using Nop.Web.Models.Common;
using Nop.Core.Http;

namespace Nop.Plugin.Tax.TaxJar.Controllers;

public class TaxJarAddressValidationController : BaseController
{
    #region Fields
    private readonly IAddressService _addressService;
    private readonly ICustomerService _customerService;
    private readonly IWorkContext _workContext;
    private readonly TaxJarTaxManager _taxJarTaxManager;
    private readonly IAddressModelFactory _addressModelFactory;
    private readonly AddressSettings _addressSettings;
    private readonly IStateProvinceService _stateProvinceService;
    private readonly TaxJarSettings _taxJarSettings;
    private readonly ICountryService _countryService;
    private readonly IGenericAttributeService _genericAttributeService;
    private readonly ILocalizationService _localizationService;
    #endregion

    #region Ctor
    public TaxJarAddressValidationController(
        IAddressService addressService,
        ICustomerService customerService,
        IWorkContext workContext,
        TaxJarTaxManager taxJarTaxManager,
        IAddressModelFactory addressModelFactory,
        AddressSettings addressSettings,
        IStateProvinceService stateProvinceService,
        TaxJarSettings taxJarSettings,
        ICountryService countryService,
        IGenericAttributeService genericAttributeService,
        ILocalizationService localizationService)
    {
        _addressService = addressService;
        _customerService = customerService;
        _workContext = workContext;
        _taxJarTaxManager = taxJarTaxManager;
        _addressModelFactory = addressModelFactory;
        _addressSettings = addressSettings;
        _stateProvinceService = stateProvinceService;
        _taxJarSettings = taxJarSettings;
        _countryService = countryService;
        _genericAttributeService = genericAttributeService;
        _localizationService = localizationService;
    }
    #endregion

    #region Methods
    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> AddressVerification(AddressModel model, IFormCollection form)
    {
        var addressId = 0;
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (form.ContainsKey("billing_address_id"))
        {
            bool useDefaultAddress = false;
            if (form.ContainsKey("UseDefaultAddress"))
            {
                useDefaultAddress = Convert.ToBoolean(form["UseDefaultAddress"].ToString().Split(',')[0]);
                if (useDefaultAddress)
                {
                    var defaultAddressId = await _genericAttributeService.GetAttributeAsync<int>(customer, "DefaultAddress");
                    if (defaultAddressId > 0)
                    {
                        var billingAddress = await _customerService.GetCustomerAddressAsync(customer.Id, addressId);
                        await _addressModelFactory.PrepareAddressModelAsync(model, address: billingAddress, excludeProperties: false, addressSettings: _addressSettings);
                    }
                }
            }

            if (!useDefaultAddress)
            {
                int.TryParse(form["billing_address_id"], out var billingAddressId);
                if (billingAddressId > 0)
                {
                    addressId = billingAddressId;
                    var billingAddress = await _customerService.GetCustomerAddressAsync(customer.Id, billingAddressId);
                    await _addressModelFactory.PrepareAddressModelAsync(model, address: billingAddress, excludeProperties: false, addressSettings: _addressSettings);
                }
            }
        }
        else
        {
            int.TryParse(form["shipping_address_id"], out var shippingAddressId);
            if (shippingAddressId > 0)
            {
                addressId = shippingAddressId;
                var shippingAddress = await _customerService.GetCustomerAddressAsync(customer.Id, shippingAddressId);
                await _addressModelFactory.PrepareAddressModelAsync(model, address: shippingAddress, excludeProperties: false, addressSettings: _addressSettings);
            }
        }

        var result = new AddressValidationModel();
        var address = await _customerService.GetCustomerAddressAsync(customer.Id, addressId);

        if (addressId > 1)
        {
            if (_taxJarSettings.OnlyNewAddress)
                return Json(result);

            var preValidate = await _genericAttributeService.GetAttributeAsync<bool>(address, "SmartyStreets");
            if (preValidate)
                return Json(result);
        }

        if (addressId == 0)
            await _addressModelFactory.PrepareAddressModelAsync(model, null, excludeProperties: false, addressSettings: _addressSettings);

        //validate address
        var validationResult = await _taxJarTaxManager.ValidateAddressAsync(model.ToEntity());

        //if there are no errors and no validated addresses, nothing to display
        if (!validationResult?.Any() ?? true)
            return Json(result);

        //get validated address info
        var candidate = validationResult.FirstOrDefault();
        if (candidate != null)
        {
            result.HasAddress = true;
            if (addressId == 0)
            {
                address = model.ToEntity();
                result.IsNewAddress = true;
            }

            address.Address1 = candidate.Street;
            address.Address2 = string.Empty;
            address.City = candidate.City;
            address.StateProvinceId = (await _stateProvinceService.GetStateProvinceByAbbreviationAsync(candidate.State, address.CountryId))?.Id ?? address.StateProvinceId;
            address.ZipPostalCode = candidate.Zip;

            if (addressId > 0)
                result.AddressId = addressId;

            var validatedAddress = new AddressModel();
            await _addressModelFactory.PrepareAddressModelAsync(validatedAddress, address, excludeProperties: false, addressSettings: _addressSettings);

            if (await _addressService.IsAddressValidAsync(address))
            {
                var existingAddress = _addressService.FindAddress(
                    (await _customerService.GetAddressesByCustomerIdAsync(customer.Id)).ToList(), address.FirstName,
                    address.LastName, address.PhoneNumber, address.Email, address.FaxNumber, address.Company,
                    address.Address1, address.Address2, address.City, address.County, address.StateProvinceId,
                    address.ZipPostalCode, address.CountryId, address.CustomAttributes);

                if (existingAddress != null && addressId > 0)
                {
                    await _addressService.UpdateAddressAsync(address);
                    await _genericAttributeService.SaveAttributeAsync(address, "SmartyStreets", true);
                }

                async Task<string> getAddressLineAsync(AddressModel address)
                {
                    return WebUtility.HtmlEncode($"{(!string.IsNullOrEmpty(address.Address1) ? $"{address.Address1}, " : string.Empty)}" +
                        $"{(!string.IsNullOrEmpty(address.Address2) ? $"{address.Address2}, " : string.Empty)}" +
                        $"{(!string.IsNullOrEmpty(address.City) ? $"{address.City}, " : string.Empty)}" +
                        $"{(await _stateProvinceService.GetStateProvinceByIdAsync(address.StateProvinceId ?? 0) is StateProvince stateProvince ? $"{stateProvince.Name}, " : string.Empty)}" +
                        $"{(await _countryService.GetCountryByIdAsync(address.CountryId ?? 0) is Country country ? $"{country.Name}, " : string.Empty)}" +
                        $"{(!string.IsNullOrEmpty(address.ZipPostalCode) ? $"{address.ZipPostalCode}, " : string.Empty)}"
                        .TrimEnd(' ').TrimEnd(','));
                }

                var message = new AddressValidationResultModel()
                {
                    CurrentAddress = await getAddressLineAsync(model),
                    ValidAddress = await getAddressLineAsync(validatedAddress),
                };

                result.Message = await RenderPartialViewToStringAsync("~/Plugins/Tax.TaxJar/Views/Checkout/_TaxJarAddressValidationPopUp.cshtml", message);
                result.ShowModel = existingAddress == null;
                result.AvailableAddress = validatedAddress;
            }
            else
            {
                var message = new AddressValidationResultModel()
                {
                    IsError = true,
                    Warnings = [await _localizationService.GetResourceAsync("Plugins.Tax.TaxJar.AddressValidation.Error.Detail")]
                };
                result.IsError = true;
                result.ShowModel = true;
                result.Message = await RenderPartialViewToStringAsync("~/Plugins/Tax.TaxJar/Views/Checkout/_TaxJarAddressValidationPopUp.cshtml", message); ;
                result.AvailableAddress = model;
            }
        }

        //nothing to return
        return Json(result);
    }

    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> UseValidatedAddress(int addressId, AddressModel model)
    {
        //try to get an address by the passed identifier
        var address = await _addressService.GetAddressByIdAsync(addressId);
        if (address != null)
        {
            //set model value to entity
            address = model.ToEntity();

            //and update appropriate customer address
            await _addressService.UpdateAddressAsync(address);
            await _genericAttributeService.SaveAttributeAsync(address, "SmartyStreets", true);
        }

        //nothing to return
        return Content(string.Empty);
    }


    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> DefaultAddress(int addressId)
    {
        await _genericAttributeService.SaveAttributeAsync(await _workContext.GetCurrentCustomerAsync(), "DefaultAddress", addressId);

        //redirect to the address list page
        return Json(new
        {
            redirect = Url.RouteUrl(NopRouteNames.General.CUSTOMER_ADDRESSES),
        });
    }
    #endregion
}