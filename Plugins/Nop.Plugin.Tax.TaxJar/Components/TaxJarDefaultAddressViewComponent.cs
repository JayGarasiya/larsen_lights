using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Tax.TaxJar.Models;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Tax;
using Nop.Web.Framework.Components;
using Nop.Web.Framework.Infrastructure;

namespace Nop.Plugin.Tax.TaxJar.Components;

/// <summary>
/// Represents a view component that displays the default address information for the customer.
/// </summary>
public class TaxJarDefaultAddressViewComponent : NopViewComponent
{
    #region Fields
    private readonly ITaxPluginManager _taxPluginManager;
    private readonly IGenericAttributeService _genericAttributeService;
    private readonly IWorkContext _workContext;
    private readonly ICustomerService _customerService;
    #endregion

    #region Ctor
    public TaxJarDefaultAddressViewComponent(
        ITaxPluginManager taxPluginManager,
        IGenericAttributeService genericAttributeService,
        IWorkContext workContext,
        ICustomerService customerService)
    {
        _taxPluginManager = taxPluginManager;
        _genericAttributeService = genericAttributeService;
        _workContext = workContext;
        _customerService = customerService;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Invokes the widget view component to render the default address for the current customer.
    /// </summary>
    /// <param name="widgetZone">The widget zone where the component will be displayed.</param>
    /// <param name="additionalData">Additional data passed to the component.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. 
    /// The task result contains the view component result.
    /// </returns>
    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
    {
        // Ensure that the TaxJar tax provider is active for the current customer
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (!await _taxPluginManager.IsPluginActiveAsync(TaxJarDefaults.SystemName, customer))
            return Content(string.Empty);

        // Ensure the widget is being called in the correct widget zone
        if (!widgetZone.Equals(PublicWidgetZones.OpCheckoutBillingAddressTop))
            return Content(string.Empty);

        // Ensure that the customer is not a guest (guest customers do not have default addresses)
        if (await _customerService.IsGuestAsync(customer))
            return Content(string.Empty);

        var model = new AddressLookupModel();

        // Retrieve the default address ID from the customer's attributes
        var defaultAddressId = await _genericAttributeService.GetAttributeAsync<int>(customer, "DefaultAddress");
        if (defaultAddressId > 0)
            model.DefaultAddressId = defaultAddressId;

        return View("~/Plugins/Tax.TaxJar/Views/Checkout/DefaultAddress.cshtml", model);
    }
    #endregion
}