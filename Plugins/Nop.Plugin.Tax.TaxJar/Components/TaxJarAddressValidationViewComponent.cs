using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Tax.TaxJar.Models;
using Nop.Services.Cms;
using Nop.Services.Tax;
using Nop.Web.Framework.Components;
using Nop.Web.Framework.Infrastructure;

namespace Nop.Plugin.Tax.TaxJar.Components;

/// <summary>
/// Represents a view component to validate entered address 
/// and display a confirmation dialog on the checkout page
/// </summary>
public class TaxJarAddressValidationViewComponent : NopViewComponent
{
    #region Fields
    private readonly ITaxPluginManager _taxPluginManager;
    private readonly IWidgetPluginManager _widgetPluginManager;
    private readonly OrderSettings _orderSetting;
    private readonly TaxJarSettings _taxSettings;
    private readonly IWorkContext _workContext;
    #endregion

    #region Ctor
    public TaxJarAddressValidationViewComponent(
        ITaxPluginManager taxPluginManager,
        IWidgetPluginManager widgetPluginManager,
        OrderSettings orderSetting,
        TaxJarSettings taxSettings,
        IWorkContext workContext)
    {
        _taxPluginManager = taxPluginManager;
        _widgetPluginManager = widgetPluginManager;
        _orderSetting = orderSetting;
        _taxSettings = taxSettings;
        _workContext = workContext;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Invokes the widget view component for address validation.
    /// This method checks if the TaxJar address validation widget should be shown.
    /// </summary>
    /// <param name="widgetZone">Widget zone where the widget is rendered (billing or shipping address).</param>
    /// <param name="additionalData">Additional parameters (unused in this method).</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the view component result
    /// </returns>
    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
    {
        // Ensure that the address lookup plugin (TaxJar) is active
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (await _widgetPluginManager.IsPluginActiveAsync(TaxJarDefaults.AddressLookupSystemName, customer))
            return Content(string.Empty);

        // Ensure that TaxJar tax provider plugin is active
        if (!await _taxPluginManager.IsPluginActiveAsync(TaxJarDefaults.SystemName, customer))
            return Content(string.Empty);

        // Ensure that it's a valid widget zone for address validation (billing or shipping)
        if (!widgetZone.Equals(PublicWidgetZones.OpCheckoutBillingAddressBottom)
            && !widgetZone.Equals(PublicWidgetZones.CheckoutBillingAddressBottom)
            && !widgetZone.Equals(PublicWidgetZones.OpCheckoutShippingAddressBottom)
            && !widgetZone.Equals(PublicWidgetZones.CheckoutShippingAddressBottom))
            return Content(string.Empty);

        // Determine if the widget is being invoked for the billing address
        var isBilling = widgetZone.Equals(PublicWidgetZones.OpCheckoutBillingAddressBottom) || widgetZone.Equals(PublicWidgetZones.CheckoutBillingAddressBottom);
        if (isBilling && !_taxSettings.ValidateBilling)
            return Content(string.Empty);

        // Determine if the widget is being invoked for the shipping address
        var isShipping = widgetZone.Equals(PublicWidgetZones.OpCheckoutShippingAddressBottom) || widgetZone.Equals(PublicWidgetZones.CheckoutShippingAddressBottom);
        if (isShipping && !_taxSettings.ValidateShipping)
            return Content(string.Empty);

        var model = new AddressLookupModel()
        {
            ButtonClass = ".new-address-next-step-button",
            OnePageCheckoutEnabled = _orderSetting.OnePageCheckoutEnabled,
            Prefix = isBilling ? "BillingNewAddress" : "ShippingNewAddress",
            IsShippingAddress = isShipping,
            FormId = _orderSetting.OnePageCheckoutEnabled ? isBilling ? "#co-billing-form" : "#co-shipping-form" : isBilling ? "#billing-form" : "#shipping-form",
            SelectListId = _orderSetting.OnePageCheckoutEnabled ? isBilling ? "#billing-address-select" : "#shipping-address-select" : string.Empty,
            Validate = true
        };

        return View("~/Plugins/Tax.TaxJar/Views/Checkout/TaxJarAddressValidation.cshtml", model);
    }
    #endregion
}