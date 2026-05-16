using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Vendors;
using Nop.Core.Events;
using Nop.Core.Infrastructure;
using Nop.Plugin.Widgets.ProductExtension.Areas.Admin.Factories;
using Nop.Plugin.Widgets.ProductExtension.Services;
using Nop.Services.ArtificialIntelligence;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.ExportImport;
using Nop.Services.FilterLevels;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Factories;
using Nop.Web.Framework.Mvc.Filters;
using System.Text;

namespace Nop.Plugin.Widgets.ProductExtension.Areas.Admin.Controllers
{
    public class OverrideProductController : ProductController
    {
        #region Fields

        protected readonly IPriceFormatter _priceFormatter;
        protected readonly IProductExtendService _productExtendService;
        protected readonly ICustomProductModelFactory _customProductModelFactory;
        protected readonly ICustomProductService _customProductService;

        #endregion

        #region Ctor
        public OverrideProductController(AdminAreaSettings adminAreaSettings, 
            CustomerSettings customerSettings, 
            IAclService aclService, 
            IArtificialIntelligenceService artificialIntelligenceService, 
            IBackInStockSubscriptionService backInStockSubscriptionService, 
            IBaseAdminModelFactory baseAdminModelFactory, 
            ICategoryService categoryService, 
            ICopyProductService copyProductService, 
            ICurrencyService currencyService, 
            ICustomerActivityService customerActivityService, 
            IDiscountService discountService, 
            IDownloadService downloadService, 
            IEventPublisher eventPublisher, 
            IExportManager exportManager, 
            IFilterLevelValueModelFactory filterLevelValueModelFactory, 
            IFilterLevelValueService filterLevelValueService, 
            IHttpClientFactory httpClientFactory, 
            IImportManager importManager, 
            ILanguageService languageService, 
            ILocalizationService localizationService, 
            ILocalizedEntityService localizedEntityService, 
            IManufacturerService manufacturerService, 
            INopFileProvider fileProvider, 
            INotificationService notificationService, 
            IPdfService pdfService, 
            IPermissionService permissionService, 
            IPictureService pictureService, 
            IProductAttributeFormatter productAttributeFormatter, 
            IProductAttributeParser productAttributeParser, 
            IProductAttributeService productAttributeService, 
            IProductModelFactory productModelFactory, 
            IProductService productService, 
            IProductTagService productTagService, 
            ISettingService settingService, 
            IShoppingCartService shoppingCartService, 
            ISpecificationAttributeService specificationAttributeService, 
            IStoreContext storeContext, 
            IStoreMappingService storeMappingService, 
            ITranslationModelFactory translationModelFactory, 
            IUrlRecordService urlRecordService, 
            IVideoService videoService, 
            IWarehouseService warehouseService, 
            IWebHelper webHelper, 
            IWorkContext workContext, 
            CurrencySettings currencySettings, 
            LocalizationSettings localizationSettings, 
            TaxSettings taxSettings, 
            VendorSettings vendorSettings,
            IPriceFormatter priceFormatter,
            IProductExtendService productExtendService,
            ICustomProductModelFactory customProductModelFactory,
            ICustomProductService customProductService) : base(
                adminAreaSettings, 
                customerSettings, 
                aclService, 
                artificialIntelligenceService, 
                backInStockSubscriptionService, 
                baseAdminModelFactory, 
                categoryService, 
                copyProductService, 
                currencyService, 
                customerActivityService, 
                discountService, 
                downloadService, 
                eventPublisher, 
                exportManager, 
                filterLevelValueModelFactory, 
                filterLevelValueService, 
                httpClientFactory, 
                importManager, 
                languageService, 
                localizationService, 
                localizedEntityService, 
                manufacturerService, 
                fileProvider, 
                notificationService, 
                pdfService, 
                permissionService, 
                pictureService, 
                productAttributeFormatter, 
                productAttributeParser, 
                productAttributeService, 
                productModelFactory, 
                productService, 
                productTagService, 
                settingService, 
                shoppingCartService, 
                specificationAttributeService, 
                storeContext, 
                storeMappingService, 
                translationModelFactory, 
                urlRecordService, 
                videoService, 
                warehouseService, 
                webHelper, 
                workContext, 
                currencySettings, 
                localizationSettings, 
                taxSettings, 
                vendorSettings)
        {
            _priceFormatter = priceFormatter;
            _productExtendService = productExtendService;
            _customProductModelFactory = customProductModelFactory;
            _customProductService = customProductService;
        }

        #endregion

        #region Utilities

        /// <returns>A task that represents the asynchronous operation</returns>
        protected override async Task SaveConditionAttributesAsync(ProductAttributeMapping productAttributeMapping,
            ProductAttributeConditionModel model, IFormCollection form)
        {
            string attributesXml = null;
            if (model.EnableCondition)
            {
                var attribute = await _productAttributeService.GetProductAttributeMappingByIdAsync(model.SelectedProductAttributeId);
                if (attribute != null)
                {
                    var controlId = $"{NopCatalogDefaults.ProductAttributePrefix}{attribute.Id}";
                    switch (attribute.AttributeControlType)
                    {
                        case AttributeControlType.DropdownList:
                        case AttributeControlType.ColorSquares:
                        case AttributeControlType.ImageSquares:
                            var ctrlAttributes = form[controlId];
                            if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                            {
                                var selectedAttributeId = int.Parse(ctrlAttributes);
                                //for conditions we should empty values save even when nothing is selected
                                //otherwise "attributesXml" will be empty
                                //hence we won't be able to find a selected attribute
                                attributesXml = _productAttributeParser.AddProductAttribute(null, attribute,
                                    selectedAttributeId > 0 ? selectedAttributeId.ToString() : string.Empty);
                            }
                            else
                            {
                                //for conditions we should empty values save even when nothing is selected
                                //otherwise "attributesXml" will be empty
                                //hence we won't be able to find a selected attribute
                                attributesXml = _productAttributeParser.AddProductAttribute(null,
                                    attribute, string.Empty);
                            }

                            break;
                        case AttributeControlType.RadioList:
                        case AttributeControlType.Checkboxes:
                            var cblAttributes = form[controlId];
                            if (!StringValues.IsNullOrEmpty(cblAttributes))
                            {
                                var anyValueSelected = false;
                                foreach (var item in cblAttributes.ToString()
                                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                                {
                                    var selectedAttributeId = int.Parse(item);
                                    if (selectedAttributeId <= 0)
                                        continue;

                                    attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                        attribute, selectedAttributeId.ToString());
                                    anyValueSelected = true;
                                }

                                if (!anyValueSelected)
                                {
                                    //for conditions we should save empty values even when nothing is selected
                                    //otherwise "attributesXml" will be empty
                                    //hence we won't be able to find a selected attribute
                                    attributesXml = _productAttributeParser.AddProductAttribute(null,
                                        attribute, string.Empty);
                                }
                            }
                            else
                            {
                                //for conditions we should save empty values even when nothing is selected
                                //otherwise "attributesXml" will be empty
                                //hence we won't be able to find a selected attribute
                                attributesXml = _productAttributeParser.AddProductAttribute(null,
                                    attribute, string.Empty);
                            }

                            break;
                        case AttributeControlType.ReadonlyCheckboxes:
                        case AttributeControlType.TextBox:
                        case AttributeControlType.MultilineTextbox:
                        case AttributeControlType.Datepicker:
                        case AttributeControlType.FileUpload:
                        default:
                            //these attribute types are supported as conditions
                            break;
                    }
                }
            }

            productAttributeMapping.ConditionAttributeXml = attributesXml;
            await _productAttributeService.UpdateProductAttributeMappingAsync(productAttributeMapping);
        }

        #endregion

        #region Methods

        #region List/ report summary

        [CheckPermission(StandardPermission.Catalog.PRODUCTS_VIEW)]
        public override async Task<IActionResult> List()
        {
            //prepare model
            var model = await _customProductModelFactory.PrepareProductSearchModelAsync(new Models.ProductSearchModel());

            return View("~/Plugins/Widgets.ProductExtension/Areas/Admin/Views/Product/List.cshtml", model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Catalog.PRODUCTS_VIEW)]
        public virtual async Task<IActionResult> ProductsList(Models.ProductSearchModel searchModel)
        {
            var model = await _customProductModelFactory.PrepareProductListModelAsync(searchModel);

            return Json(model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Catalog.PRODUCTS_VIEW)]
        public virtual async Task<IActionResult> ReportAggregates(Models.ProductSearchModel searchModel)
        {
            //prepare model
            ArgumentNullException.ThrowIfNull(searchModel);

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

            // prepare additional model data
            var reportSummary = await _productExtendService.GetProductAverageReportLine(showHidden: true,
                categoryIds: categoryIds,
                manufacturerIds: new List<int> { searchModel.SearchManufacturerId },
                storeId: searchModel.SearchStoreId,
                vendorId: searchModel.SearchVendorId,
                warehouseId: searchModel.SearchWarehouseId,
                productType: searchModel.SearchProductTypeId > 0 ? (ProductType?)searchModel.SearchProductTypeId : null,
                keywords: searchModel.SearchProductName,
                overridePublished: overridePublished);

            return Json(new
            {
                TotalPrice = await _priceFormatter.FormatPriceAsync(reportSummary.TotalPrice, true, false),
                TotalProductCost = await _priceFormatter.FormatPriceAsync(reportSummary.TotalProductCost, true, false)
            });
        }

        #endregion

        #region Export/ Import


        [HttpPost]
        [FormValueRequired("download-catalog-stock-pdf")]
        [CheckPermission(StandardPermission.Catalog.PRODUCTS_VIEW)]
        public virtual async Task<IActionResult> DownloadCatalogStockAsPdf(Models.ProductSearchModel model)
        {
            //a vendor should have access only to his products
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null)
            {
                model.SearchVendorId = currentVendor.Id;
            }

            var categoryIds = new List<int> { model.SearchCategoryId };
            //include subcategories
            if (model.SearchIncludeSubCategories && model.SearchCategoryId > 0)
                categoryIds.AddRange(await _categoryService.GetChildCategoryIdsAsync(parentCategoryId: model.SearchCategoryId, showHidden: true));

            //0 - all (according to "ShowHidden" parameter)
            //1 - published only
            //2 - unpublished only
            bool? overridePublished = null;
            if (model.SearchPublishedId == 1)
                overridePublished = true;
            else if (model.SearchPublishedId == 2)
                overridePublished = false;

            var products = await _customProductService.SearchProductsAsync(0,
                categoryIds: categoryIds,
                manufacturerIds: new List<int> { model.SearchManufacturerId },
                storeId: model.SearchStoreId,
                vendorId: model.SearchVendorId,
                warehouseId: model.SearchWarehouseId,
                productType: model.SearchProductTypeId > 0 ? (ProductType?)model.SearchProductTypeId : ProductType.SimpleProduct,
                keywords: model.SearchProductName,
                showHidden: true,
                overridePublished: overridePublished,
                noPicture: model.SearchNoPicture,
                fromWeight: model.SearchFromWeight,
                toWeight: model.SearchToWeight);

            try
            {
                byte[] bytes;
                await using (var stream = new MemoryStream())
                {
                    await _productExtendService.PrintProductsToPdfAsync(stream, products);
                    bytes = stream.ToArray();
                }

                return File(bytes, MimeTypes.ApplicationPdf, "pdfcatalogstock.pdf");
            }
            catch (Exception exc)
            {
                await _notificationService.ErrorNotificationAsync(exc);
                return RedirectToAction("List", "Product");
            }
        }

        [HttpPost, ActionName("ExportToXml")]
        [FormValueRequired("exportxml-all")]
        [CheckPermission(StandardPermission.Catalog.PRODUCTS_VIEW)]
        public virtual async Task<IActionResult> ExportXmlAll(Models.ProductSearchModel model)
        {
            //a vendor should have access only to his products
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null)
            {
                model.SearchVendorId = currentVendor.Id;
            }

            var categoryIds = new List<int> { model.SearchCategoryId };
            //include subcategories
            if (model.SearchIncludeSubCategories && model.SearchCategoryId > 0)
                categoryIds.AddRange(await _categoryService.GetChildCategoryIdsAsync(parentCategoryId: model.SearchCategoryId, showHidden: true));

            //0 - all (according to "ShowHidden" parameter)
            //1 - published only
            //2 - unpublished only
            bool? overridePublished = null;
            if (model.SearchPublishedId == 1)
                overridePublished = true;
            else if (model.SearchPublishedId == 2)
                overridePublished = false;

            var products = await _customProductService.SearchProductsAsync(0,
                categoryIds: categoryIds,
                manufacturerIds: new List<int> { model.SearchManufacturerId },
                storeId: model.SearchStoreId,
                vendorId: model.SearchVendorId,
                warehouseId: model.SearchWarehouseId,
                productType: model.SearchProductTypeId > 0 ? (ProductType?)model.SearchProductTypeId : null,
                keywords: model.SearchProductName,
                showHidden: true,
                overridePublished: overridePublished,
                noPicture: model.SearchNoPicture,
                fromWeight: model.SearchFromWeight,
                toWeight: model.SearchToWeight);

            try
            {
                var xml = await _exportManager.ExportProductsToXmlAsync(products);

                return File(Encoding.UTF8.GetBytes(xml), MimeTypes.ApplicationXml, "products.xml");
            }
            catch (Exception exc)
            {
                await _notificationService.ErrorNotificationAsync(exc);
                return RedirectToAction("List");
            }
        }

        [HttpPost, ActionName("CustomExportToExcel")]
        [FormValueRequired("exportexcel-all")]
        [CheckPermission(StandardPermission.Catalog.PRODUCTS_VIEW)]
        public virtual async Task<IActionResult> ExportExcelAll(Models.ProductSearchModel model)
        {
            //a vendor should have access only to his products
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null)
            {
                model.SearchVendorId = currentVendor.Id;
            }

            var categoryIds = new List<int> { model.SearchCategoryId };
            //include subcategories
            if (model.SearchIncludeSubCategories && model.SearchCategoryId > 0)
                categoryIds.AddRange(await _categoryService.GetChildCategoryIdsAsync(parentCategoryId: model.SearchCategoryId, showHidden: true));

            //0 - all (according to "ShowHidden" parameter)
            //1 - published only
            //2 - unpublished only
            bool? overridePublished = null;
            if (model.SearchPublishedId == 1)
                overridePublished = true;
            else if (model.SearchPublishedId == 2)
                overridePublished = false;

            var products = await _customProductService.SearchProductsAsync(0,
                categoryIds: categoryIds,
                manufacturerIds: new List<int> { model.SearchManufacturerId },
                storeId: model.SearchStoreId,
                vendorId: model.SearchVendorId,
                warehouseId: model.SearchWarehouseId,
                productType: model.SearchProductTypeId > 0 ? (ProductType?)model.SearchProductTypeId : null,
                keywords: model.SearchProductName,
                showHidden: true,
                overridePublished: overridePublished,
                noPicture: model.SearchNoPicture,
                fromWeight: model.SearchFromWeight,
                toWeight: model.SearchToWeight);

            try
            {
                var bytes = await _exportManager.ExportProductsToXlsxAsync(products);

                return File(bytes, MimeTypes.TextXlsx, "products.xlsx");
            }
            catch (Exception exc)
            {
                await _notificationService.ErrorNotificationAsync(exc);

                return RedirectToAction("List");
            }
        }

        #endregion

        #endregion
    }
}