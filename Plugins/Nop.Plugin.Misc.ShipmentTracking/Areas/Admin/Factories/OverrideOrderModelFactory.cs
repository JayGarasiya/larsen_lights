using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Plugin.Misc.ShipmentTracking.Domain;
using Nop.Services.Affiliates;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Services.Vendors;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.Orders;

namespace Nop.Plugin.Misc.ShipmentTracking.Areas.Admin.Factories
{
    /// <summary>
    /// Represent an override order model factory
    /// </summary>
    public class OverrideOrderModelFactory : OrderModelFactory
    {
        #region Fields

        protected readonly IPaymentService _paymentService;

        #endregion

        #region Ctor

        public OverrideOrderModelFactory(AddressSettings addressSettings,
           CatalogSettings catalogSettings,
           CurrencySettings currencySettings,
           IActionContextAccessor actionContextAccessor,
           IAddressModelFactory addressModelFactory,
           IAddressService addressService,
           IAffiliateService affiliateService,
           IBaseAdminModelFactory baseAdminModelFactory,
           ICountryService countryService,
           ICurrencyService currencyService,
           ICustomerService customerService,
           IDateTimeHelper dateTimeHelper,
           IDiscountService discountService,
           IDownloadService downloadService,
           IEncryptionService encryptionService,
           IGiftCardService giftCardService,
           ILocalizationService localizationService,
           IMeasureService measureService,
           IOrderProcessingService orderProcessingService,
           IOrderReportService orderReportService,
           IOrderService orderService,
           IPaymentPluginManager paymentPluginManager,
           IPictureService pictureService,
           IPriceCalculationService priceCalculationService,
           IPriceFormatter priceFormatter,
           IProductAttributeService productAttributeService,
           IProductService productService,
           IReturnRequestService returnRequestService,
           IRewardPointService rewardPointService,
           ISettingService settingService,
           IShipmentService shipmentService,
           IStateProvinceService stateProvinceService,
           IStoreService storeService,
           ITaxService taxService,
           IUrlHelperFactory urlHelperFactory,
           IVendorService vendorService,
           IWarehouseService warehouseService,
           IWorkContext workContext,
           MeasureSettings measureSettings,
           NopHttpClient nopHttpClient,
           OrderSettings orderSettings,
           ShippingSettings shippingSettings,
           IUrlRecordService urlRecordService,
           TaxSettings taxSettings,
           IPaymentService paymentService)
           : base(addressSettings, catalogSettings, currencySettings, actionContextAccessor, addressModelFactory, addressService, affiliateService, baseAdminModelFactory, countryService, currencyService, customerService, dateTimeHelper, discountService, downloadService, encryptionService, giftCardService, localizationService, measureService, orderProcessingService, orderReportService, orderService, paymentPluginManager, pictureService, priceCalculationService, priceFormatter, productAttributeService, productService, returnRequestService, rewardPointService, settingService, shipmentService, stateProvinceService, storeService, taxService, urlHelperFactory, vendorService, warehouseService, workContext, measureSettings, nopHttpClient, orderSettings, shippingSettings, urlRecordService, taxSettings)
        {
            _paymentService = paymentService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Prepare order model
        /// </summary>
        /// <param name="model">Order model</param>
        /// <param name="order">Order</param>
        /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the order model
        /// </returns>
        public override async Task<OrderModel> PrepareOrderModelAsync(OrderModel model, Order order, bool excludeProperties = false)
        {
            if (order != null)
            {
                //fill in model values from the entity
                model ??= new OrderModel
                {
                    Id = order.Id,
                    OrderStatusId = order.OrderStatusId,
                    VatNumber = order.VatNumber,
                    CheckoutAttributeInfo = order.CheckoutAttributeDescription
                };

                var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);

                model.OrderGuid = order.OrderGuid;
                model.CustomOrderNumber = order.CustomOrderNumber;
                model.CustomerIp = order.CustomerIp;
                model.CustomerId = customer.Id;
                model.OrderStatus = await _localizationService.GetLocalizedEnumAsync(order.OrderStatus);
                model.StoreName = (await _storeService.GetStoreByIdAsync(order.StoreId))?.Name ?? "Deleted";
                model.CustomerInfo = await _customerService.IsRegisteredAsync(customer) ? customer.Email : await _localizationService.GetResourceAsync("Admin.Customers.Guest");
                model.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(order.CreatedOnUtc, DateTimeKind.Utc);
                model.CustomValues.FillByXml(order.CustomValuesXml);

                var affiliate = await _affiliateService.GetAffiliateByIdAsync(order.AffiliateId);
                if (affiliate != null)
                {
                    model.AffiliateId = affiliate.Id;
                    model.AffiliateName = await _affiliateService.GetAffiliateFullNameAsync(affiliate);
                }

                //prepare order totals
                await PrepareOrderModelTotalsAsync(model, order);

                //prepare order items
                await PrepareOrderItemModelsAsync(model.Items, order);
                model.HasDownloadableProducts = model.Items.Any(item => item.IsDownload);

                //prepare payment info
                await PrepareOrderModelPaymentInfoAsync(model, order);

                //prepare shipping info
                await PrepareOrderModelShippingInfoAsync(model, order);

                //prepare nested search model
                PrepareOrderShipmentSearchModel(model.OrderShipmentSearchModel, order);
                PrepareOrderNoteSearchModel(model.OrderNoteSearchModel, order);

                //compare billing and shipping address is it different
                if (!order.PickupInStore)
                {
                    var billingAddress = await _addressService.GetAddressByIdAsync(order.BillingAddressId);
                    var shippingAddress = await _addressService.GetAddressByIdAsync(order.ShippingAddressId.Value);
                    var isDiffAddress = !billingAddress.IsEqualTo(shippingAddress);
                    model.ShippingAddress.CustomProperties.Add("IsDiffAddress", isDiffAddress.ToString());
                }
            }

            model.IsLoggedInAsVendor = await _workContext.GetCurrentVendorAsync() != null;
            model.AllowCustomersToSelectTaxDisplayType = _taxSettings.AllowCustomersToSelectTaxDisplayType;
            model.TaxDisplayType = _taxSettings.TaxDisplayType;

            return model;
        }

        #endregion
    }
}
