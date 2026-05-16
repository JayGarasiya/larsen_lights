using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Factories;
using Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Models;
using Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Services;
using Nop.Plugin.Widgets.MakeTypeModel.Domain;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Orders;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Controllers
{
    public class OverrideAdminReturnRequestController : ReturnRequestController
    {
        #region Fields

        protected readonly ICustomAdminReturnRequestModelFactory _customAdminReturnRequestModelFactory;
        protected readonly ICustomReturnRequestService _customReturnRequestService;

        #endregion

        #region Ctor

        public OverrideAdminReturnRequestController(ICustomerActivityService customerActivityService,
            ILocalizationService localizationService,
            ILocalizedEntityService localizedEntityService,
            INotificationService notificationService,
            IOrderService orderService,
            IProductService productService,
            IPermissionService permissionService,
            IReturnRequestModelFactory returnRequestModelFactory,
            IReturnRequestService returnRequestService,
            IWorkflowMessageService workflowMessageService,
            ICustomAdminReturnRequestModelFactory customAdminReturnRequestModelFactory, 
            ICustomReturnRequestService customReturnRequestService)
            : base(customerActivityService,
                localizationService,
                localizedEntityService,
                notificationService,
                orderService,
                productService,
                permissionService,
                returnRequestModelFactory,
                returnRequestService,
                workflowMessageService)
        {
            _customAdminReturnRequestModelFactory = customAdminReturnRequestModelFactory;
            _customReturnRequestService = customReturnRequestService;
        }

        #endregion

        #region Methods

        [CheckPermission(StandardPermission.Orders.RETURN_REQUESTS_VIEW)]
        public override async Task<IActionResult> Edit(int id)
        {
            //try to get a return request with the specified id
            var returnRequest = await _returnRequestService.GetReturnRequestByIdAsync(id);
            if (returnRequest == null)
                return RedirectToAction("List");

            //prepare model
            var model = await _customAdminReturnRequestModelFactory.PrepareReturnRequestModelAsync(null, returnRequest);

            return View("~/Plugins/Widgets.MakeTypeModel/Areas/Admin/Views/ReturnRequests/Edit.cshtml", model);
        }
   
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        [CheckPermission(StandardPermission.Orders.RETURN_REQUESTS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> RequestEdit(OverrideReturnRequestModel model, bool continueEditing)
        {
            //try to get a return request with the specified id
            var returnRequest = await _returnRequestService.GetReturnRequestByIdAsync(model.Id);
            if (returnRequest == null)
                return RedirectToAction("List", "ReturnRequest");

            if (ModelState.IsValid)
            {
                var quantityToReturn = model.ReturnedQuantity - returnRequest.ReturnedQuantity;
                if (quantityToReturn < 0)
                    _notificationService.ErrorNotification(string.Format(await _localizationService.GetResourceAsync("Admin.ReturnRequests.Fields.ReturnedQuantity.CannotBeLessThanQuantityAlreadyReturned"), returnRequest.ReturnedQuantity));
                else
                {
                    if (quantityToReturn > 0)
                    {
                        var orderItem = await _orderService.GetOrderItemByIdAsync(returnRequest.OrderItemId);
                        if (orderItem != null)
                        {
                            var product = await _productService.GetProductByIdAsync(orderItem.ProductId);
                            if (product != null)
                            {
                                var productStockChangedMessage = string.Format(await _localizationService.GetResourceAsync("Admin.ReturnRequests.QuantityReturnedToStock"), quantityToReturn);

                                await _productService.AdjustInventoryAsync(product, quantityToReturn, orderItem.AttributesXml, productStockChangedMessage);

                                _notificationService.SuccessNotification(productStockChangedMessage);
                            }
                        }
                    }

                    returnRequest = model.ToEntity(returnRequest);
                    returnRequest.UpdatedOnUtc = DateTime.UtcNow;

                    await _returnRequestService.UpdateReturnRequestAsync(returnRequest);

                    var note = await _customReturnRequestService.GetReturnRequestNoteByIdAsync(returnRequest.Id);
                    if(note != null)
                    {
                        note.RetuenRequestId = returnRequest.Id;
                        note.Notes = model.Note;
                        await _customReturnRequestService.UpdateReturnRequestNoteAsync(note);
                    }
                    else
                    {
                        var returnRequestNote = new RetuenRequestNote();
                        returnRequestNote.Notes = model.Note;
                        returnRequestNote.RetuenRequestId = returnRequest.Id;
                        await _customReturnRequestService.InsertReturnRequestNoteAsync(returnRequestNote);
                    }

                    //activity log
                    await _customerActivityService.InsertActivityAsync("EditReturnRequest",
                        string.Format(await _localizationService.GetResourceAsync("ActivityLog.EditReturnRequest"), returnRequest.Id), returnRequest);

                    _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.ReturnRequests.Updated"));

                    return continueEditing ? RedirectToAction("Edit", new { id = returnRequest.Id }) : RedirectToAction("List", "ReturnRequest");
                }
            }

            //prepare model
            model = await _customAdminReturnRequestModelFactory.PrepareReturnRequestModelAsync(model, returnRequest, true);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost, ActionName("RequestEdit")]
        [FormValueRequired("notify-customer")]
        [CheckPermission(StandardPermission.Orders.RETURN_REQUESTS_CREATE_EDIT_DELETE)]
        public override async Task<IActionResult> NotifyCustomer(ReturnRequestModel model)
        {
            //try to get a return request with the specified id
            var returnRequest = await _returnRequestService.GetReturnRequestByIdAsync(model.Id);
            if (returnRequest == null)
                return RedirectToAction("List");

            var orderItem = await _orderService.GetOrderItemByIdAsync(returnRequest.OrderItemId);
            if (orderItem is null)
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.ReturnRequests.OrderItemDeleted"));
                return RedirectToAction("Edit", new { id = returnRequest.Id });
            }

            var order = await _orderService.GetOrderByIdAsync(orderItem.OrderId);

            var queuedEmailIds = await _workflowMessageService.SendReturnRequestStatusChangedCustomerNotificationAsync(returnRequest, orderItem, order);
            if (queuedEmailIds.Any())
                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.ReturnRequests.Notified"));

            return RedirectToAction("Edit", new { id = returnRequest.Id });
        }

        #endregion
    }
}
