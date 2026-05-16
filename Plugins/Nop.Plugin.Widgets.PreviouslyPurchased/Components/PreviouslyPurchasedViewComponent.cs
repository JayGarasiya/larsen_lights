using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Widgets.PreviouslyPurchased.Services;
using Nop.Web.Factories;
using Nop.Web.Framework.Components;
using Nop.Web.Models.Catalog;

namespace Nop.Plugin.Widgets.PreviouslyPurchased.Components;

/// <summary>
/// Represents the view component that displays the previously purchased products widget.
/// </summary>
public class PreviouslyPurchasedViewComponent : NopViewComponent
{
    #region Fields
    private readonly IPreviouslyPurchasedService _previouslyPurchasedService;
    private readonly PreviouslyPurchasedSettings _previouslyPurchasedSettings;
    private readonly IProductModelFactory _productModelFactory;
    private readonly IStoreContext _storeContext;
    private readonly IWorkContext _workContext;
    #endregion

    #region Ctor
    public PreviouslyPurchasedViewComponent(
        IPreviouslyPurchasedService previouslyPurchasedService,
        PreviouslyPurchasedSettings previouslyPurchasedSettings,
        IProductModelFactory productModelFactory,
        IStoreContext storeContext,
        IWorkContext workContext)
    {
        _previouslyPurchasedService = previouslyPurchasedService;
        _previouslyPurchasedSettings = previouslyPurchasedSettings;
        _productModelFactory = productModelFactory;
        _storeContext = storeContext;
        _workContext = workContext;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Invokes the view component to display the previously purchased products widget.
    /// </summary>
    /// <param name="widgetZone">The name of the widget zone where the component will be displayed.</param>
    /// <param name="additionalData">Additional data passed from the widget zone (not used in this case).</param>
    /// <returns>
    /// A task representing the asynchronous operation. 
    /// The task result contains the view component result.
    /// </returns>
    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
    {
        // Check if the widget zone matches the configured widget zone
        if (widgetZone != _previouslyPurchasedSettings.WidgetZone)
            return Content(string.Empty);

        // Check if the number of products to display is set to 0
        if (_previouslyPurchasedSettings.PreviouslyPurchasedProductsNumber == 0)
            return Content(string.Empty);

        // Get the current customer and store information
        var customer = await _workContext.GetCurrentCustomerAsync();
        var store = await _storeContext.GetCurrentStoreAsync();

        // Load previously purchased products for the customer
        var products = await _previouslyPurchasedService.PreviouslyPurchasedReportAsync(customer.Id, storeId: store.Id, pageSize: _previouslyPurchasedSettings.PreviouslyPurchasedProductsNumber);

        // If no products are found, return an empty content result
        if (!products.Any())
            return Content(string.Empty);

        // Prepare the model to display the product overview
        var model = new List<ProductOverviewModel>();
        model.AddRange(await _productModelFactory.PrepareProductOverviewModelsAsync(products, false, true, 64));

        // Return the view with the prepared product models
        return View("~/Plugins/Widgets.PreviouslyPurchased/Views/PublicInfo.cshtml", model);
    }
    #endregion
}