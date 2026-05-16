using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Tax;
using Nop.Plugin.Tax.TaxJar.Domain;
using Nop.Plugin.Tax.TaxJar.Factories;
using Nop.Plugin.Tax.TaxJar.Models;
using Nop.Plugin.Tax.TaxJar.Services;
using Nop.Services;
using Nop.Services.Attributes;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using RestSharp;
using static Nop.Plugin.Tax.TaxJar.Models.ConfigurationModel;

namespace Nop.Plugin.Tax.TaxJar.Controllers;

public class TaxJarController : BaseAdminController
{
    #region Fields
    private readonly TaxJarTaxManager _taxJarTaxManager;
    private readonly TaxJarSettings _taxJarSettings;
    private readonly IBaseAdminModelFactory _baseAdminModelFactory;
    private readonly ILocalizationService _localizationService;
    private readonly INotificationService _notificationService;
    private readonly ISettingService _settingService;
    private readonly IAttributeService<CustomerAttribute, CustomerAttributeValue> _customerAttributeService;
    private readonly ICustomerService _customerService;
    private readonly IWorkContext _workContext;
    private readonly IOrderService _orderService;
    private readonly ICustomerActivityService _customerActivityService;
    private readonly ITaxJarService _taxJarService;
    private readonly ITaxJarModelFactory _taxJarModelFactory;
    private readonly TaxSettings _taxSettings;
    private readonly ICurrencyService _currencyService;
    private readonly CurrencySettings _currencySettings;
    private readonly IPriceFormatter _priceFormatter;
    private readonly OrderSettings _orderSettings;
    private readonly IAttributeService<AddressAttribute, AddressAttributeValue> _addressAttributeService;
    private readonly IGenericAttributeService _genericAttributeService;
    #endregion

    #region Ctor
    public TaxJarController(TaxJarTaxManager taxJarTaxManager,
        TaxJarSettings taxJarSettings,
        IBaseAdminModelFactory baseAdminModelFactory,
        ILocalizationService localizationService,
        INotificationService notificationService,
        ISettingService settingService,
        IAttributeService<CustomerAttribute, CustomerAttributeValue> customerAttributeService,
        ICustomerService customerService,
        IWorkContext workContext,
        IOrderService orderService,
        ICustomerActivityService customerActivityService,
        ITaxJarService taxJarService,
        ITaxJarModelFactory taxJarModelFactory,
        TaxSettings taxSettings,
        ICurrencyService currencyService,
        CurrencySettings currencySettings,
        IPriceFormatter priceFormatter,
        OrderSettings orderSettings,
        IAttributeService<AddressAttribute, AddressAttributeValue> addressAttributeService,
        IGenericAttributeService genericAttributeService)
    {
        _taxJarTaxManager = taxJarTaxManager;
        _taxJarSettings = taxJarSettings;
        _baseAdminModelFactory = baseAdminModelFactory;
        _localizationService = localizationService;
        _notificationService = notificationService;
        _settingService = settingService;
        _customerAttributeService = customerAttributeService;
        _customerService = customerService;
        _workContext = workContext;
        _orderService = orderService;
        _customerActivityService = customerActivityService;
        _taxJarService = taxJarService;
        _taxJarModelFactory = taxJarModelFactory;
        _taxSettings = taxSettings;
        _currencyService = currencyService;
        _currencySettings = currencySettings;
        _priceFormatter = priceFormatter;
        _orderSettings = orderSettings;
        _addressAttributeService = addressAttributeService;
        _genericAttributeService = genericAttributeService;
    }
    #endregion

    #region Utilities
    protected virtual async Task LogEditOrderAsync(int orderId)
    {
        var order = await _orderService.GetOrderByIdAsync(orderId);

        await _customerActivityService.InsertActivityAsync("EditOrder",
            string.Format(await _localizationService.GetResourceAsync("ActivityLog.EditOrder"), order.CustomOrderNumber), order);
    }
    #endregion

    #region Methods
    #region Configuration
    [CheckPermission(StandardPermission.Configuration.MANAGE_TAX_SETTINGS)]
    public async Task<IActionResult> Configure(string testTaxResult = null)
    {
        //prepare common properties
        var model = new ConfigurationModel
        {
            ApiToken = _taxJarSettings.ApiToken,
            UseSandbox = _taxJarSettings.UseSandbox,
            IsTaxExempt = _taxJarSettings.IsTaxExempt,
            PaymentMethods = _taxJarSettings.PaymentMethods,
            CommitTransactions = _taxJarSettings.CommitTransactions,
            ValidateAddress = _taxJarSettings.ValidateAddress,
            ValidateBilling = _taxJarSettings.ValidateBilling,
            ValidateShipping = _taxJarSettings.ValidateShipping,
            OnlyNewAddress = _taxJarSettings.OnlyNewAddress,
            TaxOriginAddressTypeId = (int)_taxJarSettings.TaxOriginAddressType,
            ExcludeTaxOnStateProvinceIds = _taxJarSettings.ExcludeTaxOnStateProvinceIds,
            AttachPdfInvoiceToOrderRefundSelectedEmail = _taxJarSettings.AttachPdfInvoiceToOrderRefundSelectedEmail,
            CCAttributeId = _taxJarSettings.CCAttributeId,
            HiddenCustomerRoleId = _taxJarSettings.HiddenCustomerRoleId,
            TaxExclusionOnCall = _taxJarSettings.TaxExclusionOnCall,
            TestTaxResult = testTaxResult,
            IsConfigured = !string.IsNullOrEmpty(_taxJarSettings.ApiToken)
        };

        var customer = await _workContext.GetCurrentCustomerAsync();

        model.HideGeneralBlock = await _genericAttributeService.GetAttributeAsync<bool>(customer, TaxJarDefaults.HideGeneralBlock);
        model.HideLogBlock = await _genericAttributeService.GetAttributeAsync<bool>(customer, TaxJarDefaults.HideLogBlock);

        var customerAttribute = await _customerAttributeService.GetAllAttributesAsync();
        if (customerAttribute.Count > 0)
            model.AvailableCustomerAttribute = customerAttribute.Select(attribute => new SelectListItem
            {
                Text = attribute.Name,
                Value = attribute.Id.ToString(),
                Selected = _taxJarSettings.IsTaxExempt.Any(t => t.Equals(attribute.Id))
            }).ToList();

        model.TaxOriginAddressTypes = (await TaxOriginAddressType.DefaultTaxAddress.ToSelectListAsync(false))
            .Select(type => new SelectListItem(type.Text, type.Value)).ToList();

        model.TaxExclusionOnModel.AvailableMethod = (await Method.GET.ToSelectListAsync(false))
            .Select(type => new SelectListItem(type.Text.Replace(" ", ""), type.Value)).ToList();

        var addressAttributes = await _addressAttributeService.GetAllAttributesAsync();
        foreach (var item in addressAttributes)
            model.AddressAttributes.Add(new SelectListItem { Text = item.Name, Value = item.Id.ToString() });

        //insert this default item at first
        model.AddressAttributes.Insert(0, new SelectListItem { Text = await _localizationService.GetResourceAsync("Admin.Common.EmptyItemText"), Value = "0" });

        model.TaxExclusionOn = _taxJarSettings.TaxExclusions().Select(t => new TaxExclusionModel()
        {
            ControllerName = t.ControllerName,
            ActionName = t.ActionName,
            Method = t.Method
        }).ToList();

        //prepare available customer roles
        var availableRoles = await _customerService.GetAllCustomerRolesAsync(showHidden: true);
        model.AvailableCustomerRoles = availableRoles.Select(role => new SelectListItem
        {
            Text = role.Name,
            Value = role.Id.ToString(),
            Selected = model.HiddenCustomerRoleId == role.Id
        }).ToList();

        //prepare address model
        await _baseAdminModelFactory.PrepareCountriesAsync(model.TestAddress.AvailableCountries);
        await _baseAdminModelFactory.PrepareStatesAndProvincesAsync(model.TestAddress.AvailableStates, _taxJarSettings.CountryId);
        await _baseAdminModelFactory.PrepareStatesAndProvincesAsync(model.AvailableStates, _taxJarSettings.CountryId);
        
        //prepare taxjar transaction log model
        model.TaxJarRequestLogSearchModel.SetGridPageSize();

        return View("~/Plugins/Tax.TaxJar/Views/Configuration/Configure.cshtml", model);
    }

    [HttpPost, ActionName("Configure")]
    [FormValueRequired("save")]
    [CheckPermission(StandardPermission.Configuration.MANAGE_TAX_SETTINGS)]
    public async Task<IActionResult> Configure(ConfigurationModel model)
    {
        if (!ModelState.IsValid)
            return await Configure();

        //save settings
        _taxJarSettings.ApiToken = model.ApiToken;
        _taxJarSettings.IsTaxExempt = model.IsTaxExempt.ToList();
        _taxJarSettings.PaymentMethods = model.PaymentMethods;
        _taxJarSettings.UseSandbox = model.UseSandbox;
        _taxJarSettings.CommitTransactions = model.CommitTransactions;
        _taxJarSettings.ValidateAddress = model.ValidateAddress;
        _taxJarSettings.ValidateBilling = model.ValidateBilling;
        _taxJarSettings.ValidateShipping = model.ValidateShipping;
        _taxJarSettings.OnlyNewAddress = model.OnlyNewAddress;
        _taxJarSettings.TaxOriginAddressType = (TaxOriginAddressType)model.TaxOriginAddressTypeId;
        _taxJarSettings.ExcludeTaxOnStateProvinceIds = model.ExcludeTaxOnStateProvinceIds.ToList();
        _taxJarSettings.AttachPdfInvoiceToOrderRefundSelectedEmail = model.AttachPdfInvoiceToOrderRefundSelectedEmail;
        _taxJarSettings.CCAttributeId = model.CCAttributeId;
        _taxJarSettings.HiddenCustomerRoleId = model.HiddenCustomerRoleId;
        _taxJarSettings.TaxExclusionOnCall = model.TaxExclusionOnCall;
        await _settingService.SaveSettingAsync(_taxJarSettings);

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

        return await Configure();
    }

    [HttpPost, ActionName("Configure")]
    [FormValueRequired("verifyCredentials")]
    [CheckPermission(StandardPermission.Configuration.MANAGE_TAX_SETTINGS)]
    public async Task<IActionResult> VerifyCredentials()
    {
        //verify credentials 
        var result = await _taxJarTaxManager.PingAsync();
        if (result?.Any() ?? false)
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugins.Tax.TaxJar.VerifyCredentials.Verified"));
        else
            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Plugins.Tax.TaxJar.VerifyCredentials.Declined"));

        return await Configure();
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Configuration.MANAGE_TAX_SETTINGS)]
    public async Task<IActionResult> AddTaxExclusion(TaxExclusionModel model)
    {
        var taxExclusionOn = _taxJarSettings.TaxExclusions();
        taxExclusionOn.Add(new TaxExclusionOn()
        {
            ControllerName = model.ControllerName,
            ActionName = model.ActionName,
            Method = model.Method
        });

        _taxJarSettings.TaxExclusionOn = string.Join(";", taxExclusionOn.Select(t => string.Format("{0},{1},{2}", t.ControllerName, t.ActionName, t.Method)));

        await _settingService.SaveSettingAsync(_taxJarSettings);

        return Json(new { success = true });
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Configuration.MANAGE_TAX_SETTINGS)]
    public async Task<IActionResult> DeleteTaxExclusion(int index)
    {
        var taxExclusionOn = _taxJarSettings.TaxExclusions();
        taxExclusionOn.RemoveAt(index - 1);

        _taxJarSettings.TaxExclusionOn = string.Join(";", taxExclusionOn.Select(t => string.Format("{0},{1},{2}", t.ControllerName, t.ActionName, t.Method)));
        await _settingService.SaveSettingAsync(_taxJarSettings);

        return Json(new { success = true });
    }

    [HttpPost, ActionName("Configure")]
    [FormValueRequired("testTax")]
    [CheckPermission(StandardPermission.Configuration.MANAGE_TAX_SETTINGS)]
    public async Task<IActionResult> TestTaxRequest(ConfigurationModel model)
    {
        if (!ModelState.IsValid)
            return await Configure();

        //get result
        var transaction = await _taxJarTaxManager.GetTaxRateAsync(new Address
        {
            City = model.TestAddress?.City,
            CountryId = model.TestAddress?.CountryId,
            Address1 = model.TestAddress?.Address1,
            ZipPostalCode = model.TestAddress?.ZipPostalCode,
            StateProvinceId = model.TestAddress?.StateProvinceId
        });

        var testTaxResult = string.Empty;
        if (transaction?.Rate != null)
        {
            //display tax rates by jurisdictions
            testTaxResult = $"Total tax rate: {transaction.Rate * 100:0.00}% {Environment.NewLine}";
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugins.Tax.TaxJar.TestTax.Success"));
        }
        else
            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Plugins.Tax.TaxJar.TestTax.Error"));

        return await Configure(testTaxResult);
    }

    public async Task<IActionResult> ChangeOriginAddressType(int typeId)
    {
        var message = (TaxOriginAddressType)typeId switch
        {
            TaxOriginAddressType.ShippingOrigin => string.Format(await _localizationService
                .GetResourceAsync("Plugins.Tax.TaxJar.Fields.TaxOriginAddressType.ShippingOrigin.Warning"), Url.Action("Shipping", "Setting")),
            TaxOriginAddressType.DefaultTaxAddress => string.Format(await _localizationService
                .GetResourceAsync("Plugins.Tax.TaxJar.Fields.TaxOriginAddressType.DefaultTaxAddress.Warning"), Url.Action("Tax", "Setting")),
            _ => null
        };

        return Json(new { Result = message });
    }

    #endregion

    #region Tax exemption certificate

    public virtual async Task<IActionResult> TaxExemptionCertificateDownload(int id)
    {
        //try to get an customer with the specified id
        var customer = await _customerService.GetCustomerByIdAsync(id);
        if (customer == null)
            return RedirectToAction("List", "Customer");

        if (!customer.IsTaxExempt)
            return RedirectToAction("List", "Customer");

        try
        {
            byte[] bytes;
            await using (var stream = new MemoryStream())
            {
                await _taxJarService.PrintSalesTaxExemptionCertificateToPdfAsync(stream, customer);
                bytes = stream.ToArray();
            }

            return File(bytes, MimeTypes.ApplicationForceDownload, $"customer_{customer.Id}_sales-tax-exemption-certificate.pdf");
        }
        catch (Exception ex)
        {
            await _notificationService.ErrorNotificationAsync(ex);
            return RedirectToAction("List", "Customer");
        }
    }
    #endregion

    #region Refund selected items
    [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
    public virtual async Task<IActionResult> RefundOrderSelected(int orderId, bool online)
    {
        //try to get an order with the specified id
        var order = await _orderService.GetOrderByIdAsync(orderId);
        if (order == null)
            return RedirectToAction("List", "Order");

        //a vendor does not have access to this functionality
        if (await _workContext.GetCurrentVendorAsync() != null)
            return RedirectToAction("Edit", "Order", new { id = orderId });

        //prepare model
        var model = await _taxJarModelFactory.PrepareRefundOrderModelAsync(null, order);
        model.OnlineRefund = online;

        return View("~/Plugins/Tax.TaxJar/Areas/Admin/Views/Order/RefundOrderSelected.cshtml", model);
    }

    [HttpPost]
    [IgnoreAntiforgeryToken]
    public virtual async Task<IActionResult> RefundOrder_ItemChange(int orderId, RefundOrderModel model, IFormCollection form)
    {
        //try to get an order with the specified id
        var order = await _orderService.GetOrderByIdAsync(orderId);
        if (order == null)
            return new NullJsonResult();

        var errors = new List<string>();
        var (refundOrder, _, _) = await _taxJarService.ParseRefundOrderAsync(
            order, form, model.OrderShippingInclTaxValue, model.OrderShippingExclTaxValue,
            model.PaymentMethodAdditionalFeeInclTaxValue, model.PaymentMethodAdditionalFeeExclTaxValue, errors);
        
        var pricesIncludeTax = _taxSettings.PricesIncludeTax;
        var primaryStoreCurrency = await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId);
        var languageId = (await _workContext.GetWorkingLanguageAsync()).Id;

        var maxAmountToRefund = order.OrderTotal - order.RefundedAmount;
        if (refundOrder.OrderTotal > maxAmountToRefund)
        {
            errors.Add(string.Format(await _localizationService.GetResourceAsync("Plugins.Tax.TaxJar.Fields.CalculatedRefund.AmountToRefund.MaxError"),
                                    string.Format(await _localizationService.GetResourceAsync("Admin.Orders.Fields.PartialRefund.AmountToRefund.Max"),
                                                  maxAmountToRefund.ToString("G29"),
                                                  primaryStoreCurrency?.CurrencyCode)));
        }

        var refundOrderTotal = string.Format(await _localizationService.GetResourceAsync("Plugins.Tax.TaxJar.Fields.CalculatedRefund"), 
            refundOrder.OrderTotal <= decimal.Zero 
            ? string.Empty 
            : await _priceFormatter.FormatOrderPriceAsync(refundOrder.OrderTotal, order.CurrencyRate,
                                                          order.CustomerCurrencyCode,
                                                          _orderSettings.DisplayCustomerCurrencyOnOrders,
                                                          primaryStoreCurrency, languageId, null, false));

        return Json(new
        {
            pricesIncludeTax,
            orderSubtotal = pricesIncludeTax ? refundOrder.OrderSubtotalInclTax : refundOrder.OrderSubtotalExclTax,
            orderSubtotalDiscount = pricesIncludeTax ? refundOrder.OrderSubTotalDiscountInclTax : refundOrder.OrderSubTotalDiscountExclTax,
            orderTax = refundOrder.OrderTax,
            orderTotalDisocunt = refundOrder.OrderDiscount,
            refundOrderTotal,
            message = errors.Count != 0 ? errors.ToArray() : null
        });
    }

    [HttpPost]
    [FormValueRequired("calculaterefund")]
    [CheckPermission(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE)]
    public virtual async Task<IActionResult> RefundOrderSelected(int orderId, bool online, RefundOrderModel model, IFormCollection form)
    {
        //try to get an order with the specified id
        var order = await _orderService.GetOrderByIdAsync(orderId);
        if (order == null)
            return RedirectToAction("List", "Order");

        //a vendor does not have access to this functionality
        if (await _workContext.GetCurrentVendorAsync() != null)
            return RedirectToAction("Edit", "Order", new { id = orderId });

        try
        {
            var errors = new List<string>();
            var (refundOrder, refundOrderItem, _) = await _taxJarService.ParseRefundOrderAsync(order, form, model.OrderShippingInclTaxValue, model.OrderShippingExclTaxValue, model.PaymentMethodAdditionalFeeInclTaxValue,
                model.PaymentMethodAdditionalFeeExclTaxValue, errors);

            var pricesIncludeTax = _taxSettings.PricesIncludeTax;
            var primaryStoreCurrency = await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId);
            var languageId = (await _workContext.GetWorkingLanguageAsync()).Id;

            var maxAmountToRefund = order.OrderTotal - order.RefundedAmount;
            if (refundOrder.OrderTotal > maxAmountToRefund)
                errors.Add(string.Format(await _localizationService.GetResourceAsync("Plugins.Tax.TaxJar.Fields.CalculatedRefund.AmountToRefund.MaxError"),
                    string.Format(await _localizationService.GetResourceAsync("Admin.Orders.Fields.PartialRefund.AmountToRefund.Max"), maxAmountToRefund.ToString("G29"), primaryStoreCurrency?.CurrencyCode)));

            if (errors.Count != 0)
            {
                foreach (var error in errors)
                    _notificationService.ErrorNotification(error);

                return RedirectToAction("RefundOrderSelected", "TaxJar", new { orderId, online });
            }

            var amountToRefund = refundOrder.OrderTotal;
            if (amountToRefund <= decimal.Zero)
                throw new NopException("Select any order item to refund");

            if (online)
                errors = (await _taxJarService.RefundSelectedAsync(order, refundOrder, refundOrderItem)).ToList();
            else
                await _taxJarService.RefundSelectedOfflineAsync(order, refundOrder, refundOrderItem);

            await LogEditOrderAsync(order.Id);

            if (!errors.Any())
                return RedirectToAction("Edit", "Order", new { id = orderId });

            foreach (var error in errors)
                _notificationService.ErrorNotification(error);

            return RedirectToAction("RefundOrderSelected", "TaxJar", new { orderId, online });
        }
        catch (Exception exc)
        {
            await _notificationService.ErrorNotificationAsync(exc);
            return RedirectToAction("RefundOrderSelected", "TaxJar", new { orderId, online });
        }
    }
    #endregion

    #region Resend invoice
    [HttpPost]
    public virtual async Task<IActionResult> ResendInvoice(int orderId, string email)
    {
        //try to get an order with the specified id
        var order = await _orderService.GetOrderByIdAsync(orderId);
        if (order == null || order.Deleted)
            return Content(string.Empty);

        try
        {
            await _taxJarService.ResendOrderInvoiceAsync(order, email);
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugins.Tax.TaxJar.ResendInvoice.Success"));
        }
        catch(Exception exc)
        {
            await _notificationService.ErrorNotificationAsync(exc);
        }
        return Json(new { success = true });
    }
    #endregion

    #region Resync order 
    [HttpPost]
    public virtual async Task<IActionResult> ResyncOrderTaxTransaction(int orderId)
    {
        //try to get an order with the specified id
        var order = await _orderService.GetOrderByIdAsync(orderId);
        if (order == null || order.Deleted)
            return Content(string.Empty);

        try
        {
            await _taxJarTaxManager.ResyncOrderTaxTransactionAsync(order);
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugins.Tax.TaxJar.ResendTransaction.Success"));
        }
        catch (Exception exc)
        {
            await _notificationService.ErrorNotificationAsync(exc);
        }
        return Json(new { success = true });
    }
    #endregion

    #region Reassign customer
    [HttpPost]
    public async Task<IActionResult> ReassignOrders(int id, int selectedCustomerId, string selectedIds)
    {
        //try to get a customer with the specified id
        var customer = await _customerService.GetCustomerByIdAsync(id);
        if (customer == null)
            return RedirectToAction("List", "Customer");

        var orders = new List<Order>();
        if (selectedIds != null)
        {
            var ids = selectedIds
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => Convert.ToInt32(x))
                .ToArray();
            orders.AddRange(await (await _orderService.GetOrdersByIdsAsync(ids)).ToListAsync());
        }

        try
        {
            if(selectedCustomerId == 0)
            {
                //create guest if not exists
                var guestCustomer = await _customerService.InsertGuestCustomerAsync();
                selectedCustomerId = guestCustomer.Id;
            }

            foreach(var order in orders)
            {
                order.CustomerId = selectedCustomerId;
                await _orderService.UpdateOrderAsync(order);
            }

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugins.Tax.TaxJar.ReassignOrders.Reassign.Success"));
        }
        catch (Exception exc)
        {
            _notificationService.ErrorNotification(exc.Message);
        }

        return RedirectToAction("Edit", "Customer", new { id = customer.Id });
    }

    [CheckPermission(StandardPermission.Security.ACCESS_ADMIN_PANEL)]
    public virtual async Task<IActionResult> SearchCustomerAutoComplete(string term)
    {
        const int searchTermMinimumLength = 3;
        if (string.IsNullOrWhiteSpace(term) || term.Length < searchTermMinimumLength)
            return Content(string.Empty);

        //customers
        const int customerNumber = 15;
        var selectedCustomerRoleIds = new List<int>();
        //search registered customers by default
        var registeredRole = await _customerService.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.RegisteredRoleName);
        if (registeredRole != null)
            selectedCustomerRoleIds.Add(registeredRole.Id);

        //get customers
        var customers = await _customerService.GetAllCustomersAsync(customerRoleIds: [.. selectedCustomerRoleIds],
                                                                    email: term, pageSize: customerNumber);

        var result = customers.SelectAwait(async customer => {
            var fullName = await _customerService.GetCustomerFullNameAsync(customer);
            return new
            {
                label = $"{customer.Email} ({fullName})",
                customerid = customer.Id
            };
        });

        return Json(result);
    }
    #endregion
    #endregion
}