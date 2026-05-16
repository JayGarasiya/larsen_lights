using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Orders;

namespace Nop.Plugin.Widgets.MakeTypeModel.Services.Orders
{
    /// <summary>
    /// Override return request service
    /// </summary>
    public partial class OverrideReturnRequestService : ReturnRequestService
    {
        #region Fields

        protected readonly IProductService _productService;
        protected readonly IProductAttributeParser _productAttributeParser;
        protected readonly IProductAttributeService _productAttributeService;

        #endregion

        #region Ctor

        public OverrideReturnRequestService(IRepository<ReturnRequest> returnRequestRepository,
            IRepository<ReturnRequestAction> returnRequestActionRepository,
            IRepository<ReturnRequestReason> returnRequestReasonRepository,
            IRepository<OrderItem> orderItemRepository,
            IRepository<Product> productRepository,
            IProductService productService,
            IProductAttributeParser productAttributeParser,
            IProductAttributeService productAttributeService) : base(returnRequestRepository,
                returnRequestActionRepository,
                returnRequestReasonRepository,
                orderItemRepository,
                productRepository)
        {
            _productService = productService;
            _productAttributeParser = productAttributeParser;
            _productAttributeService = productAttributeService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the return request availability
        /// </summary>
        /// <param name="orderId">The order identifier</param>
        /// <returns>The <see cref="Task"/> containing the <see cref="ReturnRequestAvailability"/></returns>
        public override async Task<ReturnRequestAvailability> GetReturnRequestAvailabilityAsync(int orderId)
        {
            var result = new ReturnRequestAvailability
            {
                ReturnableOrderItems = new List<ReturnableOrderItem>()
            };

            if (orderId < 0)
                return result;

            var cancelledStatusId = (int)ReturnRequestStatus.Cancelled;

            var requestedOrderItemsForReturn =
                from rr in _returnRequestRepository.Table
                where rr.ReturnRequestStatusId != cancelledStatusId
                group rr by rr.OrderItemId into g
                select new
                {
                    OrderItemId = g.Key,
                    RequestedQuantityForReturn = g.Sum(rr => rr.Quantity)
                };

            var requestedQtyDict = (await requestedOrderItemsForReturn.ToListAsync())
                .ToDictionary(x => x.OrderItemId, x => x.RequestedQuantityForReturn);

            var orderItems = await _orderItemRepository.Table
                .Where(oi => oi.OrderId == orderId)
                .ToListAsync();

            foreach (var oi in orderItems)
            {
                var product = await _productService.GetProductByIdAsync(oi.ProductId);
                if (product == null || product.NotReturnable)
                    continue;

                var requestedQty = requestedQtyDict.TryGetValue(oi.Id, out var qty) ? qty : 0;
                var availableQty = Math.Max(oi.Quantity - requestedQty, 0);

                bool isKitReturnable = false;

                var attributeMappings = await _productAttributeParser.ParseProductAttributeMappingsAsync(oi.AttributesXml)
                                         ?? new List<ProductAttributeMapping>();

                foreach (var mapping in attributeMappings)
                {
                    var values = _productAttributeParser.ParseValues(oi.AttributesXml, mapping.Id)
                                 ?? new List<string>();

                    foreach (var value in values)
                    {
                        if (int.TryParse(value, out var pavId))
                        {
                            var pav = await _productAttributeService.GetProductAttributeValueByIdAsync(pavId);
                            if (pav != null)
                            {
                                var attributeMapping = await _productAttributeService
                                    .GetProductAttributeMappingByIdAsync(pav.ProductAttributeMappingId);

                                if (attributeMapping != null)
                                {
                                    var kitProduct = await _productService.GetProductByIdAsync(attributeMapping.ProductId);
                                    if (kitProduct != null && !kitProduct.NotReturnable)
                                    {
                                        isKitReturnable = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    if (isKitReturnable)
                        break;
                }

                var finalAvailableQty = availableQty > 0 || isKitReturnable ? 1 : 0;

                if (finalAvailableQty > 0)
                {
                    result.ReturnableOrderItems.Add(new ReturnableOrderItem
                    {
                        OrderItem = oi,
                        AvailableQuantityForReturn = finalAvailableQty
                    });
                }
            }

            return result;
        }

        #endregion
    }
}
