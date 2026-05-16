using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Models;
using Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Services;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Framework.Factories;

namespace Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Factories
{
    /// <summary>
    /// Represent custom admin retune request model factory
    /// </summary>
    public partial class CustomAdminReturnRequestModelFactory : ICustomAdminReturnRequestModelFactory
    {
        #region Fields

        protected readonly IDateTimeHelper _dateTimeHelper;
        protected readonly IDownloadService _downloadService;
        protected readonly ICustomerService _customerService;
        protected readonly ILocalizationService _localizationService;
        protected readonly IOrderService _orderService;
        protected readonly IProductService _productService;
        protected readonly IGenericAttributeService _genericAttributeService;
        protected readonly IProductAttributeParser _productAttributeParser;
        protected readonly IProductAttributeService _productAttributeService;
        protected readonly ICustomReturnRequestService _customReturnRequestService;

        #endregion

        #region Ctor

        public CustomAdminReturnRequestModelFactory(IBaseAdminModelFactory baseAdminModelFactory,
            IDateTimeHelper dateTimeHelper,
            IDownloadService downloadService,
            ICustomerService customerService,
            ILocalizationService localizationService,
            ILocalizedModelFactory localizedModelFactory,
            IOrderService orderService,
            IProductService productService,
            IReturnRequestService returnRequestService,
            IGenericAttributeService genericAttributeService,
            IProductAttributeParser productAttributeParser,
            IProductAttributeService productAttributeService,
            ICustomReturnRequestService customReturnRequestService)
        {
            _dateTimeHelper = dateTimeHelper;
            _downloadService = downloadService;
            _customerService = customerService;
            _localizationService = localizationService;
            _orderService = orderService;
            _productService = productService;
            _genericAttributeService = genericAttributeService;
            _productAttributeParser = productAttributeParser;
            _productAttributeService = productAttributeService;
            _customReturnRequestService = customReturnRequestService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Prepare return request model
        /// </summary>
        /// <param name="model">Return request model</param>
        /// <param name="returnRequest">Return request</param>
        /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the return request model
        /// </returns>
        public virtual async Task<OverrideReturnRequestModel> PrepareReturnRequestModelAsync(OverrideReturnRequestModel model,
                ReturnRequest returnRequest, bool excludeProperties = false)
        {
            if (returnRequest == null)
                return model;

            if (model == null)
            {
                model = new OverrideReturnRequestModel
                {
                    Id = returnRequest.Id,
                    ReasonForReturn = returnRequest.ReasonForReturn,
                    RequestedAction = returnRequest.RequestedAction,
                    CustomerComments = returnRequest.CustomerComments,
                    StaffNotes = returnRequest.StaffNotes,
                    ReturnRequestStatusId = returnRequest.ReturnRequestStatusId,
                    Quantity = returnRequest.Quantity,
                    ReturnedQuantity = returnRequest.ReturnedQuantity,
                    CustomNumber = returnRequest.CustomNumber,
                };
            }

            var customer = await _customerService.GetCustomerByIdAsync(returnRequest.CustomerId);
            model.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(returnRequest.CreatedOnUtc, DateTimeKind.Utc);
            model.CustomerInfo = await _customerService.IsRegisteredAsync(customer)
                ? customer.Email
                : await _localizationService.GetResourceAsync("Admin.Customers.Guest");
            model.UploadedFileGuid = (await _downloadService.GetDownloadByIdAsync(returnRequest.UploadedFileId))?.DownloadGuid ?? Guid.Empty;
            model.ReturnRequestStatusStr = await _localizationService.GetLocalizedEnumAsync(returnRequest.ReturnRequestStatus);

            var orderItem = await _orderService.GetOrderItemByIdAsync(returnRequest.OrderItemId);
            var genericAttribute = (await _genericAttributeService.GetAttributesForEntityAsync(returnRequest.Id, "ReturnRequest")).FirstOrDefault();

            var matchingAttributes = new List<ProductAttributeValue>();

            if (genericAttribute != null && orderItem != null)
            {
                var attributeValues = await _productAttributeParser.ParseProductAttributeValuesAsync(orderItem.AttributesXml);

                if (attributeValues != null)
                {
                    matchingAttributes = attributeValues
                        .Where(x => x.Id.ToString() == genericAttribute.Value)
                        .ToList();
                }
            }
            var matchingName = matchingAttributes.FirstOrDefault()?.Name;

            if (orderItem != null)
            {
                var order = await _orderService.GetOrderByIdAsync(orderItem.OrderId);
                var product = await _productService.GetProductByIdAsync(orderItem.ProductId);

                model.ProductId = product.Id;
                model.OrderId = order.Id;
                model.AttributeInfo = !string.IsNullOrEmpty(matchingName) ? matchingName : orderItem.AttributeDescription;
                model.CustomOrderNumber = order.CustomOrderNumber;

                var kitAttributeId = await _genericAttributeService.GetAttributeAsync<int>(returnRequest, "ReturnRequest.KitAttributeId");
                if (kitAttributeId > 0)
                {
                    var attributeValue = await _productAttributeService.GetProductAttributeValueByIdAsync(kitAttributeId);
                    model.ProductName = attributeValue?.Name;

                    if (attributeValue.AssociatedProductId > 0)
                    {
                        var associatedProduct = await _productService.GetProductByIdAsync(attributeValue.AssociatedProductId);
                            model.SKU = associatedProduct?.Sku;
                    }
                    else
                        model.SKU = product.Sku;
                }
                else
                {
                    model.ProductName = product.Name;
                    model.SKU = product.Sku;
                }
            }

            if (excludeProperties)
                return model;

            model.ReasonForReturn = returnRequest.ReasonForReturn;
            model.RequestedAction = returnRequest.RequestedAction;
            model.CustomerComments = returnRequest.CustomerComments;
            model.StaffNotes = returnRequest.StaffNotes;
            model.ReturnRequestStatusId = returnRequest.ReturnRequestStatusId;

            var note = await _customReturnRequestService.GetReturnRequestNoteByIdAsync(returnRequest.Id);
            if(note != null)
                model.Note = note.Notes;

            return model;
        }

        #endregion

    }
}
