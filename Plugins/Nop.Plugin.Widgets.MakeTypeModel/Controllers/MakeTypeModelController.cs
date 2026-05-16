using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Cms;
using Nop.Core.Domain.Vendors;
using Nop.Core.Infrastructure;
using Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Models;
using Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Models.AddModelProductToProduct;
using Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Models.GranitProduct;
using Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Models.MakeProduct;
using Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Models.ModelCategory;
using Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Models.ModelProduct;
using Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Models.ModelProductMapping;
using Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Models.PriceImports;
using Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Models.ProductImport;
using Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Models.TypeProduct;
using Nop.Plugin.Widgets.MakeTypeModel.Domain.GranitImport;
using Nop.Plugin.Widgets.MakeTypeModel.Domain.MakeTypeModelProduct;
using Nop.Plugin.Widgets.MakeTypeModel.Domain.PriceImport;
using Nop.Plugin.Widgets.MakeTypeModel.Domain.ProductImport;
using Nop.Plugin.Widgets.MakeTypeModel.Factories;
using Nop.Plugin.Widgets.MakeTypeModel.Services.GranitProductImport;
using Nop.Plugin.Widgets.MakeTypeModel.Services.MakeTypeModel;
using Nop.Plugin.Widgets.MakeTypeModel.Services.PriceImport;
using Nop.Plugin.Widgets.MakeTypeModel.Services.ProductImport;
using Nop.Services;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Services.Vendors;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Validators;

namespace Nop.Plugin.Widgets.MakeTypeModel.Controllers
{
    public class MakeTypeModelController : BaseAdminController
    {
        #region Fields

        protected readonly ILocalizationService _localizationService;
        protected readonly INotificationService _notificationService;
        protected readonly IPermissionService _permissionService;
        protected readonly ISettingService _settingService;
        protected readonly IStoreContext _storeContext;
        protected readonly IMakeTypeModelService _makeTypeModelService;
        protected readonly ICategoryService _categoryService;
        protected readonly IProductService _productService;
        protected readonly IBaseAdminModelFactory _baseAdminModelFactory;
        protected readonly IUrlRecordService _urlRecordService;
        protected readonly IProductImportService _productImportFormExcelService;
        protected readonly IManufacturerService _manufacturerService;
        protected readonly IVendorService _vendorService;
        protected readonly IShippingService _shippingService;
        protected readonly ILogger _logger;
        protected readonly INopFileProvider _nopFileProvider;
        protected readonly IDateTimeHelper _dateTimeHelper;
        protected readonly IWorkContext _workContext;
        protected readonly VendorSettings _vendorSettings;
        protected readonly MakeTypeModelSettings _makeTypeModelSettings;
        protected readonly IGranitProductImportService _granitProductImportService;
        protected readonly IPriceImportService _priceImportService;
        protected readonly ICurrencyService _currencyService;
        protected readonly IPriceFormatter _priceFormatter;
        protected readonly IStaticCacheManager _staticCacheManager;
        protected readonly INopFileProvider _fileProvider;
        protected readonly IDownloadService _downloadService;
        protected readonly IMakeTypeModelFactory _makeTypeModelFactory;

        #endregion

        #region Ctor

        public MakeTypeModelController(ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            ISettingService settingService,
            IStoreContext storeContext,
            IMakeTypeModelService makeTypeModelService,
            ICategoryService categoryService,
            IProductService productService,
            IBaseAdminModelFactory baseAdminModelFactory,
            IUrlRecordService urlRecordService,
            IProductImportService productImportFormExcelService,
            IManufacturerService manufacturerService,
            IVendorService vendorService,
            IShippingService shippingService,
            ILogger logger,
            INopFileProvider nopFileProvider,
            IDateTimeHelper dateTimeHelper,
            IWorkContext workContext,
            VendorSettings vendorSettings,
            IGranitProductImportService granitProductImportService,
            IPriceImportService priceImportService,
            ICurrencyService currencyService,
            IPriceFormatter priceFormatter,
            IStaticCacheManager staticCacheManager,
            MakeTypeModelSettings makeTypeModelSettings,
            INopFileProvider fileProvider,
            IDownloadService downloadService,
            IMakeTypeModelFactory makeTypeModelFactory)
        {
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _settingService = settingService;
            _storeContext = storeContext;
            _makeTypeModelService = makeTypeModelService;
            _categoryService = categoryService;
            _productService = productService;
            _baseAdminModelFactory = baseAdminModelFactory;
            _urlRecordService = urlRecordService;
            _productImportFormExcelService = productImportFormExcelService;
            _manufacturerService = manufacturerService;
            _vendorService = vendorService;
            _shippingService = shippingService;
            _logger = logger;
            _nopFileProvider = nopFileProvider;
            _dateTimeHelper = dateTimeHelper;
            _workContext = workContext;
            _vendorSettings = vendorSettings;
            _granitProductImportService = granitProductImportService;
            _priceImportService = priceImportService;
            _currencyService = currencyService;
            _priceFormatter = priceFormatter;
            _staticCacheManager = staticCacheManager;
            _makeTypeModelSettings = makeTypeModelSettings;
            _fileProvider = fileProvider;
            _downloadService = downloadService;
            _staticCacheManager = staticCacheManager;
            _makeTypeModelFactory = makeTypeModelFactory;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Prepare default item
        /// </summary>
        /// <param name="items">Available items</param>
        /// <param name="withSpecialDefaultItem">Whether to insert the first special item for the default value</param>
        /// <param name="defaultItemText">Default item text; pass null to use "All" text</param>
        /// <param name="defaultItemValue">Default item value; defaults 0</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task PrepareDefaultItemAsync(IList<SelectListItem> items, bool withSpecialDefaultItem, string defaultItemText = null, string defaultItemValue = "0")
        {
            ArgumentNullException.ThrowIfNull(items);

            //whether to insert the first special item for the default value
            if (!withSpecialDefaultItem)
                return;

            //prepare item text
            defaultItemText ??= await _localizationService.GetResourceAsync("Admin.Common.All");

            //insert this default item at first
            items.Insert(0, new SelectListItem { Text = defaultItemText, Value = defaultItemValue });
        }

        /// <summary>
        /// Prepare available import types
        /// </summary>
        /// <param name="items">import type items</param>
        /// <param name="withSpecialDefaultItem">Whether to insert the first special item for the default value</param>
        /// <param name="defaultItemText">Default item text; pass null to use default value of the default item text</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task PrepareProductImportTypesAsync(IList<SelectListItem> items, bool withSpecialDefaultItem = true, string defaultItemText = null)
        {
            ArgumentNullException.ThrowIfNull(items);

            //prepare available import types
            var availableStatusItems = await ProductImportTypeEnum.Products.ToSelectListAsync(false);
            foreach (var statusItem in availableStatusItems)
            {
                items.Add(statusItem);
            }

            //insert special item for the default value
            await PrepareDefaultItemAsync(items, withSpecialDefaultItem, defaultItemText);

            if (items.Any())
                items.FirstOrDefault().Selected = true;
        }

        /// <summary>
        /// Prepare available import statuses
        /// </summary>
        /// <param name="items">import status items</param>
        /// <param name="withSpecialDefaultItem">Whether to insert the first special item for the default value</param>
        /// <param name="defaultItemText">Default item text; pass null to use default value of the default item text</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task PrepareProductImportStatusesAsync(IList<SelectListItem> items, bool withSpecialDefaultItem = true, string defaultItemText = null)
        {
            ArgumentNullException.ThrowIfNull(items);

            //prepare available import statuses
            var availableStatusItems = await ProductImportStatusEnum.Pending.ToSelectListAsync(false);
            foreach (var statusItem in availableStatusItems)
            {
                items.Add(statusItem);
            }

            //insert special item for the default value
            await PrepareDefaultItemAsync(items, withSpecialDefaultItem, defaultItemText);

            if (items.Any())
                items.FirstOrDefault().Selected = true;
        }

        /// <summary>
        /// Prepare default item
        /// </summary>
        /// <param name="items">Available items</param>
        /// <param name="withSpecialDefaultItem">Whether to insert the first special item for the default value</param>
        /// <param name="defaultItemText">Default item text; pass null to use "All" text</param>
        /// <param name="defaultItemValue">Default item value; defaults 0</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task PrepareDefaultGranitItemAsync(IList<SelectListItem> items, bool withSpecialDefaultItem, string defaultItemText = null, string defaultItemValue = "0")
        {
            ArgumentNullException.ThrowIfNull(items);

            //whether to insert the first special item for the default value
            if (!withSpecialDefaultItem)
                return;

            //prepare item text
            defaultItemText ??= await _localizationService.GetResourceAsync("Admin.Common.All");

            //insert this default item at first
            items.Insert(0, new SelectListItem { Text = defaultItemText, Value = defaultItemValue });
        }

        /// <summary>
        /// Prepare available import statuses
        /// </summary>
        /// <param name="items">import status items</param>
        /// <param name="withSpecialDefaultItem">Whether to insert the first special item for the default value</param>
        /// <param name="defaultItemText">Default item text; pass null to use default value of the default item text</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task PrepareGranitProductImportStatusesAsync(IList<SelectListItem> items, bool withSpecialDefaultItem = true, string defaultItemText = null)
        {
            ArgumentNullException.ThrowIfNull(items);

            //prepare available import statuses
            var availableStatusItems = await GranitProductImportStatusEnum.Pending.ToSelectListAsync(false);
            foreach (var statusItem in availableStatusItems)
            {
                items.Add(statusItem);
            }

            //insert special item for the default value
            await PrepareDefaultGranitItemAsync(items, withSpecialDefaultItem, defaultItemText);

            if (items.Any())
                items.FirstOrDefault().Selected = true;
        }

        /// <summary>
        /// Prepare price import range
        /// </summary>
        /// <param name="searchModel"></param>
        /// <returns>A task that represents the asynchronous operation</returns>
        /// <exception cref="ArgumentNullException"></exception>
        protected virtual PriceRangeSearchModel PreparePriceImportRangeAsync(PriceRangeSearchModel searchModel)
        {
            ArgumentNullException.ThrowIfNull(searchModel);

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }
        
        #endregion

        #region Methods

        #region Configuration

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_WIDGETS))
                return AccessDeniedView();

            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var widgetSettings = await _settingService.LoadSettingAsync<WidgetSettings>(storeId);
            var makeTypeModelSettings = await _settingService.LoadSettingAsync<MakeTypeModelSettings>(storeId);

            var model = new ConfigurationModel
            {
                Enabled = widgetSettings.ActiveWidgetSystemNames.Contains(MakeTypeModelDefaults.SystemName),
                HyCapacityCategoryId = makeTypeModelSettings.HyCapacityCategoryId,
                HyCapacityManufacturerId = makeTypeModelSettings.HyCapacityManufacturerId,
                HyCapacityVendorId = makeTypeModelSettings.HyCapacityVendorId,
                HyCapacityWarehouseId = makeTypeModelSettings.HyCapacityWarehouseId,
                KitProductModelCategoryId = makeTypeModelSettings.KitProductModelCategoryId,
                KitProductCategoryId = makeTypeModelSettings.KitProductCategoryId,
                FindPartsCategoryIds = makeTypeModelSettings.FindPartsCategoryIds?.ToList(),
                FindPartsIncludeSubCategories = makeTypeModelSettings.FindPartsIncludeSubCategories,
                ActiveStoreScopeConfiguration = storeId,

                GranitCategoryId = makeTypeModelSettings.GranitCategoryId,
                GranitVendorId = makeTypeModelSettings.GranitVendorId,
                GranitWarehouseId = makeTypeModelSettings.GranitWarehouseId,
                UpdateProductImage = makeTypeModelSettings.UpdateProductImage,

                AuthorizationAPIURL = makeTypeModelSettings.AuthorizationAPIURL,
                ProductsAPIURL = makeTypeModelSettings.ProductsAPIURL,
                InventoryAPIURL = makeTypeModelSettings.InventoryAPIURL,
                Username = makeTypeModelSettings.Username,
                Password = makeTypeModelSettings.Password,
                ProductUpdateAPIPagesize = makeTypeModelSettings.ProductUpdateAPIPagesize,
                ProductUpdateAPICurrentPage = makeTypeModelSettings.ProductUpdateAPICurrentPage,
                ProductUpdateAPINoOfPages = makeTypeModelSettings.ProductUpdateAPINoOfPages,
                LastExecutedProductAPI = makeTypeModelSettings.LastExecutedProductAPI,
                LastExecutedStockAPI = makeTypeModelSettings.LastExecutedStockAPI,
                TokenGuidId = makeTypeModelSettings.TokenGuidId,
                ExcludeCategory = makeTypeModelSettings.ExcludeCategory,

                EnabledFreeSkus = makeTypeModelSettings.EnabledFreeSkus,
                FreeItemsSkus = makeTypeModelSettings.FreeItemsSkus,
                SubTotalGreaterThan = makeTypeModelSettings.SubTotalGreaterThan,
                FreeItemsWarehouseIds = makeTypeModelSettings.FreeItemsWarehouseIds?.ToList(),
            };

            //prepare available categories
            await _baseAdminModelFactory.PrepareCategoriesAsync(model.AvailableCategories);

            //prepare available manufacturers
            await _baseAdminModelFactory.PrepareManufacturersAsync(model.AvailableManufacturers);

            //prepare available vendors
            await _baseAdminModelFactory.PrepareVendorsAsync(model.AvailableVendors);

            //prepare available warehouses
            await _baseAdminModelFactory.PrepareWarehousesAsync(model.AvailableWarehouses);

            //all model categories
            var modelCategories = await _makeTypeModelService.GetAllModelCategoriesAsync();
            model.AvailableModelCategories = await modelCategories.SelectAwait(async m => new SelectListItem() { Text = (await _categoryService.GetCategoryByIdAsync(m.CategoryId))?.Name, Value = m.CategoryId.ToString(), Selected = m.CategoryId.Equals(model.KitProductModelCategoryId) }).ToListAsync();

            var categories = await _categoryService.GetAllCategoriesByParentCategoryIdAsync(0, true);
            model.AvailableProductCategories = await categories.Select(m => new SelectListItem() { Text = m.Name, Value = m.Id.ToString(), Selected = m.Id.Equals(model.KitProductCategoryId) }).ToListAsync();

            if (storeId > 0)
            {
                model.Enabled_OverrideForStore = await _settingService.SettingExistsAsync(widgetSettings, setting => setting.ActiveWidgetSystemNames, storeId);
                model.HyCapacityCategoryId_OverrideForStore = await _settingService.SettingExistsAsync(makeTypeModelSettings, setting => setting.HyCapacityCategoryId, storeId);
                model.HyCapacityManufacturerId_OverrideForStore = await _settingService.SettingExistsAsync(makeTypeModelSettings, setting => setting.HyCapacityManufacturerId, storeId);
                model.HyCapacityVendorId_OverrideForStore = await _settingService.SettingExistsAsync(makeTypeModelSettings, setting => setting.HyCapacityVendorId, storeId);
                model.HyCapacityWarehouseId_OverrideForStore = await _settingService.SettingExistsAsync(makeTypeModelSettings, setting => setting.HyCapacityWarehouseId, storeId);
                model.KitProductModelCategoryId_OverrideForStore = await _settingService.SettingExistsAsync(makeTypeModelSettings, setting => setting.KitProductModelCategoryId, storeId);
                model.KitProductCategoryId_OverrideForStore = await _settingService.SettingExistsAsync(makeTypeModelSettings, setting => setting.KitProductCategoryId, storeId);
                model.FindPartsCategoryIds_OverrideForStore = await _settingService.SettingExistsAsync(makeTypeModelSettings, setting => setting.FindPartsCategoryIds, storeId);
                model.FindPartsIncludeSubCategories_OverrideForStore = await _settingService.SettingExistsAsync(makeTypeModelSettings, setting => setting.FindPartsIncludeSubCategories, storeId);

                model.GranitCategoryId_OverrideForStore = await _settingService.SettingExistsAsync(makeTypeModelSettings, setting => setting.GranitCategoryId, storeId);
                model.GranitVendorId_OverrideForStore = await _settingService.SettingExistsAsync(makeTypeModelSettings, setting => setting.GranitVendorId, storeId);
                model.GranitWarehouseId_OverrideForStore = await _settingService.SettingExistsAsync(makeTypeModelSettings, setting => setting.GranitWarehouseId, storeId);
                model.UpdateProductImage_OverrideForStore = await _settingService.SettingExistsAsync(makeTypeModelSettings, setting => setting.UpdateProductImage, storeId);

                model.AuthorizationAPIURL_OverrideForStore = await _settingService.SettingExistsAsync(makeTypeModelSettings, setting => setting.AuthorizationAPIURL, storeId);
                model.ProductsAPIURL_OverrideForStore = await _settingService.SettingExistsAsync(makeTypeModelSettings, setting => setting.ProductsAPIURL, storeId);
                model.InventoryAPIURL_OverrideForStore = await _settingService.SettingExistsAsync(makeTypeModelSettings, setting => setting.InventoryAPIURL, storeId);
                model.Username_OverrideForStore = await _settingService.SettingExistsAsync(makeTypeModelSettings, setting => setting.Username, storeId);
                model.Password_OverrideForStore = await _settingService.SettingExistsAsync(makeTypeModelSettings, setting => setting.Password, storeId);
                model.ProductUpdateAPIPagesize_OverrideForStore = await _settingService.SettingExistsAsync(makeTypeModelSettings, setting => setting.ProductUpdateAPIPagesize, storeId);
                model.ProductUpdateAPICurrentPage_OverrideForStore = await _settingService.SettingExistsAsync(makeTypeModelSettings, setting => setting.ProductUpdateAPICurrentPage, storeId);
                model.ProductUpdateAPINoOfPages_OverrideForStore = await _settingService.SettingExistsAsync(makeTypeModelSettings, setting => setting.ProductUpdateAPINoOfPages, storeId);
                model.LastExecutedProductAPI_OverrideForStore = await _settingService.SettingExistsAsync(makeTypeModelSettings, setting => setting.LastExecutedProductAPI, storeId);
                model.LastExecutedStockAPI_OverrideForStore = await _settingService.SettingExistsAsync(makeTypeModelSettings, setting => setting.LastExecutedStockAPI, storeId);
                model.TokenGuidId_OverrideForStore = await _settingService.SettingExistsAsync(makeTypeModelSettings, setting => setting.TokenGuidId, storeId);
                model.ExcludeCategory_OverrideForStore = await _settingService.SettingExistsAsync(makeTypeModelSettings, setting => setting.ExcludeCategory, storeId);

                model.EnabledFreeSkus_OverrideForStore = await _settingService.SettingExistsAsync(makeTypeModelSettings, setting => setting.EnabledFreeSkus, storeId);
                model.FreeItemsSkus_OverrideForStore = await _settingService.SettingExistsAsync(makeTypeModelSettings, setting => setting.FreeItemsSkus, storeId);
                model.SubTotalGreaterThan_OverrideForStore = await _settingService.SettingExistsAsync(makeTypeModelSettings, setting => setting.SubTotalGreaterThan, storeId);
                model.FreeItemsWarehouseIds_OverrideForStore = await _settingService.SettingExistsAsync(makeTypeModelSettings, setting => setting.FreeItemsWarehouseIds, storeId);
            }

            return View("~/Plugins/Widgets.MakeTypeModel/Areas/Admin/Views/MakeTypeModel/Configure.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_WIDGETS))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return await Configure();

            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var widgetSettings = await _settingService.LoadSettingAsync<WidgetSettings>(storeId);
            var makeTypeModelSettings = await _settingService.LoadSettingAsync<MakeTypeModelSettings>(storeId);

            if (model.Enabled && !widgetSettings.ActiveWidgetSystemNames.Contains(MakeTypeModelDefaults.SystemName))
                widgetSettings.ActiveWidgetSystemNames.Add(MakeTypeModelDefaults.SystemName);
            if (!model.Enabled && widgetSettings.ActiveWidgetSystemNames.Contains(MakeTypeModelDefaults.SystemName))
                widgetSettings.ActiveWidgetSystemNames.Remove(MakeTypeModelDefaults.SystemName);

            makeTypeModelSettings.HyCapacityCategoryId = model.HyCapacityCategoryId;
            makeTypeModelSettings.HyCapacityManufacturerId = model.HyCapacityManufacturerId;
            makeTypeModelSettings.HyCapacityVendorId = model.HyCapacityVendorId;
            makeTypeModelSettings.HyCapacityWarehouseId = model.HyCapacityWarehouseId;
            makeTypeModelSettings.KitProductModelCategoryId = model.KitProductModelCategoryId;
            makeTypeModelSettings.KitProductCategoryId = model.KitProductCategoryId;
            makeTypeModelSettings.FindPartsCategoryIds = model.FindPartsCategoryIds?.ToList() ?? new List<int>();
            makeTypeModelSettings.FindPartsIncludeSubCategories = model.FindPartsIncludeSubCategories;

            makeTypeModelSettings.GranitCategoryId = model.GranitCategoryId;
            makeTypeModelSettings.GranitVendorId = model.GranitVendorId;
            makeTypeModelSettings.GranitWarehouseId = model.GranitWarehouseId;
            makeTypeModelSettings.UpdateProductImage = model.UpdateProductImage;

            makeTypeModelSettings.AuthorizationAPIURL = model.AuthorizationAPIURL;
            makeTypeModelSettings.ProductsAPIURL = model.ProductsAPIURL;
            makeTypeModelSettings.InventoryAPIURL = model.InventoryAPIURL;
            makeTypeModelSettings.Username = model.Username;
            makeTypeModelSettings.Password = model.Password;
            makeTypeModelSettings.ProductUpdateAPIPagesize = model.ProductUpdateAPIPagesize;
            makeTypeModelSettings.ProductUpdateAPICurrentPage = model.ProductUpdateAPICurrentPage;
            makeTypeModelSettings.ProductUpdateAPINoOfPages = model.ProductUpdateAPINoOfPages;
            makeTypeModelSettings.LastExecutedProductAPI = model.LastExecutedProductAPI;
            makeTypeModelSettings.LastExecutedStockAPI = model.LastExecutedProductAPI;
            makeTypeModelSettings.TokenGuidId = model.TokenGuidId;
            makeTypeModelSettings.ExcludeCategory = model.ExcludeCategory;

            makeTypeModelSettings.EnabledFreeSkus = model.EnabledFreeSkus;
            makeTypeModelSettings.FreeItemsSkus = model.FreeItemsSkus;
            makeTypeModelSettings.SubTotalGreaterThan = model.SubTotalGreaterThan;
            makeTypeModelSettings.FreeItemsWarehouseIds = model.FreeItemsWarehouseIds?.ToList() ?? new List<int>();

            // Save per-store overrides
            await _settingService.SaveSettingOverridablePerStoreAsync(makeTypeModelSettings, s => s.HyCapacityCategoryId, model.HyCapacityCategoryId_OverrideForStore, storeId, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(makeTypeModelSettings, s => s.HyCapacityManufacturerId, model.HyCapacityManufacturerId_OverrideForStore, storeId, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(makeTypeModelSettings, s => s.HyCapacityVendorId, model.HyCapacityVendorId_OverrideForStore, storeId, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(makeTypeModelSettings, s => s.HyCapacityWarehouseId, model.HyCapacityWarehouseId_OverrideForStore, storeId, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(makeTypeModelSettings, s => s.KitProductModelCategoryId, model.KitProductModelCategoryId_OverrideForStore, storeId, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(makeTypeModelSettings, s => s.KitProductCategoryId, model.KitProductCategoryId_OverrideForStore, storeId, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(makeTypeModelSettings, s => s.FindPartsCategoryIds, model.FindPartsCategoryIds_OverrideForStore, storeId, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(makeTypeModelSettings, s => s.FindPartsIncludeSubCategories, model.FindPartsIncludeSubCategories_OverrideForStore, storeId, false);

            await _settingService.SaveSettingOverridablePerStoreAsync(makeTypeModelSettings, s => s.GranitCategoryId, model.GranitCategoryId_OverrideForStore, storeId, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(makeTypeModelSettings, s => s.GranitVendorId, model.GranitVendorId_OverrideForStore, storeId, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(makeTypeModelSettings, s => s.GranitWarehouseId, model.GranitWarehouseId_OverrideForStore, storeId, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(makeTypeModelSettings, s => s.UpdateProductImage, model.UpdateProductImage_OverrideForStore, storeId, false);

            await _settingService.SaveSettingOverridablePerStoreAsync(makeTypeModelSettings, s => s.AuthorizationAPIURL, model.AuthorizationAPIURL_OverrideForStore, storeId, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(makeTypeModelSettings, s => s.ProductsAPIURL, model.ProductsAPIURL_OverrideForStore, storeId, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(makeTypeModelSettings, s => s.InventoryAPIURL, model.InventoryAPIURL_OverrideForStore, storeId, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(makeTypeModelSettings, s => s.Username, model.Username_OverrideForStore, storeId, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(makeTypeModelSettings, s => s.Password, model.Password_OverrideForStore, storeId, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(makeTypeModelSettings, s => s.ProductUpdateAPIPagesize, model.ProductUpdateAPIPagesize_OverrideForStore, storeId, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(makeTypeModelSettings, s => s.ProductUpdateAPICurrentPage, model.ProductUpdateAPICurrentPage_OverrideForStore, storeId, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(makeTypeModelSettings, s => s.ProductUpdateAPINoOfPages, model.ProductUpdateAPINoOfPages_OverrideForStore, storeId, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(makeTypeModelSettings, s => s.LastExecutedProductAPI, model.LastExecutedProductAPI_OverrideForStore, storeId, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(makeTypeModelSettings, s => s.LastExecutedStockAPI, model.LastExecutedStockAPI_OverrideForStore, storeId, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(makeTypeModelSettings, s => s.TokenGuidId, model.TokenGuidId_OverrideForStore, storeId, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(makeTypeModelSettings, s => s.ExcludeCategory, model.ExcludeCategory_OverrideForStore, storeId, false);

            await _settingService.SaveSettingOverridablePerStoreAsync(makeTypeModelSettings, s => s.EnabledFreeSkus, model.EnabledFreeSkus_OverrideForStore, storeId, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(makeTypeModelSettings, s => s.FreeItemsSkus, model.FreeItemsSkus_OverrideForStore, storeId, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(makeTypeModelSettings, s => s.SubTotalGreaterThan, model.SubTotalGreaterThan_OverrideForStore, storeId, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(makeTypeModelSettings, s => s.FreeItemsWarehouseIds, model.FreeItemsWarehouseIds_OverrideForStore, storeId, false);

            await _settingService.SaveSettingOverridablePerStoreAsync(widgetSettings, s => s.ActiveWidgetSystemNames, model.Enabled_OverrideForStore, storeId, false);

            await _settingService.SaveSettingAsync(widgetSettings);
            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

            return await Configure();
        }

        #endregion

        #region Make 

        public async Task<IActionResult> MakeList()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
                return AccessDeniedView();

            var model = new MakeProductSearchModel();

            //prepare page parameters
            model.SetGridPageSize();

            return View("~/Plugins/Widgets.MakeTypeModel/Areas/Admin/Views/MakeTypeModel/MakeList.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> MakeList(MakeProductSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
                return await AccessDeniedJsonAsync();

            //prepare model
            var model = await _makeTypeModelFactory.PrepareMakeProductListModelAsync(searchModel); 
            
            return Json(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> MakeUpdate([Validate] MakeProductModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
                return AccessDeniedView();

            if (model.Name != null)
                model.Name = model.Name.Trim();

            if (!ModelState.IsValid)
            {
                return ErrorJson(ModelState.SerializeErrors());
            }

            var make = await _makeTypeModelService.GetMakeProductByIdAsync(model.Id);
            // if the make name changed, ensure it isn't being used by another make
            if (!make.Name.Equals(model.Name, StringComparison.InvariantCultureIgnoreCase))
            {
                var mak = await _makeTypeModelService.GetMakeProductByNameAsync(model.Name);
                if (mak != null && mak.Id != make.Id)
                {
                    return ErrorJson(string.Format(await _localizationService.GetResourceAsync("Plugins.Widgets.MakeTypeModel.MakeProduct.Fields.Name.NameAlreadyExists"), mak.Name));
                }
            }

            //fill entity from model
            make.Name = model.Name;
            make.Published = model.Published;
            make.DisplayOrder = model.DisplayOrder;

            await _makeTypeModelService.UpdateMakeProductAsync(make);

            return new NullJsonResult();
        }

        [HttpPost]
        public virtual async Task<IActionResult> MakeAdd([Validate] MakeProductModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
                return AccessDeniedView();

            if (model.Name != null)
                model.Name = model.Name.Trim();

            if (!ModelState.IsValid)
            {
                return ErrorJson(ModelState.SerializeErrors());
            }

            var mak = await _makeTypeModelService.GetMakeProductByNameAsync(model.Name);
            if (mak == null)
            {
                //fill entity from model
                var make = new MakeProduct()
                {
                    Name = model.Name,
                    Published = model.Published,
                    DisplayOrder = model.DisplayOrder
                };

                await _makeTypeModelService.InsertMakeProductAsync(make);
            }
            else
            {
                return ErrorJson(string.Format(await _localizationService.GetResourceAsync("Plugins.Widgets.MakeTypeModel.MakeProduct.Fields.Name.NameAlreadyExists"), model.Name));
            }

            return Json(new { Result = true });
        }

        [HttpPost]
        public virtual async Task<IActionResult> MakeDelete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
                return AccessDeniedView();

            //try to get a make product with the specified id
            var make = await _makeTypeModelService.GetMakeProductByIdAsync(id)
                ?? throw new ArgumentException("No make product found with the specified id", nameof(id));

            await _makeTypeModelService.DeleteMakeProductAsync(make);

            return new NullJsonResult();
        }

        #endregion

        #region Type 

        public async Task<IActionResult> TypeList()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
                return AccessDeniedView();

            var model = new TypeProductSearchModel();

            //prepare page parameters
            model.SetGridPageSize();

            return View("~/Plugins/Widgets.MakeTypeModel/Areas/Admin/Views/MakeTypeModel/TypeList.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> TypeList(TypeProductSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
                return await AccessDeniedJsonAsync();

            //get types product
           
            var model = await _makeTypeModelFactory.PrepareTypeProductListModelAsync(searchModel);
            return Json(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> TypeUpdate([Validate] TypeProductModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
                return AccessDeniedView();

            if (model.Name != null)
                model.Name = model.Name.Trim();

            if (!ModelState.IsValid)
            {
                return ErrorJson(ModelState.SerializeErrors());
            }

            var type = await _makeTypeModelService.GetTypeProductByIdAsync(model.Id);
            // if the type name changed, ensure it isn't being used by another type
            if (!type.Name.Equals(model.Name, StringComparison.InvariantCultureIgnoreCase))
            {
                var typ = await _makeTypeModelService.GetTypeProductByNameAsync(model.Name);
                if (typ != null && typ.Id != type.Id)
                {
                    return ErrorJson(string.Format(await _localizationService.GetResourceAsync("Plugins.Widgets.MakeTypeModel.TypeProduct.Fields.Name.NameAlreadyExists"), typ.Name));
                }
            }

            //fill entity from model
            type.Name = model.Name;
            type.Published = model.Published;
            type.DisplayOrder = model.DisplayOrder;

            await _makeTypeModelService.UpdateTypeProductAsync(type);

            return new NullJsonResult();
        }

        [HttpPost]
        public virtual async Task<IActionResult> TypeAdd([Validate] TypeProductModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
                return AccessDeniedView();

            if (model.Name != null)
                model.Name = model.Name.Trim();

            if (!ModelState.IsValid)
            {
                return ErrorJson(ModelState.SerializeErrors());
            }

            var typ = await _makeTypeModelService.GetTypeProductByNameAsync(model.Name);
            if (typ == null)
            {
                //fill entity from model
                var type = new TypeProduct()
                {
                    Name = model.Name,
                    Published = model.Published,
                    DisplayOrder = model.DisplayOrder
                };

                await _makeTypeModelService.InsertTypeProductAsync(type);
            }
            else
            {
                return ErrorJson(string.Format(await _localizationService.GetResourceAsync("Plugins.Widgets.MakeTypeModel.TypeProduct.Fields.Name.NameAlreadyExists"), model.Name));
            }

            return Json(new { Result = true });
        }

        [HttpPost]
        public virtual async Task<IActionResult> TypeDelete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
                return AccessDeniedView();

            //try to get a type product with the specified id
            var type = await _makeTypeModelService.GetTypeProductByIdAsync(id)
                ?? throw new ArgumentException("No type product found with the specified id", nameof(id));

            await _makeTypeModelService.DeleteTypeProductAsync(type);

            return new NullJsonResult();
        }

        #endregion

        #region Model 

        public async Task<IActionResult> ModelList()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
                return AccessDeniedView();

            var model = await _makeTypeModelFactory.PrepareModelSearchModelAsync(new ModelProductSearchModel());

            return View("~/Plugins/Widgets.MakeTypeModel/Areas/Admin/Views/MakeTypeModel/ModelList.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> ModelList(ModelProductSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
                return await AccessDeniedJsonAsync();

            //get models product
            var model = await _makeTypeModelFactory.PrepareModelProductListModelAsync(searchModel);

            return Json(model);
        }

        public async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
                return AccessDeniedView();

            //prepare model
            var model = new ModelProductModel();

            var makes = await _makeTypeModelService.GetAllMakeProductsAsync();
            foreach (var make in makes)
                model.AvailableMakes.Add(new SelectListItem() { Text = make.Name, Value = make.Name });

            //insert this default item at first
            model.AvailableMakes.Insert(0, new SelectListItem { Text = await _localizationService.GetResourceAsync("Admin.Common.Select"), Value = string.Empty });

            var types = await _makeTypeModelService.GetAllTypeProductsAsync();
            foreach (var type in types)
                model.AvailableTypes.Add(new SelectListItem() { Text = type.Name, Value = type.Name });

            //insert this default item at first
            model.AvailableTypes.Insert(0, new SelectListItem { Text = await _localizationService.GetResourceAsync("Admin.Common.Select"), Value = string.Empty });
            
            return View("~/Plugins/Widgets.MakeTypeModel/Areas/Admin/Views/MakeTypeModel/Create.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> Create(ModelProductModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var modelProduct = new ModelProduct
                {
                    Name = model.Name,
                    Description = model.Description,
                    MakeName = model.MakeName,
                    TypeName = model.TypeName,
                    Published = model.Published,
                    DisplayOrder = model.DisplayOrder
                };
                await _makeTypeModelService.InsertModelProductAsync(modelProduct);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugins.Widgets.MakeTypeModel.ModelProduct.Added"));

                if (!continueEditing)
                    return RedirectToAction("ModelList");

                return RedirectToAction("Edit", new { id = modelProduct.Id });
            }

            var makes = await _makeTypeModelService.GetAllMakeProductsAsync();
            foreach (var make in makes)
                model.AvailableMakes.Add(new SelectListItem() { Text = make.Name, Value = make.Name });

            //insert this default item at first
            model.AvailableMakes.Insert(0, new SelectListItem { Text = await _localizationService.GetResourceAsync("Admin.Common.Select"), Value = string.Empty });

            var types = await _makeTypeModelService.GetAllTypeProductsAsync();
            foreach (var type in types)
                model.AvailableTypes.Add(new SelectListItem() { Text = type.Name, Value = type.Name });

            //insert this default item at first
            model.AvailableTypes.Insert(0, new SelectListItem { Text = await _localizationService.GetResourceAsync("Admin.Common.Select"), Value = string.Empty });
            
            return View("~/Plugins/Widgets.MakeTypeModel/Areas/Admin/Views/MakeTypeModel/Create.cshtml", model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
                return AccessDeniedView();

            //try to get a model product with the specified id
            var modelProduct = await _makeTypeModelService.GetModelProductByIdAsync(id);
            if (modelProduct == null)
                return RedirectToAction("ModelList");

            //prepare model
            var model = new ModelProductModel()
            {
                Id = modelProduct.Id,
                Name = modelProduct.Name,
                Description = modelProduct.Description,
                MakeName = modelProduct.MakeName,
                TypeName = modelProduct.TypeName,
                Published = modelProduct.Published,
                DisplayOrder = modelProduct.DisplayOrder
            };

            var makes = await _makeTypeModelService.GetAllMakeProductsAsync();
            foreach (var make in makes)
                model.AvailableMakes.Add(new SelectListItem() { Text = make.Name, Value = make.Name });

            //insert this default item at first
            model.AvailableMakes.Insert(0, new SelectListItem { Text = await _localizationService.GetResourceAsync("Admin.Common.Select"), Value = string.Empty });

            var types = await _makeTypeModelService.GetAllTypeProductsAsync();
            foreach (var type in types)
                model.AvailableTypes.Add(new SelectListItem() { Text = type.Name, Value = type.Name });

            //insert this default item at first
            model.AvailableTypes.Insert(0, new SelectListItem { Text = await _localizationService.GetResourceAsync("Admin.Common.Select"), Value = string.Empty });
            model.ModelProductMappingSearchModel.ModelId = modelProduct.Id;

            //prepare page parameters
            model.ModelProductMappingSearchModel.SetGridPageSize();

            return View("~/Plugins/Widgets.MakeTypeModel/Areas/Admin/Views/MakeTypeModel/Edit.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> Edit(ModelProductModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
                return AccessDeniedView();

            //try to get a model product with the specified id
            var modelProduct = await _makeTypeModelService.GetModelProductByIdAsync(model.Id);
            if (modelProduct == null)
                return RedirectToAction("ModelList");

            if (ModelState.IsValid)
            {
                modelProduct.Id = model.Id;
                modelProduct.Name = model.Name;
                modelProduct.Description = model.Description;
                modelProduct.MakeName = model.MakeName;
                modelProduct.TypeName = model.TypeName;
                modelProduct.Published = model.Published;
                modelProduct.DisplayOrder = model.DisplayOrder;

                await _makeTypeModelService.UpdateModelProductAsync(modelProduct);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugins.Widgets.MakeTypeModel.ModelProduct.Updated"));

                if (!continueEditing)
                    return RedirectToAction("ModelList");

                return RedirectToAction("Edit", new { id = modelProduct.Id });
            }

            var makes = await _makeTypeModelService.GetAllMakeProductsAsync();
            foreach (var make in makes)
                model.AvailableMakes.Add(new SelectListItem() { Text = make.Name, Value = make.Name });

            //insert this default item at first
            model.AvailableMakes.Insert(0, new SelectListItem { Text = await _localizationService.GetResourceAsync("Admin.Common.Select"), Value = string.Empty });

            var types = await _makeTypeModelService.GetAllTypeProductsAsync();
            foreach (var type in types)
                model.AvailableTypes.Add(new SelectListItem() { Text = type.Name, Value = type.Name });

            //insert this default item at first
            model.AvailableTypes.Insert(0, new SelectListItem { Text = await _localizationService.GetResourceAsync("Admin.Common.Select"), Value = string.Empty });
            return View("~/Plugins/Widgets.MakeTypeModel/Areas/Admin/Views/MakeTypeModel/Edit.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
                return AccessDeniedView();

            //try to get a model product with the specified id
            var modelProduct = await _makeTypeModelService.GetModelProductByIdAsync(id);
            if (modelProduct == null)
                return RedirectToAction("ModelList");

            await _makeTypeModelService.DeleteModelProductAsync(modelProduct);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugins.Widgets.MakeTypeModel.ModelProduct.Deleted"));

            return RedirectToAction("ModelList");
        }

        [HttpPost]
        public virtual async Task<IActionResult> ImportExcel(IFormFile importexcelfile)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
                return AccessDeniedView();

            try
            {
                if (importexcelfile != null && importexcelfile.Length > 0)
                {
                    if (!_productImportFormExcelService.IsImportFileNameAlreadyExists(importexcelfile.FileName))
                    {
                        //insert record on import queue
                        var productImport = new ProductImport
                        {
                            FileName = importexcelfile.FileName,
                            ImportTypeId = (int)ProductImportTypeEnum.ModelFit,
                            ImportStatusId = (int)ProductImportStatusEnum.Pending,
                            DeleteAll = false,
                            RowNumber = 2,
                            CreatedOnUtc = DateTime.UtcNow
                        };
                        await _productImportFormExcelService.InsertProductImportAsync(productImport);

                        //save file on disk
                        var path = _nopFileProvider.Combine(_nopFileProvider.MapPath("~/wwwroot/files/productimport"));
                        if (!Directory.Exists(path))
                            Directory.CreateDirectory(path);

                        var filePath = _nopFileProvider.Combine(path, importexcelfile.FileName);
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            importexcelfile.CopyTo(fileStream);
                        }
                    }
                }
                else
                {
                    _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Common.UploadFile"));

                    return RedirectToAction("ModelList");
                }

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugins.Widgets.MakeTypeModel.ModelProduct.Imported"));

                return RedirectToAction("ModelList");
            }
            catch (Exception exc)
            {
                await _notificationService.ErrorNotificationAsync(exc);

                return RedirectToAction("ModelList");
            }
        }

        #endregion

        #region Products

        [HttpPost]
        public virtual async Task<IActionResult> ModelProductList(ModelProductMappingSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Catalog.PRODUCTS_VIEW))
                return await AccessDeniedJsonAsync();

            //prepare model
           var model = await _makeTypeModelFactory.PrepareModelProductMappingListModelAsync(searchModel);

            return Json(model);
        }

        public virtual async Task<IActionResult> ModelProductDelete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE))
                return AccessDeniedView();

            //try to get a model product with the specified id
            var modelProduct = await _makeTypeModelService.GetModelProductMappingByIdAsync(id)
                ?? throw new ArgumentException("No model product found with the specified id");

            await _makeTypeModelService.DeleteModelProductMappingAsync(modelProduct);

            return new NullJsonResult();
        }

        public virtual async Task<IActionResult> ModelProductAddPopup(int modelId)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Catalog.CATEGORIES_CREATE_EDIT_DELETE))
                return AccessDeniedView();

            //prepare model
            var model = new AddProductToModelProductSearchModel();

            //prepare available categories
            await _baseAdminModelFactory.PrepareCategoriesAsync(model.AvailableCategories);

            //prepare available manufacturers
            await _baseAdminModelFactory.PrepareManufacturersAsync(model.AvailableManufacturers);

            //prepare available stores
            await _baseAdminModelFactory.PrepareStoresAsync(model.AvailableStores);

            //prepare available vendors
            await _baseAdminModelFactory.PrepareVendorsAsync(model.AvailableVendors);

            //prepare available product types
            await _baseAdminModelFactory.PrepareProductTypesAsync(model.AvailableProductTypes);

            //prepare page parameters
            model.SetGridPageSize();

            return View("~/Plugins/Widgets.MakeTypeModel/Areas/Admin/Views/MakeTypeModel/ModelProductAddPopup.cshtml", model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> ModelProductAddPopupList(AddProductToModelProductSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Catalog.CATEGORIES_CREATE_EDIT_DELETE))
                return await AccessDeniedJsonAsync();

            var model = await _makeTypeModelFactory.PrepareModelProductAddPopupListAsync(searchModel);

            return Json(model);
        }

        [HttpPost]
        [FormValueRequired("save")]
        public virtual async Task<IActionResult> ModelProductAddPopup(AddProductToModelProductModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Catalog.CATEGORIES_CREATE_EDIT_DELETE))
                return AccessDeniedView();

            //get selected products
            var selectedProducts = await _productService.GetProductsByIdsAsync(model.SelectedProductIds.ToArray());
            if (selectedProducts.Any())
            {
                var existingModelProducts = await _makeTypeModelService.GetAllModelProductMappingsByModelIdAsync(model.ModelId);
                foreach (var product in selectedProducts)
                {
                    //whether product category with such parameters already exists
                    if (existingModelProducts.Any(m => m.ModelId == model.ModelId && m.ProductId == product.Id))
                        continue;

                    //insert the new product category mapping
                    await _makeTypeModelService.InsertModelProductMappingAsync(new ModelProductMapping
                    {
                        ModelId = model.ModelId,
                        ProductId = product.Id
                    });
                }
            }

            ViewBag.RefreshPage = true;

            return View("~/Plugins/Widgets.MakeTypeModel/Areas/Admin/Views/MakeTypeModel/ModelProductAddPopup.cshtml", new AddProductToModelProductSearchModel());
        }

        #endregion

        #region Category

        public async Task<IActionResult> CategoryList()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Catalog.CATEGORIES_VIEW))
                return AccessDeniedView();

            var model = new ModelCategorySearchModel();

            //prepare page parameters
            model.SetGridPageSize();

            return View("~/Plugins/Widgets.MakeTypeModel/Areas/Admin/Views/MakeTypeModel/CategoryList.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> CategoryList(ModelCategorySearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Catalog.CATEGORIES_VIEW))
                return await AccessDeniedJsonAsync();

            //get model categories
           var model = await _makeTypeModelFactory.PrepareCategoryListModelAsync(searchModel);

            return Json(model);
        }

        [HttpPost]
        [FormValueRequired("model-categories-sync")]
        public async Task<IActionResult> CategorySync()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
                return await AccessDeniedJsonAsync();

            //get selected products
            var categories = await _categoryService.GetAllCategoriesByParentCategoryIdAsync(0, true);
            foreach (var category in categories)
            {
                var selectedCategories = await _categoryService.GetAllCategoriesByParentCategoryIdAsync(category.Id, true);
                //get existing model categories
                var existingModelCategories = await _makeTypeModelService.GetAllModelCategoriesAsync(true);
                foreach (var selectedCategory in selectedCategories)
                {
                    //whether model category with such parameters already exists
                    if (existingModelCategories.Any(c => c.CategoryId == selectedCategory.Id))
                        continue;

                    //insert the new product category mapping
                    await _makeTypeModelService.InsertModelCategoryAsync(new ModelCategory
                    {
                        CategoryId = selectedCategory.Id,
                        Published = selectedCategory.Published,
                        DisplayOrder = selectedCategory.DisplayOrder
                    });
                }
            }

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugins.Widgets.MakeTypeModel.ModelCategories.Sync.Successfully"));

            return RedirectToAction("CategoryList");
        }

        [HttpPost]
        public virtual async Task<IActionResult> ModelCategoryUpdate([Validate] ModelCategoryModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
                return AccessDeniedView();

            var modelCategory = await _makeTypeModelService.GetModelCategoryByIdAsync(model.Id);
            if (modelCategory == null)
                return new NullJsonResult();

            //fill entity from model
            modelCategory.Published = model.Published;
            modelCategory.DisplayOrder = model.DisplayOrder;

            await _makeTypeModelService.UpdateModelCategoryAsync(modelCategory);

            return new NullJsonResult();
        }

        #endregion

        #region Product Import

        public async Task<IActionResult> ProductImportList()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS) || !await _permissionService.AuthorizeAsync(StandardPermission.Catalog.PRODUCTS_VIEW))
                return AccessDeniedView();

            if (await _workContext.GetCurrentVendorAsync() != null && !_vendorSettings.AllowVendorsToImportProducts)
                //a vendor can not import products
                return AccessDeniedView();

            var model = new ProductImportSearchModel();

            //prepare available product statuses and types
            await PrepareProductImportStatusesAsync(model.AvailableImportStatuses);

            await PrepareProductImportTypesAsync(model.AvailableImportTypes);

            //prepare page parameters
            model.SetGridPageSize();

            return View("~/Plugins/Widgets.MakeTypeModel/Areas/Admin/Views/MakeTypeModel/ProductImportList.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> ProductImportList(ProductImportSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS) || !await _permissionService.AuthorizeAsync(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE))
                return await AccessDeniedJsonAsync();

            var model = await _makeTypeModelFactory.PrepareProductImportListModelAsync(searchModel);

            return Json(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> ProductImportDelete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS) || !await _permissionService.AuthorizeAsync(StandardPermission.Catalog.PRODUCTS_IMPORT_EXPORT))
                return AccessDeniedView();

            //try to get a make product with the specified id
            var importProduct = await _productImportFormExcelService.GetProductImportByIdAsync(id)
                ?? throw new ArgumentException("No file import found with the specified id", nameof(id));

            await _productImportFormExcelService.DeleteProductImportAsync(importProduct);

            return new NullJsonResult();
        }

        [HttpPost]
        public virtual async Task<IActionResult> ProductImportAdd(IFormFile importexcelfile, ProductImportModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE))
                return AccessDeniedView();

            if (await _workContext.GetCurrentVendorAsync() != null && !_vendorSettings.AllowVendorsToImportProducts)
                //a vendor can not import products
                return AccessDeniedView();

            try
            {
                if (importexcelfile != null && importexcelfile.Length > 0)
                {
                    if (!_productImportFormExcelService.IsImportFileNameAlreadyExists(importexcelfile.FileName))
                    {
                        //insert record on import queue
                        var productImport = new ProductImport
                        {
                            FileName = importexcelfile.FileName,
                            ImportTypeId = model.ImportTypeId,
                            ImportStatusId = (int)ProductImportStatusEnum.Pending,
                            DeleteAll = model.DeleteAll,
                            RowNumber = 2,
                            CreatedOnUtc = DateTime.UtcNow
                        };
                        await _productImportFormExcelService.InsertProductImportAsync(productImport);

                        //save file on disk
                        var path = _nopFileProvider.Combine(_nopFileProvider.MapPath("~/wwwroot/files/productimport"));
                        if (!Directory.Exists(path))
                            Directory.CreateDirectory(path);

                        var filePath = _nopFileProvider.Combine(path, importexcelfile.FileName);
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            importexcelfile.CopyTo(fileStream);
                        }

                        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugins.Widgets.MakeTypeModel.ProductImport.Imported"));

                        return RedirectToAction("ProductImportList");
                    }

                    _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Plugins.Widgets.MakeTypeModel.ProductImport.ImportedExist"));

                    return RedirectToAction("ProductImportList");
                }
                else
                {
                    _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Common.UploadFile"));

                    return RedirectToAction("ProductImportList");
                }
            }
            catch (Exception exc)
            {
                await _notificationService.ErrorNotificationAsync(exc);

                return RedirectToAction("ProductImportList");
            }
        }

        #endregion

        #region Product models

        [HttpPost]
        public virtual async Task<IActionResult> ProductModelsList(ModelProductSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Catalog.PRODUCTS_VIEW))
                return await AccessDeniedJsonAsync();

            //prepare model
            ArgumentNullException.ThrowIfNull(searchModel);

            if (searchModel.ProductId <= 0)
                return Json(new ModelProductListModel { Data = new List<ModelProductModel>() });

            var product = await _productService.GetProductByIdAsync(searchModel.ProductId);
            if (product == null)
                return Json(new ModelProductListModel { Data = new List<ModelProductModel>() });

            var model = await _makeTypeModelFactory.PrepareProductModelsSearchAndListModelAsync(searchModel);
           
            return Json(model);
        }

        public virtual async Task<IActionResult> ProductModelDelete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE))
                return await AccessDeniedJsonAsync();

            var modelProductMap = await _makeTypeModelService.GetModelProductMappingByIdAsync(id)
                ?? throw new ArgumentException("No model product mapping found with the specified id", nameof(id));

            if (modelProductMap != null)
                await _makeTypeModelService.DeleteModelProductMappingAsync(modelProductMap);

            return new NullJsonResult();
        }

        public virtual async Task<IActionResult> ProductModelsAddPopup(int productId)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE))
                return AccessDeniedView();

            var model = await _makeTypeModelFactory.PrepareProductModelsSearchModelAsync();

            return View("~/Plugins/Widgets.MakeTypeModel/Areas/Admin/Views/MakeTypeModel/ProductModelsAddPopup.cshtml", model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> ProductModelsAddPopupList(ModelProductSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE))
                return await AccessDeniedJsonAsync();

            var model = await _makeTypeModelFactory.PrepareProductModelsAddPopupListModelAsync(searchModel);

            return Json(model);
        }

        [HttpPost]
        [FormValueRequired("save")]
        public virtual async Task<IActionResult> ProductModelsAddPopup(AddModelProductToProductModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE))
                return AccessDeniedView();

            //get selected models
            var existingModelProducts = await _makeTypeModelService.GetAllModelProductMappingsAsync(modelId: 0, productId: model.ProductId);
            foreach (var modelId in model.SelectedModelProductIds)
            {
                //whether model product with such parameters already exists
                if (existingModelProducts.Any(m => m.ModelId == modelId && m.ProductId == model.ProductId))
                    continue;

                //insert the new model product mapping
                await _makeTypeModelService.InsertModelProductMappingAsync(new ModelProductMapping
                {
                    ModelId = modelId,
                    ProductId = model.ProductId
                });
            }

            ViewBag.RefreshPage = true;

            return View("~/Plugins/Widgets.MakeTypeModel/Areas/Admin/Views/MakeTypeModel/ProductModelsAddPopup.cshtml", new ModelProductSearchModel());
        }

        public virtual async Task<IActionResult> ProductModelEditPopup(int productModelId)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
                return await AccessDeniedJsonAsync();

            var modelProductMap = await _makeTypeModelService.GetModelProductMappingByIdAsync(productModelId)
                ?? throw new ArgumentException("No model product mapping found with the specified id", nameof(productModelId));

            //try to get a model product with the specified id
            var modelProduct = await _makeTypeModelService.GetModelProductByIdAsync(modelProductMap.ModelId)
                ?? throw new ArgumentException("No model product found with the specified id", nameof(modelProductMap.ModelId));

            var modelProducts = await _makeTypeModelService.GetAllModelProductMappingsAsync(modelId: modelProduct.Id, pageSize: 10);

            //prepare model
            var model = new ModelProductModel()
            {
                Id = modelProduct.Id,
                Name = modelProduct.Name,
                Description = modelProduct.Description,
                MakeName = modelProduct.MakeName,
                TypeName = modelProduct.TypeName,
                Published = modelProduct.Published,
                DisplayOrder = modelProduct.DisplayOrder,
                ProductModelId = productModelId,
                HasMoreProducts = modelProducts.TotalCount > 1,
            };

            var makes = await _makeTypeModelService.GetAllMakeProductsAsync();
            foreach (var make in makes)
                model.AvailableMakes.Add(new SelectListItem() { Text = make.Name, Value = make.Name });

            //insert this default item at first
            model.AvailableMakes.Insert(0, new SelectListItem { Text = await _localizationService.GetResourceAsync("Admin.Common.Select"), Value = string.Empty });

            var types = await _makeTypeModelService.GetAllTypeProductsAsync();
            foreach (var type in types)
                model.AvailableTypes.Add(new SelectListItem() { Text = type.Name, Value = type.Name });

            //insert this default item at first
            model.AvailableTypes.Insert(0, new SelectListItem { Text = await _localizationService.GetResourceAsync("Admin.Common.Select"), Value = string.Empty });

            return View("~/Plugins/Widgets.MakeTypeModel/Areas/Admin/Views/MakeTypeModel/ProductModelEditPopup.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-add-new", "newAdding")]
        public virtual async Task<IActionResult> ProductModelEditPopup(ModelProductModel model, bool newAdding)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
                return AccessDeniedView();

            //try to get a model product with the specified id
            var modelProduct = await _makeTypeModelService.GetModelProductByIdAsync(model.Id);
            if (modelProduct == null)
                return RedirectToAction("List", "Product");

            var modelProductMap = await _makeTypeModelService.GetModelProductMappingByIdAsync(model.ProductModelId)
                ?? throw new ArgumentException("No model product mapping found with the specified id", nameof(model.ProductModelId));

            if (ModelState.IsValid)
            {
                if (newAdding)
                {
                    modelProduct = new ModelProduct
                    {
                        Name = model.Name,
                        Description = model.Description,
                        MakeName = model.MakeName,
                        TypeName = model.TypeName,
                        Published = model.Published,
                        DisplayOrder = model.DisplayOrder
                    };
                    await _makeTypeModelService.InsertModelProductAsync(modelProduct);


                    //insert the new model product mapping
                    await _makeTypeModelService.InsertModelProductMappingAsync(new ModelProductMapping
                    {
                        ModelId = modelProduct.Id,
                        ProductId = modelProductMap.ProductId,
                    });

                    //delete the model product mapping
                    await _makeTypeModelService.DeleteModelProductMappingAsync(modelProductMap);
                }
                else
                {
                    modelProduct.Id = model.Id;
                    modelProduct.Name = model.Name;
                    modelProduct.Description = model.Description;
                    modelProduct.MakeName = model.MakeName;
                    modelProduct.TypeName = model.TypeName;
                    modelProduct.Published = model.Published;
                    modelProduct.DisplayOrder = model.DisplayOrder;

                    await _makeTypeModelService.UpdateModelProductAsync(modelProduct);
                }

                ViewBag.RefreshPage = true;

                return View("~/Plugins/Widgets.MakeTypeModel/Areas/Admin/Views/MakeTypeModel/ProductModelEditPopup.cshtml", model);
            }

            var modelProducts = await _makeTypeModelService.GetAllModelProductMappingsAsync(modelId: modelProduct.Id, pageSize: 10);
            model.ProductModelId = model.ProductModelId;
            model.HasMoreProducts = modelProducts.TotalCount > 1;

            var makes = await _makeTypeModelService.GetAllMakeProductsAsync();
            foreach (var make in makes)
                model.AvailableMakes.Add(new SelectListItem() { Text = make.Name, Value = make.Name });

            //insert this default item at first
            model.AvailableMakes.Insert(0, new SelectListItem { Text = await _localizationService.GetResourceAsync("Admin.Common.Select"), Value = string.Empty });

            var types = await _makeTypeModelService.GetAllTypeProductsAsync();
            foreach (var type in types)
                model.AvailableTypes.Add(new SelectListItem() { Text = type.Name, Value = type.Name });

            //insert this default item at first
            model.AvailableTypes.Insert(0, new SelectListItem { Text = await _localizationService.GetResourceAsync("Admin.Common.Select"), Value = string.Empty });

            //if we got this far, something failed, redisplay form
            return View("~/Plugins/Widgets.MakeTypeModel/Areas/Admin/Views/MakeTypeModel/ProductModelEditPopup.cshtml", model);
        }

        #endregion

        #region Granit Import

        public async Task<IActionResult> GranitProductImportList()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS) || !await _permissionService.AuthorizeAsync(StandardPermission.Catalog.PRODUCTS_IMPORT_EXPORT))
                return AccessDeniedView();

            if (await _workContext.GetCurrentVendorAsync() != null && !_vendorSettings.AllowVendorsToImportProducts)
                //a vendor can not import products
                return AccessDeniedView();

            var model = new GranitProductImportSearchModel();

            //prepare available product statuses and types
            await PrepareGranitProductImportStatusesAsync(model.AvailableImportStatuses);

            //prepare page parameters
            model.SetGridPageSize();

            return View("~/Plugins/Widgets.MakeTypeModel/Areas/Admin/Views/GraniteProduct/GraniteProductImportList.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> GranitProductImportList(GranitProductImportSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS) || !await _permissionService.AuthorizeAsync(StandardPermission.Catalog.PRODUCTS_IMPORT_EXPORT))
                return await AccessDeniedJsonAsync();

            var model = await _makeTypeModelFactory.PrepareGranitProductImportListModelAsync(searchModel);

            return Json(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> GranitProductImportAdd(IFormFile importexcelfile, GranitProductImportModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
                return AccessDeniedView();

            if (await _workContext.GetCurrentVendorAsync() != null && !_vendorSettings.AllowVendorsToImportProducts)
                //a vendor can not import products
                return AccessDeniedView();

            try
            {
                if (importexcelfile != null && importexcelfile.Length > 0)
                {
                    if (!_granitProductImportService.IsImportFileExists())
                    {
                        //insert record on import queue
                        var productImport = new GranitProductImport
                        {
                            FileName = importexcelfile.FileName,
                            ImportStatusId = (int)GranitProductImportStatusEnum.Pending,
                            DataRowNumber = 0,
                            CreatedOnUtc = DateTime.UtcNow
                        };
                        await _granitProductImportService.InsertGranitProductImportAsync(productImport);

                        //save file on disk
                        var path = _nopFileProvider.Combine(_nopFileProvider.MapPath("~/wwwroot/files/granitproductimport"));
                        if (!Directory.Exists(path))
                            Directory.CreateDirectory(path);

                        var filePath = _nopFileProvider.Combine(path, importexcelfile.FileName);
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            importexcelfile.CopyTo(fileStream);
                        }

                        _makeTypeModelSettings.ImportedNewFile = true;
                        await _settingService.SaveSettingAsync(_makeTypeModelSettings);

                        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugins.Widgets.MakeTypeModel.GranitProductImport.Imported"));
                        return RedirectToAction("GranitProductImportList");
                    }

                    _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Plugins.Widgets.MakeTypeModel.GranitProductImport.ImportedExist"));
                    return RedirectToAction("GranitProductImportList");
                }
                else
                {
                    _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Common.UploadFile"));
                    return RedirectToAction("GranitProductImportList");
                }
            }
            catch (Exception exc)
            {
                await _notificationService.ErrorNotificationAsync(exc);
                return RedirectToAction("GranitProductImportList");
            }
        }


        [HttpPost]
        public virtual async Task<IActionResult> GranitProductImportDelete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS) || !await _permissionService.AuthorizeAsync(StandardPermission.Catalog.PRODUCTS_IMPORT_EXPORT))
                return AccessDeniedView();

            //try to get a make product with the specified id
            var granitImport = await _granitProductImportService.GetGranitProductImportByIdAsync(id) ?? throw new ArgumentException("No file import found with the specified id", nameof(id));

            await _granitProductImportService.DeleteGranitProductImportAsync(granitImport);

            _makeTypeModelSettings.ImportedNewFile = false;

            await _settingService.SaveSettingAsync(_makeTypeModelSettings);

            var tempDataRecords = await _granitProductImportService.GetAllTempDataRecordsAsync();
            if (tempDataRecords.Count > 0)
                await _granitProductImportService.DeleteDataRecordsFromTempAsync(tempDataRecords);

            var tempAttrRecords = await _granitProductImportService.GetAllGranitAttrImportRecordsAsync();
            if (tempAttrRecords.Count > 0)
                await _granitProductImportService.DeleteAttrRecordsFromTempAsync(tempAttrRecords);
             
            return new NullJsonResult();
        }

        #endregion

        #region Price Import

        public async Task<IActionResult> PriceImportsList()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS) || !await _permissionService.AuthorizeAsync(StandardPermission.Catalog.PRODUCTS_IMPORT_EXPORT))
                return AccessDeniedView();

            if (await _workContext.GetCurrentVendorAsync() != null && !_vendorSettings.AllowVendorsToImportProducts)
                //a vendor can not import products
                return AccessDeniedView();

            var model = new PriceImportSearchModel();
            //prepare available vendors
            await _baseAdminModelFactory.PrepareVendorsAsync(model.AvailableVendors);

            //prepare available product statuses and types
            await PrepareProductImportStatusesAsync(model.AvailableImportStatuses);

            // prepare price import range
            PreparePriceImportRangeAsync(model.priceRangeSearchModel);

            //prepare page parameters
            model.SetGridPageSize();

            return View("~/Plugins/Widgets.MakeTypeModel/Areas/Admin/Views/Price/PriceImportList.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> PriceImportList(PriceImportSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS) || !await _permissionService.AuthorizeAsync(StandardPermission.Catalog.PRODUCTS_IMPORT_EXPORT))
                return await AccessDeniedJsonAsync();

            var model = await _makeTypeModelFactory.PreparePriceImportsListModelAsync(searchModel);
            return Json(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> PriceImportRange(PriceRangeSearchModel searchModel)
        {
            var model = await _makeTypeModelFactory.PreparePriceImportRangeModelAsync(searchModel);

            return Json(model);
        }

        public async Task<IActionResult> PriceImport()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS) || !await _permissionService.AuthorizeAsync(StandardPermission.Catalog.PRODUCTS_IMPORT_EXPORT))
                return AccessDeniedView();

            if (await _workContext.GetCurrentVendorAsync() != null && !_vendorSettings.AllowVendorsToImportProducts)
                return AccessDeniedView();

            var model = new PriceImportModel();
            ViewData["nop.DownloadEditor.DisableUrl"] = true;
            //prepare available vendors
            await _baseAdminModelFactory.PrepareVendorsAsync(model.AvailableVendors);

            model.PriceRanges = new List<PriceRangeJson>();

            return View("~/Plugins/Widgets.MakeTypeModel/Areas/Admin/Views/Price/ImportPriceRange.cshtml", model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> PriceImportAdd(PriceImportModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Catalog.PRODUCTS_IMPORT_EXPORT))
                return AccessDeniedView();

            if (await _workContext.GetCurrentVendorAsync() != null && !_vendorSettings.AllowVendorsToImportProducts)
                //a vendor can not import products
                return AccessDeniedView();

            try
            {
                if (ModelState.IsValid)
                {
                    if (model.DownloadId > 0)
                    {
                        var download = await _downloadService.GetDownloadByIdAsync(model.DownloadId);
                        //insert record on import queue
                        var productImport = new PriceImport
                        {
                            FileName = download != null ? download.Filename : string.Empty,
                            ImportStatusId = (int)ProductImportStatusEnum.Pending,
                            RowNumber = 2,
                            CreatedOnUtc = DateTime.UtcNow,
                            VendorId = model.VendorId,
                            PriceRange = JsonConvert.SerializeObject(model.PriceRanges),
                            IsMulitplePriceRange = model.IsMulitplePriceRange,
                            DownloadId = model.DownloadId,
                        };

                        await _priceImportService.InsertPriceImportAsync(productImport);
                        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugins.Widgets.MakeTypeModel.PriceImport.Imported"));
                        return RedirectToAction("PriceImportsList");
                    }
                    _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Common.UploadFile"));
                    return RedirectToAction("PriceImportsList");
                }
                //prepare available vendors
                await _baseAdminModelFactory.PrepareVendorsAsync(model.AvailableVendors);

                return View("~/Plugins/Widgets.MakeTypeModel/Areas/Admin/Views/Price/ImportPriceRange.cshtml", model);
            }
            catch (Exception exc)
            {
                await _notificationService.ErrorNotificationAsync(exc);
                return RedirectToAction("PriceImportsList");
            }
        }

        public virtual async Task<IActionResult> UpdatePriceImport(int id)
        {
            if (id <= 0)
                return RedirectToAction("PriceImportList");

            var price = await _priceImportService.GetPriceImportByIdAsync(id);
            if (price == null)
                return RedirectToAction("PriceImportList"); 

            var download = await _downloadService.GetDownloadByIdAsync(price.DownloadId);
            var priceImportModel = new PriceImportModel
            {
                Id = price.Id,
                PriceRanges = JsonConvert.DeserializeObject<List<PriceRangeJson>>(price.PriceRange),
                VendorId = price.VendorId,
                FileName = download != null ? download.Filename : string.Empty,
                RowNumber = price.RowNumber,
                ImportStatus = await _localizationService.GetLocalizedEnumAsync(price.ImportStatus),
                IsMulitplePriceRange = price.IsMulitplePriceRange,
                DownloadId = price.DownloadId
            };
            ViewData["nop.DownloadEditor.DisableUrl"] = true;

            var vendor = await _vendorService.GetVendorByIdAsync(price.VendorId);
            priceImportModel.VendorName = vendor != null ? vendor.Name : string.Empty;

            //prepare available vendors
            await _baseAdminModelFactory.PrepareVendorsAsync(priceImportModel.AvailableVendors);
            return View("~/Plugins/Widgets.MakeTypeModel/Areas/Admin/Views/Price/UpdatePriceImport.cshtml", priceImportModel);
        }

        [HttpPost]
        public virtual async Task<IActionResult> PriceImportUpdate(PriceImportModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Catalog.PRODUCTS_IMPORT_EXPORT))
                return AccessDeniedView();

            if (await _workContext.GetCurrentVendorAsync() != null && !_vendorSettings.AllowVendorsToImportProducts)
                return AccessDeniedView();

            try
            {
                if (ModelState.IsValid)
                {
                    if (model.DownloadId > 0)
                    {
                        var download = await _downloadService.GetDownloadByIdAsync(model.DownloadId);
                        var productImport = new PriceImport
                        {
                            Id = model.Id,
                            FileName = download != null ? download.Filename : string.Empty,
                            ImportStatusId = (int)ProductImportStatusEnum.Processing,
                            RowNumber = 2,
                            CreatedOnUtc = DateTime.UtcNow,
                            VendorId = model.VendorId,
                            PriceRange = JsonConvert.SerializeObject(model.PriceRanges),
                            IsMulitplePriceRange = model.IsMulitplePriceRange,
                            DownloadId = model.DownloadId,
                        };
                        await _priceImportService.UpdatePriceImportAsync(productImport);
                        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugins.Widgets.MakeTypeModel.PriceImport.Imported"));
                        return RedirectToAction("PriceImportsList");
                    }
                    _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Common.UploadFile"));
                    return RedirectToAction("PriceImportsList");
                }

                await _baseAdminModelFactory.PrepareVendorsAsync(model.AvailableVendors);
                return View("~/Plugins/Widgets.MakeTypeModel/Areas/Admin/Views/Price/UpdatePriceImport.cshtml", model);
            }
            catch (Exception exc)
            {
                await _notificationService.ErrorNotificationAsync(exc);
                return RedirectToAction("PriceImportsList");
            }
        }

        [HttpPost]
        public virtual async Task<IActionResult> PriceImportDelete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS) || !await _permissionService.AuthorizeAsync(StandardPermission.Catalog.PRODUCTS_IMPORT_EXPORT))
                return AccessDeniedView();

            //try to get a make product with the specified id
            var priceImport = await _priceImportService.GetPriceImportByIdAsync(id) ?? throw new ArgumentException("No file import found with the specified id", nameof(id));

            var download = await _downloadService.GetDownloadByIdAsync(priceImport.DownloadId);

            if (download != null)
                await _downloadService.DeleteDownloadAsync(download);

            await _priceImportService.DeletePriceImportAsync(priceImport);

            return new NullJsonResult();
        }
        #endregion

        #endregion
    }
}
