using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Vendors;
using Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Factories;
using Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Models.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Controllers
{
    public class CustomProductController : BaseAdminController
    {
        #region Fields

        protected readonly IWorkContext _workContext;
        protected readonly IProductService _productService;
        protected readonly IProductTagService _productTagService;
        protected readonly IUrlRecordService _urlRecordService;
        protected readonly ILocalizedEntityService _localizedEntityService;
        protected readonly ICategoryService _categoryService;
        protected readonly IManufacturerService _manufacturerService;
        protected readonly IStoreMappingService _storeMappingService;
        protected readonly IDiscountService _discountService;
        protected readonly IPictureService _pictureService;
        protected readonly IWarehouseService _warehouseService;
        protected readonly INotificationService _notificationService;
        protected readonly ICustomerActivityService _customerActivityService;
        protected readonly IStoreContext _storeContext;
        protected readonly IBackInStockSubscriptionService _backInStockSubscriptionService;
        protected readonly ICustomProductModelFactory _customProductModelFactory;
        protected readonly CurrencySettings _currencySettings;
        protected readonly LocalizationSettings _localizationSettings;
        protected readonly TaxSettings _taxSettings;
        protected readonly VendorSettings _vendorSettings;
        protected readonly IDownloadService _downloadService;
        protected readonly ILocalizationService _localizationService;

        #endregion

        #region Ctor
        public CustomProductController(
            IWorkContext workContext,
            IProductService productService,
            IProductTagService productTagService,
            IUrlRecordService urlRecordService,
            ILocalizedEntityService localizedEntityService,
            ICategoryService categoryService,
            IManufacturerService manufacturerService,
            IStoreMappingService storeMappingService,
            IDiscountService discountService,
            IPictureService pictureService,
            IWarehouseService warehouseService,
            INotificationService notificationService,
            ICustomerActivityService customerActivityService,
            IStoreContext storeContext,
            IBackInStockSubscriptionService backInStockSubscriptionService,
            ICustomProductModelFactory customProductModelFactory,
            CurrencySettings currencySettings,
            LocalizationSettings localizationSettings,
            TaxSettings taxSettings,
            VendorSettings vendorSettings,
            IDownloadService downloadService,
            ILocalizationService localizationService)
        {
            _workContext = workContext;
            _productService = productService;
            _productTagService = productTagService;
            _urlRecordService = urlRecordService;
            _localizedEntityService = localizedEntityService;
            _categoryService = categoryService;
            _manufacturerService = manufacturerService;
            _storeMappingService = storeMappingService;
            _discountService = discountService;
            _pictureService = pictureService;
            _warehouseService = warehouseService;
            _notificationService = notificationService;
            _customerActivityService = customerActivityService;
            _storeContext = storeContext;
            _backInStockSubscriptionService = backInStockSubscriptionService;
            _customProductModelFactory = customProductModelFactory;
            _currencySettings = currencySettings;
            _localizationSettings = localizationSettings;
            _taxSettings = taxSettings;
            _vendorSettings = vendorSettings;
            _downloadService = downloadService;
            _localizationService = localizationService;
        }
        #endregion

        #region Utilities

        protected virtual async Task UpdateLocalesAsync(Product product, ProductModel model)
        {
            foreach (var localized in model.Locales)
            {
                await _localizedEntityService.SaveLocalizedValueAsync(product,
                    x => x.Name,
                    localized.Name,
                    localized.LanguageId);
                await _localizedEntityService.SaveLocalizedValueAsync(product,
                    x => x.ShortDescription,
                    localized.ShortDescription,
                    localized.LanguageId);
                await _localizedEntityService.SaveLocalizedValueAsync(product,
                    x => x.FullDescription,
                    localized.FullDescription,
                    localized.LanguageId);
                await _localizedEntityService.SaveLocalizedValueAsync(product,
                    x => x.MetaKeywords,
                    localized.MetaKeywords,
                    localized.LanguageId);
                await _localizedEntityService.SaveLocalizedValueAsync(product,
                    x => x.MetaDescription,
                    localized.MetaDescription,
                    localized.LanguageId);
                await _localizedEntityService.SaveLocalizedValueAsync(product,
                    x => x.MetaTitle,
                    localized.MetaTitle,
                    localized.LanguageId);

                //search engine name
                var seName = await _urlRecordService.ValidateSeNameAsync(product, localized.SeName, localized.Name, false);
                await _urlRecordService.SaveSlugAsync(product, seName, localized.LanguageId);
            }
        }

        protected virtual async Task UpdatePictureSeoNamesAsync(Product product)
        {
            foreach (var pp in await _productService.GetProductPicturesByProductIdAsync(product.Id))
                await _pictureService.SetSeoFilenameAsync(pp.PictureId, await _pictureService.GetPictureSeNameAsync(product.Name));
        }

        protected virtual async Task SaveCategoryMappingsAsync(Product product, ProductModel model)
        {
            var existingProductCategories = await _categoryService.GetProductCategoriesByProductIdAsync(product.Id, true);

            //delete categories
            var productCategoriesToDelete = existingProductCategories.Where(pc => !model.SelectedCategoryIds.Contains(pc.CategoryId)).ToList();
            await _categoryService.DeleteProductCategoriesAsync(productCategoriesToDelete);

            //add categories
            foreach (var categoryId in model.SelectedCategoryIds)
            {
                var category = await _categoryService.GetCategoryByIdAsync(categoryId);
                if (category is null)
                    continue;

                if (!await _categoryService.CanVendorAddProductsAsync(category))
                    continue;

                if (_categoryService.FindProductCategory(existingProductCategories, product.Id, categoryId) == null)
                {
                    //find next display order
                    var displayOrder = 1;
                    var existingCategoryMapping = await _categoryService.GetProductCategoriesByCategoryIdAsync(categoryId, showHidden: true);
                    if (existingCategoryMapping.Any())
                        displayOrder = existingCategoryMapping.Max(x => x.DisplayOrder) + 1;
                    await _categoryService.InsertProductCategoryAsync(new ProductCategory
                    {
                        ProductId = product.Id,
                        CategoryId = categoryId,
                        DisplayOrder = displayOrder
                    });
                }
            }
        }

        protected virtual async Task SaveManufacturerMappingsAsync(Product product, ProductModel model)
        {
            var existingProductManufacturers = await _manufacturerService.GetProductManufacturersByProductIdAsync(product.Id, true);

            //delete manufacturers
            var productManufacturersToDelete = existingProductManufacturers.Where(pm => !model.SelectedManufacturerIds.Contains(pm.ManufacturerId)).ToList();
            await _manufacturerService.DeleteProductManufacturersAsync(productManufacturersToDelete);

            //add manufacturers
            foreach (var manufacturerId in model.SelectedManufacturerIds)
            {
                if (_manufacturerService.FindProductManufacturer(existingProductManufacturers, product.Id, manufacturerId) == null)
                {
                    //find next display order
                    var displayOrder = 1;
                    var existingManufacturerMapping = await _manufacturerService.GetProductManufacturersByManufacturerIdAsync(manufacturerId, showHidden: true);
                    if (existingManufacturerMapping.Any())
                        displayOrder = existingManufacturerMapping.Max(x => x.DisplayOrder) + 1;
                    await _manufacturerService.InsertProductManufacturerAsync(new ProductManufacturer
                    {
                        ProductId = product.Id,
                        ManufacturerId = manufacturerId,
                        DisplayOrder = displayOrder
                    });
                }
            }
        }

        protected virtual async Task SaveDiscountMappingsAsync(Product product, ProductModel model)
        {
            var allDiscounts = await _discountService.GetAllDiscountsAsync(DiscountType.AssignedToSkus, showHidden: true, isActive: null);

            foreach (var discount in allDiscounts)
            {
                if (model.SelectedDiscountIds != null && model.SelectedDiscountIds.Contains(discount.Id))
                {
                    //new discount
                    if (await _productService.GetDiscountAppliedToProductAsync(product.Id, discount.Id) is null)
                        await _productService.InsertDiscountProductMappingAsync(new DiscountProductMapping { EntityId = product.Id, DiscountId = discount.Id });
                }
                else
                {
                    //remove discount
                    if (await _productService.GetDiscountAppliedToProductAsync(product.Id, discount.Id) is DiscountProductMapping discountProductMapping)
                        await _productService.DeleteDiscountProductMappingAsync(discountProductMapping);
                }
            }

            await _productService.UpdateProductAsync(product);
        }

        protected virtual async Task SaveProductWarehouseInventoryAsync(Product product, ProductModel model)
        {
            ArgumentNullException.ThrowIfNull(product);

            if (model.ManageInventoryMethodId != (int)ManageInventoryMethod.ManageStock)
                return;

            if (!model.UseMultipleWarehouses)
                return;

            var warehouses = await _warehouseService.GetAllWarehousesAsync();

            var form = await Request.ReadFormAsync();
            var formData = form.ToDictionary(x => x.Key, x => x.Value.ToString());

            foreach (var warehouse in warehouses)
            {
                //parse stock quantity
                var stockQuantity = 0;
                foreach (var formKey in formData.Keys)
                {
                    if (!formKey.Equals($"warehouse_qty_{warehouse.Id}", StringComparison.InvariantCultureIgnoreCase))
                        continue;

                    _ = int.TryParse(formData[formKey], out stockQuantity);
                    break;
                }

                //parse reserved quantity
                var reservedQuantity = 0;
                foreach (var formKey in formData.Keys)
                    if (formKey.Equals($"warehouse_reserved_{warehouse.Id}", StringComparison.InvariantCultureIgnoreCase))
                    {
                        _ = int.TryParse(formData[formKey], out reservedQuantity);
                        break;
                    }

                //parse "used" field
                var used = false;
                foreach (var formKey in formData.Keys)
                    if (formKey.Equals($"warehouse_used_{warehouse.Id}", StringComparison.InvariantCultureIgnoreCase))
                    {
                        _ = int.TryParse(formData[formKey], out var tmp);
                        used = tmp == warehouse.Id;
                        break;
                    }

                //quantity change history message
                var message = $"{await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.MultipleWarehouses")} {await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.Edit")}";

                var existingPwI = (await _productService.GetAllProductWarehouseInventoryRecordsAsync(product.Id)).FirstOrDefault(x => x.WarehouseId == warehouse.Id);
                if (existingPwI != null)
                {
                    if (used)
                    {
                        var previousStockQuantity = existingPwI.StockQuantity;

                        //update existing record
                        existingPwI.StockQuantity = stockQuantity;
                        existingPwI.ReservedQuantity = reservedQuantity;
                        await _productService.UpdateProductWarehouseInventoryAsync(existingPwI);

                        //quantity change history
                        await _productService.AddStockQuantityHistoryEntryAsync(product, existingPwI.StockQuantity - previousStockQuantity, existingPwI.StockQuantity,
                            existingPwI.WarehouseId, message);
                    }
                    else
                    {
                        //delete. no need to store record for qty 0
                        await _productService.DeleteProductWarehouseInventoryAsync(existingPwI);

                        //quantity change history
                        await _productService.AddStockQuantityHistoryEntryAsync(product, -existingPwI.StockQuantity, 0, existingPwI.WarehouseId, message);
                    }
                }
                else
                {
                    if (!used)
                        continue;

                    //no need to insert a record for qty 0
                    existingPwI = new ProductWarehouseInventory
                    {
                        WarehouseId = warehouse.Id,
                        ProductId = product.Id,
                        StockQuantity = stockQuantity,
                        ReservedQuantity = reservedQuantity
                    };

                    await _productService.InsertProductWarehouseInventoryAsync(existingPwI);

                    //quantity change history
                    await _productService.AddStockQuantityHistoryEntryAsync(product, existingPwI.StockQuantity, existingPwI.StockQuantity,
                        existingPwI.WarehouseId, message);
                }
            }
        }

        protected virtual string[] ParseProductTags(string productTags)
        {
            var result = new List<string>();
            if (string.IsNullOrWhiteSpace(productTags))
                return result.ToArray();

            var values = productTags.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var val in values)
                if (!string.IsNullOrEmpty(val.Trim()))
                    result.Add(val.Trim());

            return result.ToArray();
        }

        #endregion

        #region Methods

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> Create(OverrideProductModel model, bool continueEditing)
        {
            //validate maximum number of products per vendor
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (_vendorSettings.MaximumProductNumber > 0 &&
                currentVendor != null &&
                await _productService.GetNumberOfProductsByVendorIdAsync(currentVendor.Id) >= _vendorSettings.MaximumProductNumber)
            {
                _notificationService.ErrorNotification(string.Format(await _localizationService.GetResourceAsync("Admin.Catalog.Products.ExceededMaximumNumber"),
                    _vendorSettings.MaximumProductNumber));
                return RedirectToAction("List");
            }

            if (ModelState.IsValid)
            {
                //a vendor should have access only to his products
                if (currentVendor != null)
                    model.VendorId = currentVendor.Id;

                //vendors cannot edit "Show on home page" property
                if (currentVendor != null && model.ShowOnHomepage)
                    model.ShowOnHomepage = false;

                //product
                var product = model.ToEntity<Product>();
                product.CreatedOnUtc = DateTime.UtcNow;
                product.UpdatedOnUtc = DateTime.UtcNow;
                await _productService.InsertProductAsync(product);

                //search engine name
                model.SeName = await _urlRecordService.ValidateSeNameAsync(product, model.SeName, product.Name, true);
                await _urlRecordService.SaveSlugAsync(product, model.SeName, 0);

                //locales
                await UpdateLocalesAsync(product, model);

                //categories
                await SaveCategoryMappingsAsync(product, model);

                //manufacturers
                await SaveManufacturerMappingsAsync(product, model);

                //stores
                await _storeMappingService.SaveStoreMappingsAsync(product, model.SelectedStoreIds);

                //discounts
                await SaveDiscountMappingsAsync(product, model);

                //tags
                await _productTagService.UpdateProductTagsAsync(product, ParseProductTags(model.ProductTags));

                //warehouses
                await SaveProductWarehouseInventoryAsync(product, model);

                //quantity change history
                await _productService.AddStockQuantityHistoryEntryAsync(product, product.StockQuantity, product.StockQuantity, product.WarehouseId,
                    await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.Edit"));

                //activity log
                await _customerActivityService.InsertActivityAsync("AddNewProduct",
                    string.Format(await _localizationService.GetResourceAsync("ActivityLog.AddNewProduct"), product.Name), product);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Products.Added"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = product.Id });
            }

            //prepare model
            model = await _customProductModelFactory.PrepareCustomProductModelAsync(model, null, true);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> Edit(OverrideProductModel model, bool continueEditing)
        {
            //try to get a product with the specified id
            var product = await _productService.GetProductByIdAsync(model.Id);
            if (product == null || product.Deleted)
                return RedirectToAction("List", "Product");

            //a vendor should have access only to his products
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null && product.VendorId != currentVendor.Id)
                return RedirectToAction("List", "Product");

            //check if the product quantity has been changed while we were editing the product
            //and if it has been changed then we show error notification
            //and redirect on the editing page without data saving
            if (product.StockQuantity != model.LastStockQuantity)
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Products.Fields.StockQuantity.ChangedWarning"));
                return RedirectToAction("Edit", "Product", new { id = product.Id });
            }

            if (ModelState.IsValid)
            {
                //a vendor should have access only to his products
                if (currentVendor != null)
                    model.VendorId = currentVendor.Id;

                //we do not validate maximum number of products per vendor when editing existing products (only during creation of new products)
                //vendors cannot edit "Show on home page" property
                if (currentVendor != null && model.ShowOnHomepage != product.ShowOnHomepage)
                    model.ShowOnHomepage = product.ShowOnHomepage;

                //some previously used values
                var prevTotalStockQuantity = await _productService.GetTotalStockQuantityAsync(product);
                var prevDownloadId = product.DownloadId;
                var prevSampleDownloadId = product.SampleDownloadId;
                var previousStockQuantity = product.StockQuantity;
                var previousWarehouseId = product.WarehouseId;
                var previousProductType = product.ProductType;

                //product
                product = model.ToEntity(product);

                product.UpdatedOnUtc = DateTime.UtcNow;
                await _productService.UpdateProductAsync(product);

                //remove associated products
                if (previousProductType == ProductType.GroupedProduct && product.ProductType == ProductType.SimpleProduct)
                {
                    var store = await _storeContext.GetCurrentStoreAsync();
                    var storeId = store?.Id ?? 0;
                    var vendorId = currentVendor?.Id ?? 0;

                    var associatedProducts = await _productService.GetAssociatedProductsAsync(product.Id, storeId, vendorId);
                    foreach (var associatedProduct in associatedProducts)
                    {
                        associatedProduct.ParentGroupedProductId = 0;
                        await _productService.UpdateProductAsync(associatedProduct);
                    }
                }

                //search engine name
                model.SeName = await _urlRecordService.ValidateSeNameAsync(product, model.SeName, product.Name, true);
                await _urlRecordService.SaveSlugAsync(product, model.SeName, 0);

                //locales
                await UpdateLocalesAsync(product, model);

                //tags
                await _productTagService.UpdateProductTagsAsync(product, ParseProductTags(model.ProductTags));
                
                //warehouses
                await SaveProductWarehouseInventoryAsync(product, model);

                //categories
                await SaveCategoryMappingsAsync(product, model);

                //manufacturers
                await SaveManufacturerMappingsAsync(product, model);

                //stores
                await _storeMappingService.SaveStoreMappingsAsync(product, model.SelectedStoreIds);

                //discounts
                await SaveDiscountMappingsAsync(product, model);

                //picture seo names
                await UpdatePictureSeoNamesAsync(product);

                //back in stock notifications
                if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStock &&
                    product.BackorderMode == BackorderMode.NoBackorders &&
                    product.AllowBackInStockSubscriptions &&
                    await _productService.GetTotalStockQuantityAsync(product) > 0 &&
                    prevTotalStockQuantity <= 0 &&
                    product.Published &&
                    !product.Deleted)
                {
                    await _backInStockSubscriptionService.SendNotificationsToSubscribersAsync(product);
                }

                //delete an old "download" file (if deleted or updated)
                if (prevDownloadId > 0 && prevDownloadId != product.DownloadId)
                {
                    var prevDownload = await _downloadService.GetDownloadByIdAsync(prevDownloadId);
                    if (prevDownload != null)
                        await _downloadService.DeleteDownloadAsync(prevDownload);
                }

                //delete an old "sample download" file (if deleted or updated)
                if (prevSampleDownloadId > 0 && prevSampleDownloadId != product.SampleDownloadId)
                {
                    var prevSampleDownload = await _downloadService.GetDownloadByIdAsync(prevSampleDownloadId);
                    if (prevSampleDownload != null)
                        await _downloadService.DeleteDownloadAsync(prevSampleDownload);
                }

                //quantity change history
                if (previousWarehouseId != product.WarehouseId)
                {
                    //warehouse is changed 
                    //compose a message
                    var oldWarehouseMessage = string.Empty;
                    if (previousWarehouseId > 0)
                    {
                        var oldWarehouse = await _warehouseService.GetWarehouseByIdAsync(previousWarehouseId);
                        if (oldWarehouse != null)
                            oldWarehouseMessage = string.Format(await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.EditWarehouse.Old"), oldWarehouse.Name);
                    }

                    var newWarehouseMessage = string.Empty;
                    if (product.WarehouseId > 0)
                    {
                        var newWarehouse = await _warehouseService.GetWarehouseByIdAsync(product.WarehouseId);
                        if (newWarehouse != null)
                            newWarehouseMessage = string.Format(await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.EditWarehouse.New"), newWarehouse.Name);
                    }

                    var message = string.Format(await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.EditWarehouse"), oldWarehouseMessage, newWarehouseMessage);

                    //record history
                    await _productService.AddStockQuantityHistoryEntryAsync(product, -previousStockQuantity, 0, previousWarehouseId, message);
                    await _productService.AddStockQuantityHistoryEntryAsync(product, product.StockQuantity, product.StockQuantity, product.WarehouseId, message);
                }
                else
                {
                    await _productService.AddStockQuantityHistoryEntryAsync(product, product.StockQuantity - previousStockQuantity, product.StockQuantity,
                        product.WarehouseId, await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.Edit"));
                }

                //activity log
                await _customerActivityService.InsertActivityAsync("EditProduct",
                    string.Format(await _localizationService.GetResourceAsync("ActivityLog.EditProduct"), product.Name), product);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Products.Updated"));

                if (!continueEditing)
                    return RedirectToAction("List", "Product");

                return RedirectToAction("Edit", "Product", new { id = product.Id });

            }

            //prepare model
            model = (OverrideProductModel) await _customProductModelFactory.PrepareCustomProductModelAsync(model, product, true);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        #endregion
    }
}
