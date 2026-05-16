using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Widgets.PreviouslyPurchased.Factories;
using Nop.Plugin.Widgets.PreviouslyPurchased.Services;
using Nop.Web.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Models.Catalog;

namespace Nop.Plugin.Widgets.PreviouslyPurchased.Controllers;

[AutoValidateAntiforgeryToken]
public class PreviouslyPurchasedCatalogController : BasePublicController
{
    #region Fields
    private readonly IPreviouslyPurchasedService _previouslyPurchasedService;
    private readonly IPreviouslyPurchasedProductsModelFactory _previouslyPurchasedProductsModelFactory;
    #endregion

    #region Ctor
    public PreviouslyPurchasedCatalogController(
        IPreviouslyPurchasedService previouslyPurchasedService,
        IPreviouslyPurchasedProductsModelFactory previouslyPurchasedProductsModelFactory)
    {
        _previouslyPurchasedService = previouslyPurchasedService;
        _previouslyPurchasedProductsModelFactory = previouslyPurchasedProductsModelFactory;
    }
    #endregion

    #region Methods
    public virtual async Task<IActionResult> PreviouslyPurchasedProducts(CatalogProductsCommand command)
    {
        // Check if the plugin is active for the current customer and store
        if (!await _previouslyPurchasedService.PluginActiveAsync())
            return InvokeHttp404();  // Return 404 if the plugin is not active

        // Prepare the model containing previously purchased products based on the provided command
        var model = await _previouslyPurchasedProductsModelFactory.PreparePreviouslyPurchasedProductsModelAsync(command);

        return View("~/Plugins/Widgets.PreviouslyPurchased/Views/PreviouslyPurchasedCatalog/PreviouslyPurchasedProducts.cshtml", model);
    }

    [CheckLanguageSeoCode(ignore: true)]
    public virtual async Task<IActionResult> GetPreviouslyPurchasedProducts(CatalogProductsCommand command)
    {
        // Check if the plugin is active for the current customer and store
        if (!await _previouslyPurchasedService.PluginActiveAsync())
            return NotFound();

        // Prepare the model containing previously purchased products based on the provided command
        var model = await _previouslyPurchasedProductsModelFactory.PreparePreviouslyPurchasedProductsModelAsync(command);

        return PartialView("_ProductsInGridOrLines", model.CatalogProductsModel);
    }

    #endregion
}
