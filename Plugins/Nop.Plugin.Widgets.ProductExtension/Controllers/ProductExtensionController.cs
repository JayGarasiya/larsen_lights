using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Primitives;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Cms;
using Nop.Core.Domain.Vendors;
using Nop.Plugin.Widgets.ProductExtension.Domain;
using Nop.Plugin.Widgets.ProductExtension.Factories;
using Nop.Plugin.Widgets.ProductExtension.Models;
using Nop.Plugin.Widgets.ProductExtension.Services;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Widgets.ProductExtension.Controllers
{
    public class ProductExtensionController : BaseAdminController
    {
        #region Fields

        protected readonly ILocalizationService _localizationService;
        protected readonly INotificationService _notificationService;
        protected readonly IPermissionService _permissionService;
        protected readonly ISettingService _settingService;
        protected readonly IStoreContext _storeContext;
        protected readonly IProductExtendService _productExtendService;
        protected readonly IWorkContext _workContext;
        protected readonly VendorSettings _vendorSettings;
        protected readonly IProductAttributeService _productAttributeService;
        protected readonly ICustomerService _customerService;
        protected readonly IProductExtensionFactory _productExtensionFactory;

        #endregion

        #region Ctor

        public ProductExtensionController(ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            ISettingService settingService,
            IStoreContext storeContext,
            IProductExtendService productExtendService,
            IWorkContext workContext,
            VendorSettings vendorSettings,
            IProductAttributeService productAttributeService,
            ICustomerService customerService,
            IProductExtensionFactory productExtensionFactory)
        {
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _settingService = settingService;
            _storeContext = storeContext;
            _productExtendService = productExtendService;
            _workContext = workContext;
            _vendorSettings = vendorSettings;
            _productAttributeService = productAttributeService;
            _customerService = customerService;
            _productExtensionFactory = productExtensionFactory;
        }

        #endregion

        #region Methods

        #region Configuration

        /// <returns>A task that represents the asynchronous operation</returns>

        [CheckPermission(StandardPermission.Configuration.MANAGE_WIDGETS)]
        public async Task<IActionResult> Configure()
        {
            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var widgetSettings = await _settingService.LoadSettingAsync<WidgetSettings>(storeId);
            var settings = await _settingService.LoadSettingAsync<ProductExtensionSettings>(storeId);

            var model = new ConfigurationModel
            {
                Enabled = widgetSettings.ActiveWidgetSystemNames.Contains(ProductExtensionDefaults.SystemName),
                DealerRoleIds = settings.DealerRoleIds,
                ActiveStoreScopeConfiguration = storeId
            };

            if (storeId > 0)
            {
                model.Enabled_OverrideForStore = await _settingService.SettingExistsAsync(widgetSettings, setting => setting.ActiveWidgetSystemNames, storeId);
                model.DealerRoleIds_OverrideForStore = await _settingService.SettingExistsAsync(settings, setting => setting.DealerRoleIds, storeId);
            }

            //prepare available customer roles
            var availableRoles = await _customerService.GetAllCustomerRolesAsync(showHidden: true);
            model.AvailableCustomerRoles = availableRoles.Select(role => new SelectListItem
            {
                Text = role.Name,
                Value = role.Id.ToString(),
                Selected = model.DealerRoleIds.Any(t => t.Equals(role.Id))
            }).ToList();

            return View("~/Plugins/Widgets.ProductExtension/Views/Configure.cshtml", model);
        }

        [HttpPost]
        /// <returns>A task that represents the asynchronous operation</returns>

        [CheckPermission(StandardPermission.Configuration.MANAGE_WIDGETS)]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return await Configure();

            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var widgetSettings = await _settingService.LoadSettingAsync<WidgetSettings>(storeId);
            var settings = await _settingService.LoadSettingAsync<ProductExtensionSettings>(storeId);

            settings.DealerRoleIds = model.DealerRoleIds.ToList();
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, setting => setting.DealerRoleIds, model.DealerRoleIds_OverrideForStore, storeId, false);

            if (model.Enabled && !widgetSettings.ActiveWidgetSystemNames.Contains(ProductExtensionDefaults.SystemName))
                widgetSettings.ActiveWidgetSystemNames.Add(ProductExtensionDefaults.SystemName);
            if (!model.Enabled && widgetSettings.ActiveWidgetSystemNames.Contains(ProductExtensionDefaults.SystemName))
                widgetSettings.ActiveWidgetSystemNames.Remove(ProductExtensionDefaults.SystemName);
            await _settingService.SaveSettingOverridablePerStoreAsync(widgetSettings, setting => setting.ActiveWidgetSystemNames, model.Enabled_OverrideForStore, storeId, false);

            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

            return await Configure();
        }

        #endregion

        #region Product Notes 

        /// <returns>A task that represents the asynchronous operation</returns>

        [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
        public virtual IActionResult ProductNotes()
        {
            //prepare model
            var model =  _productExtensionFactory.PrepareProductNotesSearchModelAsync(new ProductNoteSearchModel());

            return View("~/Plugins/Widgets.ProductExtension/Views/ProductNote/ProductNotes.cshtml", model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> ProductNotes(ProductNoteSearchModel searchModel)
        {
            var model = await _productExtensionFactory.PrepareProductNotesListModelAsync(searchModel);

            return Json(model);
        }

        /// <returns>A task that represents the asynchronous operation</returns>

        [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> CreateProductNote()
        {
            //prepare model
            var model = await _productExtensionFactory.PrepareProductNoteModelAsync(new ProductNoteModel(), null);

            return View("~/Plugins/Widgets.ProductExtension/Views/ProductNote/CreateProductNote.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")] 
        [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> CreateProductNote(ProductNoteModel model, bool continueEditing)
        {
            if (ModelState.IsValid)
            {
                var productNote = new ProductNote()
                {
                    Name = model.Name,
                    Description = model.Description,
                    WidgetZone = model.WidgetZone,
                    Published = model.Published,
                    DisplayOrder = model.DisplayOrder,
                    ShowDisplayName = model.ShowDisplayName
                };
                await _productExtendService.InsertProductNoteAsync(productNote);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugins.Widgets.ProductExtension.ProductNote.Added"));

                return continueEditing ? RedirectToAction("EditProductNote", new { id = productNote.Id }) : RedirectToAction("ProductNotes");
            }

            //prepare model
            model = await _productExtensionFactory.PrepareProductNoteModelAsync(model, null);

            //if we got this far, something failed, redisplay form
            return View("~/Plugins/Widgets.ProductExtension/Views/ProductNote/CreateProductNote.cshtml", model);
        }

        /// <returns>A task that represents the asynchronous operation</returns>

        [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> EditProductNote(int id, bool showtour = false)
        {
            //try to get a product note with the specified id
            var productNote = await _productExtendService.GetProductNoteByIdAsync(id);
            if (productNote == null)
                return RedirectToAction("ProductNotes");

            //prepare model
            var model = await _productExtensionFactory.PrepareProductNoteModelAsync(null, productNote);

            return View("~/Plugins/Widgets.ProductExtension/Views/ProductNote/EditProductNote.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> EditProductNote(ProductNoteModel model, bool continueEditing)
        {
            //try to get a product note with the specified id
            var productNote = await _productExtendService.GetProductNoteByIdAsync(model.Id);
            if (productNote == null)
                return RedirectToAction("ProductNotes");

            if (ModelState.IsValid)
            {
                productNote.Name = model.Name;
                productNote.Description = model.Description;
                productNote.WidgetZone = model.WidgetZone;
                productNote.Published = model.Published;
                productNote.DisplayOrder = model.DisplayOrder;
                productNote.ShowDisplayName = model.ShowDisplayName;

                await _productExtendService.UpdateProductNoteAsync(productNote);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugins.Widgets.ProductExtension.ProductNote.Updated"));

                return continueEditing ? RedirectToAction("EditProductNote", new { id = productNote.Id }) : RedirectToAction("ProductNotes");
            }

            //prepare model
            model = await _productExtensionFactory.PrepareProductNoteModelAsync(model, productNote);

            //if we got this far, something failed, redisplay form
            return View("~/Plugins/Widgets.ProductExtension/Views/ProductNote/EditProductNote.cshtml", model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Configuration.MANAGE_STORES)]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> Delete(int id)
        {
            //try to get a product note with the specified id
            var productNote = await _productExtendService.GetProductNoteByIdAsync(id);
            if (productNote == null)
                return RedirectToAction("ProductNotes");

            try
            {
                await _productExtendService.DeleteProductNoteAsync(productNote);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugins.Widgets.ProductExtension.ProductNote.Deleted"));

                return RedirectToAction("ProductNotes");
            }
            catch (Exception exc)
            {
                await _notificationService.ErrorNotificationAsync(exc);
                return RedirectToAction("EditProductNote", new { id = productNote.Id });
            }
        }

        #endregion

        #region Import Product Price & Stock

        [HttpPost]
        [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> ImportPriceExcel(IFormFile importexcelfile)
        {
            //a vendor can not import products
            if (await _workContext.GetCurrentVendorAsync() != null && !_vendorSettings.AllowVendorsToImportProducts)
                return AccessDeniedView();

            try
            {
                if (importexcelfile != null && importexcelfile.Length > 0)
                {
                    await _productExtendService.ImportProductPriceFromXlsxAsync(importexcelfile.OpenReadStream());
                    _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugins.Widgets.ProductExtension.ProductPrice.Imported"));
                }
                else
                    _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Common.UploadFile"));
            }
            catch (Exception exc)
            {
                await _notificationService.ErrorNotificationAsync(exc);
            }

            return RedirectToAction("List", "Product");
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> ImportStockExcel(IFormFile importexcelfile)
        {
            //a vendor can not import products
            if (await _workContext.GetCurrentVendorAsync() != null && !_vendorSettings.AllowVendorsToImportProducts)
                return AccessDeniedView();

            try
            {
                if (importexcelfile != null && importexcelfile.Length > 0)
                {
                    await _productExtendService.ImportProductStockFromXlsxAsync(importexcelfile.OpenReadStream());
                    _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugins.Widgets.ProductExtension.ProductStock.Imported"));
                }
                else
                    _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Common.UploadFile"));
            }
            catch (Exception exc)
            {
                await _notificationService.ErrorNotificationAsync(exc);
            }

            return RedirectToAction("List", "Product");
        }

        #endregion

        #region Product Attribute Value Condition

        [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
        public async Task<IActionResult> ProductAttributeValueConditions(int id)
        {
            //try to get a product attribute value with the specified id
            var productAttributeValue = await _productAttributeService.GetProductAttributeValueByIdAsync(id);
            if (productAttributeValue == null)
                return Json(new { success = false });

            //try to get a product attribute mapping with the specified id
            var attribute = await _productAttributeService.GetProductAttributeMappingByIdAsync(productAttributeValue.ProductAttributeMappingId);
            if (attribute == null)
                return Json(new { success = false });

            if (attribute.AttributeControlType != AttributeControlType.Checkboxes)
                return Json(new { success = false });

            var model = new ProductAttributeValueConditionModel
            {
                Id = productAttributeValue.Id,
                Name = productAttributeValue.Name,
                ProductAttributeMappingId = attribute.Id,
                TextPrompt = attribute.TextPrompt
            };

            if (attribute.ShouldHaveValues())
            {
                //values
                var attributeValues = await _productAttributeService.GetProductAttributeValuesAsync(attribute.Id);
                var pavConditions = await _productExtendService.GetAllProductAttributeValueConditionByValueIdAsync(productAttributeValue.Id);
                foreach (var attributeValue in attributeValues)
                {
                    if (attributeValue.Id == productAttributeValue.Id)
                        continue;

                    var attributeValueModel = new ProductAttributeValueConditionModel.ProductAttributeValueModel
                    {
                        Id = attributeValue.Id,
                        Name = attributeValue.Name,
                        IsPreSelected = pavConditions.Any(pavc => pavc.ProductAttributeValueId2 == attributeValue.Id)
                    };
                    model.Values.Add(attributeValueModel);
                }
            }

            var updateservicesectionhtml = await RenderPartialViewToStringAsync("~/Plugins/Widgets.ProductExtension/Areas/Admin/Views/Product/_CreateOrUpdateProductAttributeMapping.ValuesCondition.cshtml", model);

            return Json(new { success = true, updateservicesectionhtml });
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
        public async Task<IActionResult> ProductAttributeValueConditions(int id, IFormCollection form)
        {
            //try to get a product attribute value with the specified id
            var productAttributeValue = await _productAttributeService.GetProductAttributeValueByIdAsync(id);
            if (productAttributeValue == null)
                return RedirectToAction("List", "Product");

            //try to get a product attribute mapping with the specified id
            var attribute = await _productAttributeService.GetProductAttributeMappingByIdAsync(productAttributeValue.ProductAttributeMappingId);
            if (attribute == null)
                return RedirectToAction("List", "Product");

            var existingConditions = await _productExtendService.GetAllProductAttributeValueConditionByValueIdAsync(productAttributeValue.Id);
            var selectedAttributeIds = new List<int>();
            var controlId = $"{NopCatalogDefaults.ProductAttributePrefix}{attribute.Id}";
            switch (attribute.AttributeControlType)
            {
                case AttributeControlType.Checkboxes:
                    var cblAttributes = form[controlId];
                    if (!StringValues.IsNullOrEmpty(cblAttributes))
                    {
                        foreach (var item in cblAttributes.ToString()
                            .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            var selectedAttributeId = int.Parse(item);
                            if (selectedAttributeId <= 0)
                                continue;

                            //insert new condition if any 
                            if (!existingConditions.Any(ec => ec.ProductAttributeValueId1 == productAttributeValue.Id && ec.ProductAttributeValueId2 == selectedAttributeId))
                                await _productExtendService.InsertProductAttributeValueConditionAsync(new ProductAttributeValueCondition { ProductAttributeValueId1 = productAttributeValue.Id, ProductAttributeValueId2 = selectedAttributeId });
                            
                            selectedAttributeIds.Add(selectedAttributeId);
                        }
                    }
                    break;
                case AttributeControlType.DropdownList:
                case AttributeControlType.RadioList:
                case AttributeControlType.ColorSquares:
                case AttributeControlType.ImageSquares:
                case AttributeControlType.ReadonlyCheckboxes:
                case AttributeControlType.TextBox:
                case AttributeControlType.MultilineTextbox:
                case AttributeControlType.Datepicker:
                case AttributeControlType.FileUpload:
                default:
                    //these attribute types are not supported as conditions
                    break;
            }

            //remove all other that are not selected
            var removeAttributes = existingConditions.Where(eavc => !selectedAttributeIds.Contains(eavc.ProductAttributeValueId2)).ToList();
            removeAttributes.ForEach(async sa => await _productExtendService.DeleteProductAttributeValueConditionAsync(sa));

            //select an appropriate card
            SaveSelectedCardName("product-attribute-mapping-values");
            return RedirectToAction("ProductAttributeMappingEdit", "Product", new { id = attribute.Id });
        }

        #endregion

        #endregion
    }
}