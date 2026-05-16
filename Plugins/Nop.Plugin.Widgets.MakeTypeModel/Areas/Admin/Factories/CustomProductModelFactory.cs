using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Vendors;
using Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Models.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Factories;
using System.ComponentModel;
using System.Text;
using System.Text.Encodings.Web;

namespace Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Factories
{
    /// <summary>
    /// Represent custom product model factory
    /// </summary>
    public partial class CustomProductModelFactory : ICustomProductModelFactory
    {

        #region Fields

        protected readonly CatalogSettings _catalogSettings;
        protected readonly CurrencySettings _currencySettings;
        protected readonly TaxSettings _taxSettings;
        protected readonly VendorSettings _vendorSettings;
        protected readonly MeasureSettings _measureSettings;
        protected readonly IBaseAdminModelFactory _baseAdminModelFactory;
        protected readonly ICategoryService _categoryService;
        protected readonly ICurrencyService _currencyService;
        protected readonly IDateTimeHelper _dateTimeHelper;
        protected readonly IDiscountService _discountService;
        protected readonly IDiscountSupportedModelFactory _discountSupportedModelFactory;
        protected readonly ILanguageService _languageService;
        protected readonly ILocalizationService _localizationService;
        protected readonly ILocalizedModelFactory _localizedModelFactory;
        protected readonly IManufacturerService _manufacturerService;
        protected readonly IMeasureService _measureService;
        protected readonly IOrderService _orderService;
        protected readonly IPictureService _pictureService;
        protected readonly IProductAttributeFormatter _productAttributeFormatter;
        protected readonly IProductAttributeParser _productAttributeParser;
        protected readonly IProductAttributeService _productAttributeService;
        protected readonly IProductService _productService;
        protected readonly IProductTagService _productTagService;
        protected readonly IProductTemplateService _productTemplateService;
        protected readonly ISettingModelFactory _settingModelFactory;
        protected readonly IShipmentService _shipmentService;
        protected readonly ISpecificationAttributeService _specificationAttributeService;
        protected readonly IStoreMappingSupportedModelFactory _storeMappingSupportedModelFactory;
        protected readonly IStoreContext _storeContext;
        protected readonly IStoreService _storeService;
        protected readonly IUrlRecordService _urlRecordService;
        protected readonly IWarehouseService _warehouseService;
        protected readonly IWorkContext _workContext;

        #endregion

        #region Ctor
        public CustomProductModelFactory(
            CatalogSettings catalogSettings,
            CurrencySettings currencySettings,
            TaxSettings taxSettings,
            VendorSettings vendorSettings,
            MeasureSettings measureSettings,
            IBaseAdminModelFactory baseAdminModelFactory,
            ICategoryService categoryService,
            ICurrencyService currencyService,
            IDateTimeHelper dateTimeHelper,
            IDiscountService discountService,
            IDiscountSupportedModelFactory discountSupportedModelFactory,
            ILanguageService languageService,
            ILocalizationService localizationService,
            ILocalizedModelFactory localizedModelFactory,
            IManufacturerService manufacturerService,
            IMeasureService measureService,
            IOrderService orderService,
            IPictureService pictureService,
            IProductAttributeFormatter productAttributeFormatter,
            IProductAttributeParser productAttributeParser,
            IProductAttributeService productAttributeService,
            IProductService productService,
            IProductTagService productTagService,
            IProductTemplateService productTemplateService,
            ISettingModelFactory settingModelFactory,
            IShipmentService shipmentService,
            ISpecificationAttributeService specificationAttributeService,
            IStoreMappingSupportedModelFactory storeMappingSupportedModelFactory,
            IStoreContext storeContext,
            IStoreService storeService,
            IUrlRecordService urlRecordService,
            IWarehouseService warehouseService,
            IWorkContext workContext)
        {
            _catalogSettings = catalogSettings;
            _currencySettings = currencySettings;
            _taxSettings = taxSettings;
            _vendorSettings = vendorSettings;
            _measureSettings = measureSettings;
            _baseAdminModelFactory = baseAdminModelFactory;
            _categoryService = categoryService;
            _currencyService = currencyService;
            _dateTimeHelper = dateTimeHelper;
            _discountService = discountService;
            _discountSupportedModelFactory = discountSupportedModelFactory;
            _languageService = languageService;
            _localizationService = localizationService;
            _localizedModelFactory = localizedModelFactory;
            _manufacturerService = manufacturerService;
            _measureService = measureService;
            _orderService = orderService;
            _pictureService = pictureService;
            _productAttributeFormatter = productAttributeFormatter;
            _productAttributeParser = productAttributeParser;
            _productAttributeService = productAttributeService;
            _productService = productService;
            _productTagService = productTagService;
            _productTemplateService = productTemplateService;
            _settingModelFactory = settingModelFactory;
            _shipmentService = shipmentService;
            _specificationAttributeService = specificationAttributeService;
            _storeMappingSupportedModelFactory = storeMappingSupportedModelFactory;
            _storeContext = storeContext;
            _storeService = storeService;
            _urlRecordService = urlRecordService;
            _warehouseService = warehouseService;
            _workContext = workContext;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Prepare copy product model
        /// </summary>
        /// <param name="model">Copy product model</param>
        /// <param name="product">Product</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the copy product model
        /// </returns>
        protected virtual async Task<CopyProductModel> PrepareCopyProductModelAsync(CopyProductModel model, Product product)
        {
            ArgumentNullException.ThrowIfNull(model);

            model.Id = product.Id;
            model.Name = string.Format(await _localizationService.GetResourceAsync("Admin.Catalog.Products.Copy.Name.New"), product.Name);
            model.Published = true;
            model.CopyMultimedia = true;

            return model;
        }

        /// <summary>
        /// Prepare product warehouse inventory models
        /// </summary>
        /// <param name="models">List of product warehouse inventory models</param>
        /// <param name="product">Product</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task PrepareProductWarehouseInventoryModelsAsync(IList<ProductWarehouseInventoryModel> models, Product product)
        {
            ArgumentNullException.ThrowIfNull(models);

            foreach (var warehouse in await _warehouseService.GetAllWarehousesAsync())
            {
                var model = new ProductWarehouseInventoryModel
                {
                    WarehouseId = warehouse.Id,
                    WarehouseName = warehouse.Name
                };

                if (product != null)
                {
                    var productWarehouseInventory = (await _productService.GetAllProductWarehouseInventoryRecordsAsync(product.Id))?.FirstOrDefault(inventory => inventory.WarehouseId == warehouse.Id);
                    if (productWarehouseInventory != null)
                    {
                        model.WarehouseUsed = true;
                        model.StockQuantity = productWarehouseInventory.StockQuantity;
                        model.ReservedQuantity = productWarehouseInventory.ReservedQuantity;
                        model.PlannedQuantity = await _shipmentService.GetQuantityInShipmentsAsync(product, productWarehouseInventory.WarehouseId, true, true);
                    }
                }

                models.Add(model);
            }
        }

        /// <summary>
        /// Prepare related product search model
        /// </summary>
        /// <param name="searchModel">Related product search model</param>
        /// <param name="product">Product</param>
        /// <returns>Related product search model</returns>
        protected virtual RelatedProductSearchModel PrepareRelatedProductSearchModel(RelatedProductSearchModel searchModel, Product product)
        {
            ArgumentNullException.ThrowIfNull(searchModel);
            ArgumentNullException.ThrowIfNull(product);

            searchModel.ProductId = product.Id;

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        /// <summary>
        /// Prepare cross-sell product search model
        /// </summary>
        /// <param name="searchModel">Cross-sell product search model</param>
        /// <param name="product">Product</param>
        /// <returns>Cross-sell product search model</returns>
        protected virtual CrossSellProductSearchModel PrepareCrossSellProductSearchModel(CrossSellProductSearchModel searchModel, Product product)
        {
            ArgumentNullException.ThrowIfNull(searchModel);
            ArgumentNullException.ThrowIfNull(product);

            searchModel.ProductId = product.Id;

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        /// <summary>
        /// Prepare filter level values search model
        /// </summary>
        /// <param name="searchModel">Filter level value search model</param>
        /// <param name="product">Product</param>
        /// <returns>Filter level value search model</returns>
        protected virtual FilterLevelValueSearchModel PrepareFilterLevelValuesSearchModel(FilterLevelValueSearchModel searchModel, Product product)
        {
            ArgumentNullException.ThrowIfNull(searchModel);
            ArgumentNullException.ThrowIfNull(product);

            searchModel.ProductId = product.Id;

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        /// <summary>
        /// Prepare associated product search model
        /// </summary>
        /// <param name="searchModel">Associated product search model</param>
        /// <param name="product">Product</param>
        /// <returns>Associated product search model</returns>
        protected virtual AssociatedProductSearchModel PrepareAssociatedProductSearchModel(AssociatedProductSearchModel searchModel, Product product)
        {
            ArgumentNullException.ThrowIfNull(searchModel);
            ArgumentNullException.ThrowIfNull(product);

            searchModel.ProductId = product.Id;

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        /// <summary>
        /// Prepare product picture search model
        /// </summary>
        /// <param name="searchModel">Product picture search model</param>
        /// <param name="product">Product</param>
        /// <returns>Product picture search model</returns>
        protected virtual ProductPictureSearchModel PrepareProductPictureSearchModel(ProductPictureSearchModel searchModel, Product product)
        {
            ArgumentNullException.ThrowIfNull(searchModel);
            ArgumentNullException.ThrowIfNull(product);

            searchModel.ProductId = product.Id;

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        /// <summary>
        /// Prepare product video search model
        /// </summary>
        /// <param name="searchModel">Product video search model</param>
        /// <param name="product">Product</param>
        /// <returns>Product video search model</returns>
        protected virtual ProductVideoSearchModel PrepareProductVideoSearchModel(ProductVideoSearchModel searchModel, Product product)
        {
            ArgumentNullException.ThrowIfNull(searchModel);
            ArgumentNullException.ThrowIfNull(product);

            searchModel.ProductId = product.Id;

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        /// <summary>
        /// Prepare product order search model
        /// </summary>
        /// <param name="searchModel">Product order search model</param>
        /// <param name="product">Product</param>
        /// <returns>Product order search model</returns>
        protected virtual ProductOrderSearchModel PrepareProductOrderSearchModel(ProductOrderSearchModel searchModel, Product product)
        {
            ArgumentNullException.ThrowIfNull(searchModel);
            ArgumentNullException.ThrowIfNull(product);

            searchModel.ProductId = product.Id;

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        /// <summary>
        /// Prepare tier price search model
        /// </summary>
        /// <param name="searchModel">Tier price search model</param>
        /// <param name="product">Product</param>
        /// <returns>Tier price search model</returns>
        protected virtual TierPriceSearchModel PrepareTierPriceSearchModel(TierPriceSearchModel searchModel, Product product)
        {
            ArgumentNullException.ThrowIfNull(searchModel);
            ArgumentNullException.ThrowIfNull(product);

            searchModel.ProductId = product.Id;

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        /// <summary>
        /// Prepare stock quantity history search model
        /// </summary>
        /// <param name="searchModel">Stock quantity history search model</param>
        /// <param name="product">Product</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the stock quantity history search model
        /// </returns>
        protected virtual async Task<StockQuantityHistorySearchModel> PrepareStockQuantityHistorySearchModelAsync(StockQuantityHistorySearchModel searchModel, Product product)
        {
            ArgumentNullException.ThrowIfNull(searchModel);
            ArgumentNullException.ThrowIfNull(product);

            searchModel.ProductId = product.Id;

            //prepare available warehouses
            await _baseAdminModelFactory.PrepareWarehousesAsync(searchModel.AvailableWarehouses);

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        /// <summary>
        /// Prepare product attribute mapping search model
        /// </summary>
        /// <param name="searchModel">Product attribute mapping search model</param>
        /// <param name="product">Product</param>
        /// <returns>Product attribute mapping search model</returns>
        protected virtual ProductAttributeMappingSearchModel PrepareProductAttributeMappingSearchModel(ProductAttributeMappingSearchModel searchModel,
            Product product)
        {
            ArgumentNullException.ThrowIfNull(searchModel);
            ArgumentNullException.ThrowIfNull(product);

            searchModel.ProductId = product.Id;

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        /// <summary>
        /// Prepare product attribute combination search model
        /// </summary>
        /// <param name="searchModel">Product attribute combination search model</param>
        /// <param name="product">Product</param>
        /// <returns>Product attribute combination search model</returns>
        protected virtual ProductAttributeCombinationSearchModel PrepareProductAttributeCombinationSearchModel(
            ProductAttributeCombinationSearchModel searchModel, Product product)
        {
            ArgumentNullException.ThrowIfNull(searchModel);
            ArgumentNullException.ThrowIfNull(product);

            searchModel.ProductId = product.Id;

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        /// <summary>
        /// Prepare product specification attribute search model
        /// </summary>
        /// <param name="searchModel">Product specification attribute search model</param>
        /// <param name="product">Product</param>
        /// <returns>Product specification attribute search model</returns>
        protected virtual ProductSpecificationAttributeSearchModel PrepareProductSpecificationAttributeSearchModel(
            ProductSpecificationAttributeSearchModel searchModel, Product product)
        {
            ArgumentNullException.ThrowIfNull(searchModel);
            ArgumentNullException.ThrowIfNull(product);

            searchModel.ProductId = product.Id;

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Prepare product model
        /// </summary>
        /// <param name="model">Product model</param>
        /// <param name="product">Product</param>
        /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product model
        /// </returns>
        public async Task<OverrideProductModel> PrepareCustomProductModelAsync(OverrideProductModel model, Product product, bool excludeProperties = false)
        {
            Func<ProductLocalizedModel, int, Task> localizedModelConfiguration = null;

            if (product != null)
            {
                //fill in model values from the entity
                if (model == null)
                {
                    model = new OverrideProductModel();
                    model = product.ToModel(model);
                    model.SeName = await _urlRecordService.GetSeNameAsync(product, 0, true, false);
                }

                var parentGroupedProduct = await _productService.GetProductByIdAsync(product.ParentGroupedProductId);
                if (parentGroupedProduct != null)
                {
                    model.AssociatedToProductId = product.ParentGroupedProductId;
                    model.AssociatedToProductName = parentGroupedProduct.Name;
                }

                model.LastStockQuantity = product.StockQuantity;
                if (model is OverrideProductModel overrideModel)
                {
                    overrideModel.ProductTags = string.Join(", ",
                        (await _productTagService.GetAllProductTagsByProductIdAsync(product.Id))
                            .Select(tag => tag.Name));
                }

                model.ProductAttributesExist = (await _productAttributeService.GetAllProductAttributesAsync()).Any();

                model.CanCreateCombinations = await (await _productAttributeService
                    .GetProductAttributeMappingsByProductIdAsync(product.Id)).AnyAwaitAsync(async pam => (await _productAttributeService.GetProductAttributeValuesAsync(pam.Id)).Any());

                if (!excludeProperties)
                {
                    model.SelectedCategoryIds = (await _categoryService.GetProductCategoriesByProductIdAsync(product.Id, true))
                        .Select(productCategory => productCategory.CategoryId).ToList();
                    model.SelectedManufacturerIds = (await _manufacturerService.GetProductManufacturersByProductIdAsync(product.Id, true))
                        .Select(productManufacturer => productManufacturer.ManufacturerId).ToList();
                }

                //prepare copy product model
                await PrepareCopyProductModelAsync(model.CopyProductModel, product);

                //prepare nested search model
                PrepareRelatedProductSearchModel(model.RelatedProductSearchModel, product);
                PrepareCrossSellProductSearchModel(model.CrossSellProductSearchModel, product);
                PrepareFilterLevelValuesSearchModel(model.FilterLevelValueSearchModel, product);
                PrepareAssociatedProductSearchModel(model.AssociatedProductSearchModel, product);
                PrepareProductPictureSearchModel(model.ProductPictureSearchModel, product);
                PrepareProductVideoSearchModel(model.ProductVideoSearchModel, product);
                PrepareProductSpecificationAttributeSearchModel(model.ProductSpecificationAttributeSearchModel, product);
                PrepareProductOrderSearchModel(model.ProductOrderSearchModel, product);
                PrepareTierPriceSearchModel(model.TierPriceSearchModel, product);
                await PrepareStockQuantityHistorySearchModelAsync(model.StockQuantityHistorySearchModel, product);
                PrepareProductAttributeMappingSearchModel(model.ProductAttributeMappingSearchModel, product);
                PrepareProductAttributeCombinationSearchModel(model.ProductAttributeCombinationSearchModel, product);

                //define localized model configuration action
                localizedModelConfiguration = async (locale, languageId) =>
                {
                    locale.Name = await _localizationService.GetLocalizedAsync(product, entity => entity.Name, languageId, false, false);
                    locale.FullDescription = await _localizationService.GetLocalizedAsync(product, entity => entity.FullDescription, languageId, false, false);
                    locale.ShortDescription = await _localizationService.GetLocalizedAsync(product, entity => entity.ShortDescription, languageId, false, false);
                    locale.MetaKeywords = await _localizationService.GetLocalizedAsync(product, entity => entity.MetaKeywords, languageId, false, false);
                    locale.MetaDescription = await _localizationService.GetLocalizedAsync(product, entity => entity.MetaDescription, languageId, false, false);
                    locale.MetaTitle = await _localizationService.GetLocalizedAsync(product, entity => entity.MetaTitle, languageId, false, false);
                    locale.SeName = await _urlRecordService.GetSeNameAsync(product, languageId, false, false);
                };
            }

            //set default values for the new model
            if (product == null)
            {
                model.MaximumCustomerEnteredPrice = 1000;
                model.MaxNumberOfDownloads = 10;
                model.RecurringCycleLength = 100;
                model.RecurringTotalCycles = 10;
                model.RentalPriceLength = 1;
                model.StockQuantity = 10000;
                model.NotifyAdminForQuantityBelow = 1;
                model.OrderMinimumQuantity = 1;
                model.OrderMaximumQuantity = 10000;
                model.TaxCategoryId = _taxSettings.DefaultTaxCategoryId;
                model.UnlimitedDownloads = true;
                model.IsShipEnabled = true;
                model.AllowCustomerReviews = true;
                model.Published = true;
                model.VisibleIndividually = true;
            }

            model.PrimaryStoreCurrencyCode = (await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId)).CurrencyCode;
            model.BaseWeightIn = (await _measureService.GetMeasureWeightByIdAsync(_measureSettings.BaseWeightId)).Name;
            model.BaseDimensionIn = (await _measureService.GetMeasureDimensionByIdAsync(_measureSettings.BaseDimensionId)).Name;
            model.HasAvailableSpecificationAttributes =
                (await _specificationAttributeService.GetSpecificationAttributesWithOptionsAsync()).Any();

            var currentVendor = await _workContext.GetCurrentVendorAsync();
            model.IsLoggedInAsVendor = currentVendor != null;

            //prepare localized models
            if (!excludeProperties)
                model.Locales = await _localizedModelFactory.PrepareLocalizedModelsAsync(localizedModelConfiguration);

            //prepare editor settings
            model.ProductEditorSettingsModel = await _settingModelFactory.PrepareProductEditorSettingsModelAsync();

            //prepare available product templates
            await _baseAdminModelFactory.PrepareProductTemplatesAsync(model.AvailableProductTemplates, false);

            //prepare available product types
            var productTemplates = await _productTemplateService.GetAllProductTemplatesAsync();
            foreach (var productType in Enum.GetValues(typeof(ProductType)).OfType<ProductType>())
            {
                model.ProductsTypesSupportedByProductTemplates.Add((int)productType, new List<SelectListItem>());
                foreach (var template in productTemplates)
                {
                    var list = (IList<int>)TypeDescriptor.GetConverter(typeof(List<int>)).ConvertFrom(template.IgnoredProductTypes) ?? new List<int>();
                    if (string.IsNullOrEmpty(template.IgnoredProductTypes) || !list.Contains((int)productType))
                    {
                        model.ProductsTypesSupportedByProductTemplates[(int)productType].Add(new SelectListItem
                        {
                            Text = template.Name,
                            Value = template.Id.ToString()
                        });
                    }
                }
            }

            //prepare available delivery dates
            await _baseAdminModelFactory.PrepareDeliveryDatesAsync(model.AvailableDeliveryDates,
                defaultItemText: await _localizationService.GetResourceAsync("Admin.Catalog.Products.Fields.DeliveryDate.None"));

            //prepare available product availability ranges
            await _baseAdminModelFactory.PrepareProductAvailabilityRangesAsync(model.AvailableProductAvailabilityRanges,
                defaultItemText: await _localizationService.GetResourceAsync("Admin.Catalog.Products.Fields.ProductAvailabilityRange.None"));

            //prepare available vendors
            await _baseAdminModelFactory.PrepareVendorsAsync(model.AvailableVendors,
                defaultItemText: await _localizationService.GetResourceAsync("Admin.Catalog.Products.Fields.Vendor.None"));

            //prepare available tax categories
            await _baseAdminModelFactory.PrepareTaxCategoriesAsync(model.AvailableTaxCategories);

            //prepare available warehouses
            await _baseAdminModelFactory.PrepareWarehousesAsync(model.AvailableWarehouses,
                defaultItemText: await _localizationService.GetResourceAsync("Admin.Catalog.Products.Fields.Warehouse.None"));
            await PrepareProductWarehouseInventoryModelsAsync(model.ProductWarehouseInventoryModels, product);

            //prepare available base price units
            var availableMeasureWeights = (await _measureService.GetAllMeasureWeightsAsync())
                .Select(weight => new SelectListItem { Text = weight.Name, Value = weight.Id.ToString() }).ToList();
            model.AvailableBasepriceUnits = availableMeasureWeights;
            model.AvailableBasepriceBaseUnits = availableMeasureWeights;

            //prepare model categories
            await _baseAdminModelFactory.PrepareCategoriesAsync(model.AvailableCategories, false);
            foreach (var categoryItem in model.AvailableCategories)
            {
                categoryItem.Selected = int.TryParse(categoryItem.Value, out var categoryId)
                                        && model.SelectedCategoryIds.Contains(categoryId);
            }

            //prepare model manufacturers
            await _baseAdminModelFactory.PrepareManufacturersAsync(model.AvailableManufacturers, false);
            foreach (var manufacturerItem in model.AvailableManufacturers)
            {
                manufacturerItem.Selected = int.TryParse(manufacturerItem.Value, out var manufacturerId)
                                            && model.SelectedManufacturerIds.Contains(manufacturerId);
            }

            //prepare model discounts
            var availableDiscounts = await _discountService.GetAllDiscountsAsync(
                discountType: DiscountType.AssignedToSkus,
                showHidden: true, isActive: null,
                vendorId: currentVendor?.Id ?? 0);

            await _discountSupportedModelFactory.PrepareModelDiscountsAsync(model, product, availableDiscounts, excludeProperties);

            //prepare model stores
            await _storeMappingSupportedModelFactory.PrepareModelStoresAsync(model, product, excludeProperties);

            await _baseAdminModelFactory.PreparePreTranslationSupportModelAsync(model);

            var productTags = await _productTagService.GetAllProductTagsAsync();
            var productTagsSb = new StringBuilder();
            productTagsSb.Append("var initialProductTags = [");
            for (var i = 0; i < productTags.Count; i++)
            {
                var tag = productTags[i];
                productTagsSb.Append('\'');
                productTagsSb.Append(JavaScriptEncoder.Default.Encode(tag.Name));
                productTagsSb.Append('\'');
                if (i != productTags.Count - 1)
                    productTagsSb.Append(',');
            }
            productTagsSb.Append(']');

            if (model is OverrideProductModel overridesModel)
            {
                overridesModel.InitialProductTags = productTagsSb.ToString();
            }
            return model;
        }

        #endregion
    }
}
