using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Orders;
using Nop.Web.Framework.Factories;

namespace Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Factories
{
    /// <summary>
    /// Represent override retune request model factory
    /// </summary>
    public partial class OverrideReturnRequestModelFactory : ReturnRequestModelFactory
    {
        #region Fields

        protected readonly IGenericAttributeService _genericAttributeService;
        protected readonly IProductAttributeParser _productAttributeParser;
        protected readonly IProductAttributeService _productAttributeService;

        #endregion

        #region Ctor

        public OverrideReturnRequestModelFactory(IBaseAdminModelFactory baseAdminModelFactory,
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
            IProductAttributeService productAttributeService
            ) : base(baseAdminModelFactory,
                dateTimeHelper,
                downloadService,
                customerService,
                localizationService,
                localizedModelFactory,
                orderService,
                productService,
                returnRequestService)
        {
            _genericAttributeService = genericAttributeService;
            _productAttributeParser = productAttributeParser;
            _productAttributeService = productAttributeService;
        }

        #endregion

        #region Method

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
        public override async Task<ReturnRequestModel> PrepareReturnRequestModelAsync(ReturnRequestModel model,
            ReturnRequest returnRequest, bool excludeProperties = false)
        {
            if (returnRequest == null)
                return model;

            //fill in model values from the entity
            model ??= returnRequest.ToModel<ReturnRequestModel>();

            var customer = await _customerService.GetCustomerByIdAsync(returnRequest.CustomerId);

            model.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(returnRequest.CreatedOnUtc, DateTimeKind.Utc);
            model.CustomerInfo = await _customerService.IsRegisteredAsync(customer)
                ? customer.Email : await _localizationService.GetResourceAsync("Admin.Customers.Guest");
            model.UploadedFileGuid = (await _downloadService.GetDownloadByIdAsync(returnRequest.UploadedFileId))?.DownloadGuid ?? Guid.Empty;
            model.ReturnRequestStatusStr = await _localizationService.GetLocalizedEnumAsync(returnRequest.ReturnRequestStatus);
            var orderItem = await _orderService.GetOrderItemByIdAsync(returnRequest.OrderItemId);

            var genericAttribute = (await _genericAttributeService.GetAttributesForEntityAsync(returnRequest.Id, "ReturnRequest")).FirstOrDefault();
            var matchingAttributes = new List<ProductAttributeValue>();

            if (genericAttribute != null)
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
                        model.AttributeInfo = $"SKU: {associatedProduct?.Sku}";
                    }
                }
                else
                {
                    model.ProductName = product.Name;
                    model.AttributeInfo = $"SKU: {product?.Sku}";
                }
            }

            if (excludeProperties)
                return model;

            model.ReasonForReturn = returnRequest.ReasonForReturn;
            model.RequestedAction = returnRequest.RequestedAction;
            model.CustomerComments = returnRequest.CustomerComments;
            model.StaffNotes = returnRequest.StaffNotes;
            model.ReturnRequestStatusId = returnRequest.ReturnRequestStatusId;

            return model;
        }

        #endregion
    }
}
