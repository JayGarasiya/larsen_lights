using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Core.Infrastructure;
using Nop.Plugin.Widgets.MakeTypeModel.Factories;
using Nop.Plugin.Widgets.MakeTypeModel.Models;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Web.Controllers;
using Nop.Web.Factories;

namespace Nop.Plugin.Widgets.MakeTypeModel.Controllers
{
    [AutoValidateAntiforgeryToken]
    public class OverrideReturnRequestController : ReturnRequestController
    {
        #region Fields

        protected readonly ICustomReturnRequestModelFactory _customReturnRequestModelFactory;
        protected readonly IProductAttributeParser _productAttributeParser;
        protected readonly IGenericAttributeService _genericAttributeService;
        protected readonly IProductAttributeService _productAttributeService;

        #endregion

        #region Ctor

        public OverrideReturnRequestController(ICustomerService customerService,
            ICustomNumberFormatter customNumberFormatter,
            IDownloadService downloadService,
            ILocalizationService localizationService,
            INopFileProvider fileProvider,
            IOrderProcessingService orderProcessingService,
            IOrderService orderService,
            IReturnRequestModelFactory returnRequestModelFactory,
            IReturnRequestService returnRequestService,
            IStoreContext storeContext,
            IWorkContext workContext,
            IWorkflowMessageService workflowMessageService,
            LocalizationSettings localizationSettings,
            OrderSettings orderSettings,
            ICustomReturnRequestModelFactory customReturnRequestModelFactory,
            IProductAttributeParser productAttributeParser,
            IGenericAttributeService genericAttributeService,
            IProductAttributeService productAttributeService)
            : base(customerService,
                customNumberFormatter,
                downloadService,
                localizationService,
                fileProvider,
                orderProcessingService,
                orderService,
                returnRequestModelFactory,
                returnRequestService,
                storeContext,
                workContext,
                workflowMessageService,
                localizationSettings,
                orderSettings)
        {
            _customReturnRequestModelFactory = customReturnRequestModelFactory;
            _productAttributeParser = productAttributeParser;
            _genericAttributeService = genericAttributeService;
            _productAttributeService = productAttributeService;
        }

        #endregion

        #region Method

        public override async Task<IActionResult> ReturnRequest(int orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (order == null || order.Deleted || customer.Id != order.CustomerId)
                return Challenge();

            if (!await _orderProcessingService.IsReturnRequestAllowedAsync(order))
                return RedirectToRoute(Core.Http.NopRouteNames.General.HOMEPAGE);

            var model = new CustomSubmitReturnRequestModel();
            model = await _customReturnRequestModelFactory.PrepareSubmitReturnRequestModelAsync(model, order);
            return View("~/Plugins/Widgets.MakeTypeModel/Views/ReturnRequest/ReturnRequest.cshtml", model);
        }

        [HttpPost, ActionName("CustomReturnRequest")]
        public virtual async Task<IActionResult> CustomReturnRequestSubmit(int orderId, CustomSubmitReturnRequestModel model, IFormCollection form)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (order == null || order.Deleted || customer.Id != order.CustomerId)
                return Challenge();

            if (!await _orderProcessingService.IsReturnRequestAllowedAsync(order))
                return RedirectToRoute(Core.Http.NopRouteNames.General.HOMEPAGE);

            var count = 0;
            var downloadId = 0;

            if (_orderSettings.ReturnRequestsAllowFiles)
            {
                var download = await _downloadService.GetDownloadByGuidAsync(model.UploadedFileGuid);
                if (download != null)
                    downloadId = download.Id;
            }

            var orderItems = await _orderService.GetOrderItemsAsync(order.Id, isNotReturnable: false);

            foreach (var orderItem in orderItems)
            {
                bool isProductSelected = false;
                var quantity = 0;

                foreach (var formKey in form.Keys)
                {
                    if (formKey.Equals($"SelectedProduct_{orderItem.ProductId}", StringComparison.InvariantCultureIgnoreCase))
                    {
                        isProductSelected = true;
                    }

                    if (formKey.Equals($"quantity{orderItem.Id}", StringComparison.InvariantCultureIgnoreCase))
                    {
                        _ = int.TryParse(form[formKey], out quantity);
                        break;
                    }
                }

                if (isProductSelected && quantity > 0)
                {
                    var rrr = await _returnRequestService.GetReturnRequestReasonByIdAsync(model.ReturnRequestReasonId);
                    var rra = await _returnRequestService.GetReturnRequestActionByIdAsync(model.ReturnRequestActionId);
                    var store = await _storeContext.GetCurrentStoreAsync();

                    var rr = new ReturnRequest
                    {
                        CustomNumber = "",
                        StoreId = store.Id,
                        OrderItemId = orderItem.Id,
                        Quantity = quantity,
                        CustomerId = customer.Id,
                        ReasonForReturn = rrr != null ? await _localizationService.GetLocalizedAsync(rrr, x => x.Name) : "not available",
                        RequestedAction = rra != null ? await _localizationService.GetLocalizedAsync(rra, x => x.Name) : "not available",
                        CustomerComments = model.Comments,
                        UploadedFileId = downloadId,
                        StaffNotes = string.Empty,
                        ReturnRequestStatus = ReturnRequestStatus.Pending,
                        CreatedOnUtc = DateTime.UtcNow,
                        UpdatedOnUtc = DateTime.UtcNow
                    };

                    await _returnRequestService.InsertReturnRequestAsync(rr);
                    rr.CustomNumber = _customNumberFormatter.GenerateReturnRequestCustomNumber(rr);
                    await _customerService.UpdateCustomerAsync(customer);
                    await _returnRequestService.UpdateReturnRequestAsync(rr);

                    await _workflowMessageService.SendNewReturnRequestStoreOwnerNotificationAsync(rr, orderItem, order, _localizationSettings.DefaultAdminLanguageId);
                    await _workflowMessageService.SendNewReturnRequestCustomerNotificationAsync(rr, orderItem, order);

                    count++;
                }
                else
                {
                    var attributes = await _productAttributeParser.ParseProductAttributeValuesAsync(orderItem.AttributesXml);
                    foreach (var attr in attributes)
                    {
                        var isKitSelected = false;
                        var selectedKitAttributeId = 0;

                        foreach (var formKey in form.Keys)
                        {
                            if (formKey.Equals($"SelectedAttributes_{attr.Id}", StringComparison.InvariantCultureIgnoreCase))
                            {
                                isKitSelected = true;
                                selectedKitAttributeId = attr.Id;
                            }

                            if (formKey.Equals($"quantity{attr.Id}", StringComparison.InvariantCultureIgnoreCase))
                            {
                                _ = int.TryParse(form[formKey], out quantity);
                                break;
                            }
                        }
                        if (isKitSelected && quantity > 0)
                        {
                            var reason = await _returnRequestService.GetReturnRequestReasonByIdAsync(model.ReturnRequestReasonId);
                            var action = await _returnRequestService.GetReturnRequestActionByIdAsync(model.ReturnRequestActionId);
                            var store = await _storeContext.GetCurrentStoreAsync();

                            string selectedAttributeComment = string.Empty;
                            var productAttributeMapping = await _productAttributeService.GetProductAttributeMappingByIdAsync(attr.ProductAttributeMappingId);
                            if (productAttributeMapping != null)
                            {
                                var productAttribute = await _productAttributeService.GetProductAttributeByIdAsync(productAttributeMapping.ProductAttributeId);
                                if (productAttribute != null)
                                {
                                    var attributeName = await _localizationService.GetLocalizedAsync(productAttribute, x => x.Name);
                                    var attributeValue = attr.Name;
                                    selectedAttributeComment = $"Returned Kit Attribute: {attributeName} - {attributeValue}";
                                }
                            }

                            var rr = new ReturnRequest
                            {
                                CustomNumber = string.Empty,
                                StoreId = store.Id,
                                OrderItemId = orderItem.Id,
                                Quantity = quantity,
                                CustomerId = customer.Id,
                                ReasonForReturn = reason != null ? await _localizationService.GetLocalizedAsync(reason, x => x.Name) : "Not available",
                                RequestedAction = action != null ? await _localizationService.GetLocalizedAsync(action, x => x.Name) : "Not available",
                                CustomerComments = $"{model.Comments} {(string.IsNullOrEmpty(model.Comments) ? "" : " - ")}{selectedAttributeComment}",
                                UploadedFileId = downloadId,
                                StaffNotes = string.Empty,
                                ReturnRequestStatus = ReturnRequestStatus.Pending,
                                CreatedOnUtc = DateTime.UtcNow,
                                UpdatedOnUtc = DateTime.UtcNow
                            };

                            await _returnRequestService.InsertReturnRequestAsync(rr);
                            await _genericAttributeService.SaveAttributeAsync(rr, "ReturnRequest.KitAttributeId", selectedKitAttributeId);

                            rr.CustomNumber = _customNumberFormatter.GenerateReturnRequestCustomNumber(rr);
                            await _returnRequestService.UpdateReturnRequestAsync(rr);
                            await _customerService.UpdateCustomerAsync(customer);

                            await _workflowMessageService.SendNewReturnRequestStoreOwnerNotificationAsync(rr, orderItem, order, _localizationSettings.DefaultAdminLanguageId);
                            await _workflowMessageService.SendNewReturnRequestCustomerNotificationAsync(rr, orderItem, order);

                            count++;
                        }
                    }
                }
            }

            model = await _customReturnRequestModelFactory.PrepareSubmitReturnRequestModelAsync(model, order);
            if (count > 0)
                model.Result = await _localizationService.GetResourceAsync("ReturnRequests.Submitted");
            else
                model.Result = await _localizationService.GetResourceAsync("ReturnRequests.NoItemsSubmitted");

            return View("~/Plugins/Widgets.MakeTypeModel/Views/ReturnRequest/ReturnRequest.cshtml", model);
        }

        #endregion
    }
}
