using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Seo;
using Nop.Web.Factories;
using Nop.Web.Models.Order;

namespace Nop.Plugin.Widgets.MakeTypeModel.Factories
{
    /// <summary>
    /// Represents a override web return request model factory
    /// </summary>
    public class OverrideWebReturnRequestModelFactory : ReturnRequestModelFactory
    {
        #region Fields 

        protected readonly IGenericAttributeService _genericAttributeService;
        protected readonly IProductAttributeService _productAttributeService;

        #endregion

        #region Ctor

        public OverrideWebReturnRequestModelFactory(ICurrencyService currencyService,
            IDateTimeHelper dateTimeHelper,
            IDownloadService downloadService,
            ILocalizationService localizationService,
            IOrderService orderService,
            IPriceFormatter priceFormatter,
            IProductService productService,
            IReturnRequestService returnRequestService,
            IStoreContext storeContext,
            IUrlRecordService urlRecordService,
            IWorkContext workContext,
            OrderSettings orderSettings,
            IGenericAttributeService genericAttributeService,
            IProductAttributeService productAttributeService) : base(currencyService,
                dateTimeHelper,
                downloadService,
                localizationService,
                orderService,
                priceFormatter,
                productService,
                returnRequestService,
                storeContext,
                urlRecordService,
                workContext,
                orderSettings)
        {
            _genericAttributeService = genericAttributeService;
            _productAttributeService = productAttributeService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Prepare the customer return requests model
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the customer return requests model
        /// </returns>
        public override async Task<CustomerReturnRequestsModel> PrepareCustomerReturnRequestsModelAsync()
        {
            var model = new CustomerReturnRequestsModel();
            var store = await _storeContext.GetCurrentStoreAsync();
            var customer = await _workContext.GetCurrentCustomerAsync();
            var returnRequests = await _returnRequestService.SearchReturnRequestsAsync(store.Id, customer.Id);

            foreach (var returnRequest in returnRequests)
            {
                var orderItem = await _orderService.GetOrderItemByIdAsync(returnRequest.OrderItemId);
                if (orderItem != null)
                {
                    var product = await _productService.GetProductByIdAsync(orderItem.ProductId);

                    var download = await _downloadService.GetDownloadByIdAsync(returnRequest.UploadedFileId);

                    var itemModel = new CustomerReturnRequestsModel.ReturnRequestModel
                    {
                        Id = returnRequest.Id,
                        CustomNumber = returnRequest.CustomNumber,
                        ReturnRequestStatus = await _localizationService.GetLocalizedEnumAsync(returnRequest.ReturnRequestStatus),
                        ProductId = product.Id,
                        ProductSeName = await _urlRecordService.GetSeNameAsync(product),
                        Quantity = returnRequest.Quantity,
                        ReturnAction = returnRequest.RequestedAction,
                        ReturnReason = returnRequest.ReasonForReturn,
                        Comments = returnRequest.CustomerComments,
                        UploadedFileGuid = download?.DownloadGuid ?? Guid.Empty,
                        CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(returnRequest.CreatedOnUtc, DateTimeKind.Utc),
                    };

                    var kitAttributeId = await _genericAttributeService.GetAttributeAsync<int>(returnRequest, "ReturnRequest.KitAttributeId");
                    if (kitAttributeId > 0)
                    {
                        var attributeValue = await _productAttributeService.GetProductAttributeValueByIdAsync(kitAttributeId);
                        itemModel.ProductName = attributeValue?.Name;
                    }
                    else
                    {
                        itemModel.ProductName = product.Name;
                    }

                    model.Items.Add(itemModel);
                }
            }

            return model;
        }

        #endregion
    }
}
