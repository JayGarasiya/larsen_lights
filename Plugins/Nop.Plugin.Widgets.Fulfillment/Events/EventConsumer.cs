using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Events;
using Nop.Plugin.Widgets.Fulfillment.Domain;
using Nop.Plugin.Widgets.Fulfillment.Domain.WMS.Orders;
using Nop.Plugin.Widgets.Fulfillment.Services.ThreePl;
using Nop.Plugin.Widgets.Fulfillment.Services.WMS;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Stores;
using Nop.Services.Tax;
using NUglify.Helpers;

namespace Nop.Plugin.Widgets.Fulfillment.Events
{
    /// <summary>
    /// Represents event consumer
    /// </summary>
    public class EventConsumer :
        IConsumer<OrderPlacedEvent>,
        IConsumer<OrderPaidEvent>,
        IConsumer<OrderStatusChangedEvent>,
        IConsumer<EntityDeletedEvent<Order>>,
        IConsumer<EntityDeletedEvent<OrderItem>>,
        IConsumer<EntityInsertedEvent<ShoppingCartItem>>,
        IConsumer<EntityUpdatedEvent<ShoppingCartItem>>,
        IConsumer<EntityDeletedEvent<ShoppingCartItem>>
    {
        #region Fields

        protected readonly IThreePlService _threePlService;
        protected readonly IPaymentService _paymentService;
        protected readonly ILocalizationService _localizationService;
        protected readonly IOrderService _orderService;
        protected readonly IWorkContext _workContext;
        protected readonly IGenericAttributeService _genericAttributeService;
        protected readonly ICustomerService _customerService;
        protected readonly IThreePlWarehouseService _threePlWarehouseService;
        protected readonly IProductService _productService;
        protected readonly IShoppingCartService _shoppingCartService;
        protected readonly ITaxService _taxService;
        protected readonly ICurrencyService _currencyService;
        protected readonly IStoreService _storeService;

        #endregion

        #region Ctor

        public EventConsumer(IThreePlService threePlService,
            IPaymentService paymentService,
            ILocalizationService localizationService,
            IOrderService orderService,
            IWorkContext workContext,
            IGenericAttributeService genericAttributeService,
            ICustomerService customerService,
            IThreePlWarehouseService threePlWarehouseService,
            IProductService productService,
            IShoppingCartService shoppingCartService,
            ITaxService taxService,
            ICurrencyService currencyService,
            IStoreService storeService)
        {
            _threePlService = threePlService;
            _paymentService = paymentService;
            _localizationService = localizationService;
            _orderService = orderService;
            _workContext = workContext;
            _genericAttributeService = genericAttributeService;
            _customerService = customerService;
            _threePlWarehouseService = threePlWarehouseService;
            _productService = productService;
            _shoppingCartService = shoppingCartService;
            _taxService = taxService;
            _currencyService = currencyService;
            _storeService = storeService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Handle the order placed event
        /// </summary>
        /// <param name="eventMessage">The event message.</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(OrderPlacedEvent eventMessage)
        {
            //handle event
            var order = eventMessage.Order;

            if (!string.IsNullOrEmpty(order.PaymentMethodSystemName))
            {
                if (order.PaymentMethodSystemName.Equals("Payments.PurchaseOrder"))
                {
                    var customValues = await _genericAttributeService.GetAttributeAsync<Dictionary<string, object>>(order, "CustomValues", order.StoreId)
                      ?? new Dictionary<string, object>(); // fallback to empty dictionary

                    var poKey = await _localizationService.GetResourceAsync("Plugins.Payment.PurchaseOrder.PurchaseOrderNumber");

                    if (customValues != null && !string.IsNullOrEmpty(poKey) && customValues.ContainsKey(poKey))
                    {
                        if (_workContext.OriginalCustomerIfImpersonated != null)
                        {
                            var outOfStockProducts = false;
                            var note = string.Empty;
                            var syncItems = await _genericAttributeService.GetAttributeAsync<List<SyncOrderItem>>(order, "SyncOrderItem", order.StoreId);
                            if (syncItems != null || (syncItems?.Any() ?? false))
                            {
                                outOfStockProducts = syncItems.Any(s => s.OutOfStockItems.Any());
                                note = string.Join(" ", syncItems.Select(s => s.Note));
                            }

                            var threePlBRecord = new ThreePlRecord()
                            {
                                OrderId = order.Id,
                                ThreePlStutusId = outOfStockProducts ? (int)ThreePlStutus.PendingStock : (int)ThreePlStutus.Pending,
                                Note = outOfStockProducts ? note : string.Empty
                            };
                            await _threePlService.InsertThreePlRecordAsync(threePlBRecord);
                        }
                        else
                        {
                            var threePlRecord = new ThreePlRecord()
                            { 
                                OrderId = order.Id,
                                ThreePlStutusId = (int)ThreePlStutus.Pending
                            };
                            await _threePlService.InsertThreePlRecordAsync(threePlRecord);
                        }
                    }
                    else
                    {
                        var threePlRecord = new ThreePlRecord()
                        {
                            OrderId = order.Id,
                            ThreePlStutusId = (int)ThreePlStutus.Pending
                        };
                        await _threePlService.InsertThreePlRecordAsync(threePlRecord);
                    }
                    //update order status as processing for payment method 'Payments.PurchaseOrder'
                    order.OrderStatusId = (int)OrderStatus.Processing;
                    await _orderService.UpdateOrderAsync(order);
                }

                if (order.PaymentMethodSystemName.Equals("Payments.CheckMoneyOrder"))
                {
                    if (_workContext.OriginalCustomerIfImpersonated != null)
                    {
                        var outOfStockProducts = false;
                        var note = string.Empty;
                        var syncItems = await _genericAttributeService.GetAttributeAsync<List<SyncOrderItem>>(order, "SyncOrderItem", order.StoreId);
                        if (syncItems != null || (syncItems?.Any() ?? false))
                        {
                            outOfStockProducts = syncItems.Any(s => s.OutOfStockItems.Any());
                            note = string.Join(" ", syncItems.Select(s => s.Note));
                        }

                        var threePlBRecord = new ThreePlRecord()
                        {
                            OrderId = order.Id,
                            ThreePlStutusId = outOfStockProducts ? (int)ThreePlStutus.PendingStock : (int)ThreePlStutus.Pending,
                            Note = outOfStockProducts ? note : string.Empty,
                            PaidByCheck = true,
                        };
                        await _threePlService.InsertThreePlRecordAsync(threePlBRecord);
                    }
                    else
                    {
                        var threePlRecord = new ThreePlRecord()
                        {
                            OrderId = order.Id,
                            ThreePlStutusId = (int)ThreePlStutus.Pending,
                            PaidByCheck = true
                        };
                        await _threePlService.InsertThreePlRecordAsync(threePlRecord);
                    }
                }
            }
        }

        /// <summary>
        /// Handle the order paid event
        /// </summary>
        /// <param name="eventMessage">The event message.</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(OrderPaidEvent eventMessage)
        {
            //handle event
            var order = eventMessage.Order;
            var threePlRecord = await _threePlService.GetThreePlRecordByOrderIdAsync(order.Id);
            if (threePlRecord == null)
            {
                var outOfStockProducts = false;
                var note = string.Empty;
                var syncItems = await _genericAttributeService.GetAttributeAsync<List<SyncOrderItem>>(order, "SyncOrderItem", order.StoreId);
                if (syncItems != null || (syncItems?.Any() ?? false))
                {
                    outOfStockProducts = syncItems.Any(s => s.OutOfStockItems.Any());
                    note = string.Join(" ", syncItems.Select(s => s.Note));
                }

                threePlRecord = new ThreePlRecord()
                {
                    OrderId = order.Id,
                    ThreePlStutusId = outOfStockProducts ? (int)ThreePlStutus.PendingStock : (int)ThreePlStutus.Pending,
                    Note = note
                };
                await _threePlService.InsertThreePlRecordAsync(threePlRecord);
            }
        }

        /// <summary>
        /// Handle the order status change event
        /// </summary>
        /// <param name="eventMessage">The event message.</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(OrderStatusChangedEvent eventMessage)
        {
            //handle event
            var order = eventMessage.Order;
            var prevOrderStatus = eventMessage.PreviousOrderStatus;

            if (prevOrderStatus != OrderStatus.Cancelled &&
                order.OrderStatus == OrderStatus.Cancelled)
            {
                var threePlRecord = await _threePlService.GetThreePlRecordByOrderIdAsync(order.Id);
                if (threePlRecord != null)
                {
                    var threePlOrders = await _threePlService.GetThreePlOrdersByOrderIdAsync(order.Id);
                    if(threePlOrders.Any())
                    {
                        threePlOrders.ForEach(async threePlOrder => {
                            if (threePlOrder.ThreePlOrderId > 0)
                            {
                                threePlOrder.ThreePlStutusId = (int)ThreePlStutus.Pending;
                                threePlOrder.CancelledDate = DateTime.UtcNow;

                                await _threePlService.UpdateThreePlOrderAsync(threePlOrder);
                            }
                        });

                        threePlRecord.ThreePlStutusId = (int)ThreePlStutus.Pending;
                        threePlRecord.Note = "3PL Central - Order cancelled. Require to sync.";

                    }
                    else
                    {
                        threePlRecord.ThreePlStutusId = (int)ThreePlStutus.Cancel;
                        threePlRecord.Note = "3PL Central - Order cancelled. Not require to sync.";
                    }

                    threePlRecord.CancelledDate = DateTime.UtcNow;
                    await _threePlService.UpdateThreePlRecordAsync(threePlRecord);
                }

                //remove validates a product for standard properties
                var removeValue = (await _genericAttributeService.GetAttributesForEntityAsync(order.Id, order.GetType().Name))
                    .Where(x => x.Key.Equals("SyncOrderItem"))
                    .FirstOrDefault();
                if (removeValue != null)
                    await _genericAttributeService.DeleteAttributeAsync(removeValue);
            }

        }

        /// <summary>
        /// Handle the delete order event
        /// </summary>
        /// <param name="eventMessage">The event message.</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(EntityDeletedEvent<Order> eventMessage)
        {
            //handle event
            var order = eventMessage.Entity;

            var threePlRecord = await _threePlService.GetThreePlRecordByOrderIdAsync(order.Id);
            if (threePlRecord != null)
            {
                var threePlOrders = await _threePlService.GetThreePlOrdersByOrderIdAsync(order.Id);
                if (threePlRecord.ThreePlStutusId != (int)ThreePlStutus.Cancel && threePlOrders.Any() )
                {
                    threePlOrders.ForEach(async threePlOrder => {
                        if (threePlOrder.ThreePlOrderId > 0)
                        {
                            threePlOrder.ThreePlStutusId = (int)ThreePlStutus.Pending;
                            threePlOrder.CancelledDate = DateTime.UtcNow;

                            await _threePlService.UpdateThreePlOrderAsync(threePlOrder);
                        }
                    });

                    threePlRecord.ThreePlStutusId = (int)ThreePlStutus.Pending;
                    threePlRecord.CancelledDate = DateTime.UtcNow;
                    threePlRecord.Note = "3PL Central - Order deleted. Require to sync.";

                }
                else
                    threePlRecord.Note = "3PL Central - Order deleted. Not require to sync.";

                await _threePlService.UpdateThreePlRecordAsync(threePlRecord);
            }

            //remove validates a product for standard properties
            var removeValue = (await _genericAttributeService.GetAttributesForEntityAsync(order.Id, order.GetType().Name))
                .Where(x => x.Key.Equals("SyncOrderItem"))
                .FirstOrDefault();
            if (removeValue != null)
                await _genericAttributeService.DeleteAttributeAsync(removeValue);
        }

        /// <summary>
        /// Handle the delete order item event
        /// </summary>
        /// <param name="eventMessage">The event message.</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(EntityDeletedEvent<OrderItem> eventMessage)
        {
            //handle event
            var orderItem = eventMessage.Entity;
            var order = await _orderService.GetOrderByIdAsync(orderItem.OrderId);
            var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);

            //retrive order items and filter for sync
            var orderItems = await _orderService.GetOrderItemsAsync(order.Id);
            if (!orderItems.Any())
            {
                var threePlRecord = await _threePlService.GetThreePlRecordByOrderIdAsync(order.Id);
                if (threePlRecord != null)
                {
                    threePlRecord.ThreePlStutusId = (int)ThreePlStutus.NotRequire;
                    threePlRecord.Note = "3PL Central - Order item deleted. Not require to sync.";
                    await _threePlService.UpdateThreePlRecordAsync(threePlRecord);
                }

                //remove validates a product for standard properties
                var removeValue = (await _genericAttributeService.GetAttributesForEntityAsync(order.Id, order.GetType().Name))
                    .Where(x => x.Key.Equals("SyncOrderItem"))
                    .FirstOrDefault();
                if (removeValue != null)
                    await _genericAttributeService.DeleteAttributeAsync(removeValue);
            }
            else
            {
                var syncItems = await _genericAttributeService.GetAttributeAsync<List<SyncOrderItem>>(order, "SyncOrderItem", order.StoreId);
                if (syncItems != null || (syncItems?.Any() ?? false))
                {
                    //enable validates a product for standard properties
                    await _genericAttributeService.SaveAttributeAsync(customer, "ImpersonatedValidation", true);

                    //filter item by order item identifier and remove from list
                    var removeSyncItem = syncItems.FirstOrDefault(si => si.OrderItemId == orderItem.Id);
                    if (removeSyncItem != null)
                        syncItems.Remove(removeSyncItem);

                    //save order items
                    if (syncItems != null || (syncItems?.Any() ?? false))
                        await _genericAttributeService.SaveAttributeAsync(order, "SyncOrderItem", syncItems, order.StoreId);
                    else
                    {
                        var threePlRecord = await _threePlService.GetThreePlRecordByOrderIdAsync(order.Id);
                        if (threePlRecord != null)
                        {
                            threePlRecord.ThreePlStutusId = (int)ThreePlStutus.NotRequire;
                            threePlRecord.Note = "3PL Central - Order item deleted. Not require to sync.";
                            await _threePlService.UpdateThreePlRecordAsync(threePlRecord);
                        }

                        //remove validates a product for standard properties
                        var removeValue = (await _genericAttributeService.GetAttributesForEntityAsync(order.Id, order.GetType().Name))
                            .Where(x => x.Key.Equals("SyncOrderItem"))
                            .FirstOrDefault();
                        if (removeValue != null)
                            await _genericAttributeService.DeleteAttributeAsync(removeValue);
                    }

                    //remove validates a product for standard properties
                    var validationValue = (await _genericAttributeService.GetAttributesForEntityAsync(customer.Id, customer.GetType().Name))
                        .Where(x => x.Key.Equals("ImpersonatedValidation"))
                        .FirstOrDefault();
                    if (validationValue != null)
                        await _genericAttributeService.DeleteAttributeAsync(validationValue);
                }
            }
        }

        /// <summary>
        /// Handle the insert shopping cart item event
        /// </summary>
        /// <param name="eventMessage">The event message.</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(EntityInsertedEvent<ShoppingCartItem> eventMessage)
        {
            var sci = eventMessage.Entity;
            if (sci.ShoppingCartType != ShoppingCartType.ShoppingCart)
                return;

            var product = await _productService.GetProductByIdAsync(sci.ProductId);

            //sub total
            var (subTotal, _, _, _) = await _shoppingCartService.GetSubTotalAsync(sci, true);
            var (shoppingCartItemSubTotalWithDiscountBase, _) = await _taxService.GetProductPriceAsync(product, subTotal);
            var shoppingCartItemSubTotalWithDiscount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(shoppingCartItemSubTotalWithDiscountBase, await _workContext.GetWorkingCurrencyAsync());

            await _genericAttributeService.SaveAttributeAsync(sci, "SubTotalWithDiscounts", shoppingCartItemSubTotalWithDiscount);
        }

        /// <summary>
        /// Handle the update shopping cart item event
        /// </summary>
        /// <param name="eventMessage">The event message.</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(EntityUpdatedEvent<ShoppingCartItem> eventMessage)
        {
            var sci = eventMessage.Entity;
            if (sci.ShoppingCartType != ShoppingCartType.ShoppingCart)
                return;

            var product = await _productService.GetProductByIdAsync(sci.ProductId);

            //sub total
            var (subTotal, _, _, _) = await _shoppingCartService.GetSubTotalAsync(sci, true);
            var (shoppingCartItemSubTotalWithDiscountBase, _) = await _taxService.GetProductPriceAsync(product, subTotal);
            var shoppingCartItemSubTotalWithDiscount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(shoppingCartItemSubTotalWithDiscountBase, await _workContext.GetWorkingCurrencyAsync());

            await _genericAttributeService.SaveAttributeAsync(sci, "SubTotalWithDiscounts", shoppingCartItemSubTotalWithDiscount);
        }

        /// <summary>
        /// Handle the delete shopping cart item event
        /// </summary>
        /// <param name="eventMessage">The event message.</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(EntityDeletedEvent<ShoppingCartItem> eventMessage)
        {
            if (eventMessage.Entity.ShoppingCartType != ShoppingCartType.ShoppingCart)
                return;

            //remove validates a product for standard properties
            var validationValue = (await _genericAttributeService.GetAttributesForEntityAsync(eventMessage.Entity.Id, eventMessage.Entity.GetType().Name))
                .Where(x => x.Key.Equals("SubTotalWithDiscounts"))
                .FirstOrDefault();
            if (validationValue != null)
                await _genericAttributeService.DeleteAttributeAsync(validationValue);
        }

        #endregion
    }
}
