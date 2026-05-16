using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Vendors;
using Nop.Core.Events;
using Nop.Plugin.Widgets.MakeTypeModel.Services.MakeTypeModel;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Vendors;
using Nop.Web.Factories;
using Nop.Web.Framework.Events;
using Nop.Web.Models.Catalog;

namespace Nop.Plugin.Widgets.MakeTypeModel.Factories
{
    /// <summary>
    /// Represents a find parts model factory
    /// </summary>
    public partial class FindPartsModelFactory : IFindPartsModelFactory
    {
        #region Fields

        protected readonly CatalogSettings _catalogSettings;
        protected readonly ICategoryService _categoryService;
        protected readonly ICurrencyService _currencyService;
        protected readonly IEventPublisher _eventPublisher;
        protected readonly IHttpContextAccessor _httpContextAccessor;
        protected readonly ILocalizationService _localizationService;
        protected readonly IManufacturerService _manufacturerService;
        protected readonly IProductModelFactory _productModelFactory;
        protected readonly ISearchTermService _searchTermService;
        protected readonly IStoreContext _storeContext;
        protected readonly IVendorService _vendorService;
        protected readonly IWorkContext _workContext;
        protected readonly VendorSettings _vendorSettings;
        protected readonly IMakeTypeModelService _makeTypeModelService;

        #endregion

        #region Ctor

        public FindPartsModelFactory(CatalogSettings catalogSettings,
            ICategoryService categoryService,
            ICurrencyService currencyService,
            IEventPublisher eventPublisher,
            IHttpContextAccessor httpContextAccessor,
            ILocalizationService localizationService,
            IManufacturerService manufacturerService,
            IProductModelFactory productModelFactory,
            ISearchTermService searchTermService,
            IStoreContext storeContext,
            IVendorService vendorService,
            IWorkContext workContext,
            VendorSettings vendorSettings,
            IMakeTypeModelService makeTypeModelService)
        {
            _catalogSettings = catalogSettings;
            _categoryService = categoryService;
            _currencyService = currencyService;
            _eventPublisher = eventPublisher;
            _httpContextAccessor = httpContextAccessor;
            _localizationService = localizationService;
            _manufacturerService = manufacturerService;
            _productModelFactory = productModelFactory;
            _searchTermService = searchTermService;
            _storeContext = storeContext;
            _vendorService = vendorService;
            _workContext = workContext;
            _vendorSettings = vendorSettings;
            _makeTypeModelService = makeTypeModelService;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Gets the price range converted to primary store currency
        /// </summary>
        /// <param name="command">Model to get the catalog products</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the <see cref="Task"/> containing the price range converted to primary store currency
        /// </returns>
        protected virtual async Task<PriceRangeModel> GetConvertedPriceRangeAsync(CatalogProductsCommand command)
        {
            var result = new PriceRangeModel();

            if (string.IsNullOrWhiteSpace(command.Price))
                return result;

            var fromTo = command.Price.Trim().Split(new[] { '-' });
            if (fromTo.Length == 2)
            {
                var rawFromPrice = fromTo[0]?.Trim();
                if (!string.IsNullOrEmpty(rawFromPrice) && decimal.TryParse(rawFromPrice, out var from))
                    result.From = from;

                var rawToPrice = fromTo[1]?.Trim();
                if (!string.IsNullOrEmpty(rawToPrice) && decimal.TryParse(rawToPrice, out var to))
                    result.To = to;

                if (result.From > result.To)
                    result.From = result.To;

                var workingCurrency = await _workContext.GetWorkingCurrencyAsync();

                if (result.From.HasValue)
                    result.From = await _currencyService.ConvertToPrimaryStoreCurrencyAsync(result.From.Value, workingCurrency);

                if (result.To.HasValue)
                    result.To = await _currencyService.ConvertToPrimaryStoreCurrencyAsync(result.To.Value, workingCurrency);
            }

            return result;
        }

        /// <summary>
        /// Prepares the price range filter
        /// </summary>
        /// <param name="selectedPriceRange">The selected price range to filter the products</param>
        /// <param name="availablePriceRange">The available price range to filter the products</param>
        /// <returns>The price range filter</returns>
        protected virtual async Task<PriceRangeFilterModel> PreparePriceRangeFilterAsync(PriceRangeModel selectedPriceRange, PriceRangeModel availablePriceRange)
        {
            var model = new PriceRangeFilterModel();

            if (!availablePriceRange.To.HasValue || availablePriceRange.To <= 0
                || availablePriceRange.To == availablePriceRange.From)
            {
                // filter by price isn't available
                selectedPriceRange.From = null;
                selectedPriceRange.To = null;

                return model;
            }

            if (selectedPriceRange.From < availablePriceRange.From)
                selectedPriceRange.From = availablePriceRange.From;

            if (selectedPriceRange.To > availablePriceRange.To || selectedPriceRange.To < availablePriceRange.From)
                selectedPriceRange.To = availablePriceRange.To;

            var workingCurrency = await _workContext.GetWorkingCurrencyAsync();

            Task<decimal> toWorkingCurrencyAsync(decimal? price)
                => _currencyService.ConvertFromPrimaryStoreCurrencyAsync(price.Value, workingCurrency);

            model.Enabled = true;
            model.AvailablePriceRange.From = availablePriceRange.From > decimal.Zero
                ? Math.Floor(await toWorkingCurrencyAsync(availablePriceRange.From))
                : decimal.Zero;
            model.AvailablePriceRange.To = Math.Ceiling(await toWorkingCurrencyAsync(availablePriceRange.To));

            if (!selectedPriceRange.From.HasValue || availablePriceRange.From == selectedPriceRange.From)
            {
                //already converted
                model.SelectedPriceRange.From = model.AvailablePriceRange.From;
            }
            else if (selectedPriceRange.From > decimal.Zero)
                model.SelectedPriceRange.From = Math.Floor(await toWorkingCurrencyAsync(selectedPriceRange.From));

            if (!selectedPriceRange.To.HasValue || availablePriceRange.To == selectedPriceRange.To)
            {
                //already converted
                model.SelectedPriceRange.To = model.AvailablePriceRange.To;
            }
            else if (selectedPriceRange.To > decimal.Zero)
                model.SelectedPriceRange.To = Math.Ceiling(await toWorkingCurrencyAsync(selectedPriceRange.To));

            return model;
        }

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

            if (products.Count == 0 && isFiltering)
                model.NoResultMessage = await _localizationService.GetResourceAsync("Catalog.Products.NoResult");
            else
            {
                model.Products = (await _productModelFactory.PrepareProductOverviewModelsAsync(products)).ToList();
                model.LoadPagedList(products);
            }
        }

        #endregion

        #region Methods 

        #region Searching

        /// <summary>
        /// Prepare search model
        /// </summary>
        /// <param name="model">Search model</param>
        /// <param name="command">Model to get the catalog products</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the search model
        /// </returns>
        public virtual async Task<Models.SearchModel> PrepareSearchModelAsync(Models.SearchModel model, CatalogProductsCommand command)
        {
            ArgumentNullException.ThrowIfNull(model);

            ArgumentNullException.ThrowIfNull(command);

            var currentStore = await _storeContext.GetCurrentStoreAsync();

            //all makes
            var makes = await _makeTypeModelService.GetAllMakeProductsAsync();
            model.AvailableMake = makes.Select(m => new SelectListItem() { Text = m.Name, Value = m.Name, Selected = m.Name.Equals(model.make_) }).ToList();

            //all categories
            var modelCategories = await _makeTypeModelService.GetAllModelCategoriesAsync();
            model.AvailableCategories = await modelCategories.SelectAwait(async m => new SelectListItem() { Text = (await _categoryService.GetCategoryByIdAsync(m.CategoryId))?.Name, Value = m.CategoryId.ToString(), Selected = m.CategoryId.Equals(model.cid) }).ToListAsync();

            //insert this default item at first
            model.AvailableMake.Insert(0, new SelectListItem
            {
                Text = await _localizationService.GetResourceAsync("Plugins.Widgets.MakeTypeModel.FindParts.Make.Hint"),
                Value = string.Empty,
                Disabled = true,
                Selected = !model.AvailableMake.Any(m => m.Selected)
            });

            //all types by make
            var isMakeSpecified = _httpContextAccessor.HttpContext.Request.Query.ContainsKey("make_");
            if (isMakeSpecified)
            {
                var modelProducts = (await _makeTypeModelService
                .GetAllModelProductsAsync(makeName: model.make_))
                .Select(t => t.TypeName)
                .Distinct().ToList();

                if (modelProducts.Any())
                {
                    var types = (await _makeTypeModelService.GetAllTypeProductsAsync())
                        .Where(t => modelProducts.Contains(t.Name))
                        .OrderBy(t => t.DisplayOrder)
                        .Select(t => t.Name)
                        .Distinct().ToList();

                    foreach (var type in types)
                        model.AvailableType.Add(new SelectListItem { Text = type, Value = type, Selected = type.Equals(model.type_) });
                }
            }

            //insert this default item at first
            model.AvailableType.Insert(0, new SelectListItem
            {
                Text = await _localizationService.GetResourceAsync("Plugins.Widgets.MakeTypeModel.FindParts.Type.Hint"),
                Value = string.Empty,
                Disabled = true,
                Selected = !model.AvailableType.Any(m => m.Selected)
            });

            //all models by make and type
            var isTypeSpecified = _httpContextAccessor.HttpContext.Request.Query.ContainsKey("type_");
            if (isMakeSpecified && isTypeSpecified)
            {
                var modelProducts = (await _makeTypeModelService
                   .GetAllModelProductsAsync(makeName: model.make_, typeName: model.type_))
                   .Select(t => t.Name.Trim())
                   .Distinct().ToList();

                foreach (var models in modelProducts)
                    model.AvailableModel.Add(new SelectListItem { Text = models, Value = models, Selected = models.Equals(model.model_) });
            }

            //insert this default item at first
            model.AvailableModel.Insert(0, new SelectListItem
            {
                Text = await _localizationService.GetResourceAsync("Plugins.Widgets.MakeTypeModel.FindParts.Model.Hint"),
                Value = string.Empty,
                Disabled = true,
                Selected = !model.AvailableModel.Any(m => m.Selected)
            });

            var categories = $"{await _localizationService.GetResourceAsync("Plugins.Widgets.MakeTypeModel.FindParts.Category.Hint")} ({await _localizationService.GetResourceAsync("Common.All")})";
            model.AvailableCategories.Insert(0, new SelectListItem { Text = categories, Value = "0" });

            var manufacturers = await _manufacturerService.GetAllManufacturersAsync(storeId: currentStore.Id);
            if (manufacturers.Any())
            {
                model.AvailableManufacturers.Add(new SelectListItem
                {
                    Value = "0",
                    Text = await _localizationService.GetResourceAsync("Common.All")
                });
                foreach (var m in manufacturers)
                    model.AvailableManufacturers.Add(new SelectListItem
                    {
                        Value = m.Id.ToString(),
                        Text = await _localizationService.GetLocalizedAsync(m, x => x.Name),
                        Selected = model.mid == m.Id
                    });
            }

            model.asv = _vendorSettings.AllowSearchByVendor;
            if (model.asv)
            {
                var vendors = await _vendorService.GetAllVendorsAsync();
                if (vendors.Any())
                {
                    model.AvailableVendors.Add(new SelectListItem
                    {
                        Value = "0",
                        Text = await _localizationService.GetResourceAsync("Common.All")
                    });
                    foreach (var vendor in vendors)
                        model.AvailableVendors.Add(new SelectListItem
                        {
                            Value = vendor.Id.ToString(),
                            Text = await _localizationService.GetLocalizedAsync(vendor, x => x.Name),
                            Selected = model.vid == vendor.Id
                        });
                }
            }

            model.CatalogProductsModel = await PrepareSearchProductsModelAsync(model, command);

            return model;
        }

        /// <summary>
        /// Prepares the search products model
        /// </summary>
        /// <param name="model">Search model</param>
        /// <param name="command">Model to get the catalog products</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the search products model
        /// </returns>
        public virtual async Task<CatalogProductsModel> PrepareSearchProductsModelAsync(Models.SearchModel searchModel, CatalogProductsCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);

            var model = new CatalogProductsModel
            {
                UseAjaxLoading = _catalogSettings.UseAjaxCatalogProductsLoading
            };

            //sorting
            await PrepareSortingOptionsAsync(model, command);
            //view mode
            await PrepareViewModesAsync(model, command);
            //page size
            await PreparePageSizeOptionsAsync(model, command, _catalogSettings.SearchPageAllowCustomersToSelectPageSize,
                _catalogSettings.SearchPagePageSizeOptions, _catalogSettings.SearchPageProductsPerPage);

            var searchTerms = searchModel.q == null
                ? string.Empty
                : searchModel.q.Trim();

            IPagedList<Product> products = new PagedList<Product>(new List<Product>(), 0, 1);
            // only search if query string search keyword is set (used to aasync Task searching or displaying search term min length error message on /search page load)
            //or make type model is set
            //we don't use "!string.IsNullOrEmpty(searchTerms)" in cases of "ProductSearchTermMinimumLength" set to 0 but searching by other parameters (e.g. category or price filter)
            
            //var isSearchTermSpecified = _httpContextAccessor.HttpContext.Request.Query.ContainsKey("q");
            var isSearchTermSpecified = !string.IsNullOrWhiteSpace(searchModel.q);
            var isMakeSpecified = _httpContextAccessor.HttpContext.Request.Query.ContainsKey("make_");
            var isTypeSpecified = _httpContextAccessor.HttpContext.Request.Query.ContainsKey("type_");
            var isModelSpecified = _httpContextAccessor.HttpContext.Request.Query.ContainsKey("model_");

            if (isSearchTermSpecified || isMakeSpecified || isTypeSpecified || isModelSpecified)
            {
                var currentStore = await _storeContext.GetCurrentStoreAsync();

                if (searchTerms.Length < _catalogSettings.ProductSearchTermMinimumLength && !(isMakeSpecified || isTypeSpecified || isModelSpecified))
                {
                    model.WarningMessage =
                        string.Format(await _localizationService.GetResourceAsync("Search.SearchTermMinimumLengthIsNCharacters"),
                            _catalogSettings.ProductSearchTermMinimumLength);
                }
                else
                {
                    var categoryIds = new List<int>();
                    var manufacturerId = 0;
                    var searchInDescriptions = false;
                    var vendorId = 0;
                    if (searchModel.advs)
                    {
                        //advanced search
                        var categoryId = searchModel.cid;
                        if (categoryId > 0)
                        {
                            categoryIds.Add(categoryId);
                            if (searchModel.isc)
                            {
                                //include subcategories
                                categoryIds.AddRange(
                                    await _categoryService.GetChildCategoryIdsAsync(categoryId, currentStore.Id));
                            }
                        }

                        manufacturerId = searchModel.mid;

                        if (searchModel.asv)
                            vendorId = searchModel.vid;

                        searchInDescriptions = searchModel.sid;
                    }

                    var searchInProductTags = searchInDescriptions;
                    var workingLanguage = await _workContext.GetWorkingLanguageAsync();

                    //price range
                    PriceRangeModel selectedPriceRange = null;
                    if (_catalogSettings.EnablePriceRangeFiltering && _catalogSettings.SearchPagePriceRangeFiltering)
                    {
                        selectedPriceRange = await GetConvertedPriceRangeAsync(command);

                        PriceRangeModel availablePriceRange = null;
                        if (!_catalogSettings.SearchPageManuallyPriceRange)
                        {
                            async Task<decimal?> getProductPriceAsync(ProductSortingEnum orderBy)
                            {
                                var products = await _makeTypeModelService.SearchProductsAsync(0, 1,
                                    categoryIds: categoryIds,
                                    manufacturerIds: new List<int> { manufacturerId },
                                    storeId: currentStore.Id,
                                    visibleIndividuallyOnly: true,
                                    keywords: searchTerms,
                                    searchDescriptions: searchInDescriptions,
                                    searchProductTags: searchInProductTags,
                                    languageId: workingLanguage.Id,
                                    vendorId: vendorId,
                                    orderBy: orderBy,
                                    make: searchModel.make_,
                                    type: searchModel.type_,
                                    model: searchModel.model_);

                                return products?.FirstOrDefault()?.Price ?? 0;
                            }

                            availablePriceRange = new PriceRangeModel
                            {
                                From = await getProductPriceAsync(ProductSortingEnum.PriceAsc),
                                To = await getProductPriceAsync(ProductSortingEnum.PriceDesc)
                            };
                        }
                        else
                        {
                            availablePriceRange = new PriceRangeModel
                            {
                                From = _catalogSettings.SearchPagePriceFrom,
                                To = _catalogSettings.SearchPagePriceTo
                            };
                        }

                        model.PriceRangeFilter = await PreparePriceRangeFilterAsync(selectedPriceRange, availablePriceRange);
                    }

                    //products
                    products = await _makeTypeModelService.SearchProductsAsync(
                        command.PageNumber - 1,
                        command.PageSize,
                        categoryIds: categoryIds,
                        manufacturerIds: new List<int> { manufacturerId },
                        storeId: currentStore.Id,
                        visibleIndividuallyOnly: true,
                        keywords: searchTerms,
                        priceMin: selectedPriceRange?.From,
                        priceMax: selectedPriceRange?.To,
                        searchDescriptions: searchInDescriptions,
                        searchProductTags: searchInProductTags,
                        languageId: workingLanguage.Id,
                        orderBy: (ProductSortingEnum)command.OrderBy,
                        vendorId: vendorId,
                        make: searchModel.make_,
                        type: searchModel.type_,
                        model: searchModel.model_);

                    //search term statistics
                    if (!string.IsNullOrEmpty(searchTerms))
                    {
                        var searchTerm =
                            await _searchTermService.GetSearchTermByKeywordAsync(searchTerms, currentStore.Id);
                        if (searchTerm != null)
                        {
                            searchTerm.Count++;
                            await _searchTermService.UpdateSearchTermAsync(searchTerm);
                        }
                        else
                        {
                            searchTerm = new SearchTerm
                            {
                                Keyword = searchTerms,
                                StoreId = currentStore.Id,
                                Count = 1
                            };
                            await _searchTermService.InsertSearchTermAsync(searchTerm);
                        }
                    }

                    //event
                    await _eventPublisher.PublishAsync(new ProductSearchEvent
                    {
                        SearchTerm = searchTerms,
                        SearchInDescriptions = searchInDescriptions,
                        CategoryIds = categoryIds,
                        ManufacturerId = manufacturerId,
                        WorkingLanguageId = workingLanguage.Id,
                        VendorId = vendorId
                    });
                }
            }

            var isFiltering = !string.IsNullOrEmpty(searchTerms) || !string.IsNullOrEmpty(searchModel.make_) || !string.IsNullOrEmpty(searchModel.type_) || !string.IsNullOrEmpty(searchModel.model_);
            await PrepareCatalogProductsAsync(model, products, isFiltering);

            return model;
        }

        #endregion

        #region Common
        
        /// <summary>
        /// Prepare sorting options
        /// </summary>
        /// <param name="model">Catalog products model</param>
        /// <param name="command">Model to get the catalog products</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task PrepareSortingOptionsAsync(CatalogProductsModel model, CatalogProductsCommand command)
        {
            //set the order by position by default
            model.OrderBy = command.OrderBy;
            command.OrderBy = (int)ProductSortingEnum.Position;

            //ensure that product sorting is enabled
            if (!_catalogSettings.AllowProductSorting)
                return;

            //get active sorting options
            var activeSortingOptionsIds = Enum.GetValues(typeof(ProductSortingEnum)).Cast<int>()
                .Except(_catalogSettings.ProductSortingEnumDisabled).ToList();
            if (!activeSortingOptionsIds.Any())
                return;

            //order sorting options
            var orderedActiveSortingOptions = activeSortingOptionsIds
                .Select(id => new { Id = id, Order = _catalogSettings.ProductSortingEnumDisplayOrder.TryGetValue(id, out var order) ? order : id })
                .OrderBy(option => option.Order).ToList();

            model.AllowProductSorting = true;
            command.OrderBy = model.OrderBy ?? orderedActiveSortingOptions.FirstOrDefault().Id;

            //prepare available model sorting options
            foreach (var option in orderedActiveSortingOptions)
            {
                model.AvailableSortOptions.Add(new SelectListItem
                {
                    Text = await _localizationService.GetLocalizedEnumAsync((ProductSortingEnum)option.Id),
                    Value = option.Id.ToString(),
                    Selected = option.Id == command.OrderBy
                });
            }
        }

        /// <summary>
        /// Prepare view modes
        /// </summary>
        /// <param name="model">Catalog products model</param>
        /// <param name="command">Model to get the catalog products</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task PrepareViewModesAsync(CatalogProductsModel model, CatalogProductsCommand command)
        {
            model.AllowProductViewModeChanging = _catalogSettings.AllowProductViewModeChanging;

            var viewMode = !string.IsNullOrEmpty(command.ViewMode)
                ? command.ViewMode
                : _catalogSettings.DefaultViewMode;
            model.ViewMode = viewMode;
            if (model.AllowProductViewModeChanging)
            {
                //grid
                model.AvailableViewModes.Add(new SelectListItem
                {
                    Text = await _localizationService.GetResourceAsync("Catalog.ViewMode.Grid"),
                    Value = "grid",
                    Selected = viewMode == "grid"
                });
                //list
                model.AvailableViewModes.Add(new SelectListItem
                {
                    Text = await _localizationService.GetResourceAsync("Catalog.ViewMode.List"),
                    Value = "list",
                    Selected = viewMode == "list"
                });
            }
        }

        /// <summary>
        /// Prepare page size options
        /// </summary>
        /// <param name="model">Catalog products model</param>
        /// <param name="command">Model to get the catalog products</param>
        /// <param name="allowCustomersToSelectPageSize">Are customers allowed to select page size?</param>
        /// <param name="pageSizeOptions">Page size options</param>
        /// <param name="fixedPageSize">Fixed page size</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual Task PreparePageSizeOptionsAsync(CatalogProductsModel model, CatalogProductsCommand command,
            bool allowCustomersToSelectPageSize, string pageSizeOptions, int fixedPageSize)
        {
            if (command.PageNumber <= 0)
                command.PageNumber = 1;

            model.AllowCustomersToSelectPageSize = false;
            if (allowCustomersToSelectPageSize && pageSizeOptions != null)
            {
                var pageSizes = pageSizeOptions.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (pageSizes.Any())
                {
                    // get the first page size entry to use as the default (category page load) or if customer enters invalid value via query string
                    if (command.PageSize <= 0 || !pageSizes.Contains(command.PageSize.ToString()))
                    {
                        if (int.TryParse(pageSizes.FirstOrDefault(), out var temp))
                        {
                            if (temp > 0)
                                command.PageSize = temp;
                        }
                    }

                    foreach (var pageSize in pageSizes)
                    {
                        if (!int.TryParse(pageSize, out var temp))
                            continue;

                        if (temp <= 0)
                            continue;

                        model.PageSizeOptions.Add(new SelectListItem
                        {
                            Text = pageSize,
                            Value = pageSize,
                            Selected = pageSize.Equals(command.PageSize.ToString(), StringComparison.InvariantCultureIgnoreCase)
                        });
                    }

                    if (model.PageSizeOptions.Any())
                    {
                        model.PageSizeOptions = model.PageSizeOptions.OrderBy(x => int.Parse(x.Value)).ToList();
                        model.AllowCustomersToSelectPageSize = true;

                        if (command.PageSize <= 0)
                            command.PageSize = int.Parse(model.PageSizeOptions.First().Value);
                    }
                }
            }
            else
            {
                //customer is not allowed to select a page size
                command.PageSize = fixedPageSize;
            }

            //ensure pge size is specified
            if (command.PageSize <= 0)
            {
                command.PageSize = fixedPageSize;
            }

            return Task.CompletedTask;
        }

        #endregion

        #endregion

    }
}