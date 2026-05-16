using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Events;
using Nop.Plugin.Misc.ShipmentTracking.Domain;
using Nop.Plugin.Misc.ShipmentTracking.Models.AdminNote;
using Nop.Plugin.Misc.ShipmentTracking.Models.OrderShipment;
using Nop.Plugin.Misc.ShipmentTracking.Services;
using Nop.Services.Attributes;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.ExportImport;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Shipping;
using Nop.Services.Shipping.Tracking;
using Nop.Services.Stores;
using Nop.Services.Vendors;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Orders;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Extensions;
using Nop.Web.Framework.Models.Extensions;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using System.Text;

namespace Nop.Plugin.Misc.ShipmentTracking.Controllers
{
    public class OrderShipmentController : OrderController
    {
        #region Constant 

        private const string ShippingMethod = "3PLCarrier";

        #endregion

        #region Fields

        protected readonly IMeasureService _measureService;
        protected readonly MeasureSettings _measureSettings;
        protected readonly IShipmentTrackingService _shipmentTrackingService;
        protected readonly IGenericAttributeService _genericAttributeService;
        protected readonly ICountryService _countryService;
        protected readonly ShippingSettings _shippingSettings;
        protected readonly IBaseAdminModelFactory _baseAdminModelFactory;
        protected readonly IStoreService _storeService;
        protected readonly IPriceFormatter _priceFormatter;
        protected readonly ICurrencyService _currencyService;
        protected readonly CurrencySettings _currencySettings;
        protected readonly AddressSettings _addressSettings;
        protected readonly CatalogSettings _catalogSettings;
        protected readonly IPaymentPluginManager _paymentPluginManager;
        protected readonly IStateProvinceService _stateProvinceService;
        protected readonly IPictureService _pictureService;
        protected readonly IVendorService _vendorService;
        protected readonly IAdminNoteService _adminNoteService;
        protected readonly IDownloadService _downloadService;
        protected readonly IWebHelper _webHelper;
        protected readonly ISettingService _settingService;
        protected readonly IWarehouseService _warehouseService;
        protected readonly CustomerSettings _customerSettings;

        #endregion

        #region Ctor

        public OrderShipmentController(IAddressService addressService,
            IAttributeParser<AddressAttribute, AddressAttributeValue> addressAttributeParser,
            ICustomerActivityService customerActivityService,
            ICustomerService customerService,
            IDateTimeHelper dateTimeHelper,
            IEncryptionService encryptionService,
            IEventPublisher eventPublisher,
            IExportManager exportManager,
            IGiftCardService giftCardService,
            IImportManager importManager,
            ILocalizationService localizationService,
            INotificationService notificationService,
            IOrderModelFactory orderModelFactory,
            IOrderProcessingService orderProcessingService,
            IOrderService orderService,
            IPaymentService paymentService,
            IPdfService pdfService,
            IPermissionService permissionService,
            IPriceCalculationService priceCalculationService,
            IProductAttributeFormatter productAttributeFormatter,
            IProductAttributeParser productAttributeParser,
            IProductAttributeService productAttributeService,
            IProductService productService,
            IShipmentService shipmentService,
            IShippingService shippingService,
            IShoppingCartService shoppingCartService,
            IStoreContext storeContext,
            IWorkContext workContext,
            IWorkflowMessageService workflowMessageService,
            OrderSettings orderSettings,
            IMeasureService measureService,
            MeasureSettings measureSettings,
            IShipmentTrackingService shipmentTrackingService,
            IGenericAttributeService genericAttributeService,
            ICountryService countryService,
            ShippingSettings shippingSettings,
            IBaseAdminModelFactory baseAdminModelFactory,
            IStoreService storeService,
            IPriceFormatter priceFormatter,
            ICurrencyService currencyService,
            CurrencySettings currencySettings,
            AddressSettings addressSettings,
            CatalogSettings catalogSettings,
            IPaymentPluginManager paymentPluginManager,
            IStateProvinceService stateProvinceService,
            IPictureService pictureService,
            IVendorService vendorService,
            IAdminNoteService adminNoteService,
            IDownloadService downloadService,
            IWebHelper webHelper,
            ISettingService settingService,
            IWarehouseService warehouseService,
            CustomerSettings customerSettings)
            : base(addressService, addressAttributeParser, customerActivityService, customerService, dateTimeHelper, encryptionService, eventPublisher, exportManager, giftCardService, importManager, localizationService, notificationService, orderModelFactory, orderProcessingService, orderService, paymentService, pdfService, permissionService, priceCalculationService, productAttributeFormatter, productAttributeParser, productAttributeService, productService, shipmentService, shippingService, shoppingCartService, storeContext, workContext, workflowMessageService, orderSettings)
        {
            _measureService = measureService;
            _measureSettings = measureSettings;
            _shipmentTrackingService = shipmentTrackingService;
            _genericAttributeService = genericAttributeService;
            _countryService = countryService;
            _shippingSettings = shippingSettings;
            _baseAdminModelFactory = baseAdminModelFactory;
            _storeService = storeService;
            _priceFormatter = priceFormatter;
            _currencyService = currencyService;
            _currencySettings = currencySettings;
            _addressSettings = addressSettings;
            _catalogSettings = catalogSettings;
            _paymentPluginManager = paymentPluginManager;
            _stateProvinceService = stateProvinceService;
            _pictureService = pictureService;
            _vendorService = vendorService;
            _adminNoteService = adminNoteService;
            _downloadService = downloadService;
            _webHelper = webHelper;
            _settingService = settingService;
            _warehouseService = warehouseService;
            _customerSettings = customerSettings;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Prepare shipment item model
        /// </summary>
        /// <param name="model">Shipment item model</param>
        /// <param name="orderItem">Order item</param>
        /// <param name="product">Product item</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task PrepareShipmentItemModelAsync(ShipmentItemModel model, OrderItem orderItem, Product product)
        {
            ArgumentNullException.ThrowIfNull(model);

            ArgumentNullException.ThrowIfNull(orderItem);

            ArgumentNullException.ThrowIfNull(product);

            if (orderItem.ProductId != product.Id)
                throw new ArgumentException($"{nameof(orderItem.ProductId)} != {nameof(product.Id)}");

            //fill in additional values (not existing in the entity)
            model.OrderItemId = orderItem.Id;
            model.ProductId = orderItem.ProductId;
            model.ProductName = product.Name;
            model.Sku = await _productService.FormatSkuAsync(product, orderItem.AttributesXml);
            model.AttributeInfo = orderItem.AttributeDescription;
            model.ShipSeparately = product.ShipSeparately;
            model.QuantityOrdered = orderItem.Quantity;
            model.QuantityInAllShipments = await _orderService.GetTotalNumberOfItemsInAllShipmentsAsync(orderItem);
            model.QuantityToAdd = await _orderService.GetTotalNumberOfItemsCanBeAddedToShipmentAsync(orderItem);

            var baseWeight = (await _measureService.GetMeasureWeightByIdAsync(_measureSettings.BaseWeightId))?.Name;
            var baseDimension = (await _measureService.GetMeasureDimensionByIdAsync(_measureSettings.BaseDimensionId))?.Name;
            if (orderItem.ItemWeight.HasValue)
                model.ItemWeight = $"{orderItem.ItemWeight:F2} [{baseWeight}]";
            model.ItemDimensions =
                $"{product.Length:F2} x {product.Width:F2} x {product.Height:F2} [{baseDimension}]";

            if (!product.IsRental)
                return;

            var rentalStartDate = orderItem.RentalStartDateUtc.HasValue
                ? _productService.FormatRentalDate(product, orderItem.RentalStartDateUtc.Value) : string.Empty;
            var rentalEndDate = orderItem.RentalEndDateUtc.HasValue
                ? _productService.FormatRentalDate(product, orderItem.RentalEndDateUtc.Value) : string.Empty;
            model.RentalInfo = string.Format(await _localizationService.GetResourceAsync("Order.Rental.FormattedDate"), rentalStartDate, rentalEndDate);
        }

        /// <summary>
        /// Prepare shipment status event models
        /// </summary>
        /// <param name="models">List of shipment status event models</param>
        /// <param name="shipment">Shipment</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task PrepareShipmentStatusEventModelsAsync(IList<ShipmentStatusEventModel> models, Shipment shipment)
        {
            ArgumentNullException.ThrowIfNull(models);

            var shipmentEvents = new List<ShipmentStatusEvent>();
            var shippingMethod = await _genericAttributeService.GetAttributeAsync<string>(shipment, ShippingMethod);
            shipmentEvents = (await _shipmentTrackingService.GetShipmentEventsAsync(shipment, shippingMethod)).ToList();

            if (shipmentEvents.Count == 0)
            {
                var shipmentTracker = await _shipmentService.GetShipmentTrackerAsync(shipment);
                if (shipmentTracker != null)
                {
                    shipmentEvents = (await shipmentTracker.GetShipmentEventsAsync(shipment.TrackingNumber)).ToList();
                    if (shipmentEvents == null)
                        return;
                }
            }

            foreach (var shipmentEvent in shipmentEvents)
            {
                var shipmentStatusEventModel = new ShipmentStatusEventModel
                {
                    Status = shipmentEvent.Status,
                    Date = shipmentEvent.Date,
                    EventName = shipmentEvent.EventName,
                    Location = shipmentEvent.Location
                };
                var shipmentEventCountry = await _countryService.GetCountryByTwoLetterIsoCodeAsync(shipmentEvent.CountryCode);
                shipmentStatusEventModel.Country = shipmentEventCountry != null
                    ? await _localizationService.GetLocalizedAsync(shipmentEventCountry, x => x.Name) : shipmentEvent.CountryCode;
                models.Add(shipmentStatusEventModel);
            }
        }

        /// <summary>
        /// Prepare paged shipment list model
        /// </summary>
        /// <param name="searchModel">Shipment search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the shipment list model
        /// </returns>
        public virtual async Task<Models.OrderShipment.ShipmentListModel> PrepareShipmentListModelAsync(Models.OrderShipment.ShipmentSearchModel searchModel)
        {
            ArgumentNullException.ThrowIfNull(searchModel);

            //get parameters to filter shipments
            var vendorId = (await _workContext.GetCurrentVendorAsync())?.Id ?? 0;
            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);

            //get shipments
            var shipments = await _shipmentTrackingService.GetAllShipmentsAsync(vendorId,
                searchModel.WarehouseId,
                searchModel.CountryId,
                searchModel.StateProvinceId,
                searchModel.County,
                searchModel.City,
                searchModel.TrackingNumber,
                searchModel.LoadNotShipped,
                searchModel.LoadNotReadyForPickup,
                searchModel.LoadNotDelivered,
                0,
                startDateValue,
                endDateValue,
                searchModel.ShippingMethod,
                searchModel.Page - 1,
                searchModel.PageSize);

            //prepare list model
            var model = await new Models.OrderShipment.ShipmentListModel().PrepareToGridAsync(searchModel, shipments, () =>
            {
                //fill in model values from the entity
                return shipments.SelectAwait(async shipment =>
                {
                    //fill in model values from the entity
                    var shipmentModel = shipment.ToModel<Models.OrderShipment.ShipmentModel>();

                    //convert dates to the user time
                    shipmentModel.ShippedDate = shipment.ShippedDateUtc.HasValue
                        ? (await _dateTimeHelper.ConvertToUserTimeAsync(shipment.ShippedDateUtc.Value, DateTimeKind.Utc)).ToString()
                        : await _localizationService.GetResourceAsync("Admin.Orders.Shipments.ShippedDate.NotYet");
                    shipmentModel.DeliveryDate = shipment.DeliveryDateUtc.HasValue
                        ? (await _dateTimeHelper.ConvertToUserTimeAsync(shipment.DeliveryDateUtc.Value, DateTimeKind.Utc)).ToString()
                        : await _localizationService.GetResourceAsync("Admin.Orders.Shipments.DeliveryDate.NotYet");

                    //fill in additional values (not existing in the entity)
                    shipmentModel.CanShip = !shipment.ShippedDateUtc.HasValue;
                    shipmentModel.CanDeliver = shipment.ShippedDateUtc.HasValue && !shipment.DeliveryDateUtc.HasValue;

                    var order = await _orderService.GetOrderByIdAsync(shipment.OrderId);

                    shipmentModel.CustomOrderNumber = order.CustomOrderNumber;

                    if (shipment.TotalWeight.HasValue)
                        shipmentModel.TotalWeight = $"{shipment.TotalWeight:F2} [{(await _measureService.GetMeasureWeightByIdAsync(_measureSettings.BaseWeightId))?.Name}]";

                    var shippingMethod = await _genericAttributeService.GetAttributeAsync<string>(shipment, ShippingMethod);
                    var availableShipping = await ShipmentMethod.UPS.ToSelectListWithStringValuesAsync(false);
                    shipmentModel.ShippingMethod = availableShipping.FirstOrDefault(s => s.Value.Equals(shippingMethod))?.Text;

                    //setup tracking url
                    shipmentModel.TrackingNumberUrl = await _shipmentTrackingService.GetTrackingUrlAsync(shipment.TrackingNumber, shippingMethod);

                    return shipmentModel;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare shipment model
        /// </summary>
        /// <param name="model">Shipment model</param>
        /// <param name="shipment">Shipment</param>
        /// <param name="order">Order</param>
        /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the shipment model
        /// </returns>
        public virtual async Task<Models.OrderShipment.ShipmentModel> PrepareShipmentModelAsync(Models.OrderShipment.ShipmentModel model, Shipment shipment, Order order,
            bool excludeProperties = false)
        {
            if (shipment != null)
            {
                //fill in model values from the entity
                model ??= shipment.ToModel<Models.OrderShipment.ShipmentModel>();

                model.CanShip = !shipment.ShippedDateUtc.HasValue;
                model.CanDeliver = shipment.ShippedDateUtc.HasValue && !shipment.DeliveryDateUtc.HasValue;

                var shipmentOrder = await _orderService.GetOrderByIdAsync(shipment.OrderId);

                model.CustomOrderNumber = shipmentOrder.CustomOrderNumber;

                model.ShippedDate = shipment.ShippedDateUtc.HasValue
                    ? (await _dateTimeHelper.ConvertToUserTimeAsync(shipment.ShippedDateUtc.Value, DateTimeKind.Utc)).ToString()
                    : await _localizationService.GetResourceAsync("Admin.Orders.Shipments.ShippedDate.NotYet");
                model.DeliveryDate = shipment.DeliveryDateUtc.HasValue
                    ? (await _dateTimeHelper.ConvertToUserTimeAsync(shipment.DeliveryDateUtc.Value, DateTimeKind.Utc)).ToString()
                    : await _localizationService.GetResourceAsync("Admin.Orders.Shipments.DeliveryDate.NotYet");

                if (shipment.TotalWeight.HasValue)
                    model.TotalWeight =
                        $"{shipment.TotalWeight:F2} [{(await _measureService.GetMeasureWeightByIdAsync(_measureSettings.BaseWeightId))?.Name}]";

                model.ShippingMethod = await _genericAttributeService.GetAttributeAsync<string>(shipment, ShippingMethod);

                //prepare shipment items
                foreach (var item in await _shipmentService.GetShipmentItemsByShipmentIdAsync(shipment.Id))
                {
                    var orderItem = await _orderService.GetOrderItemByIdAsync(item.OrderItemId);
                    if (orderItem == null)
                        continue;

                    var product = await _productService.GetProductByIdAsync(orderItem.ProductId);

                    //fill in model values from the entity
                    var shipmentItemModel = new ShipmentItemModel
                    {
                        Id = item.Id,
                        QuantityInThisShipment = item.Quantity,
                        ShippedFromWarehouse = (await _warehouseService.GetWarehouseByIdAsync(item.WarehouseId))?.Name
                    };

                    await PrepareShipmentItemModelAsync(shipmentItemModel, orderItem, product);

                    model.Items.Add(shipmentItemModel);
                }

                //prepare shipment events
                if (!string.IsNullOrEmpty(shipment.TrackingNumber))
                {
                    var shipmentTracker = await _shipmentService.GetShipmentTrackerAsync(shipment);
                    var trackingNumberUrl = await _shipmentTrackingService.GetTrackingUrlAsync(shipment.TrackingNumber, await _genericAttributeService.GetAttributeAsync<string>(shipment, ShippingMethod));
                    if (!string.IsNullOrEmpty(trackingNumberUrl))
                        model.TrackingNumberUrl = trackingNumberUrl;
                    else
                    {
                        if (shipmentTracker != null)
                            model.TrackingNumberUrl = await shipmentTracker.GetUrlAsync(shipment.TrackingNumber);
                    }

                    if (_shippingSettings.DisplayShipmentEventsToStoreOwner)
                        await PrepareShipmentStatusEventModelsAsync(model.ShipmentStatusEvents, shipment);
                }
            }

            var availableShipmentMethods = await ShipmentMethod.UPS.ToSelectListWithStringValuesAsync(false);
            foreach (var shipmentMethod in availableShipmentMethods)
            {
                model.AvailableShippingMethods.Add(shipmentMethod);
            }
            //insert this default item at first
            model.AvailableShippingMethods.Insert(0, new SelectListItem { Text = await _localizationService.GetResourceAsync("Admin.Common.Select"), Value = string.Empty });

            if (shipment != null)
                return model;

            model.OrderId = order.Id;
            model.CustomOrderNumber = order.CustomOrderNumber;

            var orderItems = (await _orderService.GetOrderItemsAsync(order.Id, isShipEnabled: true, vendorId: (await _workContext.GetCurrentVendorAsync())?.Id ?? 0)).ToList();

            foreach (var orderItem in orderItems)
            {
                var shipmentItemModel = new ShipmentItemModel();

                var product = await _productService.GetProductByIdAsync(orderItem.ProductId);

                await PrepareShipmentItemModelAsync(shipmentItemModel, orderItem, product);

                //ensure that this product can be added to a shipment
                if (shipmentItemModel.QuantityToAdd <= 0)
                    continue;

                if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStock &&
                    product.UseMultipleWarehouses)
                {
                    //multiple warehouses supported
                    shipmentItemModel.AllowToChooseWarehouse = true;
                    foreach (var pwi in (await _productService.GetAllProductWarehouseInventoryRecordsAsync(orderItem.ProductId)).OrderBy(w => w.WarehouseId).ToList())
                    {
                        if (await _warehouseService.GetWarehouseByIdAsync(pwi.WarehouseId) is Warehouse warehouse)
                        {
                            shipmentItemModel.AvailableWarehouses.Add(new ShipmentItemModel.WarehouseInfo
                            {
                                WarehouseId = warehouse.Id,
                                WarehouseName = warehouse.Name,
                                StockQuantity = pwi.StockQuantity,
                                ReservedQuantity = pwi.ReservedQuantity,
                                PlannedQuantity =
                                    await _shipmentService.GetQuantityInShipmentsAsync(product, warehouse.Id, true, true)
                            });
                        }
                    }
                }
                else
                {
                    //multiple warehouses are not supported
                    var warehouse = await _warehouseService.GetWarehouseByIdAsync(product.WarehouseId);
                    if (warehouse != null)
                    {
                        shipmentItemModel.AvailableWarehouses.Add(new ShipmentItemModel.WarehouseInfo
                        {
                            WarehouseId = warehouse.Id,
                            WarehouseName = warehouse.Name,
                            StockQuantity = product.StockQuantity
                        });
                    }
                }

                model.Items.Add(shipmentItemModel);
            }

            return model;
        }

        /// <summary>
        /// Prepare paged order shipment list model
        /// </summary>
        /// <param name="searchModel">Order shipment search model</param>
        /// <param name="order">Order</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the order shipment list model
        /// </returns>
        public virtual async Task<Models.OrderShipment.OrderShipmentListModel> PrepareOrderShipmentListModelAsync(OrderShipmentSearchModel searchModel, Order order)
        {
            ArgumentNullException.ThrowIfNull(searchModel);

            ArgumentNullException.ThrowIfNull(order);

            //get shipments
            var shipments = (await _shipmentService.GetAllShipmentsAsync(
                orderId: order.Id,
                //a vendor should have access only to his products
                vendorId: (await _workContext.GetCurrentVendorAsync())?.Id ?? 0))
                .OrderBy(shipment => shipment.CreatedOnUtc)
                .ToList();

            var pagedShipments = shipments.ToPagedList(searchModel);

            //prepare list model
            var model = await new Models.OrderShipment.OrderShipmentListModel().PrepareToGridAsync(searchModel, pagedShipments, () =>
            {
                //fill in model values from the entity
                return pagedShipments.SelectAwait(async shipment =>
                {
                    //fill in model values from the entity
                    var shipmentModel = shipment.ToModel<Models.OrderShipment.ShipmentModel>();

                    //convert dates to the user time
                    shipmentModel.ShippedDate = shipment.ShippedDateUtc.HasValue
                        ? (await _dateTimeHelper.ConvertToUserTimeAsync(shipment.ShippedDateUtc.Value, DateTimeKind.Utc)).ToString()
                        : await _localizationService.GetResourceAsync("Admin.Orders.Shipments.ShippedDate.NotYet");
                    shipmentModel.DeliveryDate = shipment.DeliveryDateUtc.HasValue
                        ? (await _dateTimeHelper.ConvertToUserTimeAsync(shipment.DeliveryDateUtc.Value, DateTimeKind.Utc)).ToString()
                        : await _localizationService.GetResourceAsync("Admin.Orders.Shipments.DeliveryDate.NotYet");

                    //fill in additional values (not existing in the entity)
                    shipmentModel.CanShip = !shipment.ShippedDateUtc.HasValue;
                    shipmentModel.CanDeliver = shipment.ShippedDateUtc.HasValue && !shipment.DeliveryDateUtc.HasValue;

                    shipmentModel.CustomOrderNumber = order.CustomOrderNumber;

                    var shippingMethod = await _genericAttributeService.GetAttributeAsync<string>(shipment, ShippingMethod);
                    var availableShipping = await ShipmentMethod.UPS.ToSelectListWithStringValuesAsync(false);
                    shipmentModel.ShippingMethod = availableShipping.FirstOrDefault(s => s.Value.Equals(shippingMethod))?.Text;

                    //setup tracking url
                    shipmentModel.TrackingNumberUrl = await _shipmentTrackingService.GetTrackingUrlAsync(shipment.TrackingNumber, shippingMethod);

                    var baseWeight = (await _measureService.GetMeasureWeightByIdAsync(_measureSettings.BaseWeightId))?.Name;

                    if (shipment.TotalWeight.HasValue)
                        shipmentModel.TotalWeight = $"{shipment.TotalWeight:F2} [{baseWeight}]";

                    return shipmentModel;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare shipment item search model
        /// </summary>
        /// <param name="searchModel">Shipment item search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the order shipment list model
        /// </returns>
        protected virtual ShipmentItemSearchModel PrepareShipmentItemSearchModel(ShipmentItemSearchModel searchModel)
        {
            ArgumentNullException.ThrowIfNull(searchModel);

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        /// <summary>
        /// Prepare shipment search model
        /// </summary>
        /// <param name="searchModel">Shipment search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the shipment search model
        /// </returns>
        public virtual async Task<Models.OrderShipment.ShipmentSearchModel> PrepareShipmentSearchModelAsync(Models.OrderShipment.ShipmentSearchModel searchModel)
        {
            ArgumentNullException.ThrowIfNull(searchModel);

            //prepare available countries
            await _baseAdminModelFactory.PrepareCountriesAsync(searchModel.AvailableCountries);

            //prepare available states and provinces
            await _baseAdminModelFactory.PrepareStatesAndProvincesAsync(searchModel.AvailableStates, searchModel.CountryId);

            //prepare available warehouses
            await _baseAdminModelFactory.PrepareWarehousesAsync(searchModel.AvailableWarehouses);

            //prepare nested search model
            PrepareShipmentItemSearchModel(searchModel.ShipmentItemSearchModel);

            var availableShipmentMethods = await ShipmentMethod.UPS.ToSelectListWithStringValuesAsync(false);
            foreach (var shipmentMethod in availableShipmentMethods)
            {
                searchModel.AvailableShippingMethods.Add(shipmentMethod);
            }
            //insert this default item at first
            searchModel.AvailableShippingMethods.Insert(0, new SelectListItem { Text = await _localizationService.GetResourceAsync("Admin.Common.Select"), Value = string.Empty });

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        /// <summary>
        /// Prepare paged order list model
        /// </summary>
        /// <param name="searchModel">Order search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the order list model
        /// </returns>
        public virtual async Task<Models.OrderShipment.OrderListModel> PrepareOrderListModelAsync(Models.OrderShipment.OrderSearchModel searchModel)
        {
            ArgumentNullException.ThrowIfNull(searchModel);

            //get parameters to filter orders
            var orderStatusIds = (searchModel.OrderStatusIds?.Contains(0) ?? true) ? null : searchModel.OrderStatusIds.ToList();
            var paymentStatusIds = (searchModel.PaymentStatusIds?.Contains(0) ?? true) ? null : searchModel.PaymentStatusIds.ToList();
            var shippingStatusIds = (searchModel.ShippingStatusIds?.Contains(0) ?? true) ? null : searchModel.ShippingStatusIds.ToList();
            if (await _workContext.GetCurrentVendorAsync() != null)
                searchModel.VendorId = (await _workContext.GetCurrentVendorAsync()).Id;
            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);
            var product = await _productService.GetProductByIdAsync(searchModel.ProductId);
            var filterByProductId = product != null && (await _workContext.GetCurrentVendorAsync() == null || product.VendorId == (await _workContext.GetCurrentVendorAsync()).Id)
                ? searchModel.ProductId : 0;

            //get orders
            var orders = await _shipmentTrackingService.SearchOrdersAsync(storeId: searchModel.StoreId,
                vendorId: searchModel.VendorId,
                productId: filterByProductId,
                warehouseId: searchModel.WarehouseId,
                paymentMethodSystemName: searchModel.PaymentMethodSystemName,
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                osIds: orderStatusIds,
                psIds: paymentStatusIds,
                ssIds: shippingStatusIds,
                billingPhone: searchModel.BillingPhone,
                billingEmail: searchModel.BillingEmail,
                billingLastName: searchModel.BillingLastName,
                billingCountryId: searchModel.BillingCountryId,
                orderNotes: searchModel.OrderNotes,
                purchaseOrderNumber: searchModel.PurchaseOrderNumber,
                billingZipPostalCode: searchModel.BillingZipPostalCode,
                company: searchModel.SearchCompany,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare list model
            var model = await new Models.OrderShipment.OrderListModel().PrepareToGridAsync(searchModel, orders, () =>
            {
                //fill in model values from the entity
                return orders.SelectAwait(async order =>
                {
                    var billingAddress = await _addressService.GetAddressByIdAsync(order.BillingAddressId);
                    var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);
                    //fill in model values from the entity
                    var orderModel = new Models.OrderShipment.OrderModel
                    {
                        Id = order.Id,
                        OrderStatusId = order.OrderStatusId,
                        PaymentStatusId = order.PaymentStatusId,
                        ShippingStatusId = order.ShippingStatusId,
                        CustomerEmail = $"<a href=\"/Admin/Customer/Edit/{order.CustomerId}\">{billingAddress.Email}</a>",
                        CustomerFullName = $"{billingAddress.FirstName} {billingAddress.LastName}",
                        CustomerId = order.CustomerId,
                        CustomOrderNumber = order.CustomOrderNumber,
                    };

                    //convert dates to the user time
                    orderModel.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(order.CreatedOnUtc, DateTimeKind.Utc);

                    //fill in additional values (not existing in the entity)
                    orderModel.StoreName = (await _storeService.GetStoreByIdAsync(order.StoreId))?.Name ?? "Deleted";
                    orderModel.OrderStatus = await _localizationService.GetLocalizedEnumAsync(order.OrderStatus);
                    orderModel.PaymentStatus = await _localizationService.GetLocalizedEnumAsync(order.PaymentStatus);
                    orderModel.ShippingStatus = await _localizationService.GetLocalizedEnumAsync(order.ShippingStatus);
                    orderModel.OrderTotal = await _priceFormatter.FormatPriceAsync(order.OrderTotal, true, false);

                    if (await _workContext.GetCurrentVendorAsync() == null)
                        orderModel.AdminNote = _orderService.FormatOrderNoteText((await _adminNoteService.GetLatestAdminNoteByOrderIdAsync(order.Id)) ?? new OrderNote());

                    //fill in additional values for customer info (not existing in the entity)
                    if (order.ShippingAddressId.HasValue)
                    {
                        var shippingAddress = await _addressService.GetAddressByIdAsync(order.ShippingAddressId ?? 0);
                        if (shippingAddress != null)
                        {
                            var isDiffAddress = !billingAddress.StateProvinceId.Equals(shippingAddress.StateProvinceId);
                            orderModel.CustomProperties.Add("IsDiffAddress", isDiffAddress.ToString());

                            var address = new StringBuilder(isDiffAddress ? "<div style=\"border: 5px double red;padding: 10px;background-color: #ffe8bc;\">" : "<div>");
                            if (await _workContext.GetCurrentVendorAsync() != null)
                                address.AppendFormat("<strong>{0}</strong>", $"{shippingAddress?.FirstName} {shippingAddress?.LastName}");
                            else
                            {
                                address.AppendFormat("<a href=\"/Admin/Customer/Edit/{0}\">{1}</a></br>", order.CustomerId, billingAddress.Email);
                                address.AppendFormat("<strong>{0}</strong>", $"{shippingAddress?.FirstName} {shippingAddress?.LastName}");
                            }

                            address.AppendFormat("<br /><span>{0}</span>", shippingAddress?.Address1);
                            if (!string.IsNullOrEmpty(shippingAddress?.Address2))
                                address.AppendFormat(", <br /><span> {0}</span>", shippingAddress?.Address2);

                            if (!string.IsNullOrEmpty(shippingAddress?.City))
                                address.AppendFormat(", <br /><span>{0}", shippingAddress?.City);

                            if (shippingAddress?.StateProvinceId != null)
                                address.AppendFormat(", {0}", (await _stateProvinceService.GetStateProvinceByIdAsync(shippingAddress.StateProvinceId ?? 0))?.Name);

                            if (!string.IsNullOrEmpty(shippingAddress?.ZipPostalCode))
                                address.AppendFormat(", {0}</span>", shippingAddress?.ZipPostalCode);

                            if (shippingAddress?.CountryId != null)
                                address.AppendFormat(", <br />" + (await _countryService.GetCountryByIdAsync(shippingAddress.CountryId ?? 0))?.Name);

                            if (await _workContext.GetCurrentVendorAsync() == null)
                            {
                                if (!string.IsNullOrEmpty(shippingAddress?.PhoneNumber))
                                    address.AppendFormat("<br /><strong>Phone Number : </strong>{0}", shippingAddress?.PhoneNumber);
                            }

                            address.AppendLine("</div>");
                            orderModel.CustomerEmail = address.ToString();

                            
                        }
                    }
                    return orderModel;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare paged order item list model
        /// </summary>
        /// <param name="searchModel">Order item search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the order list model
        /// </returns>
        public virtual async Task<OrderItemListModel> PrepareOrderItemsListModelAsync(OrderItemSearchModel searchModel)
        {
            ArgumentNullException.ThrowIfNull(searchModel);

            var orderItems = (await _orderService.GetOrderItemsAsync(orderId: searchModel.OrderId, vendorId: (await _workContext.GetCurrentVendorAsync())?.Id ?? 0)).ToPagedList(searchModel);

            //prepare list model
            var model = await new OrderItemListModel().PrepareToGridAsync(searchModel, orderItems, () =>
            {
                //fill in model values from the entity
                return orderItems.SelectAwait(async orderitem =>
                {
                    //fill in model values from the entity
                    var orderModel = new OrderItemModel { Id = orderitem.Id };

                    var product = await _productService.GetProductByIdAsync(orderitem.ProductId);
                    var primaryStoreCurrency = await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId);
                    var languageId = (await _workContext.GetWorkingLanguageAsync()).Id;
                    //picture
                    var orderItemPicture = await _pictureService.GetProductPictureAsync(product, orderitem.AttributesXml);
                    var vendor = await _vendorService.GetVendorByIdAsync(product.VendorId);

                    orderModel.PictureThumbnailUrl = (await _pictureService.GetPictureUrlAsync(orderItemPicture, 75, true)).Url;
                    orderModel.ProductId = orderitem.ProductId;
                    orderModel.ProductName = product.Name;
                    orderModel.UnitPriceInclTax = await _priceFormatter.FormatPriceAsync(orderitem.UnitPriceInclTax, true, primaryStoreCurrency, languageId, true, true);
                    orderModel.Quantity = orderitem.Quantity;
                    orderModel.DiscountInclTax = await _priceFormatter.FormatPriceAsync(orderitem.DiscountAmountInclTax, true, primaryStoreCurrency, languageId, true, true);
                    orderModel.SubTotalInclTax = await _priceFormatter.FormatPriceAsync(orderitem.PriceInclTax, true, primaryStoreCurrency, languageId, true, true);

                    var productHtmlSb = new StringBuilder("<div>");
                    productHtmlSb.AppendFormat("<strong>Product Name</strong> : <a href=\"/Admin/Product/Edit/{0}\">{1}</a></br>", product.Id, product.Name);
                    productHtmlSb.AppendFormat("<text>{0} </text><br />", orderitem.AttributeDescription);
                    productHtmlSb.AppendFormat("<strong>SKU</strong><text> : </text>{0}<br />", product.Sku);
                    productHtmlSb.Append("</div>");

                    orderModel.ProductName = productHtmlSb.ToString();

                    return orderModel;

                });
            });

            return model;
        }

        /// <summary>
        /// Prepare order aggregator model
        /// </summary>
        /// <param name="searchModel">Order search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the order aggregator model
        /// </returns>
        public virtual async Task<OrderAggreratorModel> PrepareOrderAggregatorModelAsync(Models.OrderShipment.OrderSearchModel searchModel)
        {
            ArgumentNullException.ThrowIfNull(searchModel);

            if (!_orderSettings.DisplayOrderSummary)

                return null;

            //get parameters to filter orders
            var orderStatusIds = (searchModel.OrderStatusIds?.Contains(0) ?? true) ? null : searchModel.OrderStatusIds.ToList();
            var paymentStatusIds = (searchModel.PaymentStatusIds?.Contains(0) ?? true) ? null : searchModel.PaymentStatusIds.ToList();
            var shippingStatusIds = (searchModel.ShippingStatusIds?.Contains(0) ?? true) ? null : searchModel.ShippingStatusIds.ToList();
            if (await _workContext.GetCurrentVendorAsync() != null)
                searchModel.VendorId = (await _workContext.GetCurrentVendorAsync()).Id;
            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);
            var product = await _productService.GetProductByIdAsync(searchModel.ProductId);
            var filterByProductId = product != null && (await _workContext.GetCurrentVendorAsync() == null || product.VendorId == (await _workContext.GetCurrentVendorAsync()).Id)
                ? searchModel.ProductId : 0;

            //prepare additional model data
            var reportSummary = await _shipmentTrackingService.GetOrderAverageReportLineAsync(storeId: searchModel.StoreId,
                vendorId: searchModel.VendorId,
                productId: filterByProductId,
                warehouseId: searchModel.WarehouseId,
                paymentMethodSystemName: searchModel.PaymentMethodSystemName,
                osIds: orderStatusIds,
                psIds: paymentStatusIds,
                ssIds: shippingStatusIds,
                startTimeUtc: startDateValue,
                endTimeUtc: endDateValue,
                billingPhone: searchModel.BillingPhone,
                billingEmail: searchModel.BillingEmail,
                billingLastName: searchModel.BillingLastName,
                billingCountryId: searchModel.BillingCountryId,
                orderNotes: searchModel.OrderNotes,
                purchaseOrderNumber: searchModel.PurchaseOrderNumber,
                billingZipPostalCode: searchModel.BillingZipPostalCode);

            var profit = await _shipmentTrackingService.ProfitReportAsync(storeId: searchModel.StoreId,
                vendorId: searchModel.VendorId,
                productId: filterByProductId,
                warehouseId: searchModel.WarehouseId,
                paymentMethodSystemName: searchModel.PaymentMethodSystemName,
                osIds: orderStatusIds,
                psIds: paymentStatusIds,
                ssIds: shippingStatusIds,
                startTimeUtc: startDateValue,
                endTimeUtc: endDateValue,
                billingPhone: searchModel.BillingPhone,
                billingEmail: searchModel.BillingEmail,
                billingLastName: searchModel.BillingLastName,
                billingCountryId: searchModel.BillingCountryId,
                orderNotes: searchModel.OrderNotes,
                purchaseOrderNumber: searchModel.PurchaseOrderNumber,
                billingZipPostalCode: searchModel.BillingZipPostalCode);

            var primaryStoreCurrency = await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId);
            var shippingSum = await _priceFormatter
                .FormatShippingPriceAsync(reportSummary.SumShippingExclTax, true, primaryStoreCurrency, (await _workContext.GetWorkingLanguageAsync()).Id, false);
            var taxSum = await _priceFormatter.FormatPriceAsync(reportSummary.SumTax, true, false);
            var totalSum = await _priceFormatter.FormatPriceAsync(reportSummary.SumOrders, true, false);
            var profitSum = await _priceFormatter.FormatPriceAsync(profit, true, false);

            var model = new OrderAggreratorModel
            {
                AggregatorProfit = profitSum,
                AggregatorShipping = shippingSum,
                AggregatorTax = taxSum,
                AggregatorTotal = totalSum
            };

            return model;
        }

        /// <summary>
        /// Prepare order search model
        /// </summary>
        /// <param name="searchModel">Order search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the order search model
        /// </returns>
        public virtual async Task<Models.OrderShipment.OrderSearchModel> PrepareOrderSearchModelAsync(Models.OrderShipment.OrderSearchModel searchModel)
        {
            ArgumentNullException.ThrowIfNull(searchModel);

            searchModel.IsLoggedInAsVendor = await _workContext.GetCurrentVendorAsync() != null;
            searchModel.BillingPhoneEnabled = _addressSettings.PhoneEnabled;
            searchModel.CompanyEnabled = _customerSettings.CompanyEnabled;

            //prepare available order, payment and shipping statuses
            await _baseAdminModelFactory.PrepareOrderStatusesAsync(searchModel.AvailableOrderStatuses);
            if (searchModel.AvailableOrderStatuses.Any())
            {
                if (searchModel.OrderStatusIds?.Any() ?? false)
                {
                    var ids = searchModel.OrderStatusIds.Select(id => id.ToString());
                    searchModel.AvailableOrderStatuses.Where(statusItem => ids.Contains(statusItem.Value)).ToList()
                        .ForEach(statusItem => statusItem.Selected = true);
                }
                else
                    searchModel.AvailableOrderStatuses.FirstOrDefault().Selected = true;
            }

            await _baseAdminModelFactory.PreparePaymentStatusesAsync(searchModel.AvailablePaymentStatuses);
            if (searchModel.AvailablePaymentStatuses.Any())
            {
                if (searchModel.PaymentStatusIds?.Any() ?? false)
                {
                    var ids = searchModel.PaymentStatusIds.Select(id => id.ToString());
                    searchModel.AvailablePaymentStatuses.Where(statusItem => ids.Contains(statusItem.Value)).ToList()
                        .ForEach(statusItem => statusItem.Selected = true);
                }
                else
                    searchModel.AvailablePaymentStatuses.FirstOrDefault().Selected = true;
            }

            await _baseAdminModelFactory.PrepareShippingStatusesAsync(searchModel.AvailableShippingStatuses);
            if (searchModel.AvailableShippingStatuses.Any())
            {
                if (searchModel.ShippingStatusIds?.Any() ?? false)
                {
                    var ids = searchModel.ShippingStatusIds.Select(id => id.ToString());
                    searchModel.AvailableShippingStatuses.Where(statusItem => ids.Contains(statusItem.Value)).ToList()
                        .ForEach(statusItem => statusItem.Selected = true);
                }
                else
                    searchModel.AvailableShippingStatuses.FirstOrDefault().Selected = true;
            }

            //prepare available stores
            await _baseAdminModelFactory.PrepareStoresAsync(searchModel.AvailableStores);

            //prepare available vendors
            await _baseAdminModelFactory.PrepareVendorsAsync(searchModel.AvailableVendors);

            //prepare available warehouses
            await _baseAdminModelFactory.PrepareWarehousesAsync(searchModel.AvailableWarehouses);

            //prepare available payment methods
            searchModel.AvailablePaymentMethods = (await _paymentPluginManager.LoadAllPluginsAsync()).Select(method =>
                new SelectListItem { Text = method.PluginDescriptor.FriendlyName, Value = method.PluginDescriptor.SystemName }).ToList();
            searchModel.AvailablePaymentMethods.Insert(0, new SelectListItem { Text = await _localizationService.GetResourceAsync("Admin.Common.All"), Value = string.Empty });

            //prepare available billing countries
            searchModel.AvailableCountries = (await _countryService.GetAllCountriesForBillingAsync(showHidden: true))
                .Select(country => new SelectListItem { Text = country.Name, Value = country.Id.ToString() }).ToList();
            searchModel.AvailableCountries.Insert(0, new SelectListItem { Text = await _localizationService.GetResourceAsync("Admin.Common.All"), Value = "0" });

            //prepare grid
            searchModel.SetGridPageSize();

            searchModel.HideStoresList = _catalogSettings.IgnoreStoreLimitations || searchModel.AvailableStores.SelectionIsNotPossible();

            return searchModel;
        }

        /// <summary>
        /// Prepare admin note search model
        /// </summary>
        /// <param name="searchModel">Admin note search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the admin note search model
        /// </returns>
        protected virtual AdminNoteSearchModel PrepareAdminNoteSearchModel(AdminNoteSearchModel searchModel)
        {
            ArgumentNullException.ThrowIfNull(searchModel);

            //prepare page parameters
            searchModel.SetPopupGridPageSize();

            return searchModel;
        }

        /// <summary>
        /// Prepare admin note list model
        /// </summary>
        /// <param name="searchModel">Admin note search model</param>
        /// <param name="order">Order</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the admin note list model
        /// </returns>
        protected virtual async Task<OrderNoteListModel> PrepareAdminNoteListModelAsync(AdminNoteSearchModel searchModel, Order order)
        {
            ArgumentNullException.ThrowIfNull(searchModel);

            ArgumentNullException.ThrowIfNull(order);

            //get products
            var adminNotes = await _adminNoteService.GetAdminNotesByOrderIdAsync(order.Id, searchModel.Note,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare list model
            var model = await new OrderNoteListModel().PrepareToGridAsync(searchModel, adminNotes, () =>
            {
                //fill in model values from the entity
                return adminNotes.SelectAwait(async orderNote =>
                {
                    //fill in model values from the entity
                    var orderNoteModel = orderNote.ToModel<OrderNoteModel>();

                    //convert dates to the user time
                    orderNoteModel.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(orderNote.CreatedOnUtc, DateTimeKind.Utc);

                    //fill in additional values (not existing in the entity)
                    orderNoteModel.Note = _orderService.FormatOrderNoteText(orderNote);

                    orderNoteModel.DownloadGuid = (await _downloadService.GetDownloadByIdAsync(orderNote.DownloadId))?.DownloadGuid ?? Guid.Empty;

                    return orderNoteModel;
                });
            });

            return model;
        }

        #endregion

        #region Methods

        #region List

        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task<IActionResult> List(List<int> orderStatuses = null, List<int> paymentStatuses = null, List<int> shippingStatuses = null)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Orders.ORDERS_VIEW))
                return AccessDeniedView();

            var billingPhone = _webHelper.QueryString<string>("BillingPhone");
            if(!string.IsNullOrEmpty(billingPhone))
            {
                billingPhone = CommonHelper.EnsureMaximumLength(CommonHelper.EnsureNumericOnly(billingPhone), 10);
                var orders = await _orderService.SearchOrdersAsync(billingPhone: Convert.ToInt64(billingPhone).ToString("###-###-####"));
                if(!orders.Any())
                    return RedirectToRoute("Homepage");
            }

            //prepare model
            var model = await PrepareOrderSearchModelAsync(new Models.OrderShipment.OrderSearchModel
            {
                OrderStatusIds = orderStatuses,
                PaymentStatusIds = paymentStatuses,
                ShippingStatusIds = shippingStatuses,
                BillingPhone = !string.IsNullOrEmpty(billingPhone) ? Convert.ToInt64(billingPhone).ToString("###-###-####") : string.Empty
            });

            return View("~/Plugins/Misc.ShipmentTracking/Areas/Admin/Views/Order/List.cshtml", model);
        }

        [HttpPost]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> OrderListPO(Models.OrderShipment.OrderSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Orders.ORDERS_VIEW))
                return await AccessDeniedJsonAsync();

            //prepare model
            var model = await PrepareOrderListModelAsync(searchModel);

            return Json(model);
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        [HttpPost]
        public virtual async Task<IActionResult> OrderItemByOrderId(OrderItemSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Orders.ORDERS_VIEW))
                return await AccessDeniedJsonAsync();

            searchModel.SetGridPageSize();
            //prepare model
            var model = await PrepareOrderItemsListModelAsync(searchModel);

            return Json(model);
        }

        [HttpPost]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> ReportAggregatesPO(Models.OrderShipment.OrderSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Orders.ORDERS_VIEW))
                return await AccessDeniedJsonAsync();

            //prepare model
            var model = await PrepareOrderAggregatorModelAsync(searchModel);

            return Json(model);
        }

        [HttpPost, ActionName("List")]
        [FormValueRequired("go-to-order-by-number")]
        public override async Task<IActionResult> GoToOrderId(Nop.Web.Areas.Admin.Models.Orders.OrderSearchModel model)
        {
            var order = await _orderService.GetOrderByCustomOrderNumberAsync(model.GoDirectlyToCustomOrderNumber);

            if (order == null)
                return await List();

            return RedirectToAction("Edit", "Order", new { id = order.Id });
        }

        public virtual async Task<IActionResult> PdfInvoicePreview(int orderId)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE))
                return AccessDeniedView();

            //a vendor should have access only to their orders
            if (!await HasAccessToOrderAsync(orderId))
                return RedirectToAction("List");

            //a vendor should have access only to his products
            var currentVendor = await _workContext.GetCurrentVendorAsync();

            var order = await _orderService.GetOrderByIdAsync(orderId);

            var orderFilePath = await _pdfService.SaveOrderPdfToDiskAsync(order, _orderSettings.GeneratePdfInvoiceInCustomerLanguage ? null : await _workContext.GetWorkingLanguageAsync(), vendor: currentVendor);

            return Json(Path.GetFileName(orderFilePath));
        }

        [HttpPost, ActionName("PdfInvoice")]
        [FormValueRequired("single-pdf-invoice-all")]
        public virtual async Task<IActionResult> PdfInvoiceAll(Models.OrderShipment.OrderSearchModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE))
                return AccessDeniedView();

            //a vendor should have access only to his products
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null)
            {
                model.VendorId = currentVendor.Id;
            }

            var startDateValue = model.StartDate == null ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());

            var endDateValue = model.EndDate == null ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);

            var orderStatusIds = model.OrderStatusIds != null && !model.OrderStatusIds.Contains(0)
                ? model.OrderStatusIds.ToList()
                : null;
            var paymentStatusIds = model.PaymentStatusIds != null && !model.PaymentStatusIds.Contains(0)
                ? model.PaymentStatusIds.ToList()
                : null;
            var shippingStatusIds = model.ShippingStatusIds != null && !model.ShippingStatusIds.Contains(0)
                ? model.ShippingStatusIds.ToList()
                : null;

            var filterByProductId = 0;
            var product = await _productService.GetProductByIdAsync(model.ProductId);
            if (product != null && (currentVendor == null || product.VendorId == currentVendor.Id))
                filterByProductId = model.ProductId;

            //load orders
            var orders = await _shipmentTrackingService.SearchOrdersAsync(storeId: model.StoreId,
                vendorId: model.VendorId,
                productId: filterByProductId,
                warehouseId: model.WarehouseId,
                paymentMethodSystemName: model.PaymentMethodSystemName,
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                osIds: orderStatusIds,
                psIds: paymentStatusIds,
                ssIds: shippingStatusIds,
                billingPhone: model.BillingPhone,
                billingEmail: model.BillingEmail,
                billingLastName: model.BillingLastName,
                billingCountryId: model.BillingCountryId,
                orderNotes: model.OrderNotes,
                purchaseOrderNumber: model.PurchaseOrderNumber,
                billingZipPostalCode: model.BillingZipPostalCode);

            //ensure that we at least one order selected
            if (!orders.Any())
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Orders.NoOrders"));
                return RedirectToAction("List");
            }

            try
            {
                byte[] bytes;
                await using (var stream = new MemoryStream())
                {
                    await _pdfService.PrintOrdersToPdfAsync(stream, orders, _orderSettings.GeneratePdfInvoiceInCustomerLanguage ? null : await _workContext.GetWorkingLanguageAsync(), currentVendor);
                    bytes = stream.ToArray();
                }

                return File(bytes, MimeTypes.ApplicationPdf, $"orders_{DateTime.Now.ToFileTime()}.pdf");
            }
            catch (Exception exc)
            {
                await _notificationService.ErrorNotificationAsync(exc);
                return RedirectToAction("List");
            }
        }

        [HttpPost]
        public override async Task<IActionResult> PdfInvoiceSelected(string selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE))
                return AccessDeniedView();

            var orders = new List<Order>();
            if (selectedIds != null)
            {
                var ids = selectedIds
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => Convert.ToInt32(x))
                    .ToArray();
                orders.AddRange(await _orderService.GetOrdersByIdsAsync(ids));
            }

            //a vendor should have access only to his products
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null)
            {
                orders = await orders.WhereAwait(HasAccessToOrderAsync).ToListAsync();
            }

            try
            {
                byte[] bytes;
                await using (var stream = new MemoryStream())
                {
                    await _pdfService.PrintOrdersToPdfAsync(stream, orders, _orderSettings.GeneratePdfInvoiceInCustomerLanguage ? null : await _workContext.GetWorkingLanguageAsync(), currentVendor);
                    bytes = stream.ToArray();
                }

                return File(
                    bytes,
                    MimeTypes.ApplicationZip,
                    $"orders_{DateTime.Now.ToFileTime()}.pdf"
                );
            }
            catch (Exception exc)
            {
                await _notificationService.ErrorNotificationAsync(exc);
                return RedirectToAction("List");
            }
        }

        [HttpPost, ActionName("PdfInvoiceMailable")]
        [FormValueRequired("single-pdf-invoice-all-mailable")]
        public virtual async Task<IActionResult> PdfInvoiceMailableAll(Models.OrderShipment.OrderSearchModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE))
                return AccessDeniedView();

            //a vendor should have access only to his products
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null)
            {
                model.VendorId = currentVendor.Id;
            }

            var startDateValue = model.StartDate == null ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());

            var endDateValue = model.EndDate == null ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);

            var orderStatusIds = model.OrderStatusIds != null && !model.OrderStatusIds.Contains(0)
                ? model.OrderStatusIds.ToList()
                : null;
            var paymentStatusIds = model.PaymentStatusIds != null && !model.PaymentStatusIds.Contains(0)
                ? model.PaymentStatusIds.ToList()
                : null;
            var shippingStatusIds = model.ShippingStatusIds != null && !model.ShippingStatusIds.Contains(0)
                ? model.ShippingStatusIds.ToList()
                : null;

            var filterByProductId = 0;
            var product = await _productService.GetProductByIdAsync(model.ProductId);
            if (product != null && (currentVendor == null || product.VendorId == currentVendor.Id))
                filterByProductId = model.ProductId;

            //load orders
            IList<Order> orders = await _shipmentTrackingService.SearchOrdersAsync(storeId: model.StoreId,
                vendorId: model.VendorId,
                productId: filterByProductId,
                warehouseId: model.WarehouseId,
                paymentMethodSystemName: model.PaymentMethodSystemName,
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                osIds: orderStatusIds,
                psIds: paymentStatusIds,
                ssIds: shippingStatusIds,
                billingPhone: model.BillingPhone,
                billingEmail: model.BillingEmail,
                billingLastName: model.BillingLastName,
                billingCountryId: model.BillingCountryId,
                orderNotes: model.OrderNotes,
                purchaseOrderNumber: model.PurchaseOrderNumber,
                billingZipPostalCode: model.BillingZipPostalCode);

            //an excluded customer not require pdf invoice
            var excludedCustomers = await _settingService.GetSettingByKeyAsync<List<int>>("invoicepdfsettings.excludedcustomerids", new List<int>());
            if (excludedCustomers.Any())
            {
                orders = orders.Where(o => !excludedCustomers.Contains(o.CustomerId)).ToList(); ;
            }

            //ensure that we at least one order selected
            if (!orders.Any())
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Orders.NoOrders"));
                return RedirectToAction("List");
            }

            try
            {
                byte[] bytes;
                await using (var stream = new MemoryStream())
                {
                    await _pdfService.PrintOrdersToPdfAsync(stream, orders, _orderSettings.GeneratePdfInvoiceInCustomerLanguage ? null : await _workContext.GetWorkingLanguageAsync(), currentVendor);
                    bytes = stream.ToArray();
                }

                return File(bytes, MimeTypes.ApplicationPdf, $"orders_{DateTime.Now.ToFileTime()}.pdf");
            }
            catch (Exception exc)
            {
                await _notificationService.ErrorNotificationAsync(exc);
                return RedirectToAction("List");
            }
        }

        [HttpPost]
        public virtual async Task<IActionResult> PdfInvoiceSelectedMailable(string selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE))
                return AccessDeniedView();

            var orders = new List<Order>();
            if (selectedIds != null)
            {
                var ids = selectedIds
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => Convert.ToInt32(x))
                    .ToArray();
                orders.AddRange(await _orderService.GetOrdersByIdsAsync(ids));
            }

            //a vendor should have access only to his products
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null)
            {
                orders = await orders.WhereAwait(HasAccessToOrderAsync).ToListAsync();
            }

            //an excluded customer not require pdf invoice
            var excludedCustomers = await _settingService.GetSettingByKeyAsync<List<int>>("invoicepdfsettings.excludedcustomerids", new List<int>());
            if (excludedCustomers.Any())
            {
                orders = orders.Where(o => !excludedCustomers.Contains(o.CustomerId)).ToList();
            }

            //ensure that we at least one order selected
            if (!orders.Any())
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Orders.NoOrders"));
                return RedirectToAction("List");
            }

            try
            {
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    await _pdfService.PrintOrdersToPdfAsync(stream, orders, _orderSettings.GeneratePdfInvoiceInCustomerLanguage ? null : await _workContext.GetWorkingLanguageAsync(), currentVendor);
                    bytes = stream.ToArray();
                }

                return File(bytes, MimeTypes.ApplicationPdf, $"orders_{DateTime.Now.ToFileTime()}.pdf");
            }
            catch (Exception exc)
            {
                await _notificationService.ErrorNotificationAsync(exc);
                return RedirectToAction("List");
            }
        }

        #endregion

        #region Shipments

        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task<IActionResult> ShipmentList()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Orders.SHIPMENTS_VIEW))
                return AccessDeniedView();

            //prepare model
            var model = await PrepareShipmentSearchModelAsync(new Models.OrderShipment.ShipmentSearchModel());

            return View("~/Plugins/Misc.ShipmentTracking/Areas/Admin/Views/Order/ShipmentList.cshtml", model);
        }

        [HttpPost]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> ShipmentList(Models.OrderShipment.ShipmentSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Orders.SHIPMENTS_VIEW))
                return await AccessDeniedJsonAsync();

            //prepare model
            var model = await PrepareShipmentListModelAsync(searchModel);

            return Json(model);
        }

        [HttpPost]
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task<IActionResult> ShipmentsByOrder(OrderShipmentSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Orders.SHIPMENTS_CREATE_EDIT_DELETE))
                return await AccessDeniedJsonAsync();

            //try to get an order with the specified id
            var order = await _orderService.GetOrderByIdAsync(searchModel.OrderId)
                ?? throw new ArgumentException("No order found with the specified id");

            //a vendor should have access only to his products
            if (await _workContext.GetCurrentVendorAsync() != null && !await HasAccessToOrderAsync(order))
                return Content(string.Empty);

            //prepare model
            var model = await PrepareOrderShipmentListModelAsync(searchModel, order);

            return Json(model);
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task<IActionResult> AddShipment(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Orders.SHIPMENTS_CREATE_EDIT_DELETE))
                return AccessDeniedView();

            //try to get an order with the specified id
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
                return RedirectToAction("List");

            //a vendor should have access only to his products
            if (await _workContext.GetCurrentVendorAsync() != null && !await HasAccessToOrderAsync(order))
                return RedirectToAction("List");

            //prepare model
            var model = await PrepareShipmentModelAsync(new Models.OrderShipment.ShipmentModel(), null, order);

            return View("~/Plugins/Misc.ShipmentTracking/Areas/Admin/Views/Order/AddShipment.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> SaveShipment(Models.OrderShipment.ShipmentModel model, IFormCollection form, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Orders.SHIPMENTS_VIEW))
                return AccessDeniedView();

            //try to get an order with the specified id
            var order = await _orderService.GetOrderByIdAsync(model.OrderId);
            if (order == null)
                return RedirectToAction("List");

            //a vendor should have access only to his products
            if (await _workContext.GetCurrentVendorAsync() != null && !await HasAccessToOrderAsync(order))
                return RedirectToAction("List");

            var orderItems = await _orderService.GetOrderItemsAsync(order.Id, isShipEnabled: true);
            //a vendor should have access only to his products
            if (await _workContext.GetCurrentVendorAsync() != null)
            {
                orderItems = await orderItems.WhereAwait(HasAccessToProductAsync).ToListAsync();
            }

            var shipment = new Shipment
            {
                OrderId = order.Id,
                TrackingNumber = model.TrackingNumber,
                TotalWeight = null,
                AdminComment = model.AdminComment,
                CreatedOnUtc = DateTime.UtcNow
            };

            var shipmentItems = new List<ShipmentItem>();

            decimal? totalWeight = null;

            foreach (var orderItem in orderItems)
            {
                var product = await _productService.GetProductByIdAsync(orderItem.ProductId);

                //ensure that this product can be shipped (have at least one item to ship)
                var maxQtyToAdd = await _orderService.GetTotalNumberOfItemsCanBeAddedToShipmentAsync(orderItem);
                if (maxQtyToAdd <= 0)
                    continue;

                var qtyToAdd = 0; //parse quantity
                foreach (var formKey in form.Keys)
                    if (formKey.Equals($"qtyToAdd{orderItem.Id}", StringComparison.InvariantCultureIgnoreCase))
                    {
                        int.TryParse(form[formKey], out qtyToAdd);
                        break;
                    }

                var warehouseId = 0;
                if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStock &&
                    product.UseMultipleWarehouses)
                {
                    //multiple warehouses supported
                    //warehouse is chosen by a store owner
                    foreach (var formKey in form.Keys)
                        if (formKey.Equals($"warehouse_{orderItem.Id}", StringComparison.InvariantCultureIgnoreCase))
                        {
                            int.TryParse(form[formKey], out warehouseId);
                            break;
                        }
                }
                else
                {
                    //multiple warehouses are not supported
                    warehouseId = product.WarehouseId;
                }

                //validate quantity
                if (qtyToAdd <= 0)
                    continue;
                if (qtyToAdd > maxQtyToAdd)
                    qtyToAdd = maxQtyToAdd;

                //ok. we have at least one item. let's create a shipment (if it does not exist)

                var orderItemTotalWeight = orderItem.ItemWeight * qtyToAdd;
                if (orderItemTotalWeight.HasValue)
                {
                    if (!totalWeight.HasValue)
                        totalWeight = 0;
                    totalWeight += orderItemTotalWeight.Value;
                }

                //create a shipment item
                shipmentItems.Add(new ShipmentItem
                {
                    OrderItemId = orderItem.Id,
                    Quantity = qtyToAdd,
                    WarehouseId = warehouseId
                });
            }

            //if we have at least one item in the shipment, then save it
            if (shipmentItems.Any())
            {
                shipment.TotalWeight = totalWeight;
                await _shipmentService.InsertShipmentAsync(shipment);

                if (!string.IsNullOrWhiteSpace(model.ShippingMethod))
                    await _genericAttributeService.SaveAttributeAsync(shipment, ShippingMethod, model.ShippingMethod);

                foreach (var shipmentItem in shipmentItems)
                {
                    shipmentItem.ShipmentId = shipment.Id;
                    await _shipmentService.InsertShipmentItemAsync(shipmentItem);
                }

                //add a note
                await _orderService.InsertOrderNoteAsync(new OrderNote
                {
                    OrderId = order.Id,
                    Note = "A shipment has been added",
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });

                if (model.CanShip)
                    await _orderProcessingService.ShipAsync(shipment, true);

                if (model.CanShip && model.CanDeliver)
                    await _orderProcessingService.DeliverAsync(shipment, true);

                await LogEditOrderAsync(order.Id);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Orders.Shipments.Added"));
                return continueEditing
                        ? RedirectToAction("ShipmentDetails", new { id = shipment.Id })
                        : RedirectToAction("Edit", "Order", new { id = model.OrderId });
            }

            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Orders.Shipments.NoProductsSelected"));

            return RedirectToAction("AddShipment", model);
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task<IActionResult> ShipmentDetails(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Orders.SHIPMENTS_VIEW))
                return AccessDeniedView();

            //try to get a shipment with the specified id
            var shipment = await _shipmentService.GetShipmentByIdAsync(id);
            if (shipment == null)
                return RedirectToAction("List");

            //a vendor should have access only to his products
            if (await _workContext.GetCurrentVendorAsync() != null && !await HasAccessToShipmentAsync(shipment))
                return RedirectToAction("List");

            //prepare model
            var model = await PrepareShipmentModelAsync(null, shipment, null);

            return View("~/Plugins/Misc.ShipmentTracking/Areas/Admin/Views/Order/ShipmentDetails.cshtml", model);
        }

        [HttpPost, ActionName("ShipmentDetails")]
        [FormValueRequired("setshippingmethod")]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> SetShipmentMethod(Models.OrderShipment.ShipmentModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE))
                return AccessDeniedView();

            //try to get a shipment with the specified id
            var shipment = await _shipmentService.GetShipmentByIdAsync(model.Id);
            if (shipment == null)
                return RedirectToAction("List");

            //a vendor should have access only to his products
            if (await _workContext.GetCurrentVendorAsync() != null && !await HasAccessToShipmentAsync(shipment))
                return RedirectToAction("List");

            if (!string.IsNullOrWhiteSpace(model.ShippingMethod))
                await _genericAttributeService.SaveAttributeAsync(shipment, ShippingMethod, model.ShippingMethod);

            return RedirectToAction("ShipmentDetails", new { id = shipment.Id });
        }

        [HttpPost]
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task<IActionResult> DeleteShipment(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE))
                return AccessDeniedView();

            //try to get a shipment with the specified id
            var shipment = await _shipmentService.GetShipmentByIdAsync(id);
            if (shipment == null)
                return RedirectToAction("List");

            //a vendor should have access only to his products
            if (await _workContext.GetCurrentVendorAsync() != null && !await HasAccessToShipmentAsync(shipment))
                return RedirectToAction("List");

            foreach (var shipmentItem in await _shipmentService.GetShipmentItemsByShipmentIdAsync(shipment.Id))
            {
                var orderItem = await _orderService.GetOrderItemByIdAsync(shipmentItem.OrderItemId);
                if (orderItem == null)
                    continue;

                var product = await _productService.GetProductByIdAsync(orderItem.ProductId);

                await _productService.ReverseBookedInventoryAsync(product, shipmentItem,
                    string.Format(await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.DeleteShipment"), shipment.OrderId));
            }

            var orderId = shipment.OrderId;
            await _shipmentService.DeleteShipmentAsync(shipment);

            var order = await _orderService.GetOrderByIdAsync(orderId);
            //add a note
            await _orderService.InsertOrderNoteAsync(new OrderNote
            {
                OrderId = order.Id,
                Note = "A shipment has been deleted",
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow
            });

            await LogEditOrderAsync(order.Id);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Orders.Shipments.Deleted"));
            return RedirectToAction("Edit", "Order", new { id = orderId });
        }

        #endregion

        #region Admin Note

        public virtual async Task<IActionResult> AdminNoteList(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Orders.ORDERS_VIEW))
                return AccessDeniedView();

            //prepare model
            var model = PrepareAdminNoteSearchModel(new AdminNoteSearchModel() { OrderId = id });

            return View("~/Plugins/Misc.ShipmentTracking/Areas/Admin/Views/Order/AdminNoteList.cshtml", model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> AdminNoteList(AdminNoteSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Orders.ORDERS_VIEW))
                return AccessDeniedView();

            //try to get an order with the specified id
            var order = await _orderService.GetOrderByIdAsync(searchModel.OrderId)
                ?? throw new ArgumentException("No order found with the specified id");

            //a vendor does not have access to this functionality
            if (await _workContext.GetCurrentVendorAsync() != null)
                return Content(string.Empty);

            //prepare model
            var model = await PrepareAdminNoteListModelAsync(searchModel, order);

            return Json(model);
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task<IActionResult> OrderNoteAdd(int orderId, int downloadId, bool displayToCustomer, string message)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE))
                return AccessDeniedView();

            if (string.IsNullOrEmpty(message))
                return ErrorJson(await _localizationService.GetResourceAsync("Admin.Orders.OrderNotes.Fields.Note.Validation"));

            //try to get an order with the specified id
            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order == null)
                return ErrorJson("Order cannot be loaded");

            //a vendor does not have access to this functionality
            if (await _workContext.GetCurrentVendorAsync() != null)
                return ErrorJson("No access for vendors");

            var orderNote = new OrderNote
            {
                OrderId = order.Id,
                DisplayToCustomer = displayToCustomer,
                Note = message,
                DownloadId = downloadId,
                CreatedOnUtc = DateTime.UtcNow
            };

            await _orderService.InsertOrderNoteAsync(orderNote);

            //insert manually added notes track record to admin note
            var adminNote = new AdminNote
            {
                OrderId = order.Id,
                OrderNoteId = orderNote.Id
            };

            await _adminNoteService.InsertAdminNoteAsync(adminNote);

            //new order notification
            if (displayToCustomer)
            {
                //email
                await _workflowMessageService.SendNewOrderNoteAddedCustomerNotificationAsync(orderNote, (await _workContext.GetWorkingLanguageAsync()).Id);
            }

            return Json(new { Result = true });
        }

        [HttpPost]
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task<IActionResult> OrderNoteDelete(int id, int orderId)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE))
                return AccessDeniedView();

            //try to get an order with the specified id
            _ = await _orderService.GetOrderByIdAsync(orderId)
                ?? throw new ArgumentException("No order found with the specified id");

            //a vendor does not have access to this functionality
            if (await _workContext.GetCurrentVendorAsync() != null)
                return RedirectToAction("Edit", "Order", new { id = orderId });

            //try to get an order note with the specified id
            var orderNote = await _orderService.GetOrderNoteByIdAsync(id)
                ?? throw new ArgumentException("No order note found with the specified id");

            await _orderService.DeleteOrderNoteAsync(orderNote);

            return new NullJsonResult();
        }

        #endregion

        #region Bulk tracking

        [HttpPost, ActionName("List")]
        [FormValueRequired("track-order-by-number")]
        public virtual async Task<IActionResult> TrackOrderId(Models.OrderShipment.OrderSearchModel searchModel)
        {
            var order = await _orderService.GetOrderByCustomOrderNumberAsync(searchModel.TrackOrder);

            if (order == null)
            {
                _notificationService.ErrorNotification(string.Format(await _localizationService.GetResourceAsync("Plugins.Misc.ShipmentTracking.Orders.OrderNotFound"), searchModel.TrackOrder));
                return await List();
            }

            return RedirectToAction("OrderTrackInfo", new { id = order.Id });
        }

        public virtual async Task<IActionResult> OrderTrackInfo(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE))
                return AccessDeniedView();

            //try to get an order with the specified id
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
                return RedirectToAction("List", "Order");

            //a vendor should have access only to his products
            if (await _workContext.GetCurrentVendorAsync() != null && !await HasAccessToOrderAsync(order))
                return RedirectToAction("List", "Order");

            //prepare model
            var model = await PrepareShipmentModelAsync(new Models.OrderShipment.ShipmentModel(), null, order);
            model.BulkTrackOrder = true;

            return View("~/Plugins/Misc.ShipmentTracking/Areas/Admin/Views/Order/AddShipment.cshtml", model);
        }

        [HttpPost]
        [FormValueRequired("save")]
        public virtual async Task<IActionResult> SaveOrderTrackInfo(Models.OrderShipment.ShipmentModel model, IFormCollection form)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Orders.ORDERS_CREATE_EDIT_DELETE))
                return AccessDeniedView();

            //try to get an order with the specified id
            var order = await _orderService.GetOrderByIdAsync(model.OrderId);
            if (order == null)
                return RedirectToAction("List", "Order");

            //a vendor should have access only to his products
            if (await _workContext.GetCurrentVendorAsync() != null && !await HasAccessToOrderAsync(order))
                return RedirectToAction("List", "Order");

            var orderItems = await _orderService.GetOrderItemsAsync(order.Id, isShipEnabled: true);
            //a vendor should have access only to his products
            if (await _workContext.GetCurrentVendorAsync() != null)
            {
                orderItems = await orderItems.WhereAwait(HasAccessToProductAsync).ToListAsync();
            }

            var shipment = new Shipment
            {
                OrderId = order.Id,
                TrackingNumber = model.TrackingNumber,
                TotalWeight = null,
                AdminComment = model.AdminComment,
                CreatedOnUtc = DateTime.UtcNow
            };

            var shipmentItems = new List<ShipmentItem>();

            decimal? totalWeight = null;

            foreach (var orderItem in orderItems)
            {
                var product = await _productService.GetProductByIdAsync(orderItem.ProductId);

                //ensure that this product can be shipped (have at least one item to ship)
                var maxQtyToAdd = await _orderService.GetTotalNumberOfItemsCanBeAddedToShipmentAsync(orderItem);
                if (maxQtyToAdd <= 0)
                    continue;

                var qtyToAdd = 0; //parse quantity
                foreach (var formKey in form.Keys)
                    if (formKey.Equals($"qtyToAdd{orderItem.Id}", StringComparison.InvariantCultureIgnoreCase))
                    {
                        int.TryParse(form[formKey], out qtyToAdd);
                        break;
                    }

                var warehouseId = 0;
                if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStock &&
                    product.UseMultipleWarehouses)
                {
                    //multiple warehouses supported
                    //warehouse is chosen by a store owner
                    foreach (var formKey in form.Keys)
                        if (formKey.Equals($"warehouse_{orderItem.Id}", StringComparison.InvariantCultureIgnoreCase))
                        {
                            int.TryParse(form[formKey], out warehouseId);
                            break;
                        }
                }
                else
                {
                    //multiple warehouses are not supported
                    warehouseId = product.WarehouseId;
                }

                //validate quantity
                if (qtyToAdd <= 0)
                    continue;
                if (qtyToAdd > maxQtyToAdd)
                    qtyToAdd = maxQtyToAdd;

                //ok. we have at least one item. let's create a shipment (if it does not exist)

                var orderItemTotalWeight = orderItem.ItemWeight * qtyToAdd;
                if (orderItemTotalWeight.HasValue)
                {
                    if (!totalWeight.HasValue)
                        totalWeight = 0;
                    totalWeight += orderItemTotalWeight.Value;
                }

                //create a shipment item
                shipmentItems.Add(new ShipmentItem
                {
                    OrderItemId = orderItem.Id,
                    Quantity = qtyToAdd,
                    WarehouseId = warehouseId
                });
            }

            //if we have at least one item in the shipment, then save it
            if (shipmentItems.Any())
            {
                shipment.TotalWeight = totalWeight;
                await _shipmentService.InsertShipmentAsync(shipment);

                if (!string.IsNullOrWhiteSpace(model.ShippingMethod))
                    await _genericAttributeService.SaveAttributeAsync(shipment, ShippingMethod, model.ShippingMethod);

                foreach (var shipmentItem in shipmentItems)
                {
                    shipmentItem.ShipmentId = shipment.Id;
                    await _shipmentService.InsertShipmentItemAsync(shipmentItem);
                }

                //add a note
                await _orderService.InsertOrderNoteAsync(new OrderNote
                {
                    OrderId = order.Id,
                    Note = "A shipment has been added",
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });

                if (model.CanShip)
                    await _orderProcessingService.ShipAsync(shipment, model.SendShippedEmail);

                if (model.CanShip && model.CanDeliver)
                    await _orderProcessingService.DeliverAsync(shipment, model.SendDeliveredEmail);

                await LogEditOrderAsync(order.Id);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Orders.Shipments.Added"));

                return RedirectToAction("List", "Order");
            }

            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Orders.Shipments.NoProductsSelected"));

            return RedirectToAction("OrderTrackInfo", new { id = order.Id });
        }

        #endregion

        #endregion
    }
}
