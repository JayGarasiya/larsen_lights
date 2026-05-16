using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Tax.TaxJar.Models;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Orders;
using Nop.Services.Tax;
using Nop.Web.Factories;
using Nop.Web.Framework.Components;
using Nop.Web.Framework.Infrastructure;

namespace Nop.Plugin.Tax.TaxJar.Components;

/// <summary>
/// Represents a view component to show a tax-exempt popup for customers.
/// This popup allows customers to indicate their tax-exempt status.
/// </summary>
public class TaxExemptViewComponent : NopViewComponent
{
    #region Fields
    private readonly IOrderTotalCalculationService _orderTotalCalculationService;
    private readonly IShoppingCartService _shoppingCartService;
    private readonly IWorkContext _workContext;
    private readonly ICurrencyService _currencyService;
    private readonly IStoreContext _storeContext;
    private readonly ICustomerModelFactory _customerModelFactory;
    private readonly ITaxPluginManager _taxPluginManager;
    private readonly OrderSettings _orderSetting;
    private readonly ICustomerService _customerService;
    #endregion

    #region Ctor
    public TaxExemptViewComponent(
        IOrderTotalCalculationService orderTotalCalculationService,
        IShoppingCartService shoppingCartService,
        IWorkContext workContext,
        ICurrencyService currencyService,
        IStoreContext storeContext,
        ICustomerModelFactory customerModelFactory,
        ITaxPluginManager taxPluginManager,
        OrderSettings orderSetting,
        ICustomerService customerService)
    {
        _orderTotalCalculationService = orderTotalCalculationService;
        _shoppingCartService = shoppingCartService;
        _workContext = workContext;
        _currencyService = currencyService;
        _storeContext = storeContext;
        _customerModelFactory = customerModelFactory;
        _taxPluginManager = taxPluginManager;
        _orderSetting = orderSetting;
        _customerService = customerService;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Invokes the tax-exempt popup widget.
    /// This method checks if the customer is eligible to see the tax-exempt popup and renders it if necessary.
    /// </summary>
    /// <param name="widgetZone">The name of the widget zone where the widget will be displayed.</param>
    /// <param name="additionalData">Any additional data that may be passed to the component.</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the view component result
    /// </returns>
    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();

        // Ensure that tax jar tax provider is active
        if (!await _taxPluginManager.IsPluginActiveAsync(TaxJarDefaults.SystemName, customer))
            return Content(string.Empty);

        // Ensure that it's a proper widget zone
        if (!widgetZone.Equals(PublicWidgetZones.OpCheckoutPaymentMethodBottom) && !widgetZone.Equals(PublicWidgetZones.CheckoutPaymentMethodBottom))
            return Content(string.Empty);

        // If the customer is already tax-exempt or has custom customer attributes, do not show the popup
        if (customer.IsTaxExempt && !string.IsNullOrEmpty(customer.CustomCustomerAttributesXML))
            return Content(string.Empty);

        var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart,
                                                                   (await _storeContext.GetCurrentStoreAsync()).Id);
        var (shoppingCartTaxBase, _) = await _orderTotalCalculationService.GetTaxTotalAsync(cart);
        var shoppingCartTax = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(shoppingCartTaxBase,
                                                                                          await _workContext.GetWorkingCurrencyAsync());

        // If the shopping cart has tax and the customer is not tax-exempt, show the tax-exempt popup
        if (shoppingCartTax > decimal.Zero && !customer.IsTaxExempt)
        {
            var model = new TaxExemptModel()
            {
                ButtonClass = ".payment-method-next-step-button",
                OnePageCheckoutEnabled = _orderSetting.OnePageCheckoutEnabled,
                IsGuest = await _customerService.IsGuestAsync(customer),
                IsTaxExempt = customer.IsTaxExempt,
                FormId = _orderSetting.OnePageCheckoutEnabled ? string.Empty : ".payment-method-page form"
            };

            // Custom customer attributes
            var customAttributes = await _customerModelFactory.PrepareCustomCustomerAttributesAsync(customer);
            foreach (var attribute in customAttributes)
                model.CustomerAttributeModels.Add(attribute);

            return View("~/Plugins/Tax.TaxJar/Views/Checkout/TaxExemptPopUp.cshtml", model);
        }
        return Content(string.Empty);
    }
    #endregion
}