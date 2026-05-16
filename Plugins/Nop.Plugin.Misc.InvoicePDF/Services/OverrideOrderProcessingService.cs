using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Vendors;
using Nop.Core.Events;
using Nop.Services.Affiliates;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Services.Vendors;

namespace Nop.Plugin.Misc.InvoicePDF.Services
{
    /// <summary>
    /// Override Order processing service
    /// </summary>
    public class OverrideOrderProcessingService : OrderProcessingService
    {
        #region Fields
        private readonly InvoicePDFSettings _invoicePDFSettings;
        #endregion

        #region Ctor
        public OverrideOrderProcessingService(CurrencySettings currencySettings,
            IAddressService addressService,
            IAffiliateService affiliateService,
            ICheckoutAttributeFormatter checkoutAttributeFormatter,
            ICountryService countryService,
            ICurrencyService currencyService,
            ICustomerActivityService customerActivityService,
            ICustomerService customerService,
            ICustomNumberFormatter customNumberFormatter,
            IDiscountService discountService,
            IEncryptionService encryptionService,
            IEventPublisher eventPublisher,
            IGenericAttributeService genericAttributeService,
            IGiftCardService giftCardService,
            ILanguageService languageService,
            ILocalizationService localizationService,
            ILogger logger,
            IOrderService orderService,
            IOrderTotalCalculationService orderTotalCalculationService,
            IPaymentPluginManager paymentPluginManager,
            IPaymentService paymentService,
            IPdfService pdfService,
            IPriceCalculationService priceCalculationService,
            IPriceFormatter priceFormatter,
            IProductAttributeFormatter productAttributeFormatter,
            IProductAttributeParser productAttributeParser,
            IProductService productService,
            IReturnRequestService returnRequestService,
            IRewardPointService rewardPointService,
            IShipmentService shipmentService,
            IShippingService shippingService,
            IShoppingCartService shoppingCartService,
            IStateProvinceService stateProvinceService,
            IStaticCacheManager staticCacheManager,
            IStoreContext storeContext,
            IStoreMappingService storeMappingService,
            IStoreService storeService,
            ITaxService taxService,
            IVendorService vendorService,
            IWebHelper webHelper,
            IWorkContext workContext,
            IWorkflowMessageService workflowMessageService,
            LocalizationSettings localizationSettings,
            OrderSettings orderSettings,
            PaymentSettings paymentSettings,
            RewardPointsSettings rewardPointsSettings,
            ShippingSettings shippingSettings,
            TaxSettings taxSettings,
            InvoicePDFSettings invoicePDFSettings) : base(currencySettings,
                addressService,
                affiliateService,
                checkoutAttributeFormatter,
                countryService,
                currencyService,
                customerActivityService,
                customerService,
                customNumberFormatter,
                discountService,
                encryptionService,
                eventPublisher,
                genericAttributeService,
                giftCardService,
                languageService,
                localizationService,
                logger,
                orderService,
                orderTotalCalculationService,
                paymentPluginManager,
                paymentService,
                pdfService,
                priceCalculationService,
                priceFormatter,
                productAttributeFormatter,
                productAttributeParser,
                productService,
                returnRequestService,
                rewardPointService,
                shipmentService,
                shippingService,
                shoppingCartService,
                stateProvinceService,
                staticCacheManager,
                storeContext,
                storeMappingService,
                storeService,
                taxService,
                vendorService,
                webHelper,
                workContext,
                workflowMessageService,
                localizationSettings,
                orderSettings,
                paymentSettings,
                rewardPointsSettings,
                shippingSettings,
                taxSettings)
        {
            _invoicePDFSettings = invoicePDFSettings;
        }
        #endregion

        #region Utilities

        /// <summary>
        /// Send "order placed" notifications and save order notes
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected override async Task SendNotificationsAndSaveNotesAsync(Order order)
        {
            //notes, messages
            await base.AddOrderNoteAsync(order, _workContext.OriginalCustomerIfImpersonated != null
                ? $"Order placed by a store owner ('{_workContext.OriginalCustomerIfImpersonated.Email}'. ID = {_workContext.OriginalCustomerIfImpersonated.Id}) impersonating the customer."
                : "Order placed");

            //send email notifications
            var orderPlacedStoreOwnerNotificationQueuedEmailIds = await _workflowMessageService
                .SendOrderPlacedStoreOwnerNotificationAsync(order, _localizationSettings.DefaultAdminLanguageId);
            if (orderPlacedStoreOwnerNotificationQueuedEmailIds.Any())
                await base.AddOrderNoteAsync(order, $"\"Order placed\" email (to store owner) has been queued. Queued email identifiers: {string.Join(", ", orderPlacedStoreOwnerNotificationQueuedEmailIds)}.");

            //var currecntvendor = await _workContext.GetCurrentVendorAsync();
            //var language = await _workContext.GetWorkingLanguageAsync();
             
            var orderPlacedAttachmentFilePath = _orderSettings.AttachPdfInvoiceToOrderPlacedEmail ?
                await _pdfService.SaveOrderPdfToDiskAsync(order) : null;

            var orderPlacedAttachmentFileName = _orderSettings.AttachPdfInvoiceToOrderPlacedEmail ?
                "order.pdf" : null;

            if (!string.IsNullOrEmpty(orderPlacedAttachmentFileName))
            {
                if (_invoicePDFSettings.AttachOrderId)
                {
                    if (_invoicePDFSettings.UseOrderMask)
                        orderPlacedAttachmentFileName = $"order_{order.CustomOrderNumber}.pdf";
                    else
                        orderPlacedAttachmentFileName = $"order_{order.Id}.pdf";
                }
            }

            var orderPlacedCustomerNotificationQueuedEmailIds = await _workflowMessageService
                .SendOrderPlacedCustomerNotificationAsync(order, order.CustomerLanguageId, orderPlacedAttachmentFilePath, orderPlacedAttachmentFileName);
            if (orderPlacedCustomerNotificationQueuedEmailIds.Any())
                await base.AddOrderNoteAsync(order, $"\"Order placed\" email (to customer) has been queued. Queued email identifiers: {string.Join(", ", orderPlacedCustomerNotificationQueuedEmailIds)}.");

            var vendors = await GetVendorsInOrderAsync(order);
            foreach (var vendor in vendors)
            {
                var orderPlacedVendorNotificationQueuedEmailIds = await _workflowMessageService.SendOrderPlacedVendorNotificationAsync(order, vendor, _localizationSettings.DefaultAdminLanguageId);
                if (orderPlacedVendorNotificationQueuedEmailIds.Any())
                    await base.AddOrderNoteAsync(order, $"\"Order placed\" email (to vendor) has been queued. Queued email identifiers: {string.Join(", ", orderPlacedVendorNotificationQueuedEmailIds)}.");
            }

            if (order.AffiliateId == 0)
                return;

            var orderPlacedAffiliateNotificationQueuedEmailIds = await _workflowMessageService.SendOrderPlacedAffiliateNotificationAsync(order, _localizationSettings.DefaultAdminLanguageId);
            if (orderPlacedAffiliateNotificationQueuedEmailIds.Any())
                await base.AddOrderNoteAsync(order, $"\"Order placed\" email (to affiliate) has been queued. Queued email identifiers: {string.Join(", ", orderPlacedAffiliateNotificationQueuedEmailIds)}.");
        }

        /// <summary>
        /// Get a list of vendors in order (order items)
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the vendors
        /// </returns>
        protected override async Task<IList<Vendor>> GetVendorsInOrderAsync(Order order)
        {
            var pIds = new List<int>();
            var orderItems = await _orderService.GetOrderItemsAsync(order.Id);
            foreach(var orderItem in orderItems)
            {
                var product = await _productService.GetProductByIdAsync(orderItem.ProductId);
                if (product.ManageInventoryMethod != ManageInventoryMethod.ManageStockByAttributes)
                    pIds.Add(product.Id);
                else
                {
                    foreach (var productAttributeValue in await _productAttributeParser.ParseProductAttributeValuesAsync(orderItem.AttributesXml))
                    {
                        if (productAttributeValue.AttributeValueType == AttributeValueType.AssociatedToProduct)
                            pIds.Add(productAttributeValue.AssociatedProductId);
                    }
                }
            }

            return await _vendorService.GetVendorsByProductIdsAsync(pIds.ToArray());
        }

        /// <summary>
        /// Process order paid status
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected override async Task ProcessOrderPaidAsync(Order order)
        {
            ArgumentNullException.ThrowIfNull(order);

            //raise event
            await _eventPublisher.PublishAsync(new OrderPaidEvent(order));

            //order paid email notification
            if (order.OrderTotal != decimal.Zero)
            {
                //we should not send it for free ($0 total) orders?
                //remove this "if" statement if you want to send it in this case

                var currecntvendor = await _workContext.GetCurrentVendorAsync();
                var language = await _workContext.GetWorkingLanguageAsync();

                var orderPaidAttachmentFilePath = _orderSettings.AttachPdfInvoiceToOrderPaidEmail ?
                      await _pdfService.SaveOrderPdfToDiskAsync(order, language, currecntvendor) : null;
                var orderPaidAttachmentFileName = _orderSettings.AttachPdfInvoiceToOrderPaidEmail ?
                    (string.Format(await _localizationService.GetResourceAsync("PDFInvoice.FileName"), order.CustomOrderNumber) + ".pdf") : null;

                if (!string.IsNullOrEmpty(orderPaidAttachmentFileName))
                {
                    if (_invoicePDFSettings.AttachOrderId)
                    {
                        if (_invoicePDFSettings.UseOrderMask)
                            orderPaidAttachmentFileName = $"order_{order.CustomOrderNumber}.pdf";
                        else
                            orderPaidAttachmentFileName = $"order_{order.Id}.pdf";
                    }
                }

                var orderPaidCustomerNotificationQueuedEmailIds = await _workflowMessageService.SendOrderPaidCustomerNotificationAsync(order, order.CustomerLanguageId,
                    orderPaidAttachmentFilePath, orderPaidAttachmentFileName);

                if (orderPaidCustomerNotificationQueuedEmailIds.Any())
                    await base.AddOrderNoteAsync(order, $"\"Order paid\" email (to customer) has been queued. Queued email identifiers: {string.Join(", ", orderPaidCustomerNotificationQueuedEmailIds)}.");

                var orderPaidStoreOwnerNotificationQueuedEmailIds = await _workflowMessageService.SendOrderPaidStoreOwnerNotificationAsync(order, _localizationSettings.DefaultAdminLanguageId);
                if (orderPaidStoreOwnerNotificationQueuedEmailIds.Any())
                    await base.AddOrderNoteAsync(order, $"\"Order paid\" email (to store owner) has been queued. Queued email identifiers: {string.Join(", ", orderPaidStoreOwnerNotificationQueuedEmailIds)}.");

                var vendors = await GetVendorsInOrderAsync(order);
                foreach (var vendor in vendors)
                {
                    var orderPaidVendorNotificationQueuedEmailIds = await _workflowMessageService.SendOrderPaidVendorNotificationAsync(order, vendor, _localizationSettings.DefaultAdminLanguageId);

                    if (orderPaidVendorNotificationQueuedEmailIds.Any())
                        await base.AddOrderNoteAsync(order, $"\"Order paid\" email (to vendor) has been queued. Queued email identifiers: {string.Join(", ", orderPaidVendorNotificationQueuedEmailIds)}.");
                }

                if (order.AffiliateId != 0)
                {
                    var orderPaidAffiliateNotificationQueuedEmailIds = await _workflowMessageService.SendOrderPaidAffiliateNotificationAsync(order,
                        _localizationSettings.DefaultAdminLanguageId);
                    if (orderPaidAffiliateNotificationQueuedEmailIds.Any())
                        await base.AddOrderNoteAsync(order, $"\"Order paid\" email (to affiliate) has been queued. Queued email identifiers: {string.Join(", ", orderPaidAffiliateNotificationQueuedEmailIds)}.");
                }
            }

            //customer roles with "purchased with product" specified
            await base.ProcessCustomerRolesWithPurchasedProductSpecifiedAsync(order, true);
        }

        #endregion
    }
}