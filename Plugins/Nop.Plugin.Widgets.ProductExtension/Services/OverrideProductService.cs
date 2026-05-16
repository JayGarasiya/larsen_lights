using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Shipping.Date;
using Nop.Services.Stores;
using Nop.Services.Vendors;

namespace Nop.Plugin.Widgets.ProductExtension.Services
{
    /// <summary>
    /// Product service
    /// </summary>
    public class OverrideProductService : ProductService
    {
        #region Fields

        protected readonly IHttpContextAccessor _httpContextAccessor;

        #endregion

        #region Ctor
        public OverrideProductService(CatalogSettings catalogSettings, 
            IAclService aclService, 
            ICustomerService customerService, 
            IDateRangeService dateRangeService, 
            ILanguageService languageService, 
            ILocalizationService localizationService, 
            IProductAttributeParser productAttributeParser, 
            IProductAttributeService productAttributeService, 
            IRepository<Category> categoryRepository, 
            IRepository<CrossSellProduct> crossSellProductRepository, 
            IRepository<DiscountProductMapping> discountProductMappingRepository, 
            IRepository<LocalizedProperty> localizedPropertyRepository, 
            IRepository<Manufacturer> manufacturerRepository, 
            IRepository<Product> productRepository, 
            IRepository<ProductAttributeCombination> productAttributeCombinationRepository, 
            IRepository<ProductAttributeMapping> productAttributeMappingRepository, 
            IRepository<ProductCategory> productCategoryRepository, 
            IRepository<ProductManufacturer> productManufacturerRepository, 
            IRepository<ProductPicture> productPictureRepository, 
            IRepository<ProductProductTagMapping> productTagMappingRepository, 
            IRepository<ProductSpecificationAttribute> productSpecificationAttributeRepository, 
            IRepository<ProductTag> productTagRepository, 
            IRepository<ProductVideo> productVideoRepository,
            IRepository<ProductWarehouseInventory> productWarehouseInventoryRepository, 
            IRepository<RelatedProduct> relatedProductRepository, 
            IRepository<Shipment> shipmentRepository, 
            IRepository<StockQuantityHistory> stockQuantityHistoryRepository, 
            IRepository<TierPrice> tierPriceRepository, 
            ISearchPluginManager searchPluginManager, 
            IStaticCacheManager staticCacheManager, 
            IVendorService vendorService, 
            IStoreMappingService storeMappingService, 
            IWorkContext workContext, 
            LocalizationSettings localizationSettings,
            IHttpContextAccessor httpContextAccessor) : base(
                catalogSettings, 
                aclService, 
                customerService, 
                dateRangeService, 
                languageService, 
                localizationService, 
                productAttributeParser, 
                productAttributeService, 
                categoryRepository, 
                crossSellProductRepository, 
                discountProductMappingRepository, 
                localizedPropertyRepository, 
                manufacturerRepository, 
                productRepository, 
                productAttributeCombinationRepository, 
                productAttributeMappingRepository, 
                productCategoryRepository, 
                productManufacturerRepository, 
                productPictureRepository, 
                productTagMappingRepository, 
                productSpecificationAttributeRepository, 
                productTagRepository, 
                productVideoRepository, 
                productWarehouseInventoryRepository, 
                relatedProductRepository, 
                shipmentRepository, 
                stockQuantityHistoryRepository, 
                tierPriceRepository, 
                searchPluginManager, 
                staticCacheManager, 
                vendorService, 
                storeMappingService, 
                workContext, 
                localizationSettings)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Get stock message for a product with attributes
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the message
        /// </returns>
        protected override async Task<string> GetStockMessageForAttributesAsync(Product product, string attributesXml)
        {
            if (!product.DisplayStockAvailability)
                return string.Empty;

            string stockMessage;
            var shoppingCartService = EngineContext.Current.Resolve<IShoppingCartService>();
            var currentCustomer = await _workContext.GetCurrentCustomerAsync();
            var combination = await _productAttributeParser.FindProductAttributeCombinationAsync(product, attributesXml);
            if (combination != null)
            {
                //combination exists
                var stockQuantity = combination.StockQuantity;
                if (stockQuantity > 0)
                {
                    var warnings = await shoppingCartService.GetShoppingCartItemAttributeWarningsAsync(currentCustomer,
                        ShoppingCartType.ShoppingCart, product,
                        quantity: product.OrderMinimumQuantity,
                        attributesXml: attributesXml,
                        ignoreNonCombinableAttributes: true);
                    if (!warnings.Any())
                        stockMessage = product.DisplayStockQuantity
                            ?
                            //display "in stock" with stock quantity
                            string.Format(await _localizationService.GetResourceAsync("Products.Availability.InStockWithQuantity"), stockQuantity)
                            :
                            //display "in stock" without stock quantity
                            await _localizationService.GetResourceAsync("Products.Availability.InStock");
                    else
                        stockMessage = await _localizationService.GetResourceAsync("Products.Availability.OutOfStock");
                }
                else if (combination.AllowOutOfStockOrders)
                {
                    stockMessage = await _localizationService.GetResourceAsync("Products.Availability.InStock");
                }
                else
                {
                    var productAvailabilityRange = await
                        _dateRangeService.GetProductAvailabilityRangeByIdAsync(product.ProductAvailabilityRangeId);
                    stockMessage = productAvailabilityRange == null
                        ? await _localizationService.GetResourceAsync("Products.Availability.OutOfStock")
                        : string.Format(await _localizationService.GetResourceAsync("Products.Availability.AvailabilityRange"),
                            await _localizationService.GetLocalizedAsync(productAvailabilityRange, range => range.Name));
                }
            }
            else
            {
                //no combination configured
                if (product.AllowAddingOnlyExistingAttributeCombinations)
                {
                    var productAvailabilityRange = await
                        _dateRangeService.GetProductAvailabilityRangeByIdAsync(product.ProductAvailabilityRangeId);
                    stockMessage = productAvailabilityRange == null
                        ? await _localizationService.GetResourceAsync("Products.Availability.OutOfStock")
                        : string.Format(await _localizationService.GetResourceAsync("Products.Availability.AvailabilityRange"),
                            await _localizationService.GetLocalizedAsync(productAvailabilityRange, range => range.Name));
                }
                else
                {
                    var warnings = await shoppingCartService.GetShoppingCartItemAttributeWarningsAsync(currentCustomer,
                        ShoppingCartType.ShoppingCart, product,
                        quantity: product.OrderMinimumQuantity,
                        attributesXml: attributesXml,
                        ignoreNonCombinableAttributes: true);
                    if (!warnings.Any())
                        stockMessage = await _localizationService.GetResourceAsync("Products.Availability.InStock");
                    else
                        stockMessage = await _localizationService.GetResourceAsync("Products.Availability.OutOfStock");
                }
            }

            return stockMessage;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get number of product (published and visible) in certain category
        /// </summary>
        /// <param name="categoryIds">Category identifiers</param>
        /// <param name="storeId">Store identifier; 0 to load all records</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the number of products
        /// </returns>
        public override async Task<int> GetNumberOfProductsInCategoryAsync(IList<int> categoryIds = null, int storeId = 0)
        {
            //validate "categoryIds" parameter
            if (categoryIds != null && categoryIds.Contains(0))
                categoryIds.Remove(0);

            var query = _productRepository.Table.Where(p => p.Published && !p.Deleted && p.VisibleIndividually);

            //apply store mapping constraints
            query = await _storeMappingService.ApplyStoreMapping(query, storeId);

            //apply ACL constraints
            var customer = await _workContext.GetCurrentCustomerAsync();
            var customerRoleIds = await _customerService.GetCustomerRoleIdsAsync(customer);
            query = await _aclService.ApplyAcl(query, customerRoleIds);

            //category filtering
            if (categoryIds != null && categoryIds.Any())
            {
                query = (from p in query
                         join pc in _productCategoryRepository.Table on p.Id equals pc.ProductId
                         where categoryIds.Contains(pc.CategoryId)
                         select p).Distinct();
            }

            var cacheKey = _staticCacheManager
                .PrepareKeyForDefaultCache(NopCatalogDefaults.CategoryProductsNumberCacheKey, customerRoleIds, storeId, categoryIds);

            //only distinct products
            return await _staticCacheManager.GetAsync(cacheKey, () => query.Select(p => p.Id).Count());
        }

       
        #endregion
    }
}
