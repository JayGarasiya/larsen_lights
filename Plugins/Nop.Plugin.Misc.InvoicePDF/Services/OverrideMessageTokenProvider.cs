using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Core;
using Nop.Core.Domain;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Vendors;
using Nop.Core.Events;
using Nop.Services.Attributes;
using Nop.Services.Blogs;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Html;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.News;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using System.Net;
using System.Text;
using Nop.Core.Http;

namespace Nop.Plugin.Misc.InvoicePDF.Services
{
    /// <summary>
    /// PDF service
    /// </summary>
    public class OverrideMessageTokenProvider : MessageTokenProvider
    {
        #region Fields
        private readonly InvoicePDFSettings _invoicePDFSettings;
        private readonly IProductAttributeFormatter _productAttributeFormatter;
        #endregion

        #region Ctor
        public OverrideMessageTokenProvider(CatalogSettings catalogSettings,
            CurrencySettings currencySettings,
            IActionContextAccessor actionContextAccessor,
            IAddressService addressService,
            IAttributeFormatter<AddressAttribute, AddressAttributeValue> addressAttributeFormatter,
            IAttributeFormatter<CustomerAttribute, CustomerAttributeValue> customerAttributeFormatter,
            IAttributeFormatter<VendorAttribute, VendorAttributeValue> vendorAttributeFormatter,
            IBlogService blogService,
            ICountryService countryService,
            ICurrencyService currencyService,
            ICustomerService customerService,
            IDateTimeHelper dateTimeHelper,
            IEventPublisher eventPublisher,
            IGenericAttributeService genericAttributeService,
            IGiftCardService giftCardService,
            IHtmlFormatter htmlFormatter,
            ILanguageService languageService,
            ILocalizationService localizationService,
            ILogger logger,
            INewsService newsService,
            IOrderService orderService,
            IPaymentPluginManager paymentPluginManager,
            IPaymentService paymentService,
            IPriceFormatter priceFormatter,
            IProductService productService,
            IRewardPointService rewardPointService,
            IShipmentService shipmentService,
            IStateProvinceService stateProvinceService,
            IStoreContext storeContext,
            IStoreService storeService,
            IUrlHelperFactory urlHelperFactory,
            IUrlRecordService urlRecordService,
            IWorkContext workContext,
            MessageTemplatesSettings templatesSettings,
            PaymentSettings paymentSettings,
            StoreInformationSettings storeInformationSettings,
            TaxSettings taxSettings,
            InvoicePDFSettings invoicePDFSettings,
            IProductAttributeFormatter productAttributeFormatter) : base(catalogSettings,
                currencySettings,
                actionContextAccessor,
                addressService,
                addressAttributeFormatter,
                customerAttributeFormatter,
                vendorAttributeFormatter,
                blogService,
                countryService,
                currencyService,
                customerService,
                dateTimeHelper,
                eventPublisher,
                genericAttributeService,
                giftCardService,
                htmlFormatter,
                languageService,
                localizationService,
                logger,
                newsService,
                orderService,
                paymentPluginManager,
                paymentService,
                priceFormatter,
                productService,
                rewardPointService,
                shipmentService,
                stateProvinceService,
                storeContext,
                storeService,
                urlHelperFactory,
                urlRecordService,
                workContext,
                templatesSettings,
                paymentSettings,
                storeInformationSettings,
                taxSettings)
        {
            _invoicePDFSettings = invoicePDFSettings;
            _productAttributeFormatter = productAttributeFormatter;
        }
        #endregion

        #region Utilities
        /// <summary>
        /// Convert a collection to a HTML table
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="languageId">Language identifier</param>
        /// <param name="vendorId">Vendor identifier (used to limit products by vendor</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the hTML table of products
        /// </returns>
        protected override async Task<string> ProductListToHtmlTableAsync(Order order, int languageId, int vendorId)
        {
            var language = await _languageService.GetLanguageByIdAsync(languageId);

            var sb = new StringBuilder();
            sb.AppendLine("<table border=\"0\" style=\"width:100%;\">");

            sb.AppendLine($"<tr style=\"background-color:{_templatesSettings.Color1};text-align:center;\">");
            sb.AppendLine($"<th>{await _localizationService.GetResourceAsync("Messages.Order.Product(s).Name", languageId)}</th>");
            sb.AppendLine($"<th>{await _localizationService.GetResourceAsync("Messages.Order.Product(s).Price", languageId)}</th>");
            sb.AppendLine($"<th>{await _localizationService.GetResourceAsync("Messages.Order.Product(s).Quantity", languageId)}</th>");
            sb.AppendLine($"<th>{await _localizationService.GetResourceAsync("Messages.Order.Product(s).Total", languageId)}</th>");
            sb.AppendLine("</tr>");

            var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);
            var store = await _storeContext.GetCurrentStoreAsync();

            var table = await _orderService.GetOrderItemsAsync(order.Id, vendorId: vendorId);
            for (var i = 0; i <= table.Count - 1; i++)
            {
                var orderItem = table[i];

                var product = await _productService.GetProductByIdAsync(orderItem.ProductId);

                if (product == null)
                    continue;

                sb.AppendLine($"<tr style=\"background-color: {_templatesSettings.Color2};text-align: center;\">");
                //product name
                var productName = await _localizationService.GetLocalizedAsync(product, x => x.Name, languageId);

                sb.AppendLine("<td style=\"padding: 0.6em 0.4em;text-align: left;\">" + WebUtility.HtmlEncode(productName));

                //add download link
                if (await _orderService.IsDownloadAllowedAsync(orderItem))
                {
                    var downloadUrl = await RouteUrlAsync(order.StoreId, NopRouteNames.Standard.GET_DOWNLOAD, new { orderItemId = orderItem.OrderItemGuid });
                    var downloadLink = $"<a class=\"link\" href=\"{downloadUrl}\">{await _localizationService.GetResourceAsync("Messages.Order.Product(s).Download", languageId)}</a>";
                    sb.AppendLine("<br />");
                    sb.AppendLine(downloadLink);
                }
                //add download link
                if (await _orderService.IsLicenseDownloadAllowedAsync(orderItem))
                {
                    var licenseUrl = await RouteUrlAsync(order.StoreId, NopRouteNames.Standard.GET_LICENSE, new { orderItemId = orderItem.OrderItemGuid });
                    var licenseLink = $"<a class=\"link\" href=\"{licenseUrl}\">{await _localizationService.GetResourceAsync("Messages.Order.Product(s).License", languageId)}</a>";
                    sb.AppendLine("<br />");
                    sb.AppendLine(licenseLink);
                }
                //attributes
                if (!string.IsNullOrEmpty(orderItem.AttributeDescription))
                {
                    sb.AppendLine("<br />");
                    if (!_invoicePDFSettings.RenderAttributeValuePrices)
                    {
                        var attributeDescription = await _productAttributeFormatter.FormatAttributesAsync(product, orderItem.AttributesXml, customer, store,
                            renderPrices: _invoicePDFSettings.RenderAttributeValuePrices);
                        sb.AppendLine(attributeDescription);
                    }
                    else
                        sb.AppendLine(orderItem.AttributeDescription);
                }
                //rental info
                if (product.IsRental)
                {
                    var rentalStartDate = orderItem.RentalStartDateUtc.HasValue
                        ? _productService.FormatRentalDate(product, orderItem.RentalStartDateUtc.Value) : string.Empty;
                    var rentalEndDate = orderItem.RentalEndDateUtc.HasValue
                        ? _productService.FormatRentalDate(product, orderItem.RentalEndDateUtc.Value) : string.Empty;
                    var rentalInfo = string.Format(await _localizationService.GetResourceAsync("Order.Rental.FormattedDate", languageId),
                        rentalStartDate, rentalEndDate);
                    sb.AppendLine("<br />");
                    sb.AppendLine(rentalInfo);
                }
                //SKU
                if (_catalogSettings.ShowSkuOnProductDetailsPage)
                {
                    var sku = await _productService.FormatSkuAsync(product, orderItem.AttributesXml);
                    if (!string.IsNullOrEmpty(sku))
                    {
                        sb.AppendLine("<br />");
                        sb.AppendLine(string.Format(await _localizationService.GetResourceAsync("Messages.Order.Product(s).SKU", languageId), WebUtility.HtmlEncode(sku)));
                    }
                }

                sb.AppendLine("</td>");

                string unitPriceStr;
                if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
                {
                    //including tax
                    var unitPriceInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.UnitPriceInclTax, order.CurrencyRate);
                    unitPriceStr = await _priceFormatter.FormatPriceAsync(unitPriceInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, true);
                }
                else
                {
                    //excluding tax
                    var unitPriceExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.UnitPriceExclTax, order.CurrencyRate);
                    unitPriceStr = await _priceFormatter.FormatPriceAsync(unitPriceExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, false);
                }

                var unitProductCostInCustomerCurrency = decimal.Zero;
                if (_invoicePDFSettings.VendorCost && vendorId > 0)
                {
                    unitProductCostInCustomerCurrency = _currencyService.ConvertCurrency(product.ProductCost, order.CurrencyRate);
                    unitPriceStr = await _priceFormatter.FormatPriceAsync(unitProductCostInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, false);
                }

                sb.AppendLine($"<td style=\"padding: 0.6em 0.4em;text-align: right;\">{unitPriceStr}</td>");

                sb.AppendLine($"<td style=\"padding: 0.6em 0.4em;text-align: center;\">{orderItem.Quantity}</td>");

                string priceStr;
                if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
                {
                    //including tax
                    var priceInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.PriceInclTax, order.CurrencyRate);
                    priceStr = await _priceFormatter.FormatPriceAsync(priceInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, true);
                }
                else
                {
                    //excluding tax
                    var priceExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.PriceExclTax, order.CurrencyRate);
                    priceStr = await _priceFormatter.FormatPriceAsync(priceExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, false);
                }

                if (_invoicePDFSettings.VendorCost && vendorId > 0)
                {
                    var productCost = unitProductCostInCustomerCurrency * orderItem.Quantity;
                    var productCostInCustomerCurrency = _currencyService.ConvertCurrency(productCost, order.CurrencyRate);
                    priceStr = await _priceFormatter.FormatPriceAsync(productCostInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, false);
                }

                sb.AppendLine($"<td style=\"padding: 0.6em 0.4em;text-align: right;\">{priceStr}</td>");

                sb.AppendLine("</tr>");
            }

            if (vendorId == 0)
            {
                //we render checkout attributes and totals only for store owners (hide for vendors)

                if (!string.IsNullOrEmpty(order.CheckoutAttributeDescription))
                {
                    sb.AppendLine("<tr><td style=\"text-align:right;\" colspan=\"1\">&nbsp;</td><td colspan=\"3\" style=\"text-align:right\">");
                    sb.AppendLine(order.CheckoutAttributeDescription);
                    sb.AppendLine("</td></tr>");
                }

                //totals
                await WriteTotalsAsync(order, language, sb);
            }

            sb.AppendLine("</table>");
            var result = sb.ToString();
            return result;
        }
        #endregion
    }
}
