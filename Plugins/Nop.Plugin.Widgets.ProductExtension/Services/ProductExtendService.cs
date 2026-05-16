using ClosedXML.Excel;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Localization;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Plugin.Widgets.ProductExtension.Domain;
using Nop.Services.Catalog;
using Nop.Services.Cms;
using Nop.Services.Common;
using Nop.Services.Common.Pdf;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.ExportImport.Help;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Stores;
using PdfRpt.Core.Contracts;

namespace Nop.Plugin.Widgets.ProductExtension.Services
{
    /// <summary>
    /// Product extend service
    /// </summary>
    public partial class ProductExtendService : IProductExtendService
    {
        #region Fields

        protected readonly IAclService _aclService;
        protected readonly IRepository<Category> _categoryRepository;
        protected readonly IRepository<Manufacturer> _manufacturerRepository;
        protected readonly IRepository<Product> _productRepository;
        protected readonly IRepository<ProductAttributeCombination> _productAttributeCombinationRepository;
        protected readonly IRepository<ProductNote> _productNoteRepository;
        protected readonly IRepository<LocalizedProperty> _localizedPropertyRepository;
        protected readonly IRepository<ProductAttributeValueCondition> _pavConditionRepository;
        protected readonly IRepository<ProductCategory> _productCategoryRepository;
        protected readonly IRepository<ProductManufacturer> _productManufacturerRepository;
        protected readonly IRepository<ProductProductTagMapping> _productTagMappingRepository;
        protected readonly IRepository<ProductSpecificationAttribute> _productSpecificationAttributeRepository;
        protected readonly IRepository<ProductTag> _productTagRepository;
        protected readonly IRepository<ProductWarehouseInventory> _productWarehouseInventoryRepository;
        protected readonly IRepository<ProductAttributeMapping> _productAttributeMappingRepository;
        protected readonly IRepository<ProductAttributeValuePicture> _productAttributeValuePictureRepository;
        protected readonly IRepository<ProductAttributeValue> _productAttributeValueRepository;
        protected readonly IRepository<ProductPicture> _productPictureRepository;
        protected readonly IStaticCacheManager _staticCacheManager;
        protected readonly IStoreContext _storeContext;
        protected readonly IWidgetPluginManager _widgetPluginManager;
        protected readonly IWorkContext _workContext;
        protected readonly IProductService _productService;
        protected readonly CatalogSettings _catalogSettings;
        protected readonly ILanguageService _languageService;
        protected readonly IStoreMappingService _storeMappingService;
        protected readonly ISearchPluginManager _searchPluginManager;
        protected readonly ISettingService _settingService;
        protected readonly IPictureService _pictureService;
        protected readonly ILocalizationService _localizationService;
        protected readonly INopFileProvider _fileProvider;
        protected readonly IDateTimeHelper _dateTimeHelper;
        protected readonly ICurrencyService _currencyService;
        protected readonly CurrencySettings _currencySettings;
        protected readonly IPriceFormatter _priceFormatter;
        protected readonly IBackInStockSubscriptionService _backInStockSubscriptionService;
        protected readonly IGenericAttributeService _genericAttributeService;
        protected readonly ProductEditorSettings _productEditorSettings;

        #endregion

        #region Ctor

        public ProductExtendService(IAclService aclService,
            IRepository<Category> categoryRepository,
            IRepository<Manufacturer> manufacturerRepository,
            IRepository<Product> productRepository,
            IRepository<ProductAttributeCombination> productAttributeCombinationRepository,
            IRepository<ProductNote> productNoteRepository,
            IRepository<LocalizedProperty> localizedPropertyRepository,
            IRepository<ProductAttributeValueCondition> pavConditionRepository,
            IRepository<ProductCategory> productCategoryRepository,
            IRepository<ProductManufacturer> productManufacturerRepository,
            IRepository<ProductProductTagMapping> productTagMappingRepository,
            IRepository<ProductSpecificationAttribute> productSpecificationAttributeRepository,
            IRepository<ProductTag> productTagRepository,
            IRepository<ProductWarehouseInventory> productWarehouseInventoryRepository,
            IRepository<ProductAttributeMapping> productAttributeMappingRepository,
            IRepository<ProductAttributeValue> productAttributeValueRepository,
            IRepository<ProductPicture> productPictureRepository,
            IStaticCacheManager staticCacheManager,
            IStoreContext storeContext,
            IWidgetPluginManager widgetPluginManager,
            IWorkContext workContext,
            IProductService productService,
            CatalogSettings catalogSettings,
            ILanguageService languageService,
            IStoreMappingService storeMappingService,
            ISearchPluginManager searchPluginManager,
            ISettingService settingService,
            IPictureService pictureService,
            ILocalizationService localizationService,
            INopFileProvider fileProvider,
            IDateTimeHelper dateTimeHelper,
            ICurrencyService currencyService,
            CurrencySettings currencySettings,
            IPriceFormatter priceFormatter,
            IBackInStockSubscriptionService backInStockSubscriptionService,
            IRepository<ProductAttributeValuePicture> productAttributeValuePictureRepository,
            IGenericAttributeService genericAttributeService,
            ProductEditorSettings productEditorSettings)
        {
            _aclService = aclService;
            _categoryRepository = categoryRepository;
            _manufacturerRepository = manufacturerRepository;
            _productRepository = productRepository;
            _productAttributeCombinationRepository = productAttributeCombinationRepository;
            _productNoteRepository = productNoteRepository;
            _localizedPropertyRepository = localizedPropertyRepository;
            _pavConditionRepository = pavConditionRepository;
            _productCategoryRepository = productCategoryRepository;
            _productManufacturerRepository = productManufacturerRepository;
            _productTagMappingRepository = productTagMappingRepository;
            _productSpecificationAttributeRepository = productSpecificationAttributeRepository;
            _productTagRepository = productTagRepository;
            _productWarehouseInventoryRepository = productWarehouseInventoryRepository;
            _productAttributeMappingRepository = productAttributeMappingRepository;
            _productAttributeValueRepository = productAttributeValueRepository;
            _productPictureRepository = productPictureRepository;
            _staticCacheManager = staticCacheManager;
            _storeContext = storeContext;
            _widgetPluginManager = widgetPluginManager;
            _workContext = workContext;
            _productService = productService;
            _catalogSettings = catalogSettings;
            _languageService = languageService;
            _storeMappingService = storeMappingService;
            _searchPluginManager = searchPluginManager;
            _settingService = settingService;
            _pictureService = pictureService;
            _localizationService = localizationService;
            _fileProvider = fileProvider;
            _dateTimeHelper = dateTimeHelper;
            _currencyService = currencyService;
            _currencySettings = currencySettings;
            _priceFormatter = priceFormatter;
            _backInStockSubscriptionService = backInStockSubscriptionService;
            _productAttributeValuePictureRepository = productAttributeValuePictureRepository;
            _genericAttributeService = genericAttributeService;
            _productEditorSettings = productEditorSettings;
        }

        /// <summary>
        /// Gets a key for caching
        /// </summary>
        /// <remarks>
        /// {0} : product attribute mapping ID
        /// </remarks>
        public static CacheKey ProductAttributeValuesByProductCacheKey => new("Nop.productattributevalue.byproduct.{0}");

        #endregion

        #region Utilities
        protected virtual async Task<bool> IgnoreExportProductPropertyAsync(Func<ProductEditorSettings, bool> func)
        {
            var productAdvancedMode = true;
            try
            {
                productAdvancedMode = await _genericAttributeService.GetAttributeAsync<bool>(await _workContext.GetCurrentCustomerAsync(), "product-advanced-mode");
            }
            catch (ArgumentNullException)
            {
            }

            return !productAdvancedMode && !func(_productEditorSettings);
        }

        #endregion

        #region Methods

        #region Common

        /// <summary>
        /// Check whether the plugin is active for the current customer and the current store
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public async Task<bool> PluginActiveAsync()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();
            return await _widgetPluginManager.IsPluginActiveAsync(ProductExtensionDefaults.SystemName, customer, store?.Id ?? 0);
        }

        #endregion

        #region Product Notes

        /// <summary>
        /// Deletes a Product note
        /// </summary>
        /// <param name="productNote">Product note</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task DeleteProductNoteAsync(ProductNote productNote)
        {
            await _productNoteRepository.DeleteAsync(productNote);
        }

        /// <summary>
        /// Gets all Product notes
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the ax rates
        /// </returns>
        public virtual async Task<IPagedList<ProductNote>> GetAllProductNotesAsync(string widgetZone = null, bool showHidden = false, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var rez = await _productNoteRepository.GetAllAsync(query =>
            {
                if (!string.IsNullOrEmpty(widgetZone))
                    query = query.Where(p => p.WidgetZone.Equals(widgetZone));

                if (!showHidden)
                    query = query.Where(p => p.Published);

                return query.OrderBy(p => p.DisplayOrder).ThenBy(p => p.Id);
            });

            var records = new PagedList<ProductNote>(rez, pageIndex, pageSize);

            return records;
        }

        /// <summary>
        /// Gets all Product note widget zones
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the widget zones
        /// </returns>
        public virtual async Task<IList<string>> GetAllProductNoteWidgetsAsync()
        {
            var getAllProductNoteWidgest = (from pn in _productNoteRepository.Table
                         select pn.WidgetZone).Distinct();

            return await getAllProductNoteWidgest.ToListAsync();
        }

        /// <summary>
        /// Gets a Product note
        /// </summary>
        /// <param name="productNoteId">Product note identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the ax rate
        /// </returns>
        public virtual async Task<ProductNote> GetProductNoteByIdAsync(int productNoteId)
        {
            return await _productNoteRepository.GetByIdAsync(productNoteId);
        }

        /// <summary>
        /// Inserts a Product note
        /// </summary>
        /// <param name="productNote">Product note</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task InsertProductNoteAsync(ProductNote productNote)
        {
            await _productNoteRepository.InsertAsync(productNote);
        }

        /// <summary>
        /// Updates the Product note
        /// </summary>
        /// <param name="productNote">Product note</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task UpdateProductNoteAsync(ProductNote productNote)
        {
            await _productNoteRepository.UpdateAsync(productNote);
        }

        #endregion

        #region Import Price & Stock

        /// <summary>
        /// Import product price from XLSX file
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task ImportProductPriceFromXlsxAsync(Stream stream)
        {
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheets.FirstOrDefault();

            if (worksheet == null)
                throw new NopException("No worksheet found");

            var properties = new[]
            {
                new PropertyByName<Product>("SKU", (p, _) => p.Sku),
                new PropertyByName<Product>("Price", (p, _) => p.Price),
                new PropertyByName<Product>("ProductCost", (p, _) => p.ProductCost, await IgnoreExportProductPropertyAsync(p => p.ProductCost)),
                new PropertyByName<Product>("Weight", (p, _) => p.Weight, await IgnoreExportProductPropertyAsync(p => p.Weight)),
            };

            var manager = new PropertyManager<Product>(properties, _catalogSettings);

            var iRow = 2;
            while (true)
            {
                var allColumnsAreEmpty = manager.GetDefaultProperties
                    .Select(p => worksheet.Row(iRow).Cell(p.PropertyOrderPosition))
                    .All(cell => string.IsNullOrEmpty(cell?.GetString()));

                if (allColumnsAreEmpty)
                    break;

                manager.ReadDefaultFromXlsx(worksheet, iRow);

                var productSku = manager.GetDefaultProperty("SKU").StringValue?.Trim();
                var product = await _productService.GetProductBySkuAsync(productSku);

                if (product != null)
                {
                    if (manager.GetDefaultProperty("Price").DecimalValue != 0)
                        product.Price = manager.GetDefaultProperty("Price").DecimalValue;

                    if (manager.GetDefaultProperty("ProductCost").DecimalValue != 0)
                        product.ProductCost = manager.GetDefaultProperty("ProductCost").DecimalValue;

                    if (manager.GetDefaultProperty("Weight").DecimalValue != 0)
                        product.Weight = manager.GetDefaultProperty("Weight").DecimalValue;

                    await _productService.UpdateProductAsync(product);
                }

                iRow++;
            }
        }


        /// <summary>
        /// Import product stock from XLSX file
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task ImportProductStockFromXlsxAsync(Stream stream)
        {
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheets.FirstOrDefault();

            if (worksheet == null)
                throw new NopException("No worksheet found");

            var properties = new[]
            {
                new PropertyByName<Product>("SKU", (p, _) => p.Sku),
                new PropertyByName<Product>("StockQuantity", (p, _) => p.StockQuantity),
            };

            var manager = new PropertyManager<Product>(properties, _catalogSettings);

            var iRow = 2;
            while (true)
            {
                var allColumnsAreEmpty = manager.GetDefaultProperties
                    .Select(p => worksheet.Row(iRow).Cell(p.PropertyOrderPosition))
                    .All(cell => string.IsNullOrEmpty(cell?.GetString()));

                if (allColumnsAreEmpty)
                    break;

                manager.ReadDefaultFromXlsx(worksheet, iRow);

                var productSku = manager.GetDefaultProperty("SKU").StringValue?.Trim();
                var product = await _productService.GetProductBySkuAsync(productSku);

                if (product != null)
                {
                    var newStockQuantity = manager.GetDefaultProperty("StockQuantity").IntValue;
                    if (newStockQuantity >= 0)
                    {
                        var prevTotalStock = await _productService.GetTotalStockQuantityAsync(product);
                        var prevStock = product.StockQuantity;

                        product.StockQuantity = newStockQuantity;
                        await _productService.UpdateProductAsync(product);

                        if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStock &&
                            product.BackorderMode == BackorderMode.NoBackorders &&
                            product.AllowBackInStockSubscriptions &&
                            await _productService.GetTotalStockQuantityAsync(product) > 0 &&
                            prevTotalStock <= 0 &&
                            product.Published &&
                            !product.Deleted)
                        {
                            await _backInStockSubscriptionService.SendNotificationsToSubscribersAsync(product);
                        }

                        await _productService.AddStockQuantityHistoryEntryAsync(
                            product,
                            product.StockQuantity - prevStock,
                            product.StockQuantity,
                            product.WarehouseId,
                            await _localizationService.GetResourceAsync(
                                "Admin.StockQuantityHistory.Messages.Edit"));
                    }
                }

                iRow++;
            }
        }


        #endregion

        #region Product Attribute Value

        /// <summary>
        /// Gets product attribute values by product identifier
        /// </summary>
        /// <param name="productId">The product identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product attribute value picture id collection who has picture
        /// </returns>
        public virtual async Task<IList<int>> GetProductAttributeValuesByProductIdAsync(int productId)
        {
            var key = _staticCacheManager.PrepareKeyForDefaultCache(
                ProductAttributeValuesByProductCacheKey, productId);

            var query =
                from pav in _productAttributeValueRepository.Table
                join pam in _productAttributeMappingRepository.Table
                    on pav.ProductAttributeMappingId equals pam.Id
                join pavp in _productAttributeValuePictureRepository.Table
                    on pav.Id equals pavp.ProductAttributeValueId
                join pp in _productPictureRepository.Table
                    on pavp.PictureId equals pp.PictureId
                where pam.ProductId == productId
                      && pp.DisplayOrder == 8888
                orderby pav.DisplayOrder, pav.Id
                select pavp.PictureId;

            return await _staticCacheManager.GetAsync(
                key, async () => await query.ToListAsync());
        }


        #endregion

        #region Product Attribute Value Condition

        /// <summary>
        /// Deletes a Product attribute value condition
        /// </summary>
        /// <param name="pavCondition">Product attribute value condition</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task DeleteProductAttributeValueConditionAsync(ProductAttributeValueCondition pavCondition)
        {
            await _pavConditionRepository.DeleteAsync(pavCondition);
        }

        /// <summary>
        /// Gets all Product attribute value conditions
        /// </summary>
        /// <param name="paValueId">Product attribute value identifier</param>
        /// <param name="showCrossValue">A value indicating whether to show cross reference attribute value condition</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product attribute value identifier
        /// </returns>
        public virtual async Task<IList<ProductAttributeValueCondition>> GetAllProductAttributeValueConditionByValueIdAsync(int paValueId, bool showCrossValue = false)
        {
            if (paValueId == 0)
                return new List<ProductAttributeValueCondition>();

            return await _pavConditionRepository.GetAllAsync(query =>
            {
                if (!showCrossValue)
                    query = query.Where(p => p.ProductAttributeValueId1 == paValueId);
                else
                    query = query.Where(p => p.ProductAttributeValueId1 == paValueId || p.ProductAttributeValueId2 == paValueId);

                return query;
            });
        }

        /// <summary>
        /// Inserts a Product note
        /// </summary>
        /// <param name="pavCondition">Product attribute value condition</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task InsertProductAttributeValueConditionAsync(ProductAttributeValueCondition pavCondition)
        {
            await _pavConditionRepository.InsertAsync(pavCondition);
        }

        /// <summary>
        /// A value indicating whether this product attribute should has condition on value
        /// </summary>
        /// <param name="paValueId">Product attribute value identifier</param>
        /// <returns>Result</returns>
        public virtual async Task<bool> ShouldHasConditionOnAttributeValueAsync(int paValueId)
        {
            if (paValueId == 0)
                return false;

            return await _pavConditionRepository.Table.AnyAsync(query => query.ProductAttributeValueId2 == paValueId);
        }

        #endregion

        #region Product

        /// <summary>
        /// get product average report
        /// </summary>
        /// <param name="categoryIds">Category identifiers</param>
        /// <param name="manufacturerIds">Manufacturer identifiers</param>
        /// <param name="storeId">Store identifier; 0 to load all records</param>
        /// <param name="vendorId">Vendor identifier; 0 to load all records</param>
        /// <param name="warehouseId">Warehouse identifier; 0 to load all records</param>
        /// <param name="productType">Product type; 0 to load all records</param>
        /// <param name="visibleIndividuallyOnly">A values indicating whether to load only products marked as "visible individually"; "false" to load all records; "true" to load "visible individually" only</param>
        /// <param name="excludeFeaturedProducts">A value indicating whether loaded products are marked as featured (relates only to categories and manufacturers); "false" (by default) to load all records; "true" to exclude featured products from results</param>
        /// <param name="priceMin">Minimum price; null to load all records</param>
        /// <param name="priceMax">Maximum price; null to load all records</param>
        /// <param name="productTagId">Product tag identifier; 0 to load all records</param>
        /// <param name="keywords">Keywords</param>
        /// <param name="searchDescriptions">A value indicating whether to search by a specified "keyword" in product descriptions</param>
        /// <param name="searchManufacturerPartNumber">A value indicating whether to search by a specified "keyword" in manufacturer part number</param>
        /// <param name="searchSku">A value indicating whether to search by a specified "keyword" in product SKU</param>
        /// <param name="searchProductTags">A value indicating whether to search by a specified "keyword" in product tags</param>
        /// <param name="languageId">Language identifier (search for text searching)</param>
        /// <param name="filteredSpecOptions">Specification options list to filter products; null to load all records</param>
        /// <param name="orderBy">Order by</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <param name="overridePublished">
        /// null - process "Published" property according to "showHidden" parameter
        /// true - load only "Published" products
        /// false - load only "Unpublished" products
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the results
        /// </returns>
        public virtual async Task<(decimal TotalPrice, decimal TotalProductCost)> GetProductAverageReportLine(
            IList<int> categoryIds = null,
            IList<int> manufacturerIds = null,
            int storeId = 0,
            int vendorId = 0,
            int warehouseId = 0,
            ProductType? productType = null,
            bool visibleIndividuallyOnly = false,
            bool excludeFeaturedProducts = false,
            decimal? priceMin = null,
            decimal? priceMax = null,
            int productTagId = 0,
            string keywords = null,
            bool searchDescriptions = false,
            bool searchManufacturerPartNumber = true,
            bool searchSku = true,
            bool searchProductTags = false,
            int languageId = 0,
            IList<SpecificationAttributeOption> filteredSpecOptions = null,
            ProductSortingEnum orderBy = ProductSortingEnum.Position,
            bool showHidden = false,
            bool? overridePublished = null)
        {
            if (productType == null)
                productType = ProductType.SimpleProduct;

            var productsQuery = _productRepository.Table;

            if (!showHidden)
                productsQuery = productsQuery.Where(p => p.Published);
            else if (overridePublished.HasValue)
                productsQuery = productsQuery.Where(p => p.Published == overridePublished.Value);

            if (!showHidden || storeId > 0)
            {
                //apply store mapping constraints
                productsQuery = await _storeMappingService.ApplyStoreMapping(productsQuery, storeId);
            }

            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!showHidden)
            {
                //apply ACL constraints
                productsQuery = await _aclService.ApplyAcl(productsQuery, customer);
            }

            productsQuery =
                from p in productsQuery
                where !p.Deleted && p.ManageInventoryMethodId == (int)ManageInventoryMethod.ManageStock &&
                    (!visibleIndividuallyOnly || p.VisibleIndividually) &&
                    (vendorId == 0 || p.VendorId == vendorId) &&
                    (
                        warehouseId == 0 ||
                        (
                            !p.UseMultipleWarehouses ? p.WarehouseId == warehouseId :
                                _productWarehouseInventoryRepository.Table.Any(pwi => pwi.WarehouseId == warehouseId && pwi.ProductId == p.Id)
                        )
                    ) &&
                    (productType == null || p.ProductTypeId == (int)productType) &&
                    (showHidden ||
                            DateTime.UtcNow >= (p.AvailableStartDateTimeUtc ?? DateTime.MinValue) &&
                            DateTime.UtcNow <= (p.AvailableEndDateTimeUtc ?? DateTime.MaxValue)
                    ) &&
                    (priceMin == null || p.Price >= priceMin) &&
                    (priceMax == null || p.Price <= priceMax)
                select p;

            if (!string.IsNullOrEmpty(keywords))
            {
                var langs = await _languageService.GetAllLanguagesAsync(showHidden: true);

                //Set a flag which will to points need to search in localized properties. If showHidden doesn't set to true should be at least two published languages.
                var searchLocalizedValue = languageId > 0 && langs.Count >= 2 && (showHidden || langs.Count(l => l.Published) >= 2);
                IQueryable<int> productsByKeywords;

                var activeSearchProvider = await _searchPluginManager.LoadPrimaryPluginAsync(customer, storeId);

                if (activeSearchProvider is not null)
                {
                    productsByKeywords = (await activeSearchProvider.SearchProductsAsync(keywords, searchLocalizedValue)).AsQueryable();
                }
                else
                {
                    productsByKeywords =
                        from p in _productRepository.Table
                        where p.Name.Contains(keywords) ||
                            (searchDescriptions &&
                                (p.ShortDescription.Contains(keywords) || p.FullDescription.Contains(keywords))) ||
                            (searchManufacturerPartNumber && p.ManufacturerPartNumber == keywords) ||
                            (searchSku && p.Sku == keywords)
                        select p.Id;

                    if (searchLocalizedValue)
                    {
                        productsByKeywords = productsByKeywords.Union(
                            from lp in _localizedPropertyRepository.Table
                            let checkName = lp.LocaleKey == nameof(Product.Name) &&
                                            lp.LocaleValue.Contains(keywords)
                            let checkShortDesc = searchDescriptions &&
                                            lp.LocaleKey == nameof(Product.ShortDescription) &&
                                            lp.LocaleValue.Contains(keywords)
                            where
                                lp.LocaleKeyGroup == nameof(Product) && lp.LanguageId == languageId && (checkName || checkShortDesc)

                            select lp.EntityId);
                    }
                }

                //search by SKU for ProductAttributeCombination
                if (searchSku)
                {
                    productsByKeywords = productsByKeywords.Union(
                        from pac in _productAttributeCombinationRepository.Table
                        where pac.Sku == keywords
                        select pac.ProductId);
                }

                //search by category name if admin allows
                if (_catalogSettings.AllowCustomersToSearchWithCategoryName)
                {
                    var categoryQuery = _categoryRepository.Table;

                    if (!showHidden)
                        categoryQuery = categoryQuery.Where(p => p.Published);
                    else if (overridePublished.HasValue)
                        categoryQuery = categoryQuery.Where(p => p.Published == overridePublished.Value);

                    if (!showHidden || storeId > 0)
                        categoryQuery = await _storeMappingService.ApplyStoreMapping(categoryQuery, storeId);

                    if (!showHidden)
                        categoryQuery = await _aclService.ApplyAcl(categoryQuery, customer);

                    productsByKeywords = productsByKeywords.Union(
                        from pc in _productCategoryRepository.Table
                        join c in categoryQuery on pc.CategoryId equals c.Id
                        where c.Name.Contains(keywords) && !c.Deleted
                        select pc.ProductId
                    );

                    if (searchLocalizedValue)
                    {
                        productsByKeywords = productsByKeywords.Union(
                        from pc in _productCategoryRepository.Table
                        join lp in _localizedPropertyRepository.Table on pc.CategoryId equals lp.EntityId
                        where lp.LocaleKeyGroup == nameof(Category) &&
                              lp.LocaleKey == nameof(Category.Name) &&
                              lp.LocaleValue.Contains(keywords) &&
                              lp.LanguageId == languageId
                        select pc.ProductId);
                    }
                }

                //search by manufacturer name if admin allows
                if (_catalogSettings.AllowCustomersToSearchWithManufacturerName)
                {
                    var manufacturerQuery = _manufacturerRepository.Table;

                    if (!showHidden)
                        manufacturerQuery = manufacturerQuery.Where(p => p.Published);
                    else if (overridePublished.HasValue)
                        manufacturerQuery = manufacturerQuery.Where(p => p.Published == overridePublished.Value);

                    if (!showHidden || storeId > 0)
                        manufacturerQuery = await _storeMappingService.ApplyStoreMapping(manufacturerQuery, storeId);

                    if (!showHidden)
                        manufacturerQuery = await _aclService.ApplyAcl(manufacturerQuery, customer);

                    productsByKeywords = productsByKeywords.Union(
                        from pm in _productManufacturerRepository.Table
                        join m in manufacturerQuery on pm.ManufacturerId equals m.Id
                        where m.Name.Contains(keywords) && !m.Deleted
                        select pm.ProductId
                    );

                    if (searchLocalizedValue)
                    {
                        productsByKeywords = productsByKeywords.Union(
                        from pm in _productManufacturerRepository.Table
                        join lp in _localizedPropertyRepository.Table on pm.ManufacturerId equals lp.EntityId
                        where lp.LocaleKeyGroup == nameof(Manufacturer) &&
                              lp.LocaleKey == nameof(Manufacturer.Name) &&
                              lp.LocaleValue.Contains(keywords) &&
                              lp.LanguageId == languageId
                        select pm.ProductId);
                    }
                }

                if (searchProductTags)
                {
                    productsByKeywords = productsByKeywords.Union(
                        from pptm in _productTagMappingRepository.Table
                        join pt in _productTagRepository.Table on pptm.ProductTagId equals pt.Id
                        where pt.Name.Contains(keywords)
                        select pptm.ProductId
                    );

                    if (searchLocalizedValue)
                    {
                        productsByKeywords = productsByKeywords.Union(
                        from pptm in _productTagMappingRepository.Table
                        join lp in _localizedPropertyRepository.Table on pptm.ProductTagId equals lp.EntityId
                        where lp.LocaleKeyGroup == nameof(ProductTag) &&
                              lp.LocaleKey == nameof(ProductTag.Name) &&
                              lp.LocaleValue.Contains(keywords) &&
                              lp.LanguageId == languageId
                        select pptm.ProductId);
                    }
                }

                productsQuery =
                    from p in productsQuery
                    join pbk in productsByKeywords on p.Id equals pbk
                    select p;
            }

            if (categoryIds is not null)
            {
                if (categoryIds.Contains(0))
                    categoryIds.Remove(0);

                if (categoryIds.Any())
                {
                    var productCategoryQuery =
                        from pc in _productCategoryRepository.Table
                        where (!excludeFeaturedProducts || !pc.IsFeaturedProduct) &&
                            categoryIds.Contains(pc.CategoryId)
                        group pc by pc.ProductId into pc
                        select new
                        {
                            ProductId = pc.Key,
                            DisplayOrder = pc.First().DisplayOrder
                        };

                    productsQuery =
                        from p in productsQuery
                        join pc in productCategoryQuery on p.Id equals pc.ProductId
                        orderby pc.DisplayOrder, p.Name
                        select p;
                }
            }

            if (manufacturerIds is not null)
            {
                if (manufacturerIds.Contains(0))
                    manufacturerIds.Remove(0);

                if (manufacturerIds.Any())
                {
                    var productManufacturerQuery =
                        from pm in _productManufacturerRepository.Table
                        where (!excludeFeaturedProducts || !pm.IsFeaturedProduct) &&
                            manufacturerIds.Contains(pm.ManufacturerId)
                        group pm by pm.ProductId into pm
                        select new
                        {
                            ProductId = pm.Key,
                            DisplayOrder = pm.First().DisplayOrder
                        };

                    productsQuery =
                        from p in productsQuery
                        join pm in productManufacturerQuery on p.Id equals pm.ProductId
                        orderby pm.DisplayOrder, p.Name
                        select p;
                }
            }

            if (productTagId > 0)
            {
                productsQuery =
                    from p in productsQuery
                    join ptm in _productTagMappingRepository.Table on p.Id equals ptm.ProductId
                    where ptm.ProductTagId == productTagId
                    select p;
            }

            if (filteredSpecOptions?.Count > 0)
            {
                var specificationAttributeIds = filteredSpecOptions
                    .Select(sao => sao.SpecificationAttributeId)
                    .Distinct();

                foreach (var specificationAttributeId in specificationAttributeIds)
                {
                    var optionIdsBySpecificationAttribute = filteredSpecOptions
                        .Where(o => o.SpecificationAttributeId == specificationAttributeId)
                        .Select(o => o.Id);

                    var productSpecificationQuery =
                        from psa in _productSpecificationAttributeRepository.Table
                        where psa.AllowFiltering && optionIdsBySpecificationAttribute.Contains(psa.SpecificationAttributeOptionId)
                        select psa;

                    productsQuery =
                        from p in productsQuery
                        where productSpecificationQuery.Any(pc => pc.ProductId == p.Id)
                        select p;
                }
            }

            var item = await (from pq in productsQuery
                group pq by 1
                into result
                select new
                {
                    TotalPrice = result.Sum(o => o.Price * o.StockQuantity),
                    TotalProductCost = result.Sum(o => o.ProductCost * o.StockQuantity)
                }).FirstOrDefaultAsync();


            item ??= new
            {
                TotalPrice = decimal.Zero,
                TotalProductCost = decimal.Zero,
            };

            return (item.TotalPrice, item.TotalProductCost);
        }


        /// <summary>
        /// Write PDF catalog stock to the specified stream
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="products">Products</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task PrintProductsToPdfAsync(Stream stream, IList<Product> products)
        {
            ArgumentNullException.ThrowIfNull(stream);

            ArgumentNullException.ThrowIfNull(products);

            var currentStore = await _storeContext.GetCurrentStoreAsync();
            var pdfSettingsByStore = await _settingService.LoadSettingAsync<PdfSettings>(currentStore.Id);
            var lang = await _workContext.GetWorkingLanguageAsync();

            byte[] logo = null;
            var logoPicture = await _pictureService.GetPictureByIdAsync(pdfSettingsByStore.LogoPictureId);
            if (logoPicture != null)
            {
                var pictureBinary = await _pictureService.LoadPictureBinaryAsync(logoPicture);
                logo = logoPicture.MimeType == MimeTypes.ImageSvg
                    ? await _pictureService.ConvertSvgToPngAsync(new MemoryStream(pictureBinary))
                    : pictureBinary;
            }

            var date = await _dateTimeHelper.ConvertToUserTimeAsync(DateTime.UtcNow, DateTimeKind.Utc);

            var productItems = new List<CatalogProductItem>();
            decimal totalPrice = 0, totalCost = 0;

            foreach (var product in products)
            {
                if (product.ProductType != ProductType.SimpleProduct ||
                    product.ManageInventoryMethod != ManageInventoryMethod.ManageStock)
                    continue;

                var productName = await _localizationService.GetLocalizedAsync(product, x => x.Name, lang.Id);
                var priceStr = await _priceFormatter.FormatPriceAsync(product.Price, true, false);
                if (product.IsRental)
                    priceStr = await _priceFormatter.FormatRentalProductPeriodAsync(product, priceStr);

                var costStr = await _priceFormatter.FormatPriceAsync(product.ProductCost, true, false);
                var stockQuantity = await _productService.GetTotalStockQuantityAsync(product);

                var totalProductPrice = product.Price * stockQuantity;
                var totalProductCost = product.ProductCost * stockQuantity;

                totalPrice += totalProductPrice;
                totalCost += totalProductCost;

                var item = new CatalogProductItem
                {
                    Name = productName,
                    Sku = product.Sku,
                    ProductCost = costStr,
                    Price = priceStr,
                    Stock = stockQuantity.ToString(),
                    TotalProductCost = await _priceFormatter.FormatPriceAsync(totalProductCost, true, false),
                    Total = await _priceFormatter.FormatPriceAsync(totalProductPrice, true, false)
                };

                var pictures = await _pictureService.GetPicturesByProductIdAsync(product.Id);
                var firstPicture = pictures?.FirstOrDefault();
                if (firstPicture != null)
                {
                    item.PicturePath = await _pictureService.LoadPictureBinaryAsync(firstPicture);
                }
                productItems.Add(item);
            }

            var source = new CatalogStockDocument
            {
                StoreUrl = currentStore.Url?.Trim('/'),
                Language = lang,
                PageSize = pdfSettingsByStore.LetterPageSizeEnabled ? PdfPageSize.Letter : PdfPageSize.A4,
                Font = PdfDocumentHelper.GetFont(
                    lang.Rtl ? pdfSettingsByStore.RtlFontName : pdfSettingsByStore.LtrFontName,
                    pdfSettingsByStore.BaseFontSize > 0 ? pdfSettingsByStore.BaseFontSize : 10),
                ImageTargetSize = 80,
                GetResourceAsync = (key, id) => _localizationService.GetResourceAsync(key, id),

                CreatedOnDateUser = date,
                LogoData = logo,
                Products = productItems,
                Totals = new CatalogStockTotal
                {
                    TotalPrice = await _priceFormatter.FormatPriceAsync(totalPrice, true, false),
                    TotalProductCost = await _priceFormatter.FormatPriceAsync(totalCost, true, false)
                }
            };

            await using var pdfStream = new MemoryStream();
            source.Generate(pdfStream);
            pdfStream.Position = 0;
            await pdfStream.CopyToAsync(stream);
        }

        #endregion

        #endregion
    }
}