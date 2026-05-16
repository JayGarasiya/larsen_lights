using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Misc.Inventory.Order.Domain;
using Nop.Plugin.Misc.Inventory.Order.Factories;
using Nop.Plugin.Misc.Inventory.Order.Models;
using Nop.Plugin.Misc.Inventory.Order.Services;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Misc.Inventory.Order.Controllers
{
    public class InventoryOrderController : BaseAdminController
    {
        #region Fields
        protected readonly INotificationService _notificationService;
        protected readonly IProductService _productService;
        protected readonly IManufacturerService _manufacturerService;
        protected readonly IInventoryOrderFactory _inventoryOrderFactory;
        protected readonly IPoOrderService _poOrderService;
        protected readonly IPictureService _pictureService;
        protected readonly ILocalizationService _localizationService;
        protected readonly IPermissionService _permissionService;
        protected readonly ISettingService _settingService;
        protected readonly IStoreContext _storeContext;
        protected readonly IBaseAdminModelFactory _baseAdminModelFactory;
        protected readonly IWorkContext _workContext;
        #endregion

        #region Ctor
        public InventoryOrderController(INotificationService notificationService,
            IProductService productService,
            IManufacturerService manufacturerService,
            IInventoryOrderFactory inventoryOrderFactory,
            IPoOrderService poOrderService,
            IPictureService pictureService,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            ISettingService settingService,
            IStoreContext storeContext,
            IBaseAdminModelFactory baseAdminModelFactory,
            IWorkContext workContext)
        {
            _notificationService = notificationService;
            _productService = productService;
            _manufacturerService = manufacturerService;
            _inventoryOrderFactory = inventoryOrderFactory;
            _poOrderService = poOrderService;
            _pictureService = pictureService;
            _localizationService = localizationService;
            _permissionService = permissionService;
            _settingService = settingService;
            _storeContext = storeContext;
            _baseAdminModelFactory = baseAdminModelFactory;
            _workContext = workContext;
        }
        #endregion

        #region Methods

        #region Configure
        [CheckPermission(StandardPermission.Catalog.PRODUCTS_VIEW)]
        public async Task<IActionResult> Configure()
        {
            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var inventoryOrderSettings = await _settingService.LoadSettingAsync<InventoryOrderSettings>(storeId);

            if (inventoryOrderSettings.AllowedManufacturers == null)
                inventoryOrderSettings.AllowedManufacturers = new List<int>();

            var model = new ConfigurationModel
            {
                AllowedManufacturers = inventoryOrderSettings.AllowedManufacturers,
            };

            await _baseAdminModelFactory.PrepareManufacturersAsync(model.AvailableManufacturers, false);
            return View("~/Plugins/Inventory.Order/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Catalog.PRODUCTS_VIEW)]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return await Configure();

            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var inventoryOrderSettings = await _settingService.LoadSettingAsync<InventoryOrderSettings>(storeId);

            inventoryOrderSettings.AllowedManufacturers = model.AllowedManufacturers.ToList();
            await _settingService.SaveSettingAsync(inventoryOrderSettings);

            await _settingService.ClearCacheAsync();
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));
            return await Configure();
        }
        #endregion

        #region Po Order
        [HttpGet]
        public async Task<IActionResult> List()
        {
            var model = await _inventoryOrderFactory.PrepareManageInventoryListModel();
            return View("~/Plugins/Inventory.Order/Views/List.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> List(ManageInventorySearchModel searchModel)
        {
            var model = await _inventoryOrderFactory.PrepareManageInventorySearchModel(searchModel);
            return Json(model);
        }

        public virtual async Task<IActionResult> CreatePoOrderPopup(int poOrderId = 0)
        {
            var model = new PoOrderModel();

            if (poOrderId > 0)
            {
                var poOrder = await _poOrderService.GetPoOrderById(poOrderId);
                if (poOrder != null)
                {
                    model.Id = poOrder.Id;
                    model.PONumber = poOrder.PONumber;
                }
            }

            return Json(new { html = await RenderPartialViewToStringAsync("~/Plugins/Inventory.Order/Views/PoOrderComment.cshtml", model) });
        }

        [HttpPost]
        public virtual async Task<IActionResult> CreatePoOrderPopup(PoOrderModel model)
        {
            try
            {
                var poorder = new PoOrder
                {
                    Comment = model.Comment,
                    PercentAdjusted = model.PercentAdjusted,
                    CreatedOnUTC = DateTime.UtcNow,
                    TotalVolume = model.TotalVolume,
                    AvailableDateOnUTC = model.AvailableDateOnUTC
                };
                await _poOrderService.InsertPoOrder(poorder);

                poorder.PONumber = $"P{string.Format("{0:D7}", poorder.Id)}";
                await _poOrderService.UpdatePoOrder(poorder);

                var selectedIds = model.SelectedIds.ToList();
                var selectedCartoons = model.SelectedCartoon.ToList();
                for (int i = 0; i < selectedIds.Count; i++)
                {
                    var splitIds = selectedIds[i].Split(",");
                    if (splitIds.Length > 0)
                    {
                        var poOrderItem = new PoOrderItem();
                        var productDemension = new ProductDimensions();
                        if (!string.IsNullOrEmpty(splitIds[0]))
                            poOrderItem.ManufacturerId = int.Parse(splitIds[0]);
                        if (!string.IsNullOrEmpty(splitIds[1]))
                        {
                            poOrderItem.ProductId = int.Parse(splitIds[1]);
                            productDemension = await _poOrderService.GetProductDimensionsByProductId(int.Parse(splitIds[1]));
                        }
                        if (!string.IsNullOrEmpty(splitIds[2]))
                            poOrderItem.OrderedQty = productDemension == null ? int.Parse(splitIds[2]) : productDemension.QtyCartoon * selectedCartoons[i];

                        poOrderItem.PoOrderId = poorder.Id;
                        await _poOrderService.InsertPoOrderItem(poOrderItem);
                    }
                }
                return Json(new { Result = true });
            }
            catch (Exception)
            {
                return Json(new { Result = false });
            }
        }

        [HttpPost]
        public virtual async Task<IActionResult> DeletePoOrder(int id)
        {
            await _poOrderService.DeletePoOrder(id);
            return new NullJsonResult();
        }
        #endregion

        #region Po Prder Item
        [HttpGet]
        public async Task<IActionResult> PoOrderItemAdd(int id)
        {
            var model = await _inventoryOrderFactory.PrepareManageInventoryListModel();

            var poOrder = await _poOrderService.GetPoOrderById(id);
            model.PoOrderId = poOrder.Id;
            model.PONumber = poOrder.PONumber;

            return View("~/Plugins/Inventory.Order/Views/List.cshtml", model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> AddSelectedPoOrderItems(PoOrderModel model)
        {
            try
            {
                var poOrder = await _poOrderService.GetPoOrderById(model.Id);
                if (poOrder != null)
                {
                    poOrder.Comment = model.Comment;
                    poOrder.TotalVolume = poOrder.TotalVolume + model.TotalVolume;
                    await _poOrderService.UpdatePoOrder(poOrder);

                    var selectedIds = model.SelectedIds.ToList();
                    var selectedCartoons = model.SelectedCartoon.ToList();
                    for (int i = 0; i < selectedIds.Count; i++)
                    {
                        var splitIds = selectedIds[i].Split(",");
                        if (splitIds.Length > 0)
                        {
                            if (!string.IsNullOrEmpty(splitIds[1]))
                            {
                                var productId = int.Parse(splitIds[1]);
                                var productDemension = await _poOrderService.GetProductDimensionsByProductId(productId);
                                var existingItem = await _poOrderService.GetPoOrderItemByPoOrderIdAndProductId(poOrder.Id, productId);
                                if (existingItem == null)
                                {
                                    var poOrderItem = new PoOrderItem()
                                    {
                                        PoOrderId = poOrder.Id,
                                        ProductId = productId
                                    };
                                    if (!string.IsNullOrEmpty(splitIds[0]))
                                        poOrderItem.ManufacturerId = int.Parse(splitIds[0]);
                                    if (!string.IsNullOrEmpty(splitIds[2]))
                                        poOrderItem.OrderedQty = productDemension == null ? int.Parse(splitIds[2]) : productDemension.QtyCartoon * selectedCartoons[i];

                                    await _poOrderService.InsertPoOrderItem(poOrderItem);
                                }
                                else
                                {
                                    if (!string.IsNullOrEmpty(splitIds[2]))
                                        existingItem.OrderedQty = productDemension == null ? int.Parse(splitIds[2]) : productDemension.QtyCartoon * selectedCartoons[i];

                                    await _poOrderService.UpdatePoOrderitem(existingItem);
                                }
                            }
                        }
                    }

                    _notificationService.SuccessNotification($"PO Order items successfully added in PO Order - {poOrder.PONumber}.");
                    return Json(new { Result = true });
                }
            }
            catch (Exception ex)
            {
                await _notificationService.ErrorNotificationAsync(ex);
                return Json(new { Result = false });
            }

            _notificationService.ErrorNotification("Unable to retrive PO Order.");
            return Json(new { Result = false });
        }

        public virtual async Task<IActionResult> EditPoOrderPopup(int id)
        {
            var poOrderDetails = await _poOrderService.GetPoOrderById(id);

            var model = new PoOrderModel();

            model.AvailableDateOnUTC = poOrderDetails.AvailableDateOnUTC;
            model.Comment = poOrderDetails.Comment;

            return View("~/Plugins/Inventory.Order/Views/PoOrderDetailsEditPopup.cshtml", model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> EditPoOrderPopup(PoOrderModel model)
        {
            var poOrderDetails = await _poOrderService.GetPoOrderById(model.Id);
            poOrderDetails.AvailableDateOnUTC = model.AvailableDateOnUTC;
            poOrderDetails.Comment = model.Comment;

            await _poOrderService.UpdatePoOrder(poOrderDetails);

            ViewBag.RefreshPage = true;

            return View("~/Plugins/Inventory.Order/Views/PoOrderDetailsEditPopup.cshtml", model);
        }

        [HttpGet]
        public async Task<IActionResult> ManagePoOrder()
        {
            var model = await _inventoryOrderFactory.PreparePoOrderSearchModel(new PoOrderSearchModel());
            return View("~/Plugins/Inventory.Order/Views/ManagePoOrderItems.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> ManagePoOrder(PoOrderSearchModel searchModel)
        {
            var model = await _inventoryOrderFactory.PreparePoOrderListModel(searchModel);
            return Json(model);
        }

        [HttpPost]
        public async Task<IActionResult> PoOrderItemByPoNumber(PoOrderItemSearchModel searchModel)
        {
            var model = await _inventoryOrderFactory.PreparePoOrderItemByPoNumber(searchModel);
            return Json(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> PoOrderUpdate(PoOrderModel model)
        {
            var poOrder = await _poOrderService.GetPoOrderById(model.Id)
                ?? throw new ArgumentException("No poorder found with the specified id");

            try
            {
                poOrder.HasReceived = !model.HasReceived;
                poOrder.ReceivedOnUTC = !model.HasReceived ? DateTime.UtcNow : null;

                await _poOrderService.UpdatePoOrder(poOrder);
                return Json(new { Result = true });
            }
            catch (Exception ex)
            {
                await _notificationService.ErrorNotificationAsync(ex);
                return Json(new { Result = false });
            }
        }

        [HttpPost]
        public virtual async Task<IActionResult> PoOrderAdminCommentUpdate(PoOrderModel model)
        {
            var poOrder = await _poOrderService.GetPoOrderById(model.Id)
                ?? throw new ArgumentException("No poorder found with the specified id");

            try
            {
                if (!string.IsNullOrWhiteSpace(poOrder.Comment))
                    poOrder.Comment = model.Comment;

                poOrder.AvailableDateOnUTC = model.AvailableDateOnUTC;
                await _poOrderService.UpdatePoOrder(poOrder);
                return Json(new { Result = true });
            }
            catch (Exception ex)
            {
                await _notificationService.ErrorNotificationAsync(ex);
                return Json(new { Result = false });
            }
        }

        public virtual async Task<IActionResult> PoOrderItemEditPopup(int id)
        {
            var poOrderItem = await _poOrderService.GetByIdPoOrderItem(id);

            var model = new PoOrderItemModel();
            model.Id = poOrderItem.Id;
            model.OrderedQty = poOrderItem.OrderedQty;

            return View("~/Plugins/Inventory.Order/Views/PoOrderItemEditPopup.cshtml", model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> PoOrderItemEditPopup(PoOrderItemModel model)
        {
            var poOrderItem = await _poOrderService.GetByIdPoOrderItem(model.Id);
            poOrderItem.OrderedQty = model.OrderedQty;

            await _poOrderService.UpdatePoOrderitem(poOrderItem);

            ViewBag.RefreshPage = true;

            return View("~/Plugins/Inventory.Order/Views/PoOrderItemEditPopup.cshtml", model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> PoOrderItemDelete(int id)
        {
            var poOrderItem = await _poOrderService.GetByIdPoOrderItem(id)
               ?? throw new ArgumentException("No poorder found with the specified id");

            await _poOrderService.DeletePoOrderitem(poOrderItem);

            return new NullJsonResult();
        }
        #endregion

        #region PDF Invoice
        [HttpPost]
        [FormValueRequired("pdf-invoice-all")]
        public virtual async Task<IActionResult> PdfPoInvoiceAll(PoOrderSearchModel searchModel)
        {
            var poOrders = await _poOrderService.GetAllPoOrder(poNumber: searchModel.SearchPoNumber,
                adminComment: searchModel.SearchAdminComment,
                startDate: searchModel.StartDate,
                endDate: searchModel.EndDate,
                searchModel.Page - 1, searchModel.PageSize, true);

            //ensure that we at least one order selected
            if (!poOrders.Any())
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Plugins.InventoryOrder.PoOrder.NoPoOrders"));
                return RedirectToAction("ManagePoOrder");
            }

            try
            {
                byte[] bytes;
                await using (var stream = new MemoryStream())
                {
                    await _inventoryOrderFactory.PrintPoOrdersToPdfAsync(stream, poOrders, await _workContext.GetWorkingLanguageAsync());
                    bytes = stream.ToArray();
                }

                return File(bytes, "application/zip", "po-orders.zip");
            }
            catch (Exception exc)
            {
                await _notificationService.ErrorNotificationAsync(exc);
                return RedirectToAction("ManagePoOrder");
            }
        }

        [HttpPost]
        public virtual async Task<IActionResult> PdfPoInvoiceSelected(string selectedIds)
        {
            var orders = new List<PoOrder>();
            if (selectedIds != null)
            {
                var ids = selectedIds.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => Convert.ToInt32(x)).ToArray();
                orders.AddRange(await _poOrderService.GetPoOrderByIdsAsync(ids));
            }

            try
            {
                byte[] bytes;
                if(orders.Count == 1)
                {
                    await using var stream = new MemoryStream();

                    await _inventoryOrderFactory.PrintPoOrderToPdfAsync(stream, orders.FirstOrDefault(), await _workContext.GetWorkingLanguageAsync(), store: await _storeContext.GetCurrentStoreAsync());
                    bytes = stream.ToArray();

                    return File(bytes, MimeTypes.ApplicationPdf, $"{orders.FirstOrDefault().PONumber}.pdf");
                }
                else
                {
                    await using (var stream = new MemoryStream())
                    {
                        await _inventoryOrderFactory.PrintPoOrdersToPdfAsync(stream, orders, await _workContext.GetWorkingLanguageAsync());
                        bytes = stream.ToArray();
                    }

                    return File(bytes, "application/zip", "po-orders.zip");
                }
            }
            catch (Exception exc)
            {
                await _notificationService.ErrorNotificationAsync(exc);
                return RedirectToAction("ManagePoOrder");
            }
        }
        #endregion

        #region Po Order Item Export
        [HttpPost, ActionName("ExporpoorderitemtToExcel")]
        [FormValueRequired("exportpoorderitemexcel-all")]
        public virtual async Task<IActionResult> PoOrderItemExportExcelAll()
        {
            try
            {
                var bytes = await _inventoryOrderFactory.ExportPoOrderItemAllToXlsxAsync();
                return File(bytes, MimeTypes.TextXlsx, $"ALL.xlsx");
            }
            catch (Exception exc)
            {
                await _notificationService.ErrorNotificationAsync(exc);
                return RedirectToAction("ManagePoOrder");
            }
        }

        [HttpPost]
        public virtual async Task<IActionResult> PoOrderItemExportExcelSelected(string selectedIds)
        {
            var ids = new List<int>();
            if (selectedIds != null)
            {
                ids = selectedIds.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => Convert.ToInt32(x)).ToList();
            }
            try
            {
                var bytes = await _inventoryOrderFactory.ExportPoOrderItemToXlsxAsync(ids);
                return File(bytes, MimeTypes.TextXlsx, "PoOrder.xlsx");
            }
            catch (Exception exc)
            {
                await _notificationService.ErrorNotificationAsync(exc);
                return RedirectToAction("ManagePoOrder");
            }
        }
        #endregion

        #region Po Order Export
        [HttpPost]
        public virtual async Task<IActionResult> PoOrderExportExcel(ManageInventorySearchModel searchModel)
        {
            try
            {
                searchModel.SetGridPageSize();
                var bytes = await _inventoryOrderFactory.ExportPoOrderToXlsx(searchModel);
                return File(bytes, MimeTypes.TextXlsx, "PoOrder.xlsx");
            }
            catch (Exception exc)
            {
                await _notificationService.ErrorNotificationAsync(exc);
                return RedirectToAction("List");
            }
        }
        #endregion

        #region create product dimensions
        [HttpPost]
        public virtual async Task<IActionResult> CreateProductDimensions(ProductDimensionsModel model)
        {
            var productDimension = await _poOrderService.GetProductDimensionsByProductId(model.ProductId);
            var isNew = productDimension == null;
            try
            {
                if (isNew)
                {
                    productDimension = model.ToEntity<ProductDimensions>();
                    await _poOrderService.InsertProductDimensions(productDimension);
                }
                else
                {
                    var productDimession = model.ToEntity<ProductDimensions>();
                    productDimession.Id = productDimension.Id;
                    await _poOrderService.UpdateProductDimensions(productDimession);
                }
                return Json(new { Result = true });
            }
            catch (Exception ex)
            {
                await _notificationService.ErrorNotificationAsync(ex);
                return Json(new { Result = false });
            }
        }
        #endregion

        #endregion
    }
}
