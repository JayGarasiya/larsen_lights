using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Vendors;
using Nop.Plugin.Misc.ShipmentTracking.Services;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Services.Shipping.Tracking;
using Nop.Services.Vendors;
using Nop.Web.Factories;
using Nop.Web.Models.Order;

namespace Nop.Plugin.Misc.ShipmentTracking.Factories
{
    /// <summary>
    /// Represent an order shipment model factory
    /// </summary>
    public class OrderShipmentModelFactory : OrderModelFactory
    {
        #region Constant 

        private const string ShippingMethod = "3PLCarrier";

        #endregion

        #region Fields

        protected readonly IShipmentTrackingService _shipmentTrackingService;
        protected readonly IGenericAttributeService _genericAttributeService;

        #endregion

        #region Ctor

        public OrderShipmentModelFactory(AddressSettings addressSettings, 
            CatalogSettings catalogSettings, 
            IAddressModelFactory addressModelFactory, 
            IAddressService addressService, 
            ICountryService countryService, 
            ICurrencyService currencyService, 
            ICustomerService customerService, 
            IDateTimeHelper dateTimeHelper, 
            IGiftCardService giftCardService, 
            ILocalizationService localizationService, 
            IOrderProcessingService orderProcessingService, 
            IOrderService orderService, 
            IOrderTotalCalculationService orderTotalCalculationService, 
            IPaymentPluginManager paymentPluginManager, 
            IPaymentService paymentService, 
            IPictureService pictureService, 
            IPriceFormatter priceFormatter, 
            IProductService productService, 
            IRewardPointService rewardPointService, 
            IShipmentService shipmentService, 
            IShortTermCacheManager shortTermCacheManager, 
            IStateProvinceService stateProvinceService, 
            IStaticCacheManager staticCacheManager, 
            IStoreContext storeContext, 
            IUrlRecordService urlRecordService, 
            IVendorService vendorService, 
            IWebHelper webHelper, 
            IWorkContext workContext, 
            MediaSettings mediaSettings, 
            OrderSettings orderSettings, 
            PdfSettings pdfSettings, 
            RewardPointsSettings rewardPointsSettings, 
            ShippingSettings shippingSettings, 
            TaxSettings taxSettings, 
            VendorSettings vendorSettings, 
            IShipmentTrackingService shipmentTrackingService,
            IGenericAttributeService genericAttributeService) 
            : base(addressSettings, catalogSettings, addressModelFactory, addressService, countryService, currencyService, customerService, dateTimeHelper, giftCardService, localizationService, orderProcessingService, orderService, orderTotalCalculationService, paymentPluginManager, paymentService, pictureService, priceFormatter, productService, rewardPointService, shipmentService, shortTermCacheManager, stateProvinceService, staticCacheManager, storeContext, urlRecordService, vendorService, webHelper, workContext, mediaSettings, orderSettings, pdfSettings, rewardPointsSettings, shippingSettings, taxSettings, vendorSettings)
        {
            _shipmentTrackingService = shipmentTrackingService;
            _genericAttributeService = genericAttributeService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Prepare the shipment details model
        /// </summary>
        /// <param name="shipment">Shipment</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the shipment details model
        /// </returns>
        public override async Task<ShipmentDetailsModel> PrepareShipmentDetailsModelAsync(Shipment shipment)
        {
            if (shipment == null)
                throw new ArgumentNullException(nameof(shipment));

            var order = await _orderService.GetOrderByIdAsync(shipment.OrderId);

            if (order == null)
                throw new Exception("order cannot be loaded");
            var model = new ShipmentDetailsModel
            {
                Id = shipment.Id
            };
            if (shipment.ShippedDateUtc.HasValue)
                model.ShippedDate = await _dateTimeHelper.ConvertToUserTimeAsync(shipment.ShippedDateUtc.Value, DateTimeKind.Utc);
            if (shipment.ReadyForPickupDateUtc.HasValue)
                model.ReadyForPickupDate = await _dateTimeHelper.ConvertToUserTimeAsync(shipment.ReadyForPickupDateUtc.Value, DateTimeKind.Utc);
            if (shipment.DeliveryDateUtc.HasValue)
                model.DeliveryDate = await _dateTimeHelper.ConvertToUserTimeAsync(shipment.DeliveryDateUtc.Value, DateTimeKind.Utc);

            //tracking number and shipment information
            if (!string.IsNullOrEmpty(shipment.TrackingNumber))
            {
                model.TrackingNumber = shipment.TrackingNumber;
                var shipmentTracker = await _shipmentService.GetShipmentTrackerAsync(shipment);
                var trackingNumberUrl = await _shipmentTrackingService.GetTrackingUrlAsync(shipment.TrackingNumber, await _genericAttributeService.GetAttributeAsync<string>(shipment, ShippingMethod));
                if (!string.IsNullOrEmpty(trackingNumberUrl))
                    model.TrackingNumberUrl = trackingNumberUrl;
                else
                {
                    if (shipmentTracker != null)
                        model.TrackingNumberUrl = await shipmentTracker.GetUrlAsync(shipment.TrackingNumber);
                }
                if (_shippingSettings.DisplayShipmentEventsToCustomers)
                {
                    var shipmentEvents = new List<ShipmentStatusEvent>();
                    var shippingMethod = await _genericAttributeService.GetAttributeAsync<string>(shipment, ShippingMethod);
                    shipmentEvents = (await _shipmentTrackingService.GetShipmentEventsAsync(shipment, shippingMethod)).ToList();

                    if (!shipmentEvents.Any())
                    {
                        if (shipmentTracker != null)
                            shipmentEvents = (await shipmentTracker.GetShipmentEventsAsync(shipment.TrackingNumber)).ToList();
                    }

                    foreach (var shipmentEvent in shipmentEvents)
                    {
                        var shipmentStatusEventModel = new ShipmentDetailsModel.ShipmentStatusEventModel();
                        var shipmentEventCountry = await _countryService.GetCountryByTwoLetterIsoCodeAsync(shipmentEvent.CountryCode);
                        shipmentStatusEventModel.Country = shipmentEventCountry != null
                            ? await _localizationService.GetLocalizedAsync(shipmentEventCountry, x => x.Name) : shipmentEvent.CountryCode;
                        shipmentStatusEventModel.Date = shipmentEvent.Date;
                        shipmentStatusEventModel.EventName = shipmentEvent.EventName;
                        shipmentStatusEventModel.Location = shipmentEvent.Location;
                        model.ShipmentStatusEvents.Add(shipmentStatusEventModel);
                    }
                }
            }

            //products in this shipment
            model.ShowSku = _catalogSettings.ShowSkuOnProductDetailsPage;
            foreach (var shipmentItem in await _shipmentService.GetShipmentItemsByShipmentIdAsync(shipment.Id))
            {
                var orderItem = await _orderService.GetOrderItemByIdAsync(shipmentItem.OrderItemId);
                if (orderItem == null)
                    continue;

                var product = await _productService.GetProductByIdAsync(orderItem.ProductId);

                var shipmentItemModel = new ShipmentDetailsModel.ShipmentItemModel
                {
                    Id = shipmentItem.Id,
                    Sku = await _productService.FormatSkuAsync(product, orderItem.AttributesXml),
                    ProductId = product.Id,
                    ProductName = await _localizationService.GetLocalizedAsync(product, x => x.Name),
                    ProductSeName = await _urlRecordService.GetSeNameAsync(product),
                    AttributeInfo = orderItem.AttributeDescription,
                    QuantityOrdered = orderItem.Quantity,
                    QuantityShipped = shipmentItem.Quantity,
                };
                //rental info
                if (product.IsRental)
                {
                    var rentalStartDate = orderItem.RentalStartDateUtc.HasValue
                        ? _productService.FormatRentalDate(product, orderItem.RentalStartDateUtc.Value) : "";
                    var rentalEndDate = orderItem.RentalEndDateUtc.HasValue
                        ? _productService.FormatRentalDate(product, orderItem.RentalEndDateUtc.Value) : "";
                    shipmentItemModel.RentalInfo = string.Format(await _localizationService.GetResourceAsync("Order.Rental.FormattedDate"),
                        rentalStartDate, rentalEndDate);
                }
                model.Items.Add(shipmentItemModel);
            }

            //order details model
            model.Order = await PrepareOrderDetailsModelAsync(order);

            return model;
        }
        #endregion
    }
}
