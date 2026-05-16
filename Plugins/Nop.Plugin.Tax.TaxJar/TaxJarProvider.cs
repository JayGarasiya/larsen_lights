using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Nop.Core;
using Nop.Core.Domain.Cms;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Tax;
using Nop.Plugin.Tax.TaxJar.Areas.Admin.Components;
using Nop.Plugin.Tax.TaxJar.Components;
using Nop.Plugin.Tax.TaxJar.Domain;
using Nop.Plugin.Tax.TaxJar.Services;
using Nop.Services.Cms;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Plugins;
using Nop.Services.Tax;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Tax.TaxJar;

/// <summary>
/// Represents the 'Tax Jar' plugin that integrates with TaxJar API.
/// </summary>
public class TaxJarProvider : BasePlugin, ITaxProvider, IWidgetPlugin
{
    #region Fields
    private readonly ISettingService _settingService;
    private readonly IWebHelper _webHelper;
    private readonly ILocalizationService _localizationService;
    private readonly IGenericAttributeService _genericAttributeService;
    private readonly IActionContextAccessor _actionContextAccessor;
    private readonly IOrderTotalCalculationService _orderTotalCalculationService;
    private readonly TaxSettings _taxSettings;
    private readonly IPaymentService _paymentService;
    private readonly ITaxService _taxService;
    private readonly TaxJarTaxManager _taxJarTaxManager;
    private readonly WidgetSettings _widgetSettings;
    private readonly TaxJarSettings _taxJarSettings;
    private readonly ITaxPluginManager _taxPluginManager;
    #endregion

    #region Ctor
    public TaxJarProvider(
        ISettingService settingService,
        IWebHelper webHelper,
        ILocalizationService localizationService,
        IGenericAttributeService genericAttributeService,
        IActionContextAccessor actionContextAccessor,
        IOrderTotalCalculationService orderTotalCalculationService,
        TaxSettings taxSettings,
        IPaymentService paymentService,
        ITaxService taxService,
        TaxJarTaxManager taxJarTaxManager,
        WidgetSettings widgetSettings,
        TaxJarSettings taxJarSettings,
        ITaxPluginManager taxPluginManager)
    {
        _settingService = settingService;
        _webHelper = webHelper;
        _localizationService = localizationService;
        _genericAttributeService = genericAttributeService;
        _actionContextAccessor = actionContextAccessor;
        _orderTotalCalculationService = orderTotalCalculationService;
        _taxSettings = taxSettings;
        _paymentService = paymentService;
        _taxService = taxService;
        _taxJarTaxManager = taxJarTaxManager;
        _widgetSettings = widgetSettings;
        _taxJarSettings = taxJarSettings;
        _taxPluginManager = taxPluginManager;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Retrieves the tax rate for a given address.
    /// </summary>
    /// <param name="taxRateRequest">Tax rate request containing the address and other details.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. 
    /// The task result contains the tax rate.
    /// </returns>
    public async Task<TaxRateResult> GetTaxRateAsync(TaxRateRequest taxRateRequest)
    {
        if (taxRateRequest.Address is null)
            return new TaxRateResult { Errors = ["Address is not set"] };

        if (_taxJarSettings.TaxExclusionOnCall
            && !string.IsNullOrEmpty(_taxJarSettings.TaxExclusionOn)
            && _actionContextAccessor.ActionContext is not null)
        {
            var httpContext = _actionContextAccessor.ActionContext.HttpContext;
            var controllerName = httpContext.GetRouteValue(NopRoutingDefaults.RouteValue.Controller).ToString();
            var actionName = httpContext.GetRouteValue(NopRoutingDefaults.RouteValue.Action).ToString();

            if (controllerName is not null && actionName is not null)
            {
                var allowedCall = _taxJarSettings.TaxExclusions()
                    .Where(p => p.ControllerName.Equals(controllerName) && p.ActionName.Equals(actionName) && p.Method.Equals(httpContext.Request.Method))
                    .FirstOrDefault();

                if (allowedCall is not null)
                {
                    // Get tax rate from TaxJar if the call is allowed
                    var taxRate = await _taxJarTaxManager.GetTaxRateAsync(taxRateRequest);
                    if (!taxRate.HasValue)
                        return new TaxRateResult { Errors = ["No response from the service"] };

                    return new TaxRateResult { TaxRate = taxRate.Value };
                }
            }
        }
        else
        {
            // Get tax rate directly from TaxJar
            var taxRate = await _taxJarTaxManager.GetTaxRateAsync(taxRateRequest);
            if (!taxRate.HasValue)
                return new TaxRateResult { Errors = ["No response from the service"] };

            return new TaxRateResult { TaxRate = taxRate.Value };
        }

        return new TaxRateResult { TaxRate = decimal.Zero };
    }

    /// <summary>
    /// Calculates the total tax for the shopping cart.
    /// </summary>
    /// <param name="taxTotalRequest">Tax total request containing the shopping cart and other details.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. 
    /// The task result contains the calculated tax total.
    /// </returns>
    public async Task<TaxTotalResult> GetTaxTotalAsync(TaxTotalRequest taxTotalRequest)
    {
        if (_actionContextAccessor.ActionContext.HttpContext.Items.TryGetValue("nop.TaxTotal", out var result)
            && result is (TaxTotalResult taxTotalResult, decimal paymentTax))
        {
            // Short-circuit to avoid circular reference when calculating payment method additional fee during checkout
            if (!taxTotalRequest.UsePaymentMethodAdditionalFee)
                return new TaxTotalResult { TaxTotal = taxTotalResult.TaxTotal - paymentTax };

            return taxTotalResult;
        }

        var taxRates = new SortedDictionary<decimal, decimal>();
        var taxTotal = decimal.Zero;

        // Calculate tax for order sub-total (items + checkout attributes)
        var (_, _, _, _, orderSubTotalTaxRates) = await _orderTotalCalculationService
            .GetShoppingCartSubTotalAsync(taxTotalRequest.ShoppingCart, false);
        var subTotalTaxTotal = decimal.Zero;
        foreach (var kvp in orderSubTotalTaxRates)
        {
            var taxRate = kvp.Key;
            var taxValue = kvp.Value;
            subTotalTaxTotal += taxValue;

            if (taxRate > decimal.Zero && taxValue > decimal.Zero)
            {
                if (!taxRates.TryGetValue(taxRate, out decimal value))
                    taxRates.Add(taxRate, taxValue);
                else
                    taxRates[taxRate] = value + taxValue;
            }
        }
        taxTotal += subTotalTaxTotal;

        // Calculate shipping tax if applicable
        var shippingTax = decimal.Zero;
        if (_taxSettings.ShippingIsTaxable)
        {
            var (shippingExclTax, _, _) = await _orderTotalCalculationService
                .GetShoppingCartShippingTotalAsync(taxTotalRequest.ShoppingCart, false);
            var (shippingInclTax, taxRate, _) = await _orderTotalCalculationService
                .GetShoppingCartShippingTotalAsync(taxTotalRequest.ShoppingCart, true);
            if (shippingExclTax.HasValue && shippingInclTax.HasValue)
            {
                shippingTax = shippingInclTax.Value - shippingExclTax.Value;
                if (shippingTax < decimal.Zero)
                    shippingTax = decimal.Zero;

                if (taxRate > decimal.Zero && shippingTax > decimal.Zero)
                {
                    if (!taxRates.TryGetValue(taxRate, out decimal value))
                        taxRates.Add(taxRate, shippingTax);
                    else
                        taxRates[taxRate] = value + shippingTax;
                }
            }
        }
        taxTotal += shippingTax;

        // Short-circuit to avoid circular reference when calculating payment method additional fee during checkout
        if (!taxTotalRequest.UsePaymentMethodAdditionalFee)
            return new TaxTotalResult { TaxTotal = taxTotal };

        // Calculate payment method additional fee tax if applicable
        var paymentMethodAdditionalFeeTax = decimal.Zero;
        if (_taxSettings.PaymentMethodAdditionalFeeIsTaxable)
        {
            var paymentMethodSystemName = taxTotalRequest.Customer != null
                ? await _genericAttributeService
                    .GetAttributeAsync<string>(taxTotalRequest.Customer, NopCustomerDefaults.SelectedPaymentMethodAttribute, taxTotalRequest.StoreId)
                : string.Empty;

            var paymentMethodAdditionalFee = await _paymentService
                .GetAdditionalHandlingFeeAsync(taxTotalRequest.ShoppingCart, paymentMethodSystemName);
            var (paymentMethodAdditionalFeeExclTax, _) = await _taxService
                .GetPaymentMethodAdditionalFeeAsync(paymentMethodAdditionalFee, false, taxTotalRequest.Customer);
            var (paymentMethodAdditionalFeeInclTax, taxRate) = await _taxService
                .GetPaymentMethodAdditionalFeeAsync(paymentMethodAdditionalFee, true, taxTotalRequest.Customer);

            paymentMethodAdditionalFeeTax = paymentMethodAdditionalFeeInclTax - paymentMethodAdditionalFeeExclTax;
            if (paymentMethodAdditionalFeeTax < decimal.Zero)
                paymentMethodAdditionalFeeTax = decimal.Zero;

            if (taxRate > decimal.Zero && paymentMethodAdditionalFeeTax > decimal.Zero)
            {
                if (!taxRates.TryGetValue(taxRate, out decimal value))
                    taxRates.Add(taxRate, paymentMethodAdditionalFeeTax);
                else
                    taxRates[taxRate] = value + paymentMethodAdditionalFeeTax;
            }
        }
        taxTotal += paymentMethodAdditionalFeeTax;

        // Ensure there is at least one tax rate (0%)
        if (taxRates.Count == 0)
            taxRates.Add(decimal.Zero, decimal.Zero);

        if (taxTotal < decimal.Zero)
            taxTotal = decimal.Zero;

        taxTotalResult = new TaxTotalResult { TaxTotal = taxTotal, TaxRates = taxRates, };

        // Store calculated tax total in the request to avoid redundant calculations
        _actionContextAccessor.ActionContext.HttpContext.Items.TryAdd("nop.TaxTotal", (taxTotalResult, paymentMethodAdditionalFeeTax));

        return taxTotalResult;
    }

    /// <summary>
    /// Gets the URL for the configuration page of this plugin.
    /// </summary>
    public override string GetConfigurationPageUrl()
    {
        return $"{_webHelper.GetStoreLocation()}Admin/TaxJar/Configure";
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
        return Task.FromResult<IList<string>>(new List<string>
        {
            PublicWidgetZones.OpCheckoutBillingAddressTop,
            PublicWidgetZones.OpCheckoutBillingAddressBottom,
            PublicWidgetZones.OpCheckoutShippingAddressBottom,
            PublicWidgetZones.CheckoutBillingAddressBottom,
            PublicWidgetZones.CheckoutShippingAddressBottom,
            PublicWidgetZones.CheckoutPaymentMethodBottom,
            PublicWidgetZones.OpCheckoutPaymentMethodBottom,
            AdminWidgetZones.OrderDetailsButtons
        });
    }

    /// <summary>
    /// Gets the type of the view component for displaying the widget.
    /// </summary>
    /// <param name="widgetZone">Name of the widget zone.</param>
    /// <returns>View component type for the specified widget zone.</returns>
    public Type GetWidgetViewComponent(string widgetZone)
    {
        if (widgetZone.Equals(PublicWidgetZones.CheckoutPaymentMethodBottom) ||
           widgetZone.Equals(PublicWidgetZones.OpCheckoutPaymentMethodBottom))
            return typeof(TaxExemptViewComponent);

        if (widgetZone == PublicWidgetZones.OpCheckoutBillingAddressTop)
            return typeof(TaxJarDefaultAddressViewComponent);

        if (widgetZone == AdminWidgetZones.OrderDetailsButtons)
            return typeof(ResendInvoiceViewComponent);

        return typeof(TaxJarAddressValidationViewComponent);
    }

    /// <summary>
    /// Installs the plugin by saving its settings and localization resources.
    /// </summary>
    public override async Task InstallAsync()
    {
        // Save the default settings for the plugin
        await _settingService.SaveSettingAsync(new TaxJarSettings()
        {
            TaxOriginAddressType = TaxOriginAddressType.DefaultTaxAddress,
            TaxRateByAddressCacheTime = 0,
            CommitTransactions = true,
            CountryId = 1
        });

        // Activate widget if not already active
        if (!_widgetSettings.ActiveWidgetSystemNames.Contains(TaxJarDefaults.SystemName))
        {
            _widgetSettings.ActiveWidgetSystemNames.Add(TaxJarDefaults.SystemName);
            await _settingService.SaveSettingAsync(_widgetSettings);
        }

        // Add localization resources
        await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
        {
            ["Enums.Nop.Plugin.Tax.TaxJar.Domain.TaxOriginAddressType.DefaultTaxAddress"] = "Default tax address",
            ["Enums.Nop.Plugin.Tax.TaxJar.Domain.TaxOriginAddressType.ShippingOrigin"] = "Shipping origin address",
            ["Plugins.Tax.TaxJar.Configuration"] = "Configuration",
            ["Plugins.Tax.TaxJar.AddressValidation.Confirm"] = "For the correct tax calculation we need the most accurate address, so we clarified the address you entered ({0}) through the validation system. Do you confirm the use of this updated address ({1})?",
            ["Plugins.Tax.TaxJar.Fields.ApiToken"] = "API token",
            ["Plugins.Tax.TaxJar.Fields.ApiToken.Hint"] = "Specify TaxJar API token.",
            ["Plugins.Tax.TaxJar.Fields.ApiToken.Required"] = "API token is required",
            ["Plugins.Tax.TaxJar.Fields.UseSandbox"] = "Use sandbox",
            ["Plugins.Tax.TaxJar.Fields.UseSandbox.Hint"] = "Determine whether to use sandbox (testing environment).",
            ["Plugins.Tax.TaxJar.Fields.IsTaxExempt"] = "Is tax exempt",
            ["Plugins.Tax.TaxJar.Fields.IsTaxExempt.Hint"] = "Select custom customer attribute to determines whether the customer is tax exempt.",
            ["Plugins.Tax.TaxJar.Fields.IsTaxExemptNotAllow"] = "Is tax exempt not avaliable",
            ["Plugins.Tax.TaxJar.Fields.PaymentMethods"] = "Restricated payment methods",
            ["Plugins.Tax.TaxJar.Fields.PaymentMethods.Hint"] = "Enter payment method system name for Restricated this payment method order during order placed Comma-separated(,).",
            ["Plugins.Tax.TaxJar.Fields.CommitTransactions"] = "Commit transactions",
            ["Plugins.Tax.TaxJar.Fields.CommitTransactions.Hint"] = "Determine whether to commit tax transactions right after they are saved.",
            ["Plugins.Tax.TaxJar.Fields.ValidateAddress"] = "Validate address",
            ["Plugins.Tax.TaxJar.Fields.ValidateAddress.Hint"] = "Determine whether to validate entered by customer addresses before the tax calculation.",
            ["Plugins.Tax.TaxJar.Fields.ValidateBilling"] = "Validate billing address",
            ["Plugins.Tax.TaxJar.Fields.ValidateBilling.Hint"] = "Check to allow 'validate billing address' in checkout.",
            ["Plugins.Tax.TaxJar.Fields.ValidateShipping"] = "Validate shipping address",
            ["Plugins.Tax.TaxJar.Fields.ValidateShipping.Hint"] = "Check to allow 'validate shipping address' in checkout.",
            ["Plugins.Tax.TaxJar.Fields.OnlyNewAddress"] = "Only new address",
            ["Plugins.Tax.TaxJar.Fields.OnlyNewAddress.Hint"] = "Check to allow validate 'only new address' in checkout billing/shipping address else it will validate existing address.",

            ["Plugins.Tax.TaxJar.Fields.TaxOriginAddressType"] = "Tax origin address",
            ["Plugins.Tax.TaxJar.Fields.TaxOriginAddressType.Hint"] = "Choose which address will be used as the origin for tax requests to Taxjar services.",
            ["Plugins.Tax.TaxJar.Fields.TaxOriginAddressType.DefaultTaxAddress.Warning"] = "Ensure that you have correctly filled in the 'Default tax address' under <a href=\"{0}\" target=\"_blank\">Tax settings</a>",
            ["Plugins.Tax.TaxJar.Fields.TaxOriginAddressType.ShippingOrigin.Warning"] = "Ensure that you have correctly filled in the 'Shipping origin' under <a href=\"{0}\" target=\"_blank\">Shipping settings</a>",
            ["Plugins.Tax.TaxJar.Fields.ExcludeTaxOnStateProvinces"] = "Exclude tax on shipping for States / provinces",
            ["Plugins.Tax.TaxJar.Fields.ExcludeTaxOnStateProvinces.Hint"] = "Choose States / provinces for Exclude tax on shipping.",
            ["Plugins.Tax.TaxJar.Fields.ExcludeTaxOnStateProvinces.NoStates"] = "No States / provinces available. Create at least one State / province before mapping.",

            ["Plugins.Tax.TaxJar.TestTax"] = "Test tax calculation",
            ["Plugins.Tax.TaxJar.TestTax.Error"] = "An error has occurred on tax request",
            ["Plugins.Tax.TaxJar.TestTax.Success"] = "The tax was successfully received",
            ["Plugins.Tax.TaxJar.VerifyCredentials"] = "Test connection",
            ["Plugins.Tax.TaxJar.VerifyCredentials.Declined"] = "Credentials declined",
            ["Plugins.Tax.TaxJar.VerifyCredentials.Verified"] = "Credentials verified",
            ["Plugins.Tax.TaxJar.Fields.AttachPdfInvoiceToOrderRefundSelectedEmail"] = @"Attach PDF invoice(""order refund seleted"" email)",
            ["Plugins.Tax.TaxJar.Fields.AttachPdfInvoiceToOrderRefundSelectedEmail.Hint"] = @"Check to attach PDF invoice to the ""order refund seleted"" email sent to a customer.",
            ["Plugins.Tax.TaxJar.Fields.CCAttributeId"] = "CC email attribute",
            ["Plugins.Tax.TaxJar.Fields.CCAttributeId.Hint"] = "Select attribute to send a copy of the email with any recipient of your choice.",

            ["Plugins.Tax.TaxJar.Fields.TaxExclusionOnCall"] = "Force restriction tax calculation calls",
            ["Plugins.Tax.TaxJar.Fields.TaxExclusionOnCall.Hint"] = "Check to force restriction tax calculation calls only allowed methods.",
            ["Plugins.Tax.TaxJar.Fields.ControllerName"] = "Controller",
            ["Plugins.Tax.TaxJar.Fields.ActionName"] = "Action",
            ["Plugins.Tax.TaxJar.Fields.Method"] = "Method",
            ["Plugins.Tax.TaxJar.Fields.ControllerAction.Required"] = "Controller and Action both are required.",
            ["Plugins.Tax.TaxJar.TaxExempt.Confirm.Title"] = "Oops... you are charged sales tax",
            ["Plugins.Tax.TaxJar.TaxExempt.Confirm.Body"] = "If you add your sales tax detail then you will get exemption from sale tax. <Br /> Would you like to add your sales tax detail?",
            ["Plugins.Tax.TaxJar.TaxExempt.Save"] = "Save and Continue",
            ["Plugins.Tax.TaxJar.TaxExempt.Yes"] = "Yes",
            ["Plugins.Tax.TaxJar.TaxExempt.No"] = "Continue anyway",
            ["Plugins.Tax.TaxJar.TaxExempt.Info.Title"] = "Sales tax detail",
            ["Plugins.Tax.TaxJar.Fields.Download"] = "Sales tax exemption certificate",
            ["Plugins.Tax.TaxJar.Fields.Download.Button"] = "Download",
            ["Plugins.Tax.TaxJar.TaxExempt.Warings"] = "Fill in sales tax exempt info if you can claim sales tax exempt. You are responsible for wrongful info in case of audit. <br /> You must enter 1 of the 3 ID numbers and reason for tax exempt.",
            ["Plugins.Tax.TaxJar.TaxExempt.Error"] = "You must enter 1 of the 3 ID numbers and reason for tax exempt.",

            ["Plugins.Tax.TaxJar.Fields.RefundSelected"] = "Refund (Selected)",
            ["Plugins.Tax.TaxJar.Fields.RefundSelectedOffline"] = "Refund (Selected - Offline)",
            ["Plugins.Tax.TaxJar.Fields.RefundSelected.OrderInfo"] = "Refund for order #{0}",
            ["Plugins.Tax.TaxJar.Fields.CalculatedRefund"] = "Refund {0}",
            ["Plugins.Tax.TaxJar.Fields.CalculatedRefund.AmountToRefund.MaxError"] = "Unable to refund order. {0}",
            ["Plugins.Tax.TaxJar.Fields.RefundSelected.HasAmountMissMatch"] = "Kit products amount missmatch with orderitem <br /><br /><strong>Kit products total:</strong> {0} <br /><strong>Different:</strong> {1} <br /><br />Make a correction before refund for Kit products.",
            ["Plugins.Tax.TaxJar.Fields.PDFInvoice.RefundOrder#"] = "Order / Refund Invoice# {0}",
            ["Plugins.Tax.TaxJar.Fields.PDFInvoice.RefundOrderTotal"] = "Refund total",
            ["Plugins.Tax.TaxJar.Fields.PDFInvoice.RefundPaymentMethod"] = "Refunded to: {0}",
            ["Plugins.Tax.TaxJar.Fields.Messages.RefundOrder.RefundOrderTotal"] = "Refund total",

            ["Plugins.Tax.TaxJar.AddressValidation.Title"] = "Would you like to use this suggested address?",
            ["Plugins.Tax.TaxJar.AddressValidation.CurrentAddress"] = "We need the most accurate address, so we clarified the address you entered through the validation system.",
            ["Plugins.Tax.TaxJar.AddressValidation.ValidAddress"] = "Do you confirm the use of this updated address?",
            ["Plugins.Tax.TaxJar.AddressValidation.Error"] = "Opps... Address is invalid",
            ["Plugins.Tax.TaxJar.AddressValidation.Error.Title"] = "What does this mean?",
            ["Plugins.Tax.TaxJar.AddressValidation.Error.Detail"] = "The address, exactly as submitted, could not be found in the city, state, or ZIP Code provided. Either the primary number is missing, the street is missing, or the street is too badly misspelled to understand.",

            ["Plugins.Tax.TaxJar.AddressValidation.Common.Yes"] = "Yes",
            ["Plugins.Tax.TaxJar.AddressValidation.Common.No"] = "No",
            ["Plugins.Tax.TaxJar.AddressValidation.Common.Continue"] = "Continue anyway",
            ["Plugins.Tax.TaxJar.AddressValidation.Common.Back"] = "Back",
            ["Plugins.Tax.TaxJar.Checkout.UseDefaultAddress"] = "Use default address as billing address",
            ["Plugins.Tax.TaxJar.Checkout.SetDefaultAddress"] = "For Set default address Click Here",
            ["Plugins.Tax.TaxJar.Common.SetDefault"] = "Default Address",
            ["Plugins.Tax.TaxJar.Common.Default"] = "Set Default Address",

            ["Plugins.Tax.TaxJar.PdfInvoice.ResendInvoice"] = "Resend invoice",
            ["Plugins.Tax.TaxJar.PdfInvoice.ResendInvoiceInfo"] = "Please confirm order# {0} to resend invoice manually on below email address.",
            ["Plugins.Tax.TaxJar.ResendInvoice.Success"] = "Resend invoice email has been successfully queued.",
            ["Plugins.Tax.TaxJar.TaxExempt.Invalid"] = "{0} is not valid.",

            ["Plugins.Tax.TaxJar.Fields.HiddenCustomerRoleId"] = "Hidden customer role",
            ["Plugins.Tax.TaxJar.Fields.HiddenCustomerRoleId.Hint"] = "Select customer role to validate product with ACL rules and allow while impersonated.",

            ["Plugins.Tax.TaxJar.ResendTransaction"] = "Resend to Taxjar",
            ["Plugins.Tax.TaxJar.ResendTransaction.Success"] = "Resend transaction to taxjar has been successful.",

            ["Plugins.Tax.TaxJar.Fields.Order.RefundTax"] = "Refund tax",

            ["Plugins.Tax.TaxJar.Log"] = "Log",
            ["Plugins.Tax.TaxJar.Log.BackToList"] = "back to log",
            ["Plugins.Tax.TaxJar.Log.ClearLog"] = "Clear log",
            ["Plugins.Tax.TaxJar.Log.CreatedDate"] = "Created on",
            ["Plugins.Tax.TaxJar.Log.CreatedDate.Hint"] = "Date and time the log entry was created.",
            ["Plugins.Tax.TaxJar.Log.Customer"] = "Customer",
            ["Plugins.Tax.TaxJar.Log.Customer.Hint"] = "Name of the customer.",
            ["Plugins.Tax.TaxJar.Log.Deleted"] = "The log entry has been deleted successfully.",
            ["Plugins.Tax.TaxJar.Log.Hint"] = "View log entry details",
            ["Plugins.Tax.TaxJar.Log.RequestMessage"] = "Request message",
            ["Plugins.Tax.TaxJar.Log.RequestMessage.Hint"] = "The details of the request.",
            ["Plugins.Tax.TaxJar.Log.ResponseMessage"] = "Response message",
            ["Plugins.Tax.TaxJar.Log.ResponseMessage.Hint"] = "The details of the response.",
            ["Plugins.Tax.TaxJar.Log.StatusCode"] = "Status code",
            ["Plugins.Tax.TaxJar.Log.StatusCode.Hint"] = "The status code of the response.",
            ["Plugins.Tax.TaxJar.Log.Url"] = "Url",
            ["Plugins.Tax.TaxJar.Log.Url.Hint"] = "The requested URL.",
            ["Plugins.Tax.TaxJar.Log.Search.CreatedFrom"] = "Created from",
            ["Plugins.Tax.TaxJar.Log.Search.CreatedFrom.Hint"] = "The creation from date for the search.",
            ["Plugins.Tax.TaxJar.Log.Search.CreatedTo"] = "Created to",
            ["Plugins.Tax.TaxJar.Log.Search.CreatedTo.Hint"] = "The creation to date for the search.",

            ["Plugins.Tax.TaxJar.ReassignOrders"] = "Reassign Orders",
            ["Plugins.Tax.TaxJar.ReassignOrders.Customer"] = "Customer",
            ["Plugins.Tax.TaxJar.ReassignOrders.AssignToGuest"] = "Assign to guest customer",
            ["Plugins.Tax.TaxJar.ReassignOrders.Reassign"] = "Reassign selected orders",
            ["Plugins.Tax.TaxJar.ReassignOrders.Reassign.Success"] = "Orders reassign has been successfully.",
            ["Plugins.Tax.TaxJar.ReassignOrders.ReassignSelected"] = "Reassign orders",

            ["Plugins.Tax.TaxJar.Configuration.Instructions"] = "To enable this tax provider, you'll need to:<br /><br />" +
            "1. <a href=\"https://app.taxjar.com/sign_up\" target=\"_blank\">Create a TaxJar account</a><br />" +
            "2. Log in at the TaxJar portal<br />" +
            "3. Add a company and some nexus jurisdiction selections to get any tax results<br />" +
            "4. Fill in your TaxJar account details below<br />"
        });

        await base.InstallAsync();
    }

    /// <summary>
    /// Uninstalls the plugin by resetting settings and removing resources.
    /// </summary>
    public override async Task UninstallAsync()
    {
        // Reset tax provider settings and remove plugin-specific data
        _taxSettings.ActiveTaxProviderSystemName = (await _taxPluginManager.LoadAllPluginsAsync())
            .FirstOrDefault(taxProvider => !taxProvider.PluginDescriptor.SystemName.Equals(TaxJarDefaults.SystemName))
            ?.PluginDescriptor.SystemName;
        await _settingService.SaveSettingAsync(_taxSettings);
        _widgetSettings.ActiveWidgetSystemNames.Remove(TaxJarDefaults.SystemName);
        await _settingService.SaveSettingAsync(_widgetSettings);
        await _settingService.DeleteSettingAsync<TaxJarSettings>();

        // Delete locale resources
        await _localizationService.DeleteLocaleResourcesAsync("Enums.Nop.Plugin.Tax.TaxJar");
        await _localizationService.DeleteLocaleResourcesAsync("Plugins.Tax.TaxJar");

        await base.UninstallAsync();
    }
    #endregion

    #region Properties
    /// <summary>
    /// Gets a value indicating whether to hide this plugin on the widget list page in the admin area
    /// </summary>
    public bool HideInWidgetList => true;
    #endregion
}
