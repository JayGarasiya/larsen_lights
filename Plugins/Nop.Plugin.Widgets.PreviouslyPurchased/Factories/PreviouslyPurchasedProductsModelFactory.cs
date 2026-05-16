using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Widgets.PreviouslyPurchased.Models;
using Nop.Plugin.Widgets.PreviouslyPurchased.Services;
using Nop.Services.Localization;
using Nop.Web.Factories;
using Nop.Web.Models.Catalog;

namespace Nop.Plugin.Widgets.PreviouslyPurchased.Factories;

/// <summary>
/// Factory to create models for previously purchased products.
/// </summary>
public class PreviouslyPurchasedProductsModelFactory : IPreviouslyPurchasedProductsModelFactory
{
    #region Fields
    private readonly ILocalizationService _localizationService;
    private readonly IStoreContext _storeContext;
    private readonly IPreviouslyPurchasedService _previouslyPurchasedService;
    private readonly ICatalogModelFactory _catalogModelFactory;
    private readonly CatalogSettings _catalogSettings;
    private readonly IProductModelFactory _productModelFactory;
    private readonly IWorkContext _workContext;
    #endregion

    #region Ctor
    public PreviouslyPurchasedProductsModelFactory(
        ILocalizationService localizationService,
        IStoreContext storeContext,
        IPreviouslyPurchasedService previouslyPurchasedService,
        ICatalogModelFactory catalogModelFactory,
        CatalogSettings catalogSettings,
        IProductModelFactory productModelFactory,
        IWorkContext workContext)
    {
        _localizationService = localizationService;
        _storeContext = storeContext;
        _previouslyPurchasedService = previouslyPurchasedService;
        _catalogModelFactory = catalogModelFactory;
        _catalogSettings = catalogSettings;
        _productModelFactory = productModelFactory;
        _workContext = workContext;
    }
    #endregion

    #region Utilities

    /// <summary>
    /// Prepares catalog products
    /// </summary>
    /// <param name="model">Catalog products model</param>
    /// <param name="products">The products</param>
    /// <param name="isFiltering">A value indicating that filtering has been applied</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    protected virtual async Task PrepareCatalogProductsAsync(CatalogProductsModel model, IPagedList<Product> products, bool isFiltering = false)
    {
        if (!string.IsNullOrEmpty(model.WarningMessage))
            return;

        if (!products.Any() && isFiltering)
            model.NoResultMessage = await _localizationService.GetResourceAsync("Catalog.Products.NoResult");
        else
        {
            model.Products = (await _productModelFactory.PrepareProductOverviewModelsAsync(products)).ToList();
            model.LoadPagedList(products);
        }
    }
    #endregion

    #region Methods
    /// <summary>
    /// Prepares the model for the previously purchased products.
    /// This includes setting the title, URL, and retrieving catalog products.
    /// </summary>
    /// <param name="command">Model containing the catalog command parameters.</param>
    /// <returns>
    /// A task representing the asynchronous operation. 
    /// The task result contains the previously purchased products model.
    /// </returns>
    public async Task<PreviouslyPurchasedModel> PreparePreviouslyPurchasedProductsModelAsync(CatalogProductsCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var model = new PreviouslyPurchasedModel
        {
            // Set the title of the previously purchased products section
            Title = await _localizationService.GetResourceAsync("Plugins.Widgets.PreviouslyPurchased.Title"),
            Url = "previouslypurchasedproducts",
            CatalogProductsModel = await PrepareProductsModelAsync(command)
        };

        return model;
    }

    /// <summary>
    /// Prepares the catalog products model based on the previously purchased products.
    /// </summary>
    /// <param name="command">Model containing the catalog command parameters.</param>
    /// <returns>
    /// A task representing the asynchronous operation. 
    /// The task result contains the catalog products model.
    /// </returns>
    public async Task<CatalogProductsModel> PrepareProductsModelAsync(CatalogProductsCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var model = new CatalogProductsModel
        {
            UseAjaxLoading = _catalogSettings.UseAjaxCatalogProductsLoading
        };

        // Prepare sorting options for the catalog
        await _catalogModelFactory.PrepareSortingOptionsAsync(model, command);

        // Prepare the available view modes (e.g., grid, list)
        await _catalogModelFactory.PrepareViewModesAsync(model, command);

        // Prepare the page size options based on the settings
        await _catalogModelFactory.PreparePageSizeOptionsAsync(model, command, _catalogSettings.SearchPageAllowCustomersToSelectPageSize,
            _catalogSettings.SearchPagePageSizeOptions, _catalogSettings.SearchPageProductsPerPage);

        // Load the previously purchased products based on the command parameters
        var products = await _previouslyPurchasedService.PreviouslyPurchasedReportAsync(
            (await _workContext.GetCurrentCustomerAsync()).Id,
            storeId: (await _storeContext.GetCurrentStoreAsync()).Id,
            pageIndex: command.PageNumber - 1,
            pageSize: command.PageSize,
            orderBy: (ProductSortingEnum)command.OrderBy);

        // Prepare the catalog products based on the loaded products
        await PrepareCatalogProductsAsync(model, products, false);

        return model;
    }
    #endregion

}
