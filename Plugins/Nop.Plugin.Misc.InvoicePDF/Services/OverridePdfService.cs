using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Wordprocessing;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Stores;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Vendors;
using Nop.Core.Infrastructure;
using Nop.Plugin.Misc.InvoicePDF.Services.Pdf;
using Nop.Services.Attributes;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Common.Pdf;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Html;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using Nop.Services.Vendors;
using NUglify;
using PdfRpt.Core.Contracts;
using System.Globalization;
using System.IO.Compression;

namespace Nop.Plugin.Misc.InvoicePDF.Services
{
    /// <summary>
    /// PDF service
    /// </summary>
    public class OverridePdfService : PdfService
    {
        #region Fields
        private readonly ICustomerService _customerService;
        private readonly InvoicePDFSettings _invoicePDFSettings;
        private readonly IProductAttributeFormatter _productAttributeFormatter;
        private readonly IGenericAttributeService _genericAttributeService;
        #endregion

        #region Ctor
        public OverridePdfService(AddressSettings addressSettings,
            CatalogSettings catalogSettings,
            CurrencySettings currencySettings,
            IAddressService addressService,
            IAttributeFormatter<AddressAttribute, AddressAttributeValue> addressAttributeFormatter,
            ICountryService countryService,
            ICurrencyService currencyService,
            IDateTimeHelper dateTimeHelper,
            IGiftCardService giftCardService,
            IHtmlFormatter htmlFormatter,
            ILanguageService languageService,
            ILocalizationService localizationService,
            IMeasureService measureService,
            INopFileProvider fileProvider,
            IOrderService orderService,
            IPaymentPluginManager paymentPluginManager,
            IPaymentService paymentService,
            IPictureService pictureService,
            IPriceFormatter priceFormatter,
            IProductService productService,
            IRewardPointService rewardPointService,
            ISettingService settingService,
            IShipmentService shipmentService,
            IStateProvinceService stateProvinceService,
            IStoreContext storeContext,
            IStoreService storeService,
            IThumbService thumbService,
            IVendorService vendorService,
            IWorkContext workContext,
            MeasureSettings measureSettings,
            TaxSettings taxSettings,
            VendorSettings vendorSettings,
            ICustomerService customerService,
            InvoicePDFSettings invoicePDFSettings,
            IProductAttributeFormatter productAttributeFormatter,
            IGenericAttributeService genericAttributeService) : base(addressSettings,
                catalogSettings,
                currencySettings,
                addressService,
                addressAttributeFormatter,
                countryService,
                currencyService,
                dateTimeHelper,
                giftCardService,
                htmlFormatter,
                languageService,
                localizationService,
                measureService,
                fileProvider,
                orderService,
                paymentPluginManager,
                paymentService,
                pictureService,
                priceFormatter,
                productService,
                rewardPointService,
                settingService,
                shipmentService,
                stateProvinceService,
                storeContext,
                storeService,
                thumbService,
                vendorService,
                workContext,
                measureSettings,
                taxSettings,
                vendorSettings)
        {
            _customerService = customerService;
            _invoicePDFSettings = invoicePDFSettings;
            _productAttributeFormatter = productAttributeFormatter;
            _genericAttributeService = genericAttributeService;
        }
        #endregion

        #region Utilities

        /// <summary>
        /// Get product entries for document data source
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="orderItems">Collection of order items</param>
        /// <param name="language">Language</param>
        /// <param name="shipmentItems">Collection of shipment items; when using to prepare shipment items</param>
        /// <returns>A task that contains collection of product entries</returns>
        protected virtual async Task<List<ProductItem>> GetOrderProductItemsAsync(Order order, IList<OrderItem> orderItems, Language language, IList<ShipmentItem> shipmentItems = null)
        {
            var vendors = _vendorSettings.ShowVendorOnOrderDetailsPage ? await _vendorService.GetVendorsByProductIdsAsync(orderItems.Select(item => item.ProductId).ToArray()) : new List<Vendor>();

            var result = new List<ProductItem>();

            foreach (var oi in orderItems)
            {
                var productItem = new ProductItem();
                var product = await _productService.GetProductByIdAsync(oi.ProductId);
                var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);
                var store = await _storeContext.GetCurrentStoreAsync();

                //product name
                productItem.Name = await _localizationService.GetLocalizedAsync(product, x => x.Name, language.Id);

                //attributes
                if (!string.IsNullOrEmpty(oi.AttributeDescription))
                {
                    if (!_invoicePDFSettings.RenderAttributeValuePrices)
                    {
                        var attributeDescription = await _productAttributeFormatter.FormatAttributesAsync(product, oi.AttributesXml, customer, store,
                            renderPrices: _invoicePDFSettings.RenderAttributeValuePrices);
                        var attributes = _htmlFormatter.ConvertHtmlToPlainText(attributeDescription, true, true);
                        productItem.ProductAttributes = attributes.Split('\n').ToList();
                    }
                    else
                    {
                        var attributes = _htmlFormatter.ConvertHtmlToPlainText(oi.AttributeDescription, true, true);
                        productItem.ProductAttributes = attributes.Split('\n').ToList();

                    }
                }

                //SKU
                if (_catalogSettings.ShowSkuOnProductDetailsPage)
                    productItem.Sku = await _productService.FormatSkuAsync(product, oi.AttributesXml);

                //Vendor name
                if (_vendorSettings.ShowVendorOnOrderDetailsPage)
                    productItem.VendorName = vendors.FirstOrDefault(v => v.Id == product.VendorId)?.Name ?? string.Empty;

                //price
                string unitPrice;
                if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
                {
                    //including tax
                    var unitPriceInclTaxInCustomerCurrency =
                        _currencyService.ConvertCurrency(oi.UnitPriceInclTax, order.CurrencyRate);
                    unitPrice = await _priceFormatter.FormatPriceAsync(unitPriceInclTaxInCustomerCurrency, true,
                        order.CustomerCurrencyCode, language.Id, true);
                }
                else
                {
                    //excluding tax
                    var unitPriceExclTaxInCustomerCurrency =
                        _currencyService.ConvertCurrency(oi.UnitPriceExclTax, order.CurrencyRate);
                    unitPrice = await _priceFormatter.FormatPriceAsync(unitPriceExclTaxInCustomerCurrency, true,
                        order.CustomerCurrencyCode, language.Id, false);
                }

                productItem.Price = unitPrice;

                //qty
                productItem.Quantity = shipmentItems is null ?
                    oi.Quantity.ToString() :
                    shipmentItems.FirstOrDefault(x => x.OrderItemId == oi.Id).Quantity.ToString();

                //total
                string subTotal;
                if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
                {
                    //including tax
                    var priceInclTaxInCustomerCurrency =
                        _currencyService.ConvertCurrency(oi.PriceInclTax, order.CurrencyRate);
                    subTotal = await _priceFormatter.FormatPriceAsync(priceInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode,
                        language.Id, true);
                }
                else
                {
                    //excluding tax
                    var priceExclTaxInCustomerCurrency =
                        _currencyService.ConvertCurrency(oi.PriceExclTax, order.CurrencyRate);
                    subTotal = await _priceFormatter.FormatPriceAsync(priceExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode,
                        language.Id, false);
                }

                productItem.Total = subTotal;

                result.Add(productItem);
            }

            return result;
        }

        /// <summary>
        /// Get invoice totals
        /// </summary>
        /// <param name="lang">Language</param>
        /// <param name="order">Order</param>
        /// <returns>A task that contains invoice totals</returns>
        protected override async Task<InvoiceTotals> GetTotalsAsync(Language lang, Order order)
        {
            var result = new InvoiceTotals();
            var languageId = lang.Id;

            //order subtotal
            if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax &&
                !_taxSettings.ForceTaxExclusionFromOrderSubtotal)
            {
                //including tax
                var orderSubtotalInclTaxInCustomerCurrency =
                    _currencyService.ConvertCurrency(order.OrderSubtotalInclTax, order.CurrencyRate);
                result.SubTotal = await _priceFormatter.FormatPriceAsync(orderSubtotalInclTaxInCustomerCurrency, true,
                    order.CustomerCurrencyCode, languageId, true);
            }
            else
            {
                //excluding tax
                var orderSubtotalExclTaxInCustomerCurrency =
                    _currencyService.ConvertCurrency(order.OrderSubtotalExclTax, order.CurrencyRate);
                result.SubTotal = await _priceFormatter.FormatPriceAsync(orderSubtotalExclTaxInCustomerCurrency, true,
                    order.CustomerCurrencyCode, languageId, false);
            }

            //discount (applied to order subtotal)
            if (order.OrderSubTotalDiscountExclTax > decimal.Zero)
            {
                //order subtotal
                if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax &&
                    !_taxSettings.ForceTaxExclusionFromOrderSubtotal)
                {
                    //including tax
                    var orderSubTotalDiscountInclTaxInCustomerCurrency =
                        _currencyService.ConvertCurrency(order.OrderSubTotalDiscountInclTax, order.CurrencyRate);
                    result.Discount = await _priceFormatter.FormatPriceAsync(
                        -orderSubTotalDiscountInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, true);
                }
                else
                {
                    //excluding tax
                    var orderSubTotalDiscountExclTaxInCustomerCurrency =
                        _currencyService.ConvertCurrency(order.OrderSubTotalDiscountExclTax, order.CurrencyRate);
                    result.Discount = await _priceFormatter.FormatPriceAsync(
                        -orderSubTotalDiscountExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, false);
                }
            }

            //shipping
            if (order.ShippingStatus != ShippingStatus.ShippingNotRequired)
            {
                if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
                {
                    //including tax
                    var orderShippingInclTaxInCustomerCurrency =
                        _currencyService.ConvertCurrency(order.OrderShippingInclTax, order.CurrencyRate);
                    result.Shipping = await _priceFormatter.FormatShippingPriceAsync(
                        orderShippingInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, true);
                }
                else
                {
                    //excluding tax
                    var orderShippingExclTaxInCustomerCurrency =
                        _currencyService.ConvertCurrency(order.OrderShippingExclTax, order.CurrencyRate);
                    result.Shipping = await _priceFormatter.FormatShippingPriceAsync(
                        orderShippingExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, false);
                }
            }

            //payment fee
            if (order.PaymentMethodAdditionalFeeExclTax > decimal.Zero)
            {
                if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
                {
                    //including tax
                    var paymentMethodAdditionalFeeInclTaxInCustomerCurrency =
                        _currencyService.ConvertCurrency(order.PaymentMethodAdditionalFeeInclTax, order.CurrencyRate);
                    result.PaymentMethodAdditionalFee = await _priceFormatter.FormatPaymentMethodAdditionalFeeAsync(
                        paymentMethodAdditionalFeeInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, true);
                }
                else
                {
                    //excluding tax
                    var paymentMethodAdditionalFeeExclTaxInCustomerCurrency =
                        _currencyService.ConvertCurrency(order.PaymentMethodAdditionalFeeExclTax, order.CurrencyRate);
                    result.PaymentMethodAdditionalFee = await _priceFormatter.FormatPaymentMethodAdditionalFeeAsync(
                        paymentMethodAdditionalFeeExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, false);
                }
            }

            //tax
            var taxStr = string.Empty;
            var taxRates = new SortedDictionary<decimal, decimal>();
            bool displayTax;
            var displayTaxRates = true;
            if (_taxSettings.HideTaxInOrderSummary && order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
            {
                displayTax = false;
            }
            else
            {
                if (order.OrderTax == 0 && _taxSettings.HideZeroTax)
                {
                    displayTax = false;
                    displayTaxRates = false;
                }
                else
                {
                    taxRates = _orderService.ParseTaxRates(order, order.TaxRates);

                    displayTaxRates = _taxSettings.DisplayTaxRates && taxRates.Any();
                    displayTax = !displayTaxRates;

                    var orderTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderTax, order.CurrencyRate);
                    taxStr = await _priceFormatter.FormatPriceAsync(orderTaxInCustomerCurrency, true, order.CustomerCurrencyCode,
                        false, languageId);
                }
            }

            if (displayTax)
            {
                result.Tax = taxStr;
            }

            if (displayTaxRates)
            {
                foreach (var item in taxRates)
                {
                    var taxRate = string.Format(await _localizationService.GetResourceAsync("Pdf.TaxRate", languageId),
                        _priceFormatter.FormatTaxRate(item.Key));
                    var taxValue = await _priceFormatter.FormatPriceAsync(
                        _currencyService.ConvertCurrency(item.Value, order.CurrencyRate), true, order.CustomerCurrencyCode,
                        false, languageId);

                    result.TaxRates.Add($"{taxRate} {taxValue}");
                }
            }

            //discount (applied to order total)
            if (order.OrderDiscount > decimal.Zero)
            {
                var orderDiscountInCustomerCurrency =
                    _currencyService.ConvertCurrency(order.OrderDiscount, order.CurrencyRate);
                result.Discount = await _priceFormatter.FormatPriceAsync(-orderDiscountInCustomerCurrency,
                    true, order.CustomerCurrencyCode, false, languageId);
            }

            //gift cards
            foreach (var gcuh in await _giftCardService.GetGiftCardUsageHistoryAsync(order))
            {
                var gcTitle = string.Format(await _localizationService.GetResourceAsync("Pdf.GiftCardInfo", languageId),
                    (await _giftCardService.GetGiftCardByIdAsync(gcuh.GiftCardId))?.GiftCardCouponCode);
                var gcAmountStr = await _priceFormatter.FormatPriceAsync(
                    -_currencyService.ConvertCurrency(gcuh.UsedValue, order.CurrencyRate), true,
                    order.CustomerCurrencyCode, false, languageId);

                result.GiftCards.Add($"{gcTitle} {gcAmountStr}");
            }

            //reward points
            if (order.RedeemedRewardPointsEntryId.HasValue && await _rewardPointService.GetRewardPointsHistoryEntryByIdAsync(order.RedeemedRewardPointsEntryId.Value) is RewardPointsHistory redeemedRewardPointsEntry)
            {
                var rpTitle = string.Format(await _localizationService.GetResourceAsync("Pdf.RewardPoints", languageId),
                    -redeemedRewardPointsEntry.Points);
                var rpAmount = await _priceFormatter.FormatPriceAsync(
                    -_currencyService.ConvertCurrency(redeemedRewardPointsEntry.UsedAmount, order.CurrencyRate),
                    true, order.CustomerCurrencyCode, false, languageId);

                result.RewardPoints = $"{rpTitle} {rpAmount}";
            }

            //order total
            var orderTotalInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderTotal, order.CurrencyRate);
            var orderTotalStr = await _priceFormatter.FormatPriceAsync(orderTotalInCustomerCurrency, true, order.CustomerCurrencyCode, false, languageId);
            result.OrderTotal = $"{await _localizationService.GetResourceAsync("Pdf.OrderTotal", languageId)}: {orderTotalStr}";

            return result;
        }

        protected virtual Task<string> GetTrackingUrlAsync(string trackingNumber, string shippingMethod)
        {
            if (!string.IsNullOrEmpty(shippingMethod) && !string.IsNullOrWhiteSpace(trackingNumber))
            {
                trackingNumber = trackingNumber.Trim();
                //Find tracking url token to replace with new url
                if (shippingMethod.Equals("UPS"))
                {
                    return Task.FromResult($"https://www.ups.com/track?&tracknum={trackingNumber}");
                }
                else if (shippingMethod.Equals("USPS"))
                {
                    return Task.FromResult($"https://tools.usps.com/go/TrackConfirmAction?tLabels={trackingNumber}");
                }
                else if (shippingMethod.Equals("FEDEX"))
                {
                    return Task.FromResult($"https://www.fedex.com/apps/fedextrack/?action=track&tracknumbers={trackingNumber}");
                }
                else if (shippingMethod.Equals("SPEEDEE"))
                {
                    return Task.FromResult($"https://speedeedelivery.com/track-a-shipment/?barcodes={trackingNumber}");
                }
                else if (shippingMethod.Equals("DHL"))
                {
                    return Task.FromResult($"https://www.dhl.com/global-en/home/tracking/tracking-express.html?submit=1&tracking-id={trackingNumber}");
                }
            }

            return Task.FromResult(string.Empty);
        }

        protected virtual async Task<List<(string, string)>> GetShipmentTrackingDetailAsync(int orderId, int? vendorId)
        {
            var trackingDetails = new List<(string, string)>();

            //shipment tracking
            //get shipments
            var shipments = (await _shipmentService.GetAllShipmentsAsync(
                orderId: orderId,
                //a vendor should have access only to his products
                vendorId: vendorId ?? 0))
                .OrderBy(shipment => shipment.CreatedOnUtc)
                .ToList();

            foreach (var shipment in shipments)
            {
                if (!string.IsNullOrEmpty(shipment.TrackingNumber))
                {
                    var trackingNumberUrl = await GetTrackingUrlAsync(shipment.TrackingNumber, await _genericAttributeService.GetAttributeAsync<string>(shipment, "3PLCarrier"));
                    if (!string.IsNullOrEmpty(trackingNumberUrl))
                        trackingDetails.Add((shipment.TrackingNumber, trackingNumberUrl));
                    else
                    {
                        var shipmentTracker = await _shipmentService.GetShipmentTrackerAsync(shipment);
                        if (shipmentTracker != null)
                        {
                            trackingNumberUrl = await shipmentTracker.GetUrlAsync(shipment.TrackingNumber);
                            if (!string.IsNullOrEmpty(trackingNumberUrl))
                                trackingDetails.Add((shipment.TrackingNumber, trackingNumberUrl));
                        }
                    }
                }
            }

            return trackingDetails;
        }

        /// <summary>
        /// Resolve font for PDF document
        /// </summary>
        /// <param name="language">Language</param>
        /// <param name="settings">PDF settings</param>
        /// <returns>A font object</returns>
        protected virtual iTextSharp.text.Font ResolvePdfFont(Language language, PdfSettings settings)
        {
            ArgumentNullException.ThrowIfNull(settings);

            var fontName = language?.Rtl == true
                ? !string.IsNullOrEmpty(settings.RtlFontName) ? settings.RtlFontName : NopCommonDefaults.PdfRtlFontName
                : !string.IsNullOrEmpty(settings.LtrFontName) ? settings.LtrFontName : NopCommonDefaults.PdfLtrFontName;

            var fontSize = settings.BaseFontSize >= 0 ? settings.BaseFontSize : 10;

            return PdfDocumentHelper.GetFont(fontName, fontSize);
        }

        #endregion

        #region Methods
        /// <summary>
        /// Write PDF invoice to the specified stream
        /// </summary>
        /// <param name="stream">Stream to save PDF</param>
        /// <param name="order">Order</param>
        /// <param name="language">Language; null to use a language used when placing an order</param>
        /// <param name="store">Store</param>
        /// <param name="vendor">Vendor to limit products; null to print all products. If specified, then totals won't be printed</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// </returns>
        public override async Task PrintOrderToPdfAsync(Stream stream, Order order, Language language = null, Store store = null, Vendor vendor = null)
        {
            ArgumentNullException.ThrowIfNull(order);

            //store info
            store ??= await _storeContext.GetCurrentStoreAsync();

            var orderStore = order.StoreId == 0 || order.StoreId == store?.Id ? store : await _storeService.GetStoreByIdAsync(order.StoreId);

            //language info
            language ??= await _languageService.GetLanguageByIdAsync(order.CustomerLanguageId);

            if (language?.Published != true)
                language = await _workContext.GetWorkingLanguageAsync();

            //by default _pdfSettings contains settings for the current active store
            //and we need PdfSettings for the store which was used to place an order
            //so let's load it based on a store of the current order
            var pdfSettingsByStore = await _settingService.LoadSettingAsync<PdfSettings>(orderStore.Id);

            byte[] logo = null;
            var logoPicture = await _pictureService.GetPictureByIdAsync(pdfSettingsByStore.LogoPictureId);
            if (logoPicture != null)
            {
                logo = await _pictureService.LoadPictureBinaryAsync(logoPicture);

                if (logoPicture.MimeType == MimeTypes.ImageSvg)
                {
                    await using var logoStream = new MemoryStream(logo);
                    logo = await _pictureService.ConvertSvgToPngAsync(logoStream);
                }
            }

            var date = await _dateTimeHelper.ConvertToUserTimeAsync(order.CreatedOnUtc, DateTimeKind.Utc);

            //a vendor should have access only to products
            var orderItems = await _orderService.GetOrderItemsAsync(order.Id, vendorId: vendor?.Id ?? 0);

            var column1Lines = string.IsNullOrEmpty(pdfSettingsByStore.InvoiceFooterTextColumn1) ?
                new List<string>()
                : pdfSettingsByStore.InvoiceFooterTextColumn1.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToList();

            var column2Lines = string.IsNullOrEmpty(pdfSettingsByStore.InvoiceFooterTextColumn2) ?
                new List<string>()
                : pdfSettingsByStore.InvoiceFooterTextColumn2.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToList();

            var iconFilePath = _fileProvider.MapPath("/Plugins/Misc.InvoicePDF/Content/Images/");

            var returnLogoFilePath = _fileProvider.Combine(iconFilePath, "delivery-status.png");
            var returnLogo = await _fileProvider.ReadAllBytesAsync(returnLogoFilePath);

            var orderLogoFilePath = _fileProvider.Combine(iconFilePath, "billing.png");
            var orderLogo = await _fileProvider.ReadAllBytesAsync(orderLogoFilePath);

            var orderDateLogoFilePath = _fileProvider.Combine(iconFilePath, "calendar.png");
            var orderDateLogo = await _fileProvider.ReadAllBytesAsync(orderDateLogoFilePath);

            var dueLogoFilePath = _fileProvider.Combine(iconFilePath, "due-stamp.png");
            var dueLogo = await _fileProvider.ReadAllBytesAsync(dueLogoFilePath);

            var paidLogoFilePath = _fileProvider.Combine(iconFilePath, "paid-stamp.png");
            var paidLogo = await _fileProvider.ReadAllBytesAsync(paidLogoFilePath);

            var phoneLogoFilePath = _fileProvider.Combine(iconFilePath, "phone-call.png");
            var phoneLogo = await _fileProvider.ReadAllBytesAsync(phoneLogoFilePath);

            var mailLogoFilePath = _fileProvider.Combine(iconFilePath, "mail.png");
            var mailLogo = await _fileProvider.ReadAllBytesAsync(mailLogoFilePath);

            var foldLogoFilePath = _fileProvider.Combine(iconFilePath, "fold.png");
            var foldLogo = await _fileProvider.ReadAllBytesAsync(foldLogoFilePath);

            var source = new CustomInvoiceDocument
            {
                StoreUrl = orderStore.Url?.Trim('/'),
                Language = language,
                Font = ResolvePdfFont(language, pdfSettingsByStore),
                OrderDateUser = date.ToString("D", new CultureInfo(language.LanguageCulture)),
                ImageTargetSize = pdfSettingsByStore.ImageTargetSize,
                LogoData = logo,
                ReturnLogoData = returnLogo,
                OrderLogoData = orderLogo,
                OrderDateLogoData = orderDateLogo,
                DueLogoData = dueLogo,
                PaidLogoData = paidLogo,
                PhoneLogoData = phoneLogo,
                MailLogoData = mailLogo,
                FoldLogoData = foldLogo,
                ReturnAddress = _invoicePDFSettings.ReturnAddress,
                OrderNumberText = order.CustomOrderNumber,
                PaymentStatus = order.PaymentStatusId,
                PageSize = pdfSettingsByStore.LetterPageSizeEnabled ? PdfPageSize.Letter : PdfPageSize.A4,
                BillingAddress = await GetBillingAddressAsync(vendor, language, order),
                ShippingAddress = await GetShippingAddressAsync(language, order),
                TrackingNumber = await GetShipmentTrackingDetailAsync(order.Id, vendor?.Id ?? 0),
                Products = await GetOrderProductItemsAsync(order, orderItems, language),
                ShowSkuInProductList = _catalogSettings.ShowSkuOnProductDetailsPage,
                ShowVendorInProductList = _vendorSettings.ShowVendorOnOrderDetailsPage,
                CheckoutAttributes = vendor is null ? order.CheckoutAttributeDescription : string.Empty,
                Totals = vendor is null ? await GetTotalsAsync(language, order) : new(),
                OrderNotes = await GetOrderNotesAsync(pdfSettingsByStore, order, language),
                FooterTextColumn1 = column1Lines,
                FooterTextColumn2 = column2Lines,
                GetResourceAsync = async (string resourceKey, int languageId) => await _localizationService.GetResourceAsync(resourceKey, languageId)
            };

            await using var pdfStream = new MemoryStream();
            source.Generate(pdfStream);

            pdfStream.Position = 0;
            await pdfStream.CopyToAsync(stream);
        }

        /// <summary>
        /// Write ZIP archive with invoices to the specified stream
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="orders">Orders</param>
        /// <param name="language">Language; null to use a language used when placing an order</param>
        /// <param name="vendor">Vendor to limit products; null to print all products. If specified, then totals won't be printed</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        //public override async Task PrintOrdersToPdfAsync(Stream stream, IList<Order> orders, Language language = null, Vendor vendor = null)
        //{
        //    ArgumentNullException.ThrowIfNull(stream);

        //    ArgumentNullException.ThrowIfNull(orders);

        //    var currentStore = await _storeContext.GetCurrentStoreAsync();

        //    using var archive = new ZipArchive(stream, ZipArchiveMode.Create, true);

        //    foreach (var order in orders)
        //    {
        //        var entryName = string.Format(await _localizationService.GetResourceAsync("Pdf.Order"), order.CustomOrderNumber);

        //        await using var fileStreamInZip = archive.CreateEntry($"{entryName}.pdf").Open();
        //        await using var pdfStream = new MemoryStream();
        //        await PrintOrderToPdfAsync(pdfStream, order, language, currentStore, vendor);
        //        pdfStream.Position = 0;
        //        await pdfStream.CopyToAsync(fileStreamInZip);
        //    }
        //}

        public override async Task PrintOrdersToPdfAsync(Stream stream, IList<Order> orders, Language language = null, Vendor vendor = null)
        {
            ArgumentNullException.ThrowIfNull(stream);
            ArgumentNullException.ThrowIfNull(orders);

            var currentStore = await _storeContext.GetCurrentStoreAsync();

            //using var finalPdf = new iTextSharp.text.Document();
            //using var writer = PdfWriter.GetInstance(finalPdf, stream);

            //finalPdf.Open();

            using var finalPdf = new iTextSharp.text.Document();
            using var writer = PdfWriter.GetInstance(finalPdf, stream);

            bool isFirstPage = true;


            foreach (var order in orders)
            {
                using var pdfStream = new MemoryStream();

                await PrintOrderToPdfAsync(
                    pdfStream,
                    order,
                    language,
                    currentStore,
                    vendor
                );

                pdfStream.Position = 0;

                var reader = new PdfReader(pdfStream.ToArray());

                for (int i = 1; i <= reader.NumberOfPages; i++)
                {
                    var pageSize = reader.GetPageSizeWithRotation(i);

                    finalPdf.SetPageSize(pageSize);

                    if (isFirstPage)
                    {
                        finalPdf.Open();
                        isFirstPage = false;
                    }
                    else
                    {
                        finalPdf.NewPage();
                    }

                    var page = writer.GetImportedPage(reader, i);
                    writer.DirectContent.AddTemplate(page, 0, 0);
                }

                reader.Close();
            }

            finalPdf.Close();
        }

        /// <summary>
        /// Write packaging slip to the specified stream
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="shipment">Shipment</param>
        /// <param name="language">Language; null to use a language used when placing an order</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task PrintPackagingSlipToPdfAsync(Stream stream, Shipment shipment, Language language = null)
        {
            ArgumentNullException.ThrowIfNull(stream);

            ArgumentNullException.ThrowIfNull(shipment);

            var order = await _orderService.GetOrderByIdAsync(shipment.OrderId);
            var pdfSettingsByStore = await _settingService.LoadSettingAsync<PdfSettings>(order.StoreId);

            //language info
            language ??= await _languageService.GetLanguageByIdAsync(order.CustomerLanguageId);
            if (language?.Published != true)
                language = await _workContext.GetWorkingLanguageAsync();

            var shipmentItems = await _shipmentService.GetShipmentItemsByShipmentIdAsync(shipment.Id);
            if (shipmentItems?.Any() != true)
                return;

            var orderItems = await shipmentItems.SelectAwait(async si => await _orderService.GetOrderItemByIdAsync(si.OrderItemId)).Where(pi => pi != null).ToListAsync();
            if (orderItems?.Any() != true)
                return;

            await using var pdfStream = new MemoryStream();
            var document = new ShipmentDocument
            {
                PageSize = pdfSettingsByStore.LetterPageSizeEnabled ? PdfPageSize.Letter : PdfPageSize.A4,
                Language = language,
                Font = ResolvePdfFont(language, pdfSettingsByStore),
                ImageTargetSize = pdfSettingsByStore.ImageTargetSize,
                ShipmentNumberText = shipment.Id.ToString(),
                OrderNumberText = order.CustomOrderNumber,
                Address = await GetShippingAddressAsync(language, order),
                Products = await GetOrderProductItemsAsync(order, orderItems, language, shipmentItems),
                GetResourceAsync = async (string resourceKey, int languageId) => await _localizationService.GetResourceAsync(resourceKey, languageId)
            };

            document.Generate(pdfStream);

            pdfStream.Position = 0;
            await pdfStream.CopyToAsync(stream);
        }

        /// <summary>
        /// Export an order to PDF and save to disk
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="language">Language identifier; null to use a language used when placing an order</param>
        /// <param name="vendor">Vendor to limit products; null to print all products. If specified, then totals won't be printed</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains a path of generated file
        /// </returns>
        public override async Task<string> SaveOrderPdfToDiskAsync(Order order, Language language = null, Vendor vendor = null)
        {
            var fileName = $"order_{order.OrderGuid}_{CommonHelper.GenerateRandomDigitCode(4)}.pdf";
            var filePath = _fileProvider.Combine(_fileProvider.MapPath("~/wwwroot/files/exportimport"), fileName);
            await using var fileStream = new FileStream(filePath, FileMode.Create);

            await PrintOrderToPdfAsync(fileStream, order, language, store: null, vendor: vendor);

            return filePath;
        }

        #endregion
    }
}
