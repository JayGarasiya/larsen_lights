using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Html;
using Nop.Services.Orders;
using Nop.Services.Shipping;
using Nop.Services.Tax;

namespace Nop.Plugin.Misc.InvoicePDF.Services
{
    /// <summary>
    /// Override Order service
    /// </summary>
    public class OverrideOrderService : OrderService
    {
        #region Fields
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly ITaxService _taxService;
        #endregion

        #region Ctor
        public OverrideOrderService(IHtmlFormatter htmlFormatter,
           IProductService productService,
           IRepository<Address> addressRepository,
           IRepository<Customer> customerRepository,
           IRepository<Order> orderRepository,
           IRepository<OrderItem> orderItemRepository,
           IRepository<OrderNote> orderNoteRepository,
           IRepository<Product> productRepository,
           IRepository<ProductWarehouseInventory> productWarehouseInventoryRepository,
           IRepository<RecurringPayment> recurringPaymentRepository,
           IRepository<RecurringPaymentHistory> recurringPaymentHistoryRepository,
           IShipmentService shipmentService,
           IProductAttributeParser productAttributeParser,
           IShoppingCartService shoppingCartService,
           ITaxService taxService) : base(htmlFormatter,
               productService,
               addressRepository,
               customerRepository,
               orderRepository,
               orderItemRepository,
               orderNoteRepository,
               productRepository,
               productWarehouseInventoryRepository,
               recurringPaymentRepository,
               recurringPaymentHistoryRepository,
               shipmentService)
        {
            _productAttributeParser = productAttributeParser;
            _shoppingCartService = shoppingCartService;
            _taxService = taxService;
        }
        #endregion

        #region Methods

        #region Orders items

        /// <summary>
        /// Gets a list items of order
        /// </summary>
        /// <param name="orderId">Order identifier</param>
        /// <param name="isNotReturnable">Value indicating whether this product is returnable; pass null to ignore</param>
        /// <param name="isShipEnabled">Value indicating whether the entity is ship enabled; pass null to ignore</param>
        /// <param name="vendorId">Vendor identifier; pass 0 to ignore</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public override async Task<IList<OrderItem>> GetOrderItemsAsync(int orderId, bool? isNotReturnable = null, bool? isShipEnabled = null, int vendorId = 0)
        {
            if (orderId == 0)
                return new List<OrderItem>();


            if (vendorId == 0)
            {
                return await (from oi in _orderItemRepository.Table
                              join p in _productRepository.Table on oi.ProductId equals p.Id
                              where
                              oi.OrderId == orderId &&
                              (!isShipEnabled.HasValue || (p.IsShipEnabled == isShipEnabled.Value)) &&
                              (!isNotReturnable.HasValue || (p.NotReturnable == isNotReturnable)) &&
                              (vendorId <= 0 || (p.VendorId == vendorId))
                              select oi).ToListAsync();
            }
            else
            {
                var order = await base.GetOrderByIdAsync(orderId);
                var customer = (from c in _customerRepository.Table
                                where c.Id == order.CustomerId
                                select c).FirstOrDefault();

                var orderItems = new List<OrderItem>();
                foreach (var orderItem in await GetOrderItemsAsync(orderId, isNotReturnable, isShipEnabled))
                {
                    var product = await _productService.GetProductByIdAsync(orderItem.ProductId);
                    if (product.ManageInventoryMethod != ManageInventoryMethod.ManageStockByAttributes && product.VendorId == vendorId)
                        orderItems.Add(orderItem);
                    else
                    {
                        var associatedItems = new List<OrderItem>();
                        foreach (var productAttributeValue in await _productAttributeParser.ParseProductAttributeValuesAsync(orderItem.AttributesXml))
                        {
                            if (productAttributeValue.AttributeValueType == AttributeValueType.AssociatedToProduct)
                            {
                                var associatedProduct = await _productService.GetProductByIdAsync(productAttributeValue.AssociatedProductId);
                                if (associatedProduct.VendorId == vendorId)
                                {
                                    var now = DateTime.UtcNow;
                                    var sc = new ShoppingCartItem
                                    {
                                        ShoppingCartType = ShoppingCartType.ShoppingCart,
                                        StoreId = order.StoreId,
                                        ProductId = associatedProduct.Id,
                                        AttributesXml = string.Empty,
                                        CustomerEnteredPrice = decimal.Zero,
                                        Quantity = productAttributeValue.Quantity * orderItem.Quantity,
                                        CreatedOnUtc = now,
                                        UpdatedOnUtc = now,
                                        CustomerId = order.CustomerId
                                    };

                                    //prices
                                    var scUnitPrice = (await _shoppingCartService.GetUnitPriceAsync(sc, true)).unitPrice;
                                    var (scSubTotal, discountAmount, scDiscounts, _) = await _shoppingCartService.GetSubTotalAsync(sc, true);
                                    var scUnitPriceInclTax =
                                        await _taxService.GetProductPriceAsync(associatedProduct, scUnitPrice, true, customer);
                                    var scUnitPriceExclTax =
                                        await _taxService.GetProductPriceAsync(associatedProduct, scUnitPrice, false, customer);
                                    var scSubTotalInclTax =
                                        await _taxService.GetProductPriceAsync(associatedProduct, scSubTotal, true, customer);
                                    var scSubTotalExclTax =
                                        await _taxService.GetProductPriceAsync(associatedProduct, scSubTotal, false, customer);
                                    var discountAmountInclTax =
                                        await _taxService.GetProductPriceAsync(associatedProduct, discountAmount, true, customer);
                                    var discountAmountExclTax =
                                        await _taxService.GetProductPriceAsync(associatedProduct, discountAmount, false, customer);

                                    //save order item
                                    var associatedItem = new OrderItem
                                    {
                                        Id = orderItem.Id,
                                        OrderItemGuid = Guid.NewGuid(),
                                        OrderId = orderId,
                                        ProductId = associatedProduct.Id,
                                        UnitPriceInclTax = scUnitPriceInclTax.price,
                                        UnitPriceExclTax = scUnitPriceExclTax.price,
                                        PriceInclTax = scSubTotalInclTax.price,
                                        PriceExclTax = scSubTotalExclTax.price,
                                        OriginalProductCost = associatedProduct.ProductCost,
                                        Quantity = productAttributeValue.Quantity * orderItem.Quantity,
                                        DiscountAmountInclTax = discountAmountInclTax.price,
                                        DiscountAmountExclTax = discountAmountExclTax.price,
                                        DownloadCount = 0,
                                        IsDownloadActivated = false,
                                        LicenseDownloadId = 0,
                                        ItemWeight = associatedProduct.Weight
                                    };

                                    associatedItems.Add(associatedItem);
                                }
                            }
                        }
                        if (associatedItems.Any())
                            orderItems.AddRange(associatedItems);
                        else
                        {
                            if (product.VendorId == vendorId)
                                orderItems.Add(orderItem);
                        }
                    }
                }

                return orderItems;
            }
        }

        #endregion

        #endregion
    }
}