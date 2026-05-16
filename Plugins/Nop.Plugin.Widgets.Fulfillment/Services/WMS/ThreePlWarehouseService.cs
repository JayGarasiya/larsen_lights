using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Stores;
using Nop.Plugin.Widgets.Fulfillment.Domain;
using Nop.Plugin.Widgets.Fulfillment.Domain.WMS.Common;
using Nop.Plugin.Widgets.Fulfillment.Domain.WMS.Inventory;
using Nop.Plugin.Widgets.Fulfillment.Domain.WMS.Orders;
using Nop.Plugin.Widgets.Fulfillment.Services.ThreePl;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Shipping.Date;
using Nop.Services.Stores;
using NUglify.Helpers;
using Pipelines.Sockets.Unofficial.Arenas;
using RestSharp;
using System.Text;

namespace Nop.Plugin.Widgets.Fulfillment.Services.WMS
{
    /// <summary>
    /// Represents service 3PL central warehouse service
    /// </summary>
    public class ThreePlWarehouseService : IThreePlWarehouseService
    {
        #region constants 

        protected const string BASE_URL = "https://secure-wms.com";

        protected const string TOKEN_END_POINT = "/AuthServer/api/Token";
        protected const string GRANT_TYPE = "client_credentials";
        protected const string CONTENT_TYPE = "application/json";
        protected const string ACCESS_TOKEN = "access_token";

        protected const string ORDER_CREATE_END_POINT = @"/orders";
        protected const string ORDER_DETAILS_END_POINT = @"/orders/{0}?detail=All";
        protected const string ORDER_DETAILS_ETAG_END_POINT = @"/orders/{0}?detail=None";
        protected const string ORDER_ITEMS_DETAILS_END_POINT = @"/orders/{0}/items";
        protected const string ORDER_CANCEL_END_POINT = @"/orders/{0}/canceler";

        protected const string STOCK_DETAILS_END_POINT = "/inventory/stockdetails?customerid={0}&facilityid={1}";
        protected const string STOCK_DETAILS_BY_DATE_END_POINT = "/inventory/stockdetails?customerid={0}&facilityid={1}&rql=ReceivedDate=le={2}";

        #endregion

        #region Fields

        protected readonly Fulfillmen3PLtSettings _fulfillmen3PLtSettings;
        protected readonly IOrderService _orderService;
        protected readonly ILogger _logger;
        protected readonly IAddressService _addressService;
        protected readonly ICountryService _countryService;
        protected readonly IStateProvinceService _stateProvinceService;
        protected readonly IPaymentService _paymentService;
        protected readonly ILocalizationService _localizationService;
        protected readonly IProductService _productService;
        protected readonly IProductAttributeParser _productAttributeParser;
        protected readonly IShoppingCartService _shoppingCartService;
        protected readonly ICustomerService _customerService;
        protected readonly IStoreContext _storeContext;
        protected readonly IProductAttributeService _productAttributeService;
        protected readonly IGenericAttributeService _genericAttributeService;
        protected readonly IDateRangeService _dateRangeService;
        protected readonly IWorkContext _workContext;
        protected readonly IPriceCalculationService _priceCalculationService;
        protected readonly IThreePlService _threePlService;
        protected readonly IStoreService _storeService;

        #endregion

        #region Ctor

        public ThreePlWarehouseService(Fulfillmen3PLtSettings fulfillmen3PLtSettings,
            IOrderService orderService,
            ILogger logger,
            IAddressService addressService,
            ICountryService countryService,
            IStateProvinceService stateProvinceService,
            IPaymentService paymentService,
            ILocalizationService localizationService,
            IProductService productService,
            IProductAttributeParser productAttributeParser,
            IShoppingCartService shoppingCartService,
            ICustomerService customerService,
            IStoreContext storeContext,
            IProductAttributeService productAttributeService,
            IGenericAttributeService genericAttributeService,
            IDateRangeService dateRangeService,
            IWorkContext workContext,
            IPriceCalculationService priceCalculationService,
            IThreePlService threePlService,
            IStoreService storeService)
        {
            _fulfillmen3PLtSettings = fulfillmen3PLtSettings;
            _orderService = orderService;
            _logger = logger;
            _addressService = addressService;
            _countryService = countryService;
            _stateProvinceService = stateProvinceService;
            _paymentService = paymentService;
            _localizationService = localizationService;
            _productService = productService;
            _productAttributeParser = productAttributeParser;
            _shoppingCartService = shoppingCartService;
            _customerService = customerService;
            _storeContext = storeContext;
            _productAttributeService = productAttributeService;
            _genericAttributeService = genericAttributeService;
            _dateRangeService = dateRangeService;
            _workContext = workContext;
            _priceCalculationService = priceCalculationService;
            _threePlService = threePlService;
            _storeService = storeService;
        }

        #endregion

        #region Utilities

        private static string GenerateBase64StringForCredentials(string clientId, string clientSecret) =>
            $"Basic {Convert.ToBase64String(Encoding.GetEncoding("UTF-8").GetBytes(clientId + ":" + clientSecret))}";

        /// <summary>
        /// Get an order detail from the 3PL central
        /// </summary>
        /// <param name="accessToken">Access token</param>
        /// <param name="orderId">Order identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the order of the 3PL central
        /// </returns>
        private async Task<string> GetOrderETagDetails(string accessToken, int orderId)
        {
            if (string.IsNullOrEmpty(accessToken))
                throw new ArgumentNullException(nameof(accessToken), "AccessToken should be provided from calling code for each request. Consider caching the token from authenticate request");

            if (orderId == 0)
                throw new ArgumentException("Invalid or Missing OrderId argument");

            var client = new RestClient(BASE_URL);
            var request = new RestRequest(string.Format(ORDER_DETAILS_ETAG_END_POINT, orderId)) { Method = Method.GET };

            request.AddCommonHeaders(accessToken);
            var response = client.Execute(request);
            var responseContent = response.Content;

            return await Task.FromResult(response.Headers.FirstOrDefault(h => h.Name.Equals("ETag")).Value.ToString());
        }

        #endregion

        #region Method

        #region Authentication

        /// <summary>
        /// Get access token
        /// </summary>
        /// <param name="clientId">Client Id</param>
        /// <param name="clientSecret">Client Secret</param>
        /// <param name="threePlKey">3PL Key</param>
        /// <param name="userLoginId">User Login Id</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the access token of the 3PL central
        /// </returns>
        public virtual async Task<string> GetAuthenticationToken(string clientId, string clientSecret, string threePlKey, string userLoginId)
        {
            var client = new RestClient(BASE_URL);

            var request = new RestRequest(TOKEN_END_POINT) { Method = Method.POST };
            request.AddAuthHeaders(GenerateBase64StringForCredentials(clientId, clientSecret));

            var body = new { grant_type = GRANT_TYPE, tpl = threePlKey, user_login_id = userLoginId };
            request.AddParameter(CONTENT_TYPE, JsonConvert.SerializeObject(body), ParameterType.RequestBody);

            var response = client.Execute(request);
            var responseContent = response.Content;

            var token = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseContent);
            if (token.ContainsKey(ACCESS_TOKEN))
                return await Task.FromResult(token.GetValueOrDefault(ACCESS_TOKEN, null).ToString());

            return null;
        }

        #endregion

        #region Order

        /// <summary>
        /// Check product warehouse change for order
        /// </summary>
        /// <param name="orderId">Order identifiers</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// </returns>
        public async Task CheckProductWarehouseChange(int orderId)
        {
            //retrive order using order identifier 
            var nopOrder = await _orderService.GetOrderByIdAsync(orderId);
            if (nopOrder is null)
                return;

            //order items to sync
            var nopItems = await _genericAttributeService.GetAttributeAsync<List<SyncOrderItem>>(nopOrder, "SyncOrderItem", nopOrder.StoreId);
            if (nopItems is null || !nopItems.Any())
                return;

            nopItems.ForEach(async nopItem =>
            {
                var inStockItems = new List<OrdersItemDto>();
                var outStockItems = new List<OrdersItemDto>();

                if (nopItem.InStockItems.Any())
                {
                    nopItem.InStockItems.ForEach(async stock =>
                    {
                        var product = await _productService.GetProductByIdAsync(Convert.ToInt32(stock.ExternalId));
                        if (product.WarehouseId == 1)
                            inStockItems.Add(stock);
                    });
                }

                if (nopItem.OutOfStockItems.Any())
                {
                    nopItem.OutOfStockItems.ForEach(async outstock =>
                    {
                        var product = await _productService.GetProductByIdAsync(Convert.ToInt32(outstock.ExternalId));
                        if (product.WarehouseId == 1)
                            outStockItems.Add(outstock);
                    });
                }

                if (inStockItems.Any() || outStockItems.Any())
                {
                    nopItem.InStockItems = inStockItems;
                    nopItem.OutOfStockItems = outStockItems;

                    if (!nopItem.OutOfStockItems.Any())
                        nopItem.Note = string.Empty;

                    //save validates a product for standard properties
                    await _genericAttributeService.SaveAttributeAsync(nopOrder, "SyncOrderItem", nopItems, nopOrder.StoreId);
                }
            });
        }

        /// <summary>
        /// Create an order to the 3PL central
        /// </summary>
        /// <param name="accessToken">Access token</param>
        /// <param name="order">Order dto object</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the order result of the 3PL central
        /// </returns>
        public async Task<OrderResultDto> CreateOrder(string accessToken, OrderDto order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order), "Order 3PL cannot be null for this call");

            if (string.IsNullOrEmpty(accessToken))
                throw new ArgumentNullException(nameof(accessToken), "AccessToken should be provided from calling code for each request. Consider caching the token from authenticate request");

            var client = new RestClient(BASE_URL);
            var request = new RestRequest(string.Format(ORDER_CREATE_END_POINT)) { Method = Method.POST };

            request.AddCommonHeaders(accessToken);
            request.AddParameter("application/json", JsonConvert.SerializeObject(order, new DefaultJsonSerializer()), ParameterType.RequestBody);

            var response = client.Execute(request);
            var responseContent = response.Content;
            var responseKeys = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseContent);

            var checkReposnseForError = responseKeys.ContainsKey("ErrorCode") ? responseKeys.GetValueOrDefault("ErrorCode", string.Empty).ToString() : string.Empty;
            if (!string.IsNullOrEmpty(checkReposnseForError) && (checkReposnseForError.Equals("Invalid") || checkReposnseForError.Equals("ValueNotSupported")))
                throw new InvalidOperationException(responseContent);

            if (!string.IsNullOrEmpty(checkReposnseForError))
                return await Task.FromResult(new OrderResultDto { ErrorMessage = responseContent });

            return await Task.FromResult(JsonConvert.DeserializeObject<OrderResultDto>(responseContent));
        }

        /// <summary>
        /// Cancel an order to the 3PL central
        /// </summary>
        /// <param name="accessToken">Access token</param>
        /// <param name="orderId">order identifier</param>
        /// <param name="order">Order cancel dto object</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result of the 3PL central
        /// </returns>
        public async Task<bool> CancelOrder(string accessToken, int orderId, OrderCancelDto order)
        {
            if (string.IsNullOrEmpty(accessToken))
                throw new ArgumentException("AccessToken should be provided from calling code for each request. Consider caching the token from authenticate request", nameof(accessToken));

            if (orderId == 0)
                throw new ArgumentException("Invalid or Missing OrderId argument");

            if (order == null)
                throw new ArgumentException("No order provided", nameof(order));

            var client = new RestClient(BASE_URL);
            var request = new RestRequest(string.Format(ORDER_CANCEL_END_POINT, orderId)) { Method = Method.POST };
            request.AddCommonHeaders(accessToken);
            request.AddHeader("If-Match", await GetOrderETagDetails(accessToken, orderId));
            request.AddParameter("application/json", JsonConvert.SerializeObject(order, new DefaultJsonSerializer()), ParameterType.RequestBody);

            var _ = client.Execute(request);

            return await Task.FromResult(true);
        }

        /// <summary>
        /// Get an order detail from the 3PL central
        /// </summary>
        /// <param name="accessToken">Access token</param>
        /// <param name="orderId">Order identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the order of the 3PL central
        /// </returns>
        public async Task<OrderResultDto> GetOrderDetails(string accessToken, int orderId)
        {
            if (string.IsNullOrEmpty(accessToken))
                throw new ArgumentNullException(nameof(accessToken), "AccessToken should be provided from calling code for each request. Consider caching the token from authenticate request");

            if (orderId == 0)
                throw new ArgumentException("Invalid or Missing OrderId argument");

            var client = new RestClient(BASE_URL);
            var request = new RestRequest(string.Format(ORDER_DETAILS_END_POINT, orderId)) { Method = Method.GET };

            request.AddCommonHeaders(accessToken);
            var response = client.Execute(request);
            var responseContent = response.Content;

            return await Task.FromResult(JsonConvert.DeserializeObject<OrderResultDto>(responseContent));
        }

        /// <summary>
        /// Get an order items detail from the 3PL central
        /// </summary>
        /// <param name="accessToken">Access token</param>
        /// <param name="orderId">Order identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the order items of the 3PL central
        /// </returns>
        public async Task<OrderItemsResultDto> GetOrderItemsDetails(string accessToken, int orderId)
        {
            if (string.IsNullOrEmpty(accessToken))
                throw new ArgumentNullException(nameof(accessToken), "AccessToken should be provided from calling code for each request. Consider caching the token from authenticate request");

            if (orderId == 0)
                throw new ArgumentException("Invalid or Missing OrderId argument");

            var client = new RestClient(BASE_URL);
            var request = new RestRequest(string.Format(ORDER_ITEMS_DETAILS_END_POINT, orderId)) { Method = Method.GET };

            request.AddCommonHeaders(accessToken);
            var response = client.Execute(request);
            var responseContent = response.Content;

            return await Task.FromResult(JsonConvert.DeserializeObject<OrderItemsResultDto>(responseContent));
        }

        /// <summary>
        /// Sync an orders to the 3PL central
        /// </summary>
        /// <param name="accessToken">Access token</param>
        /// <param name="orders">Order identifiers</param>
        /// <param name="manualSync">Whether it's a force to synchronization manually</param>
        /// <returns>
        /// The task result contains the synced orders identifier
        /// </returns>
        public async Task<IList<int>> SyncOrders(string accessToken, List<ThreePlRecord> orders, bool manualSync = false)
        {
            //get login customer
            var loginCustomer = await _workContext.GetCurrentCustomerAsync();

            //add impersonated validation rules from generic attribute table.
            if (_workContext.OriginalCustomerIfImpersonated != null)
                await _genericAttributeService.SaveAttributeAsync(loginCustomer, "ImpersonatedValidation", true);

            var result = new List<int>();

            foreach (var order3PL in orders)
            {
                var warnings = new List<string>();
                var nopOrder = await _orderService.GetOrderByIdAsync(order3PL.OrderId);

                //order status 
                if (nopOrder.OrderStatus == OrderStatus.Cancelled)
                    continue;

                //3pl order status
                if (order3PL.ThreePlStutus == ThreePlStutus.Cancel && !order3PL.CancelledDate.HasValue)
                    continue;

                //validate by interval time
                if (!manualSync)
                {
                    if (nopOrder.CreatedOnUtc > DateTime.UtcNow.AddHours(-_fulfillmen3PLtSettings.SendOrderHoursInterval))
                        continue;

                    //validate by paid by check
                    if (order3PL.PaidByCheck && nopOrder.PaymentStatus != PaymentStatus.Paid)
                        continue;
                }

                //shipping Address
                var shippingAddress = await _addressService.GetAddressByIdAsync(nopOrder.ShippingAddressId ?? 0);
                if (shippingAddress == null)
                    warnings.Add($"ShippingAddress is null");

                if (shippingAddress != null)
                {
                    if (!shippingAddress.StateProvinceId.HasValue)
                        warnings.Add($"ShippingAddress.StateProvince is null");
                }

                //billing Address
                var billingAddress = await _addressService.GetAddressByIdAsync(nopOrder.BillingAddressId);
                if (billingAddress == null)
                    warnings.Add($"BillingAddress is null");

                if (billingAddress != null)
                {
                    if (!billingAddress.StateProvinceId.HasValue)
                        warnings.Add($"BillingAddress.StateProvince is null");
                }

                if (warnings.Any())
                {
                    var note = string.Join("<br />", warnings);
                    order3PL.Note = note;
                    await _threePlService.UpdateThreePlRecordAsync(order3PL);

                    await _orderService.InsertOrderNoteAsync(new OrderNote
                    {
                        OrderId = nopOrder.Id,
                        CreatedOnUtc = DateTime.UtcNow,
                        DisplayToCustomer = false,
                        Note = $"3PL Central - {note}"
                    });

                    continue;
                }
                else
                {
                    //customer, store and orderitems object
                    var customer = await _customerService.GetCustomerByIdAsync(nopOrder.CustomerId);
                    var store = await _storeService.GetStoreByIdAsync(nopOrder.StoreId);
                    var orderItems = await _orderService.GetOrderItemsAsync(nopOrder?.Id ?? order3PL.OrderId);
                    var threePlOrders = await _threePlService.GetThreePlOrdersByOrderIdAsync(nopOrder?.Id ?? order3PL.OrderId);

                    //order items to sync
                    var nopItems = await _genericAttributeService.GetAttributeAsync<List<SyncOrderItem>>(nopOrder, "SyncOrderItem", nopOrder.StoreId);

                    //retrive already synced order to check is there any missing item
                    if (nopItems == null || !nopItems.Any())
                    {
                        nopItems = new List<SyncOrderItem>();

                        //enable validates a product for standard properties
                        await _genericAttributeService.SaveAttributeAsync(customer, "ImpersonatedValidation", true);
                        var impersonatedOrder = false;
                        var orderNotes = await _orderService.GetOrderNotesByOrderIdAsync(nopOrder.Id);
                        if (orderNotes.Any())
                            impersonatedOrder = orderNotes.Any(on => on.Note.Contains("Order placed by a store owner ("));

                        //order items filter for sync
                        foreach (var orderItem in orderItems)
                        {
                            var syncOrderItem = await ValidateProductAsync(orderItem, customer, store, impersonatedOrder);
                            if (syncOrderItem != null)
                            {
                                //save order items
                                if (syncOrderItem.InStockItems.Any() || syncOrderItem.OutOfStockItems.Any() || !string.IsNullOrWhiteSpace(syncOrderItem.Note))
                                {
                                    //retrive already synced items and filter with order items
                                    foreach (var threePlOrder in threePlOrders)
                                    {
                                        var orderItem3pl = await GetOrderItemsDetails(accessToken, threePlOrder.ThreePlOrderId);
                                        if (orderItem3pl.Items != null)
                                            syncOrderItem = syncOrderItem.FilterAlreadySyncedItems(orderItem3pl);
                                    }

                                    nopItems.Add(syncOrderItem);
                                }
                            }
                        }

                        //remove validates a product for standard properties
                        var validationValue = (await _genericAttributeService.GetAttributesForEntityAsync(customer.Id, customer.GetType().Name))
                            .Where(x => x.Key.Equals("ImpersonatedValidation"))
                            .FirstOrDefault();
                        if (validationValue != null)
                            await _genericAttributeService.DeleteAttributeAsync(validationValue);

                        if (!nopItems.Any(ni => ni.InStockItems.Any()) && !nopItems.Any(ni => ni.OutOfStockItems.Any()))
                        {
                            order3PL.ThreePlStutusId = (int)ThreePlStutus.Sent;
                            order3PL.Note = $"3PL Central - Synced successfully.";
                            await _threePlService.UpdateThreePlRecordAsync(order3PL);
                            continue;
                        }

                        //save order items
                        if (nopItems.Any(ni => ni.InStockItems.Any()) || nopItems.Any(ni => ni.OutOfStockItems.Any()))
                        {
                            await _genericAttributeService.SaveAttributeAsync(nopOrder, "SyncOrderItem", nopItems, nopOrder.StoreId);
                            if (nopItems.Any(ni => ni.OutOfStockItems.Any()))
                            {
                                order3PL.ThreePlStutusId = threePlOrders.Any() ? (int)ThreePlStutus.PartiallySent : (int)ThreePlStutus.PendingStock;
                                order3PL.Note = nopItems.Any(ni => !string.IsNullOrEmpty(ni.Note)) ? string.Join(" ", nopItems.Select(ni => ni.Note)) : null;
                                await _threePlService.UpdateThreePlRecordAsync(order3PL);
                                continue;
                            }
                        }

                        //remove validates a product for standard properties
                        //var validationValue = (await _genericAttributeService.GetAttributesForEntityAsync(customer.Id, customer.GetType().Name))
                        //    .Where(x => x.Key.Equals("ImpersonatedValidation"))
                        //    .FirstOrDefault();
                        //if (validationValue != null)
                        //    await _genericAttributeService.DeleteAttributeAsync(validationValue);
                    }

                    var packages = new Dictionary<string, List<SyncOrderItem>>();
                    var outOfStockItems = nopItems.Where(ni => ni.OutOfStockItems.Any()).ToList();

                    var needToSplit = nopItems.Count() > 1 && orderItems.Count(oi => !string.IsNullOrEmpty(oi.AttributesXml)) > 1 && _fulfillmen3PLtSettings.MultiPackageForCountries.Contains(shippingAddress.CountryId.HasValue ? shippingAddress.CountryId.Value : 0) &&
                        orderItems.Any(oi => !string.IsNullOrEmpty(oi.AttributesXml) && nopItems.Any(ni => ni.OrderItemId == oi.Id) && oi.PriceExclTax > _fulfillmen3PLtSettings.MultiPackageOrderAmountOver) ? !threePlOrders.Any() : false;
                    if (needToSplit)
                    {
                        var outofstockOrderItemIds = outOfStockItems.Select(ni => ni.OrderItemId).ToList();
                        var kitProducts = orderItems.Where(oi => !string.IsNullOrEmpty(oi.AttributesXml) && !outofstockOrderItemIds.Contains(oi.Id) && nopItems.Any(ni => ni.OrderItemId == oi.Id))
                            .OrderByDescending(oi => oi.PriceExclTax).ToList();

                        var tempPackage = new List<SyncOrderItem>();
                        var tempAmount = decimal.Zero;
                        foreach (var kitProduct in kitProducts)
                        {
                            if (kitProduct.PriceExclTax > _fulfillmen3PLtSettings.MultiPackageOrderAmountOver)
                                packages.Add(packages.Count() == 0 ? $"{nopOrder.CustomOrderNumber}" : $"{nopOrder.CustomOrderNumber} - {packages.Count()}", new List<SyncOrderItem> { nopItems.FirstOrDefault(ni => ni.OrderItemId == kitProduct.Id) });
                            else
                            {
                                tempPackage.Add(nopItems.FirstOrDefault(ni => ni.OrderItemId == kitProduct.Id));
                                tempAmount += kitProduct.PriceExclTax;
                                if (tempAmount > _fulfillmen3PLtSettings.MultiPackageOrderAmountOver)
                                {
                                    packages.Add(packages.Count() == 0 ? $"{nopOrder.CustomOrderNumber}" : $"{nopOrder.CustomOrderNumber} - {packages.Count()}", tempPackage);

                                    //reset package values 
                                    tempPackage = new List<SyncOrderItem>();
                                    tempAmount = decimal.Zero;
                                }
                            }
                        }

                        var nonKitProductIds = orderItems.Where(oi => string.IsNullOrEmpty(oi.AttributesXml) && !outofstockOrderItemIds.Contains(oi.Id) && nopItems.Any(ni => ni.OrderItemId == oi.Id)).Select(oi => oi.Id).ToList();
                        var nonKitProductSum = orderItems.Where(oi => string.IsNullOrEmpty(oi.AttributesXml) && !outofstockOrderItemIds.Contains(oi.Id) && nopItems.Any(ni => ni.OrderItemId == oi.Id)).Select(oi => oi.PriceExclTax).Sum();

                        // insert package if not exclude limit
                        if (tempPackage.Any())
                        {
                            if (tempAmount > _fulfillmen3PLtSettings.NoSplitAmountLess)
                                packages.Add(packages.Count() == 0 ? $"{nopOrder.CustomOrderNumber}" : $"{nopOrder.CustomOrderNumber} - {packages.Count()}", tempPackage);
                            else
                            {
                                if (packages.Count() == 0)
                                    packages.Add(packages.Count() == 0 ? $"{nopOrder.CustomOrderNumber}" : $"{nopOrder.CustomOrderNumber} - {packages.Count()}", tempPackage);
                                else
                                {
                                    var lastPackage = packages.LastOrDefault();
                                    lastPackage.Value.AddRange(tempPackage);
                                    packages[lastPackage.Key] = lastPackage.Value;
                                }
                            }

                            //reset package values 
                            tempPackage = new List<SyncOrderItem>();
                            tempAmount = decimal.Zero;
                        }

                        if (nonKitProductSum > _fulfillmen3PLtSettings.MultiPackageOrderAmountOver)
                        {
                            if (packages.Count() == 0)
                                packages.Add($"{nopOrder.CustomOrderNumber}", nopItems.Where(ni => nonKitProductIds.Contains(ni.OrderItemId)).ToList());
                            else
                            {
                                var lastPackage = packages.LastOrDefault();
                                if (lastPackage.Value.Sum(p => p.InStockItems.Sum(oi => oi.FulfillInvSalePrice)) < _fulfillmen3PLtSettings.MultiPackageOrderAmountOver)
                                {
                                    lastPackage.Value.AddRange(nopItems.Where(ni => nonKitProductIds.Contains(ni.OrderItemId)).ToList());
                                    packages[lastPackage.Key] = lastPackage.Value;
                                }
                                else
                                    packages.Add(packages.Count() == 0 ? $"{nopOrder.CustomOrderNumber}" : $"{nopOrder.CustomOrderNumber} - {packages.Count()}", nopItems.Where(ni => nonKitProductIds.Contains(ni.OrderItemId)).ToList());
                            }
                        }
                        else
                        {
                            if (packages.Count() == 0)
                                packages.Add($"{nopOrder.CustomOrderNumber}", nopItems.Where(ni => nonKitProductIds.Contains(ni.OrderItemId)).ToList());
                            else
                            {
                                var lastPackage = packages.LastOrDefault();
                                lastPackage.Value.AddRange(nopItems.Where(ni => nonKitProductIds.Contains(ni.OrderItemId)).ToList());
                                packages[lastPackage.Key] = lastPackage.Value;
                            }
                        }
                    }
                    else
                        packages.Add($"{nopOrder.CustomOrderNumber}", nopItems.Where(ni => !ni.OutOfStockItems.Any()).ToList());

                    if (threePlOrders.Any())
                    {
                        order3PL.ThreePlStutusId = (int)ThreePlStutus.PartiallySent;
                        packages.ForEach(p => p.Value.ForEach(pv => outOfStockItems.Add(pv)));
                        packages = new Dictionary<string, List<SyncOrderItem>>();
                    }
                    else
                    {
                        if (manualSync && order3PL.ThreePlStutusId == (int)ThreePlStutus.PendingStock)
                        {
                            foreach (var outOfStockItem in outOfStockItems)
                            {
                                var outOfStocks = new List<OrdersItemDto>();
                                var notes = new List<string>();
                                foreach (var outOfStock in outOfStockItem.OutOfStockItems)
                                {
                                    var product = await _productService.GetProductByIdAsync(Convert.ToInt32(outOfStock.ExternalId));
                                    if (product.WarehouseId != 1)
                                        continue;

                                    var maximumQuantityCanBeAdded = await _productService.GetTotalStockQuantityAsync(product);
                                    if (maximumQuantityCanBeAdded >= (int)outOfStock.Qty)
                                        outOfStockItem.InStockItems.AddOrUpdateRecords(outOfStock);
                                    else
                                    {
                                        if (maximumQuantityCanBeAdded <= 0)
                                        {
                                            var productAvailabilityRange = await _dateRangeService.GetProductAvailabilityRangeByIdAsync(product.ProductAvailabilityRangeId);
                                            var warning = productAvailabilityRange == null ? await _localizationService.GetResourceAsync("ShoppingCart.OutOfStock")
                                                : string.Format(await _localizationService.GetResourceAsync("ShoppingCart.AvailabilityRange"),
                                                    await _localizationService.GetLocalizedAsync(productAvailabilityRange, range => range.Name));
                                            if (!string.IsNullOrEmpty(warning) && !notes.Contains($"{product?.Name} is {warning}. "))
                                                notes.Add($"{product?.Name} is {warning}. ");
                                        }
                                        else
                                            notes.Add($"{product?.Name} is {string.Format(await _localizationService.GetResourceAsync("ShoppingCart.QuantityExceedsStock"), maximumQuantityCanBeAdded)}. ");

                                        outOfStocks.Add(outOfStock);
                                    }
                                }

                                if (outOfStocks.Any())
                                {
                                    outOfStockItem.OutOfStockItems = outOfStocks;
                                    outOfStockItem.Note = string.Join(" ", notes);
                                }
                                else
                                {
                                    outOfStockItem.OutOfStockItems = new List<OrdersItemDto>();
                                    outOfStockItem.Note = string.Empty;
                                }

                                //update default list with update values
                                nopItems.ForEach(ni =>
                                {
                                    if (ni.OrderItemId == outOfStockItem.OrderItemId)
                                        ni = outOfStockItem;
                                });
                            }

                            if (packages.Count() == 0)
                                packages.Add($"{nopOrder.CustomOrderNumber}", outOfStockItems);
                            else
                            {
                                if (needToSplit && outOfStockItems.Sum(os => os.InStockItems.Sum(oi => oi.FulfillInvSalePrice)) > _fulfillmen3PLtSettings.MultiPackageOrderAmountOver)
                                    packages.Add($"{nopOrder.CustomOrderNumber} - P", outOfStockItems);
                                else
                                {
                                    var lastPackage = packages.LastOrDefault();
                                    lastPackage.Value.AddRange(outOfStockItems);
                                    packages[lastPackage.Key] = lastPackage.Value;

                                    changePackageKey(lastPackage.Key, $"{nopOrder.CustomOrderNumber} - P", packages);
                                    void changePackageKey(string existingKey, string newKey, Dictionary<string, List<SyncOrderItem>> dictionary)
                                    {
                                        if (dictionary.TryGetValue(existingKey, out List<SyncOrderItem> existingValue))//Check if the key exists in the dictionary before trying to remove it and get the existing value to save in a temp variable
                                        {
                                            dictionary.Remove(existingKey);//Delete the existing key and value
                                            dictionary.Add(newKey, existingValue);//Add the new key with the previous value into a new key-value pair
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (order3PL.ThreePlStutusId == (int)ThreePlStutus.PartiallySent)
                    {
                        var backInStocks = new List<OrdersItemDto>();
                        foreach (var outOfStockItem in outOfStockItems)
                        {
                            foreach (var outstock in outOfStockItem.OutOfStockItems)
                            {
                                var product = await _productService.GetProductByIdAsync(Convert.ToInt32(outstock.ExternalId));
                                if (product.WarehouseId != 1)
                                    continue;

                                var maximumQuantityCanBeAdded = await _productService.GetTotalStockQuantityAsync(product);
                                if (maximumQuantityCanBeAdded >= (int)outstock.Qty)
                                    backInStocks.Add(outstock);
                            }
                        }

                        if (outOfStockItems.Sum(os => os.OutOfStockItems.Count()) == backInStocks.Count)
                        {
                            var backInStockItems = new List<SyncOrderItem>();
                            outOfStockItems.ForEach(instock =>
                            {
                                var backInStockItem = new SyncOrderItem() { OrderItemId = instock.OrderItemId, InStockItems = instock.InStockItems };
                                if (instock.OutOfStockItems.Any())
                                {
                                    backInStockItem.InStockItems.AddRange(instock.OutOfStockItems);
                                    backInStockItem.OutOfStockItems = new List<OrdersItemDto>();
                                }

                                backInStockItems.Add(backInStockItem);

                                //update default list with update values
                                nopItems.ForEach(ni =>
                                {
                                    if (ni.OrderItemId == backInStockItem.OrderItemId)
                                        ni = backInStockItem;
                                });
                            });

                            packages.Add($"{nopOrder.CustomOrderNumber} - B", backInStockItems);
                        }
                    }

                    var stockWarings = nopItems.Where(ni => ni.OutOfStockItems.Any()).Select(ni => ni.Note).ToList();
                    if (!packages.Any())
                    {
                        if (outOfStockItems.Any())
                        {
                            if (threePlOrders.Any())
                                order3PL.Note = $"3PL Central - Order is partially synced. {string.Join(",", stockWarings)}";
                            else
                                order3PL.Note = $"3PL Central - Does not have products to sync. {string.Join(",", stockWarings)}";

                            await _threePlService.UpdateThreePlRecordAsync(order3PL);
                            continue;
                        }
                        else
                        {
                            order3PL.ThreePlStutusId = (int)ThreePlStutus.NotRequire;
                            order3PL.Note = "3PL Central - Does not have products to sync.";
                            await _threePlService.UpdateThreePlRecordAsync(order3PL);
                            continue;
                        }
                    }

                    //var poNumber = string.Empty;
                    //if (!string.IsNullOrEmpty(nopOrder.PaymentMethodSystemName))
                    //{
                    //    var customValues = await _genericAttributeService.GetAttributeAsync<Dictionary<string, object>>(nopOrder, "CustomValues", nopOrder.StoreId);
                    //    var poKey = await _localizationService.GetResourceAsync("Plugins.Payment.PurchaseOrder.PurchaseOrderNumber");
                    //    if (customValues.ContainsKey(poKey))
                    //        poNumber = customValues.GetValueOrDefault(poKey, string.Empty).ToString();
                    //}

                    var poNumber = string.Empty;

                    if (nopOrder?.PaymentMethodSystemName != null)
                    {
                        var customValues = await _genericAttributeService.GetAttributeAsync<Dictionary<string, object>>(nopOrder, "CustomValues", nopOrder.StoreId)
                                             ?? new Dictionary<string, object>();

                        var poKey = await _localizationService.GetResourceAsync("Plugins.Payment.PurchaseOrder.PurchaseOrderNumber");

                        if (!string.IsNullOrEmpty(poKey) && customValues.ContainsKey(poKey))
                        {
                            poNumber = customValues[poKey]?.ToString() ?? string.Empty;
                        }
                    }

                    var carrier = nopOrder.ShippingMethod;
                    var mode = carrier;
                    var threePlShipping = await _threePlService.GetThreePlShippingMethodByShippingMethodAsync(nopOrder.ShippingMethod);
                    if (threePlShipping != null)
                    {
                        carrier = threePlShipping.ThreePlCarrier;
                        mode = threePlShipping.ThreePlService;
                    }

                    try
                    {
                        var order3pl = new OrderDto
                        {
                            BillingCode = "Prepaid",
                            CustomerIdentifier = new OrderDto.CustomerDto { Id = _fulfillmen3PLtSettings.CustomerId },
                            FacilityIdentifier = new OrderDto.FacilityDto { Id = _fulfillmen3PLtSettings.FacilityId },
                            ReferenceNum = order3PL.ThreePlStutusId != (int)ThreePlStutus.PartiallySent ? nopOrder.CustomOrderNumber : $"{nopOrder.CustomOrderNumber} - B",
                            PoNum = poNumber,
                            FulfillInvInfo = new OrderDto.FulfillInvInfoDto { FulfillInvDiscountAmount = nopOrder.OrderDiscount },
                            RoutingInfo = new OrderDto.RoutingDto { Carrier = carrier, Mode = mode },
                            ShipTo = new OrderDto.AddressDto
                            {
                                CompanyName = shippingAddress.Company,
                                Name = $"{shippingAddress.FirstName} {shippingAddress.LastName}",
                                Address1 = shippingAddress.Address1,
                                Address2 = shippingAddress.Address2,
                                City = shippingAddress.City,
                                State = (await _stateProvinceService.GetStateProvinceByIdAsync(shippingAddress.StateProvinceId ?? 0)).Abbreviation,
                                Zip = shippingAddress.ZipPostalCode,
                                Country = (await _countryService.GetCountryByIdAsync(shippingAddress.CountryId ?? 0)).TwoLetterIsoCode,
                                EmailAddress = shippingAddress.Email,
                                PhoneNumber = shippingAddress.PhoneNumber,
                                Fax = shippingAddress.FaxNumber,
                            },
                            BillTo = new OrderDto.AddressDto
                            {
                                CompanyName = billingAddress.Company,
                                Name = $"{billingAddress.FirstName} {billingAddress.LastName}",
                                Address1 = billingAddress.Address1,
                                Address2 = billingAddress.Address2,
                                City = billingAddress.City,
                                State = (await _stateProvinceService.GetStateProvinceByIdAsync(billingAddress.StateProvinceId ?? 0)).Abbreviation,
                                Zip = billingAddress.ZipPostalCode,
                                Country = (await _countryService.GetCountryByIdAsync(billingAddress.CountryId ?? 0)).TwoLetterIsoCode,
                                EmailAddress = billingAddress.Email,
                                PhoneNumber = billingAddress.PhoneNumber,
                                Fax = billingAddress.FaxNumber,
                            },
                        };

                        var order3plIds = new List<int>();
                        foreach (var package in packages)
                        {
                            //Use to identify the Order in 3PL Central
                            order3pl.ReferenceNum = package.Key;

                            //allocate Items 
                            var syncItems = new List<OrdersItemDto>();
                            package.Value.ForEach(v => v.InStockItems.ForEach(instock => syncItems.AddOrUpdateRecords(instock)));

                            //allocate Items to new orders.
                            order3pl.Items = new OrderDto.OrderItems { Items = syncItems.ToArray() };

                            var response = await CreateOrder(accessToken, order3pl);
                            if (!string.IsNullOrEmpty(response.ErrorMessage))
                            {
                                order3PL.ThreePlStutusId = (int)ThreePlStutus.Error;
                                order3PL.Note = $"3PL Central - {response.ErrorMessage}.";
                                await _threePlService.UpdateThreePlRecordAsync(order3PL);

                                await _orderService.InsertOrderNoteAsync(new OrderNote
                                {
                                    OrderId = nopOrder.Id,
                                    CreatedOnUtc = DateTime.UtcNow,
                                    DisplayToCustomer = false,
                                    Note = $"3PL Central - {response.ErrorMessage}."
                                });
                            }
                            else
                            {
                                if (response.ReadOnly != null)
                                {
                                    await _threePlService.InsertThreePlOrderAsync(new ThreePlOrder()
                                    {
                                        ThreePlOrderId = response.ReadOnly.OrderId,
                                        ThreePlId = order3PL.Id,
                                        ThreePlStutusId = (int)ThreePlStutus.Sent,
                                        CreationDate = DateTime.UtcNow,
                                        FulfillInvTotal = syncItems.Sum(s => s.FulfillInvSalePrice)
                                    });

                                    order3plIds.Add(response.ReadOnly.OrderId);

                                    await _orderService.InsertOrderNoteAsync(new OrderNote
                                    {
                                        OrderId = nopOrder.Id,
                                        CreatedOnUtc = DateTime.UtcNow,
                                        DisplayToCustomer = false,
                                        Note = $"3PL Central - 3PL#: {response.ReadOnly.OrderId} Synced successfully"
                                    });

                                    nopItems.ForEach(ni =>
                                    {
                                        package.Value.Where(v => v.OrderItemId == ni.OrderItemId).ForEach(v =>
                                        {
                                            v.InStockItems.ForEach(instock => ni.InStockItems.RemoveOrUpdateRecords(instock));
                                        });

                                        ni.InStockItems.RemoveAll(instock => instock.Qty == 0);
                                        ni.OutOfStockItems.RemoveAll(outofstock => outofstock.Qty == 0);
                                    });

                                    nopItems = nopItems.Where(ni => ni.InStockItems.Any() || ni.OutOfStockItems.Any()).ToList();

                                    //save validates a product for standard properties
                                    await _genericAttributeService.SaveAttributeAsync(nopOrder, "SyncOrderItem", nopItems, nopOrder.StoreId);
                                }
                            }
                        }

                        if (order3plIds.Any())
                            result.Add(order3PL.OrderId);

                        var outOfStockProducts = nopItems.Any(ni => ni.OutOfStockItems.Any());
                        order3PL.ThreePlStutusId = outOfStockProducts ? (int)ThreePlStutus.PartiallySent : (int)ThreePlStutus.Sent;
                        if (outOfStockProducts)
                            order3PL.Note = $"3PL Central - Order is partially synced. {string.Join(", ", stockWarings)}";
                        else
                            order3PL.Note = $"3PL Central - Synced successfully.";

                        await _threePlService.UpdateThreePlRecordAsync(order3PL);
                        if (order3PL.ThreePlStutus == ThreePlStutus.Sent)
                        {
                            foreach (var threePlOrder in threePlOrders)
                            {
                                threePlOrder.ThreePlStutusId = (int)ThreePlStutus.Sent;
                                await _threePlService.UpdateThreePlOrderAsync(threePlOrder);
                            }
                        }

                        await _orderService.InsertOrderNoteAsync(new OrderNote
                        {
                            OrderId = nopOrder.Id,
                            CreatedOnUtc = DateTime.UtcNow,
                            DisplayToCustomer = false,
                            Note = order3PL.Note
                        });

                        if (nopItems.Any(ni => ni.InStockItems.Any()) || nopItems.Any(ni => ni.OutOfStockItems.Any()))
                        {
                            //save validates a product for standard properties
                            await _genericAttributeService.SaveAttributeAsync(nopOrder, "SyncOrderItem", nopItems, nopOrder.StoreId);
                        }
                        else
                        {
                            //remove validates a product for standard properties
                            var removeValue = (await _genericAttributeService.GetAttributesForEntityAsync(nopOrder.Id, nopOrder.GetType().Name))
                                .Where(x => x.Key.Equals("SyncOrderItem"))
                                .FirstOrDefault();
                            if (removeValue != null)
                                await _genericAttributeService.DeleteAttributeAsync(removeValue);
                        }
                    }
                    catch (Exception exc)
                    {
                        await _logger.ErrorAsync($"3PL Central - Order#: {nopOrder.Id}. {exc.Message}  {exc.Source}", exc);
                    }
                }
            }

            //remove impersonated validation rules from generic attribute table.
            if (_workContext.OriginalCustomerIfImpersonated != null)
            {
                var keyGroup = loginCustomer.GetType().Name;
                var validationValue = (await _genericAttributeService.GetAttributesForEntityAsync(loginCustomer.Id, keyGroup))
                    .Where(x => x.Key.Equals("ImpersonatedValidation"))
                    .FirstOrDefault();
                if (validationValue != null)
                    await _genericAttributeService.DeleteAttributeAsync(validationValue);
            }

            return result;
        }

        /// <summary>
        /// Sync cancel orders to the 3PL central
        /// </summary>
        /// <param name="accessToken">Access token</param>
        /// <param name="orders">Order identifiers</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// </returns>
        public async Task SyncCancelOrders(string accessToken, List<ThreePlRecord> orders)
        {
            foreach (var order in orders)
            {
                var nopOrder = await _orderService.GetOrderByIdAsync(order.OrderId);
                var offset = TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow);

                try
                {
                    var order3PLs = await _threePlService.GetThreePlOrdersByOrderIdAsync(order.OrderId);
                    foreach (var order3PL in order3PLs)
                    {
                        var cancel = new OrderCancelDto
                        {
                            Charge = order3PL.FulfillInvTotal,
                            Reason = "cancel from admin",
                            InvoiceCreationInfo = new OrderCancelDto.InvoiceCreationDto { SetInvoiceDate = false, UtcOffset = offset.Hours }
                        };

                        await CancelOrder(accessToken, order3PL.ThreePlOrderId, cancel);

                        await _orderService.InsertOrderNoteAsync(new OrderNote
                        {
                            OrderId = nopOrder.Id,
                            CreatedOnUtc = DateTime.UtcNow,
                            DisplayToCustomer = false,
                            Note = $"3PL Central - 3PL#: {order3PL.ThreePlOrderId} Cancelled successfully"
                        });
                    }

                    //Update history log
                    order.ThreePlStutusId = (int)ThreePlStutus.Cancel;
                    order.Note = "3PL Central - Cancelled successfully";
                    await _threePlService.UpdateThreePlRecordAsync(order);

                    await _orderService.InsertOrderNoteAsync(new OrderNote
                    {
                        OrderId = nopOrder.Id,
                        CreatedOnUtc = DateTime.UtcNow,
                        DisplayToCustomer = false,
                        Note = "3PL Central - Cancelled successfully"
                    });

                    //remove validates a product for standard properties
                    var removeValue = (await _genericAttributeService.GetAttributesForEntityAsync(nopOrder.Id, nopOrder.GetType().Name))
                        .Where(x => x.Key.Equals("SyncOrderItem"))
                        .FirstOrDefault();
                    if (removeValue != null)
                        await _genericAttributeService.DeleteAttributeAsync(removeValue);

                }
                catch (Exception exc)
                {
                    order.Note = exc.Message;
                    await _threePlService.UpdateThreePlRecordAsync(order);

                    await _logger.ErrorAsync($"3PL Central - Order#: {order.OrderId}. {exc.Message}", exc);
                }
            }
        }

        /// <summary>
        /// Validate product
        /// </summary>
        /// <param name="orderItem">OrderItem object</param>
        /// <param name="customer">Customer object</param>
        /// <param name="store">Store object</param>
        /// <param name="impersonatedOrder">Impersonated order</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the instock, outstock and error note of the 3PL central
        /// </returns>
        public async Task<SyncOrderItem> ValidateProductAsync(OrderItem orderItem, Customer customer, Store store, bool impersonatedOrder = false)
        {
            var outOfStockProducts = new List<OrdersItemDto>();
            var inStockProducts = new List<OrdersItemDto>();
            var notes = new List<string>();

            if (orderItem.Quantity == 0)
                return null;

            var product = await _productService.GetProductByIdAsync(orderItem.ProductId);
            if (product.WarehouseId != 1)
                return null;

            if (product.ManageInventoryMethodId == (int)ManageInventoryMethod.DontManageStock)
            {
                inStockProducts.AddOrUpdateRecords(new OrdersItemDto()
                {
                    ItemIdentifier = new OrdersItemDto.ItemDto { Sku = product.Sku },
                    ExternalId = $"{product.Id}",
                    Qty = orderItem.Quantity,
                    FulfillInvSalePrice = orderItem.PriceExclTax
                });

                return new SyncOrderItem { OrderItemId = orderItem.Id, InStockItems = inStockProducts, OutOfStockItems = outOfStockProducts, Note = string.Join(" ", notes) };
            }

            if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStock)
            {
                if (impersonatedOrder)
                {
                    var maximumQuantityCanBeAdded = await _productService.GetTotalStockQuantityAsync(product);
                    if (maximumQuantityCanBeAdded < orderItem.Quantity)
                    {
                        if (maximumQuantityCanBeAdded <= 0)
                        {
                            var productAvailabilityRange = await _dateRangeService.GetProductAvailabilityRangeByIdAsync(product.ProductAvailabilityRangeId);
                            var warning = productAvailabilityRange == null ? await _localizationService.GetResourceAsync("ShoppingCart.OutOfStock")
                                : string.Format(await _localizationService.GetResourceAsync("ShoppingCart.AvailabilityRange"),
                                    await _localizationService.GetLocalizedAsync(productAvailabilityRange, range => range.Name));
                            if (!string.IsNullOrEmpty(warning) && !notes.Contains($"{product?.Name} is {warning}. "))
                                notes.Add($"{product?.Name} is {warning}. ");
                        }
                        else
                            notes.Add($"{product?.Name} is {string.Format(await _localizationService.GetResourceAsync("ShoppingCart.QuantityExceedsStock"), maximumQuantityCanBeAdded)}. ");

                        outOfStockProducts.AddOrUpdateRecords(new OrdersItemDto()
                        {
                            ItemIdentifier = new OrdersItemDto.ItemDto { Sku = product.Sku },
                            ExternalId = $"{product.Id}",
                            Qty = orderItem.Quantity,
                            FulfillInvSalePrice = orderItem.PriceExclTax
                        });
                    }
                    else
                    {
                        inStockProducts.AddOrUpdateRecords(new OrdersItemDto()
                        {
                            ItemIdentifier = new OrdersItemDto.ItemDto { Sku = product.Sku },
                            ExternalId = $"{product.Id}",
                            Qty = orderItem.Quantity,
                            FulfillInvSalePrice = orderItem.PriceExclTax
                        });
                    }
                }
                else
                {
                    inStockProducts.AddOrUpdateRecords(new OrdersItemDto()
                    {
                        ItemIdentifier = new OrdersItemDto.ItemDto { Sku = product.Sku },
                        ExternalId = $"{product.Id}",
                        Qty = orderItem.Quantity,
                        FulfillInvSalePrice = orderItem.PriceExclTax
                    });
                }
            }

            var attributeValues = await _productAttributeParser.ParseProductAttributeValuesAsync(orderItem.AttributesXml);
            foreach (var attributeValue in attributeValues)
            {
                var warnings = new List<string>();

                if (attributeValue.AttributeValueType == AttributeValueType.Simple)
                    continue;

                var associatedProduct = await _productService.GetProductByIdAsync(attributeValue.AssociatedProductId);
                if (associatedProduct == null)
                    continue;

                if (associatedProduct.WarehouseId != 1)
                    continue;

                var totalQty = orderItem.Quantity * attributeValue.Quantity;
                if (impersonatedOrder)
                {
                    var cartItem = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id, orderItem.ProductId);
                    var associatedProductWarnings = await _shoppingCartService.GetShoppingCartItemWarningsAsync(customer,
                        ShoppingCartType.ShoppingCart, associatedProduct, store.Id,
                        string.Empty, decimal.Zero, null, null, totalQty, false, cartItem.FirstOrDefault()?.Id ?? 0, getRequiredProductWarnings: false);

                    var productAttributeMapping = await _productAttributeService.GetProductAttributeMappingByIdAsync(attributeValue.ProductAttributeMappingId);
                    var productAttribute = await _productAttributeService.GetProductAttributeByIdAsync(productAttributeMapping.ProductAttributeId);

                    foreach (var associatedProductWarning in associatedProductWarnings)
                    {
                        var attributeName = await _localizationService.GetLocalizedAsync(productAttribute, a => a.Name);
                        var attributeValueName = await _localizationService.GetLocalizedAsync(attributeValue, a => a.Name);
                        var warning = string.Format(
                            await _localizationService.GetResourceAsync("ShoppingCart.AssociatedAttributeWarning"),
                            attributeName, attributeValueName, associatedProductWarning);

                        if (!warnings.Contains(warning))
                            warnings.Add(warning);
                    }
                }

                if (warnings.Any())
                {
                    var warning = $"{product?.Name} with associated products ({string.Join(", ", warnings)}). ";
                    if (!notes.Contains(warning))
                        notes.Add(warning);

                    outOfStockProducts.AddOrUpdateRecords(new OrdersItemDto()
                    {
                        ItemIdentifier = new OrdersItemDto.ItemDto { Sku = associatedProduct.Sku },
                        ExternalId = $"{associatedProduct.Id}",
                        Qty = totalQty,
                        FulfillInvSalePrice = (await _priceCalculationService.GetFinalPriceAsync(associatedProduct, customer, store)).Item2 * totalQty
                    });
                }
                else
                {
                    inStockProducts.AddOrUpdateRecords(new OrdersItemDto()
                    {
                        ItemIdentifier = new OrdersItemDto.ItemDto { Sku = associatedProduct.Sku },
                        ExternalId = $"{associatedProduct.Id}",
                        Qty = totalQty,
                        FulfillInvSalePrice = (await _priceCalculationService.GetFinalPriceAsync(associatedProduct, customer, store)).Item2 * totalQty
                    });
                }
            }

            return new SyncOrderItem { OrderItemId = orderItem.Id, InStockItems = inStockProducts, OutOfStockItems = outOfStockProducts, Note = string.Join(" ", notes) };
        }

        #endregion

        #region Inventory

        /// <summary>
        /// Get product stock summary from the 3PL central
        /// </summary>
        /// <param name="accessToken">Access token</param>
        /// <param name="page">page number</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the inventory stock result of the 3PL central
        /// </returns>
        public Task<StockSummaryDto> GetItemStockSummary(string accessToken, string page)
        {
            if (string.IsNullOrEmpty(accessToken))
                throw new ArgumentNullException(nameof(accessToken), "AccessToken should be provided from calling code for each request. Consider caching the token from authenticate request");

            var client = new RestClient(BASE_URL);
            var request = new RestRequest(page) { Method = Method.GET };
            request.AddCommonHeaders(accessToken);

            var response = client.Execute(request);
            var responseContent = response.Content;

            return Task.FromResult(JsonConvert.DeserializeObject<StockSummaryDto>(responseContent));
        }

        #endregion

        #endregion
    }
}
