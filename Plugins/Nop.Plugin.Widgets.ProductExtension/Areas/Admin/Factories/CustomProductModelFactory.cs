using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Vendors;
using Nop.Plugin.Widgets.ProductExtension.Areas.Admin.Models;
using Nop.Plugin.Widgets.ProductExtension.Services;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Seo;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Common;
using Nop.Web.Framework.Extensions;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Plugin.Widgets.ProductExtension.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the Custom product model factory implementation
    /// </summary>
    public class CustomProductModelFactory : ICustomProductModelFactory
    {
        #region Fields

        protected readonly IWorkContext _workContext;
        protected readonly VendorSettings _vendorSettings;
        protected readonly ILocalizationService _localizationService;
        protected readonly IUrlRecordService _urlRecordService;
        protected readonly IBaseAdminModelFactory _baseAdminModelFactory;
        protected readonly CatalogSettings _catalogSettings;
        protected readonly IPictureService _pictureService;
        protected readonly IProductService _productService;
        protected readonly ICategoryService _categoryService;
        protected readonly ISettingService _settingService;
        protected readonly IDateTimeHelper _dateTimeHelper;
        protected readonly NopHttpClient _nopHttpClient;
        protected readonly IPriceFormatter _priceFormatter;
        protected readonly ICustomProductService _customProductService;

        #endregion

        #region Ctor
        public CustomProductModelFactory(IWorkContext workContext,
            VendorSettings vendorSettings,
            ILocalizationService localizationService,
            IUrlRecordService urlRecordService,
            IBaseAdminModelFactory baseAdminModelFactory,
            CatalogSettings catalogSettings,
            IPictureService pictureService,
            IProductService productService,
            ICategoryService categoryService,
            IDateTimeHelper dateTimeHelper,
            ISettingService settingService,
            NopHttpClient nopHttpClient,
            IPriceFormatter priceFormatter,
            ICustomProductService customProductService)
        {
            _workContext = workContext;
            _vendorSettings = vendorSettings;
            _localizationService = localizationService;
            _urlRecordService = urlRecordService;
            _baseAdminModelFactory = baseAdminModelFactory;
            _catalogSettings = catalogSettings;
            _pictureService = pictureService;
            _productService = productService;
            _categoryService = categoryService;
            _dateTimeHelper = dateTimeHelper;
            _settingService = settingService;
            _nopHttpClient = nopHttpClient;
            _priceFormatter = priceFormatter;
            _customProductService = customProductService;
        }
        #endregion

        #region Method

        /// <summary>
        /// Prepare product search model
        /// </summary>
        /// <param name="searchModel">Product search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product search model
        /// </returns>
        public virtual async Task<ProductSearchModel> PrepareProductSearchModelAsync(ProductSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //a vendor should have access only to his products
            searchModel.IsLoggedInAsVendor = await _workContext.GetCurrentVendorAsync() != null;
            searchModel.AllowVendorsToImportProducts = _vendorSettings.AllowVendorsToImportProducts;

            var licenseCheckModel = new LicenseCheckModel();
            try
            {
                var result = await _nopHttpClient.GetLicenseCheckDetailsAsync();
                if (!string.IsNullOrEmpty(result))
                {
                    licenseCheckModel = JsonConvert.DeserializeObject<LicenseCheckModel>(result);
                    if (licenseCheckModel.DisplayWarning == false && licenseCheckModel.BlockPages == false)
                        await _settingService.SetSettingAsync($"{nameof(AdminAreaSettings)}.{nameof(AdminAreaSettings.CheckLicense)}", false);
                }
            }
            catch { }
            searchModel.LicenseCheckModel = licenseCheckModel;

            //prepare available categories
            await _baseAdminModelFactory.PrepareCategoriesAsync(searchModel.AvailableCategories);

            //prepare available manufacturers
            await _baseAdminModelFactory.PrepareManufacturersAsync(searchModel.AvailableManufacturers);

            //prepare available stores
            await _baseAdminModelFactory.PrepareStoresAsync(searchModel.AvailableStores);

            //prepare available vendors
            await _baseAdminModelFactory.PrepareVendorsAsync(searchModel.AvailableVendors);

            //prepare available product types
            await _baseAdminModelFactory.PrepareProductTypesAsync(searchModel.AvailableProductTypes);

            //prepare available warehouses
            await _baseAdminModelFactory.PrepareWarehousesAsync(searchModel.AvailableWarehouses);

            searchModel.HideStoresList = _catalogSettings.IgnoreStoreLimitations || searchModel.AvailableStores.SelectionIsNotPossible();

            //prepare "published" filter (0 - all; 1 - published only; 2 - unpublished only)
            searchModel.AvailablePublishedOptions.Add(new SelectListItem
            {
                Value = "0",
                Text = await _localizationService.GetResourceAsync("Admin.Catalog.Products.List.SearchPublished.All")
            });
            searchModel.AvailablePublishedOptions.Add(new SelectListItem
            {
                Value = "1",
                Text = await _localizationService.GetResourceAsync("Admin.Catalog.Products.List.SearchPublished.PublishedOnly")
            });
            searchModel.AvailablePublishedOptions.Add(new SelectListItem
            {
                Value = "2",
                Text = await _localizationService.GetResourceAsync("Admin.Catalog.Products.List.SearchPublished.UnpublishedOnly")
            });

            //prepare grid
            searchModel.SetGridPageSize();

            return searchModel;
        }

        /// <summary>
        /// Prepare paged product list model
        /// </summary>
        /// <param name="searchModel">Product search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product list model
        /// </returns>
        public virtual async Task<Nop.Web.Areas.Admin.Models.Catalog.ProductListModel> PrepareProductListModelAsync(ProductSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get parameters to filter comments
            var overridePublished = searchModel.SearchPublishedId == 0 ? null : (bool?)(searchModel.SearchPublishedId == 1);
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null)
                searchModel.SearchVendorId = currentVendor.Id;
            var categoryIds = new List<int> { searchModel.SearchCategoryId };
            if (searchModel.SearchIncludeSubCategories && searchModel.SearchCategoryId > 0)
            {
                var childCategoryIds = await _categoryService.GetChildCategoryIdsAsync(parentCategoryId: searchModel.SearchCategoryId, showHidden: true);
                categoryIds.AddRange(childCategoryIds);
            }

            //get products
            var products = await _customProductService.SearchProductsAsync (showHidden: true,
                categoryIds: categoryIds,
                manufacturerIds: new List<int> { searchModel.SearchManufacturerId },
                storeId: searchModel.SearchStoreId,
                vendorId: searchModel.SearchVendorId,
                warehouseId: searchModel.SearchWarehouseId,
                productType: searchModel.SearchProductTypeId > 0 ? (ProductType?)searchModel.SearchProductTypeId : null,
                keywords: searchModel.SearchProductName,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize,
                overridePublished: overridePublished,
                noPicture: searchModel.SearchNoPicture,
                fromWeight: searchModel.SearchFromWeight,
                toWeight: searchModel.SearchToWeight);

            //prepare list model
            var model = await new Nop.Web.Areas.Admin.Models.Catalog.ProductListModel().PrepareToGridAsync(searchModel, products, () =>
            {
                return products.SelectAwait(async product =>
                {
                    //fill in model values from the entity
                    var productModel = product.ToModel<Nop.Web.Areas.Admin.Models.Catalog.ProductModel>();

                    //little performance optimization: ensure that "FullDescription" is not returned
                    productModel.FullDescription = string.Empty;

                    //fill in additional values (not existing in the entity)
                    productModel.SeName = await _urlRecordService.GetSeNameAsync(product, 0, true, false);
                    var defaultProductPicture = (await _pictureService.GetPicturesByProductIdAsync(product.Id, 1)).FirstOrDefault();
                    (productModel.PictureThumbnailUrl, _) = await _pictureService.GetPictureUrlAsync(defaultProductPicture, 75);
                    productModel.ProductTypeName = await _localizationService.GetLocalizedEnumAsync(product.ProductType);
                    productModel.CustomProperties.Add("PriceStr", await _priceFormatter.FormatPriceAsync(product.Price, true, false));
                    productModel.CustomProperties.Add("ProductCostStr", await _priceFormatter.FormatPriceAsync(product.ProductCost, true, false));
                    if (product.ProductType == ProductType.SimpleProduct && product.ManageInventoryMethod == ManageInventoryMethod.ManageStock)
                    {
                        var stockQuantity = await _productService.GetTotalStockQuantityAsync(product);
                        productModel.StockQuantityStr = stockQuantity.ToString();
                        if (!string.IsNullOrEmpty(productModel.StockQuantityStr))
                        {
                            productModel.CustomProperties.Add("TotalPrice", await _priceFormatter.FormatPriceAsync(product.Price * stockQuantity, true, false));
                            productModel.CustomProperties.Add("TotalProductCost", await _priceFormatter.FormatPriceAsync(product.ProductCost * stockQuantity, true, false));
                        }
                    }

                    return productModel;
                });
            });

            return model;
        }

        #endregion
    }
}
