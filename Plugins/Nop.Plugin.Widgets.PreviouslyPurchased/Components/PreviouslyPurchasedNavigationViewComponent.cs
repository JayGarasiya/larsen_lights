using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Widgets.PreviouslyPurchased.Services;
using Nop.Web.Framework.Components;
using Nop.Web.Framework.Infrastructure;

namespace Nop.Plugin.Widgets.PreviouslyPurchased.Components;

/// <summary>
/// Represents the view component for displaying 
/// the "Previously Purchased" products in the account navigation.
/// </summary>
public class PreviouslyPurchasedNavigationViewComponent : NopViewComponent
{
    #region Fields
    private readonly IPreviouslyPurchasedService _previouslyPurchasedService;
    private readonly PreviouslyPurchasedSettings _previouslyPurchasedSettings;
    private readonly IStoreContext _storeContext;
    private readonly IWorkContext _workContext;
    #endregion

    #region Ctor
    public PreviouslyPurchasedNavigationViewComponent(
        IPreviouslyPurchasedService previouslyPurchasedService,
        PreviouslyPurchasedSettings previouslyPurchasedSettings,
        IStoreContext storeContext,
        IWorkContext workContext)
    {
        _previouslyPurchasedService = previouslyPurchasedService;
        _previouslyPurchasedSettings = previouslyPurchasedSettings;
        _storeContext = storeContext;
        _workContext = workContext;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Invokes the view component to display the previously purchased products link in the account navigation.
    /// </summary>
    /// <param name="widgetZone">The name of the widget zone where this component will be rendered.</param>
    /// <param name="additionalData">Additional data passed from the widget zone (not used in this case).</param>
    /// <returns>
    /// A task representing the asynchronous operation. 
    /// The task result contains the view component result.
    /// </returns>
    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
    {
        // Check if the widget zone is 'AccountNavigationAfter' and if the 'MyAccountNavigation' setting is enabled
        if (!widgetZone.Equals(PublicWidgetZones.AccountNavigationAfter) || !_previouslyPurchasedSettings.MyAccountNavigation)
            return Content(string.Empty);

        // If the setting for the number of previously purchased products is 0, do not display the widget
        if (_previouslyPurchasedSettings.PreviouslyPurchasedProductsNumber == 0)
            return Content(string.Empty);

        // Get the current customer and store context
        var customer = await _workContext.GetCurrentCustomerAsync();
        var store = await _storeContext.GetCurrentStoreAsync();

        // Load the previously purchased products for the current customer
        var products = await _previouslyPurchasedService.PreviouslyPurchasedReportAsync(
            customer.Id,
            storeId: store.Id,
            pageSize: _previouslyPurchasedSettings.PreviouslyPurchasedProductsNumber);

        // If no previously purchased products are found, return empty content
        if (!products.Any())
            return Content(string.Empty);

        return View("~/Plugins/Widgets.PreviouslyPurchased/Views/PublicInfoNavigation.cshtml");
    }
    #endregion
}