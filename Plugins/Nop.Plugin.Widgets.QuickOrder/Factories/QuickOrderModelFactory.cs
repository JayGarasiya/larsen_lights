using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Widgets.QuickOrder.Models;
using Nop.Plugin.Widgets.QuickOrder.Services;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Customers;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Plugin.Widgets.QuickOrder.Factories;

/// <summary>
/// Factory to prepare models related to Quick Order functionality.
/// </summary>
public class QuickOrderModelFactory : IQuickOrderModelFactory
{
    #region Fields

    protected readonly ILocalizationService _localizationService;
    protected readonly CustomerSettings _customerSettings;
    protected readonly ICustomerService _customerService;
    protected readonly IAclSupportedModelFactory _aclSupportedModelFactory;
    protected readonly IDateTimeHelper _dateTimeHelper;
    protected readonly IQuickOrderService _quickOrderService;
    protected readonly ICountryService _countryService;
    protected readonly IStateProvinceService _stateProvinceService;

    #endregion

    #region Ctor
    public QuickOrderModelFactory(
        ILocalizationService localizationService,
        CustomerSettings customerSettings,
        ICustomerService customerService,
        IAclSupportedModelFactory aclSupportedModelFactory,
        IDateTimeHelper dateTimeHelper,
        IQuickOrderService quickOrderService,
        ICountryService countryService,
        IStateProvinceService stateProvinceService)
    {
        _localizationService = localizationService;
        _customerSettings = customerSettings;
        _customerService = customerService;
        _aclSupportedModelFactory = aclSupportedModelFactory;
        _dateTimeHelper = dateTimeHelper;
        _quickOrderService = quickOrderService;
        _countryService = countryService;
        _stateProvinceService = stateProvinceService;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Prepare the customer search model with necessary settings.
    /// </summary>
    /// <param name="searchModel">The customer search model to be populated.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. 
    /// The result contains the updated search model.
    /// </returns>
    public async Task<QuickOrderSearchModel> PrepareCustomerSearchModelAsync(QuickOrderSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        // Set various customer search model settings from configuration
        searchModel.UsernamesEnabled = _customerSettings.UsernamesEnabled;
        searchModel.AvatarEnabled = _customerSettings.AllowCustomersToUploadAvatars;
        searchModel.FirstNameEnabled = _customerSettings.FirstNameEnabled;
        searchModel.LastNameEnabled = _customerSettings.LastNameEnabled;
        searchModel.DateOfBirthEnabled = _customerSettings.DateOfBirthEnabled;
        searchModel.CompanyEnabled = _customerSettings.CompanyEnabled;
        searchModel.PhoneEnabled = _customerSettings.PhoneEnabled;
        searchModel.ZipPostalCodeEnabled = _customerSettings.ZipPostalCodeEnabled;

        // Add the Registered role to the search model by default
        var registeredRole = await _customerService.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.RegisteredRoleName);
        if (registeredRole != null)
            searchModel.SelectedCustomerRoleIds.Add(registeredRole.Id);

        // Prepare available customer roles for filtering
        await _aclSupportedModelFactory.PrepareModelCustomerRolesAsync(searchModel);

        searchModel.AvailableActiveValues = new List<SelectListItem> {
        new(await _localizationService.GetResourceAsync("Admin.Common.All"), string.Empty),
        new(await _localizationService.GetResourceAsync("Admin.Common.Yes"), true.ToString(), true),
        new(await _localizationService.GetResourceAsync("Admin.Common.No"), false.ToString())

    };
        // Set page size for grid pagination
        searchModel.SetGridPageSize();

        return searchModel;
    }

    /// <summary>
    /// Prepare a paged customer list model based on the search parameters.
    /// </summary>
    /// <param name="searchModel">The customer search model containing filter criteria.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. 
    /// The result contains the customer list model.
    /// </returns>
    public async Task<CustomerListModel> PrepareCustomerListModelAsync(QuickOrderSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        // Parse date filter parameters
        _ = int.TryParse(searchModel.SearchDayOfBirth, out var dayOfBirth);
        _ = int.TryParse(searchModel.SearchMonthOfBirth, out var monthOfBirth);
        var createdFromUtc = !searchModel.SearchRegistrationDateFrom.HasValue ? null
            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.SearchRegistrationDateFrom.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
        var createdToUtc = !searchModel.SearchRegistrationDateTo.HasValue ? null
            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.SearchRegistrationDateTo.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);
        var lastActivityFromUtc = !searchModel.SearchLastActivityFrom.HasValue ? null
            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.SearchLastActivityFrom.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
        var lastActivityToUtc = !searchModel.SearchLastActivityTo.HasValue ? null
            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.SearchLastActivityTo.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);

        // Ensure guests are excluded if filtering by registration date
        if (createdFromUtc.HasValue || createdToUtc.HasValue)
        {
            if (!searchModel.SelectedCustomerRoleIds.Any())
            {
                var customerRoles = await _customerService.GetAllCustomerRolesAsync(showHidden: true);
                searchModel.SelectedCustomerRoleIds = customerRoles
                    .Where(cr => cr.SystemName != NopCustomerDefaults.GuestsRoleName).Select(cr => cr.Id).ToList();
            }
            else
            {
                var guestRole = await _customerService.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.GuestsRoleName);
                if (guestRole != null)
                    searchModel.SelectedCustomerRoleIds.Remove(guestRole.Id);
            }
        }

        // Fetch customers based on search filters
        var customers = await _quickOrderService.GetAllCustomersAsync(
            customerRoleIds: searchModel.SelectedCustomerRoleIds.ToArray(),
            email: searchModel.SearchEmail,
            username: searchModel.SearchUsername,
            firstName: searchModel.SearchFirstName,
            lastName: searchModel.SearchLastName,
            dayOfBirth: dayOfBirth,
            monthOfBirth: monthOfBirth,
            company: searchModel.SearchCompany,
            createdFromUtc: createdFromUtc,
            createdToUtc: createdToUtc,
            lastActivityFromUtc: lastActivityFromUtc,
            lastActivityToUtc: lastActivityToUtc,
            phone: searchModel.SearchPhone,
            zipPostalCode: searchModel.SearchZipPostalCode,
            ipAddress: searchModel.SearchIpAddress,
            customOrderNumber: searchModel.CustomOrderNumber,
            pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

        // Prepare the customer list model to be displayed
        var model = await new CustomerListModel().PrepareToGridAsync(searchModel, customers, () =>
        {
            return customers.SelectAwait(async customer =>
            {
                // Convert customer data into model form
                var customerModel = customer.ToModel<CustomerModel>();

                // Convert and assign various date and contact details to model
                customerModel.Email = await _customerService.IsRegisteredAsync(customer)
                    ? customer.Email
                    : await _localizationService.GetResourceAsync("Admin.Customers.Guest");
                customerModel.FullName = await _customerService.GetCustomerFullNameAsync(customer);
                customerModel.Company = customer.Company;
                customerModel.Phone = customer.Phone;
                customerModel.ZipPostalCode = customer.ZipPostalCode;

                // Prepare customer roles and address details
                customerModel.CustomerRoleNames = string.Join(", ",
                    (await _customerService.GetCustomerRolesAsync(customer)).Select(role => role.Name));

                var country = (await _countryService.GetCountryByIdAsync(customer.CountryId))?.Name;
                var state = (await _stateProvinceService.GetStateProvinceByIdAsync(customer.StateProvinceId))?.Name;

                var address = string.Empty;
                if (!string.IsNullOrEmpty(customer.StreetAddress))
                    address = customer.StreetAddress;
                if (!string.IsNullOrEmpty(customer.StreetAddress2))
                    address = address.Length > 0 ? string.Join(", <br />", new[] { address, customer.StreetAddress2 }) : customer.StreetAddress2;
                if (!string.IsNullOrEmpty(customer.City))
                    address = address.Length > 0 ? string.Join(", <br />", new[] { address, customer.City }) : customer.City;
                if (!string.IsNullOrEmpty(state))
                    address = address.Length > 0 ? string.Join(", <br />", new[] { address, state }) : state;
                if (!string.IsNullOrEmpty(country))
                    address = address.Length > 0 ? string.Join(", <br />", new[] { address, country }) : country;

                customerModel.StreetAddress = address;

                return customerModel;
            });
        });

        return model;
    }

    #endregion
}
