using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Plugin.Widgets.Fulfillment.Services.ThreePl;
using Nop.Plugin.Widgets.Fulfillment.Services.WMS;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Orders;
using Nop.Services.ScheduleTasks;
using Nop.Services.Shipping;

namespace Nop.Plugin.Widgets.Fulfillment.ScheduleTasks
{
    /// <summary>
    /// Represents a schedule task to insert shipment and tracking detail
    /// </summary>
    public class FulfillmentTrackingTask : IScheduleTask
    {
        #region Constant 

        private const string ShippingMethod = "3PLCarrier";

        #endregion

        #region Fields

        protected readonly Fulfillmen3PLtSettings _fulfillmen3PLtSettings;
        protected readonly IThreePlService _threePlService;
        protected readonly IThreePlWarehouseService _threePlWarehouseService;
        protected readonly IProductService _productService;
        protected readonly IOrderService _orderService;
        protected readonly IShipmentService _shipmentService;
        protected readonly IOrderProcessingService _orderProcessingService;
        protected readonly IProductAttributeParser _productAttributeParser;
        protected readonly IGenericAttributeService _genericAttributeService;

        #endregion

        #region Ctor

        public FulfillmentTrackingTask(Fulfillmen3PLtSettings fulfillmen3PLtSettings,
            IThreePlService threePlService,
            IThreePlWarehouseService threePlWarehouseService,
            IProductService productService,
            IOrderService orderService,
            IShipmentService shipmentService,
            IOrderProcessingService orderProcessingService,
            IProductAttributeParser productAttributeParser,
            IGenericAttributeService genericAttributeService)
        {
            _fulfillmen3PLtSettings = fulfillmen3PLtSettings;
            _threePlService = threePlService;
            _threePlWarehouseService = threePlWarehouseService;
            _productService = productService;
            _orderService = orderService;
            _shipmentService = shipmentService;
            _orderProcessingService = orderProcessingService;
            _productAttributeParser = productAttributeParser;
            _genericAttributeService = genericAttributeService;
        }

        #endregion

        #region Utilities

        private async Task<decimal> AddShipmentItemAsync(Shipment shipment, OrderItem orderItem, Product product)
        {
            decimal totalWeight = 0;

            //is shippable
            if (!product.IsShipEnabled)
                return decimal.Zero;

            //ensure that this product can be shipped (have at least one item to ship)
            var maxQtyToAdd = await _orderService.GetTotalNumberOfItemsCanBeAddedToShipmentAsync(orderItem);
            if (maxQtyToAdd <= 0)
                return decimal.Zero;

            var warehouseId = product.WarehouseId;

            //ok. we have at least one item. let's create a shipment (if it does not exist)

            var orderItemTotalWeight = orderItem.ItemWeight * orderItem.Quantity;
            if (orderItemTotalWeight.HasValue)
                totalWeight = orderItemTotalWeight.Value;

            //create a shipment item
            var shipmentItem = new ShipmentItem
            {
                OrderItemId = orderItem.Id,
                Quantity = orderItem.Quantity,
                WarehouseId = warehouseId,
                ShipmentId = shipment.Id
            };

            await _shipmentService.InsertShipmentItemAsync(shipmentItem);

            return totalWeight;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Execute task
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task ExecuteAsync()
        {
            if (await _threePlService.PluginActiveAsync())
            {
                //retrive access token
                var accessToken = _fulfillmen3PLtSettings.AccessToken;
                if (!_fulfillmen3PLtSettings.ValidateAccessToken())
                    accessToken = await _threePlWarehouseService.GetAuthenticationToken(_fulfillmen3PLtSettings.ClientId, _fulfillmen3PLtSettings.ClientSecret, _fulfillmen3PLtSettings.ThreePlKey, _fulfillmen3PLtSettings.UserId);

                #region Sync tracking number

                var orders = await _orderService.SearchOrdersAsync(psIds: new List<int> { (int)PaymentStatus.Paid, (int)PaymentStatus.Pending }, ssIds: new List<int> { (int)ShippingStatus.NotYetShipped });
                foreach (var order in orders)
                {
                    var order3PLRecord = await _threePlService.GetThreePlRecordByOrderIdAsync(order.Id);
                    if (order3PLRecord != null)
                    {
                        var threePlOrders = await _threePlService.GetThreePlOrdersByOrderIdAsync(order.Id);
                        foreach(var threePlOrder in threePlOrders)
                        {
                            var response = await _threePlWarehouseService.GetOrderDetails(accessToken, threePlOrder.ThreePlOrderId);
                            if (response != null)
                            {
                                if (response.RoutingInfo != null)
                                {
                                    if (string.IsNullOrWhiteSpace(response.RoutingInfo.TrackingNumber) || string.IsNullOrEmpty(response.RoutingInfo.TrackingNumber))
                                        continue;

                                    //check if order already have this tracking number
                                    var trackingNumber = response.RoutingInfo.TrackingNumber.Trim();
                                    var shipments = await _shipmentService.GetAllShipmentsAsync(orderId: order.Id, trackingNumber: trackingNumber);
                                    if (!shipments.Any())
                                    {
                                        var shipment = new Shipment
                                        {
                                            CreatedOnUtc = DateTime.UtcNow,
                                            OrderId = order.Id,
                                            TrackingNumber = trackingNumber.Trim()
                                        };

                                        decimal totalWeight = 0;

                                        await _shipmentService.InsertShipmentAsync(shipment);

                                        //add a note
                                        await _orderService.InsertOrderNoteAsync(new OrderNote
                                        {
                                            OrderId = order.Id,
                                            Note = "3PL Central - A shipment has been added",
                                            DisplayToCustomer = false,
                                            CreatedOnUtc = DateTime.UtcNow
                                        });

                                        var shippingMethod = string.Empty;
                                        if (response.RoutingInfo.Carrier.Equals("USPS") || response.RoutingInfo.Carrier.Equals("United States Postal Service") ||
                                            response.RoutingInfo.Carrier.Equals("Stamps.com") || response.RoutingInfo.Carrier.Contains("USPS"))
                                            shippingMethod = "USPS";
                                        else if (response.RoutingInfo.Carrier.Equals("UPS") || response.RoutingInfo.Carrier.Equals("United Parcel Service") ||
                                            response.RoutingInfo.Carrier.Contains("UPS"))
                                            shippingMethod = "UPS";
                                        else if (response.RoutingInfo.Carrier.Equals("Speedee Delivery") || response.RoutingInfo.Carrier.Equals("Spee-Dee Delivery") ||
                                            response.RoutingInfo.Carrier.Contains("Speedee") || response.RoutingInfo.Carrier.Contains("Spee-Dee"))
                                            shippingMethod = "SPEEDEE";
                                        else if (response.RoutingInfo.Carrier.Contains("FedEx"))
                                            shippingMethod = "FEDEX";
                                        else if (response.RoutingInfo.Carrier.Contains("DHL"))
                                            shippingMethod = "DHL";
                                        else
                                            shippingMethod = response.RoutingInfo.Carrier;

                                        //add shipment carrier to track shipment using shipment tracking plugin
                                        await _genericAttributeService.SaveAttributeAsync(shipment, ShippingMethod, shippingMethod);

                                        //parse all product with stock manage by attribute
                                        var orderItems = await _orderService.GetOrderItemsAsync(order.Id);
                                        var itemsList = new List<OrderItem>();
                                        foreach (var orderItem in orderItems)
                                        {
                                            var product = await _productService.GetProductByIdAsync(orderItem.ProductId);

                                            //is shippable
                                            if (!product.IsShipEnabled)
                                                continue;

                                            if (product.ManageInventoryMethod != ManageInventoryMethod.ManageStockByAttributes)
                                            {
                                                if (response.Items.Items.Any(p => p.ItemIdentifier.Sku == product.Sku))
                                                {
                                                    var quantity = Convert.ToInt32(response.Items.Items.FirstOrDefault(p => p.ItemIdentifier.Sku == product.Sku)?.Qty ?? 0);
                                                    orderItem.Quantity = quantity > 0 ? quantity : orderItem.Quantity;
                                                    totalWeight += await AddShipmentItemAsync(shipment, orderItem, product);
                                                }
                                            }
                                            else
                                            {
                                                //bundled products
                                                var attributeValues = await _productAttributeParser.ParseProductAttributeValuesAsync(orderItem.AttributesXml);
                                                foreach (var attributeValue in attributeValues)
                                                {
                                                    if (attributeValue.AttributeValueType != AttributeValueType.AssociatedToProduct)
                                                        continue;

                                                    var associatedProduct = await _productService.GetProductByIdAsync(attributeValue.AssociatedProductId);
                                                    if (associatedProduct == null)
                                                        continue;

                                                    if (response.Items.Items.Any(p => p.ItemIdentifier.Sku == associatedProduct.Sku) && !itemsList.Any(p => p.Id == orderItem.Id))
                                                    {
                                                        totalWeight += await AddShipmentItemAsync(shipment, orderItem, product);
                                                        itemsList.Add(orderItem);
                                                    }
                                                }
                                            }
                                        }

                                        shipment.TotalWeight = totalWeight;

                                        await _shipmentService.UpdateShipmentAsync(shipment);

                                        await _orderProcessingService.ShipAsync(shipment, true);
                                    }
                                }
                            }
                        }
                    }
                }

                #endregion
            }
        }

        #endregion
    }
}
