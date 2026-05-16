using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Primitives;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Logging;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Vendors;
using Nop.Core.Events;
using Nop.Core.Infrastructure;
using Nop.Plugin.Tax.TaxJar.Domain;
using Nop.Services.Attributes;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Common.Pdf;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Helpers;
using Nop.Services.Html;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Stores;
using Nop.Services.Vendors;
using PdfRpt.Core.Contracts;
using System.Globalization;
using System.Net;
using System.Text;

namespace Nop.Plugin.Tax.TaxJar.Services;

/// <summary>
/// Taxjar service
/// </summary>
public partial class TaxJarService : ITaxJarService
{
    #region Fields
    private readonly IAttributeService<CustomerAttribute, CustomerAttributeValue> _customerAttributeService;
    private readonly ICustomerService _customerService;
    private readonly IStateProvinceService _stateProvinceService;
    private readonly IAttributeParser<CustomerAttribute, CustomerAttributeValue> _customerAttributeParser;
    private readonly INopFileProvider _fileProvider;
    private readonly IOrderService _orderService;
    private readonly IProductAttributeParser _productAttributeParser;
    private readonly IProductService _productService;
    private readonly ICurrencyService _currencyService;
    private readonly CurrencySettings _currencySettings;
    private readonly ShoppingCartSettings _shoppingCartSettings;
    private readonly IPriceCalculationService _priceCalculationService;
    private readonly IDiscountService _discountService;
    private readonly TaxSettings _taxSettings;
    private readonly IOrderProcessingService _orderProcessingService;
    private readonly IPaymentService _paymentService;
    private readonly ILogger _logger;
    private readonly IEventPublisher _eventPublisher;
    private readonly LocalizationSettings _localizationSettings;
    private readonly IWorkflowMessageService _workflowMessageService;
    private readonly TaxJarSettings _taxJarSettings;
    private readonly IMessageTemplateService _messageTemplateService;
    private readonly ILanguageService _languageService;
    private readonly IStoreService _storeService;
    private readonly IStoreContext _storeContext;
    private readonly ILocalizationService _localizationService;
    private readonly IEmailAccountService _emailAccountService;
    private readonly EmailAccountSettings _emailAccountSettings;
    private readonly IMessageTokenProvider _messageTokenProvider;
    private readonly IAddressService _addressService;
    private readonly ISettingService _settingService;
    private readonly IWorkContext _workContext;
    private readonly IDateTimeHelper _dateTimeHelper;
    private readonly IPictureService _pictureService;
    private readonly AddressSettings _addressSettings;
    private readonly IAttributeFormatter<AddressAttribute, AddressAttributeValue> _addressAttributeFormatter;
    private readonly ICountryService _countryService;
    private readonly IPaymentPluginManager _paymentPluginManager;
    private readonly IPriceFormatter _priceFormatter;
    private readonly IGiftCardService _giftCardService;
    private readonly IRewardPointService _rewardPointService;
    private readonly CatalogSettings _catalogSettings;
    private readonly VendorSettings _vendorSettings;
    private readonly IVendorService _vendorService;
    private readonly IProductAttributeFormatter _productAttributeFormatter;
    private readonly IActionContextAccessor _actionContextAccessor;
    private readonly IUrlHelperFactory _urlHelperFactory;
    private readonly MessageTemplatesSettings _templatesSettings;
    private readonly IPdfService _pdfService;
    private readonly IHtmlFormatter _htmlFormatter;
    #endregion

    #region Ctor
    public TaxJarService(
        IAttributeService<CustomerAttribute, CustomerAttributeValue> customerAttributeService,
        ICustomerService customerService,
        IStateProvinceService stateProvinceService,
        IAttributeParser<CustomerAttribute, CustomerAttributeValue> customerAttributeParser,
        INopFileProvider fileProvider,
        IOrderService orderService,
        IProductAttributeParser productAttributeParser,
        IProductService productService,
        ICurrencyService currencyService,
        CurrencySettings currencySettings,
        ShoppingCartSettings shoppingCartSettings,
        IPriceCalculationService priceCalculationService,
        IDiscountService discountService,
        TaxSettings taxSettings,
        IOrderProcessingService orderProcessingService,
        IPaymentService paymentService,
        ILogger logger,
        IEventPublisher eventPublisher,
        LocalizationSettings localizationSettings,
        IWorkflowMessageService workflowMessageService,
        TaxJarSettings taxJarSettings,
        IMessageTemplateService messageTemplateService,
        ILanguageService languageService,
        IStoreService storeService,
        IStoreContext storeContext,
        ILocalizationService localizationService,
        IEmailAccountService emailAccountService,
        EmailAccountSettings emailAccountSettings,
        IMessageTokenProvider messageTokenProvider,
        IAddressService addressService,
        ISettingService settingService,
        IWorkContext workContext,
        IDateTimeHelper dateTimeHelper,
        IPictureService pictureService,
        AddressSettings addressSettings,
        IAttributeFormatter<AddressAttribute, AddressAttributeValue> addressAttributeFormatter,
        ICountryService countryService,
        IPaymentPluginManager paymentPluginManager,
        IPriceFormatter priceFormatter,
        IGiftCardService giftCardService,
        IRewardPointService rewardPointService,
        CatalogSettings catalogSettings,
        VendorSettings vendorSettings,
        IVendorService vendorService,
        IProductAttributeFormatter productAttributeFormatter,
        IActionContextAccessor actionContextAccessor,
        IUrlHelperFactory urlHelperFactory,
        MessageTemplatesSettings templatesSettings,
        IPdfService pdfService,
        IHtmlFormatter htmlFormatter)
    {
        _customerAttributeService = customerAttributeService;
        _customerService = customerService;
        _stateProvinceService = stateProvinceService;
        _customerAttributeParser = customerAttributeParser;
        _fileProvider = fileProvider;
        _orderService = orderService;
        _productAttributeParser = productAttributeParser;
        _productService = productService;
        _currencyService = currencyService;
        _currencySettings = currencySettings;
        _shoppingCartSettings = shoppingCartSettings;
        _priceCalculationService = priceCalculationService;
        _discountService = discountService;
        _taxSettings = taxSettings;
        _orderProcessingService = orderProcessingService;
        _paymentService = paymentService;
        _logger = logger;
        _eventPublisher = eventPublisher;
        _localizationSettings = localizationSettings;
        _workflowMessageService = workflowMessageService;
        _taxJarSettings = taxJarSettings;
        _messageTemplateService = messageTemplateService;
        _languageService = languageService;
        _storeService = storeService;
        _storeContext = storeContext;
        _localizationService = localizationService;
        _emailAccountService = emailAccountService;
        _emailAccountSettings = emailAccountSettings;
        _messageTokenProvider = messageTokenProvider;
        _addressService = addressService;
        _settingService = settingService;
        _workContext = workContext;
        _dateTimeHelper = dateTimeHelper;
        _pictureService = pictureService;
        _addressSettings = addressSettings;
        _addressAttributeFormatter = addressAttributeFormatter;
        _countryService = countryService;
        _paymentPluginManager = paymentPluginManager;
        _priceFormatter = priceFormatter;
        _giftCardService = giftCardService;
        _rewardPointService = rewardPointService;
        _catalogSettings = catalogSettings;
        _vendorSettings = vendorSettings;
        _vendorService = vendorService;
        _productAttributeFormatter = productAttributeFormatter;
        _actionContextAccessor = actionContextAccessor;
        _urlHelperFactory = urlHelperFactory;
        _templatesSettings = templatesSettings;
        _pdfService = pdfService;
        _htmlFormatter = htmlFormatter;
    }
    #endregion

    #region Utilities
    /// <summary>
    /// Add order note
    /// </summary>
    /// <param name="order">Order</param>
    /// <param name="note">Note text</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    protected virtual async Task AddOrderNoteAsync(Order order, string note)
    {
        await _orderService.InsertOrderNoteAsync(new OrderNote
        {
            OrderId = order.Id,
            Note = note,
            DisplayToCustomer = false,
            CreatedOnUtc = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Gets subtotal discount amount
    /// </summary>
    /// <param name="order">Order</param>
    /// <param name="subTotalWithoutDiscount">SubTotalWithoutDiscount</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the applied discount amount. Sub total (without discount). Sub total (with discount).
    /// </returns>
    protected virtual async Task<(decimal discountAmount, decimal subTotalWithoutDiscount, decimal subTotalWithDiscount, SortedDictionary<decimal, decimal> taxRates)> GetOrderSubTotalDiscountAsync(
        Order order,
        decimal subTotalExclTaxWithoutDiscount,
        decimal subTotalInclTaxWithoutDiscount,
        SortedDictionary<decimal, decimal> taxRates,
        bool includingTax)
    {
        var discountAmount = decimal.Zero;
        var subTotalWithoutDiscount = decimal.Zero;
        var subTotalWithDiscount = decimal.Zero;

        // Subtotal without discount
        subTotalWithoutDiscount = includingTax ? subTotalInclTaxWithoutDiscount : subTotalExclTaxWithoutDiscount;
        if (subTotalWithoutDiscount < decimal.Zero)
            subTotalWithoutDiscount = decimal.Zero;

        if (_shoppingCartSettings.RoundPricesDuringCalculation)
            subTotalWithoutDiscount = await _priceCalculationService.RoundPriceAsync(subTotalWithoutDiscount);

        // We calculate discount amount on order subtotal excl tax (discount first)
        // Calculate discount amount ('Applied to order subtotal' discount)
        decimal discountAmountExclTax;

        // Used discounts
        var allowedDiscounts = new List<Discount>();
        var duh = await _discountService.GetAllDiscountUsageHistoryAsync(orderId: order.Id);
        foreach (var d in duh)
        {
            var discount = await _discountService.GetDiscountByIdAsync(d.DiscountId);
            if (discount.DiscountType == DiscountType.AssignedToOrderSubTotal)
                allowedDiscounts.Add(discount);
        }

        _discountService.GetPreferredDiscount(allowedDiscounts, subTotalExclTaxWithoutDiscount, out discountAmountExclTax);
        if (discountAmountExclTax < decimal.Zero)
            discountAmountExclTax = decimal.Zero;

        if (subTotalExclTaxWithoutDiscount < discountAmountExclTax)
            discountAmountExclTax = subTotalExclTaxWithoutDiscount;
        var discountAmountInclTax = discountAmountExclTax;
        // Subtotal with discount (excl tax)
        var subTotalExclTaxWithDiscount = subTotalExclTaxWithoutDiscount - discountAmountExclTax;
        var subTotalInclTaxWithDiscount = subTotalExclTaxWithDiscount;

        // Add tax for shopping items & checkout attributes
        var tempTaxRates = new Dictionary<decimal, decimal>(taxRates);
        foreach (var kvp in tempTaxRates)
        {
            var taxRate = kvp.Key;
            var taxValue = kvp.Value;

            if (taxValue == decimal.Zero)
                continue;

            // Discount the tax amount that applies to subtotal items
            if (subTotalExclTaxWithoutDiscount > decimal.Zero)
            {
                var discountTax = taxRates[taxRate] * (discountAmountExclTax / subTotalExclTaxWithoutDiscount);
                discountAmountInclTax += discountTax;
                taxValue = taxRates[taxRate] - discountTax;
                if (_shoppingCartSettings.RoundPricesDuringCalculation)
                    taxValue = await _priceCalculationService.RoundPriceAsync(taxValue);
                taxRates[taxRate] = taxValue;
            }

            // Subtotal with discount (incl tax)
            subTotalInclTaxWithDiscount += taxValue;
        }

        if (_shoppingCartSettings.RoundPricesDuringCalculation)
        {
            discountAmountInclTax = await _priceCalculationService.RoundPriceAsync(discountAmountInclTax);
            discountAmountExclTax = await _priceCalculationService.RoundPriceAsync(discountAmountExclTax);
        }

        if (includingTax)
        {
            subTotalWithDiscount = subTotalInclTaxWithDiscount;
            discountAmount = discountAmountInclTax;
        }
        else
        {
            subTotalWithDiscount = subTotalExclTaxWithDiscount;
            discountAmount = discountAmountExclTax;
        }

        if (subTotalWithDiscount < decimal.Zero)
            subTotalWithDiscount = decimal.Zero;

        if (_shoppingCartSettings.RoundPricesDuringCalculation)
            subTotalWithDiscount = await _priceCalculationService.RoundPriceAsync(subTotalWithDiscount);

        return (discountAmount, subTotalWithoutDiscount, subTotalWithDiscount, taxRates);
    }

    /// <summary>
    /// Get active message templates by the name
    /// </summary>
    /// <param name="messageTemplateName">Message template name</param>
    /// <param name="storeId">Store identifier</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the list of message templates
    /// </returns>
    protected virtual async Task<IList<MessageTemplate>> GetActiveMessageTemplatesAsync(string messageTemplateName, int storeId)
    {
        // Get message templates by the name
        var messageTemplates = await _messageTemplateService.GetMessageTemplatesByNameAsync(messageTemplateName, storeId);

        // No template found
        if (!messageTemplates?.Any() ?? true)
            return new List<MessageTemplate>();

        // Filter active templates
        messageTemplates = messageTemplates.Where(messageTemplate => messageTemplate.IsActive).ToList();

        return messageTemplates;
    }

    /// <summary>
    /// Ensure language is active
    /// </summary>
    /// <param name="languageId">Language identifier</param>
    /// <param name="storeId">Store identifier</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the return a value language identifier
    /// </returns>
    protected virtual async Task<int> EnsureLanguageIsActiveAsync(int languageId, int storeId)
    {
        // Load language by specified ID
        var language = await _languageService.GetLanguageByIdAsync(languageId);

        if (language == null || !language.Published)
        {
            // Load any language from the specified store
            language = (await _languageService.GetAllLanguagesAsync(storeId: storeId)).FirstOrDefault();
        }

        if (language == null || !language.Published)
        {
            // Load any language
            language = (await _languageService.GetAllLanguagesAsync()).FirstOrDefault();
        }

        if (language == null)
            throw new Exception("No active language could be loaded");

        return language.Id;
    }

    /// <summary>
    /// Get EmailAccount to use with a message templates
    /// </summary>
    /// <param name="messageTemplate">Message template</param>
    /// <param name="languageId">Language identifier</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the emailAccount
    /// </returns>
    protected virtual async Task<EmailAccount> GetEmailAccountOfMessageTemplateAsync(MessageTemplate messageTemplate, int languageId)
    {
        var emailAccountId = await _localizationService.GetLocalizedAsync(messageTemplate, mt => mt.EmailAccountId, languageId);
        // Some 0 validation (for localizable "Email account" dropdownlist which saves 0 if "Standard" value is chosen)
        if (emailAccountId == 0)
            emailAccountId = messageTemplate.EmailAccountId;

        var emailAccount = (await _emailAccountService.GetEmailAccountByIdAsync(emailAccountId)
                            ?? await _emailAccountService.GetEmailAccountByIdAsync(_emailAccountSettings.DefaultEmailAccountId)) 
                            ?? (await _emailAccountService.GetAllEmailAccountsAsync()).FirstOrDefault();
        return emailAccount;
    }

    /// <summary>
    /// Convert a collection to a HTML table
    /// </summary>
    /// <param name="order">Order</param>
    /// <param name="refundOrder">Refund Order</param>
    /// <param name="refundOrderItems">Refund Order Item</param>
    /// <param name="languageId">Language identifier</param>
    /// <param name="vendorId">Vendor identifier (used to limit products by vendor</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the hTML table of products
    /// </returns>
    protected virtual async Task<string> ProductListToHtmlTableAsync(
        Order order,
        RefundOrder refundOrder,
        List<RefundOrderItem> refundOrderItems,
        int languageId,
        int vendorId)
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

        var orderItems = await _orderService.GetOrderItemsAsync(order.Id, vendorId: vendorId);
        var table = refundOrderItems;
        for (var i = 0; i <= table.Count - 1; i++)
        {
            var orderItem = table[i];

            var product = await _productService.GetProductByIdAsync(orderItem.AssociatedProductId > 0 ? orderItem.AssociatedProductId : orderItem.ProductId);
            var realOrderItem = orderItems.Where(o => o.Id == orderItem.Id).FirstOrDefault();

            if (product == null)
                continue;

            sb.AppendLine($"<tr style=\"background-color: {_templatesSettings.Color2};text-align: center;\">");
            // Product name
            var productName = await _localizationService.GetLocalizedAsync(product, x => x.Name, languageId);

            // Add a note
            await AddOrderNoteAsync(order, $"Order item {productName} has been partially refunded. Amount = {refundOrder.OrderTotal}");

            sb.AppendLine("<td style=\"padding: 0.6em 0.4em;text-align: left;\">" + WebUtility.HtmlEncode(productName));

            // Add download link
            if (await _orderService.IsDownloadAllowedAsync(realOrderItem))
            {
                var downloadUrl = await RouteUrlAsync(order.StoreId, "GetDownload", new { orderItemId = realOrderItem.OrderItemGuid });
                var downloadLink = $"<a class=\"link\" href=\"{downloadUrl}\">{await _localizationService.GetResourceAsync("Messages.Order.Product(s).Download", languageId)}</a>";
                sb.AppendLine("<br />");
                sb.AppendLine(downloadLink);
            }
            // Add download link
            if (await _orderService.IsLicenseDownloadAllowedAsync(realOrderItem))
            {
                var licenseUrl = await RouteUrlAsync(order.StoreId, "GetLicense", new { orderItemId = realOrderItem.OrderItemGuid });
                var licenseLink = $"<a class=\"link\" href=\"{licenseUrl}\">{await _localizationService.GetResourceAsync("Messages.Order.Product(s).License", languageId)}</a>";
                sb.AppendLine("<br />");
                sb.AppendLine(licenseLink);
            }
            // Attributes
            if (!string.IsNullOrEmpty(realOrderItem.AttributeDescription) && orderItem.AssociatedProductId == 0)
            {
                sb.AppendLine("<br />");
                sb.AppendLine(realOrderItem.AttributeDescription);
            }
            // Rental info
            if (product.IsRental)
            {
                var rentalStartDate = realOrderItem.RentalStartDateUtc.HasValue
                    ? _productService.FormatRentalDate(product, realOrderItem.RentalStartDateUtc.Value) : string.Empty;
                var rentalEndDate = realOrderItem.RentalEndDateUtc.HasValue
                    ? _productService.FormatRentalDate(product, realOrderItem.RentalEndDateUtc.Value) : string.Empty;
                var rentalInfo = string.Format(await _localizationService.GetResourceAsync("Order.Rental.FormattedDate"),
                    rentalStartDate, rentalEndDate);
                sb.AppendLine("<br />");
                sb.AppendLine(rentalInfo);
            }
            // SKU
            if (_catalogSettings.ShowSkuOnProductDetailsPage)
            {
                var sku = await _productService.FormatSkuAsync(product, realOrderItem.AttributesXml);
                if (!string.IsNullOrEmpty(sku))
                {
                    sb.AppendLine("<br />");
                    sb.AppendLine(string.Format(await _localizationService.GetResourceAsync("Messages.Order.Product(s).SKU",
                                                                                            languageId), WebUtility.HtmlEncode(sku)));
                }
            }

            sb.AppendLine("</td>");

            string unitPriceStr;
            if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
            {
                // Including tax
                var unitPriceInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.UnitPriceInclTax, order.CurrencyRate);
                unitPriceStr = await _priceFormatter.FormatPriceAsync(unitPriceInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, true);
            }
            else
            {
                // Excluding tax
                var unitPriceExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.UnitPriceExclTax, order.CurrencyRate);
                unitPriceStr = await _priceFormatter.FormatPriceAsync(unitPriceExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, false);
            }

            sb.AppendLine($"<td style=\"padding: 0.6em 0.4em;text-align: right;\">{unitPriceStr}</td>");

            sb.AppendLine($"<td style=\"padding: 0.6em 0.4em;text-align: center;\">{orderItem.Quantity}</td>");

            string priceStr;
            if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
            {
                // Including tax
                var priceInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.PriceInclTax, order.CurrencyRate);
                priceStr = await _priceFormatter.FormatPriceAsync(priceInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, true);
            }
            else
            {
                // Excluding tax
                var priceExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.PriceExclTax, order.CurrencyRate);
                priceStr = await _priceFormatter.FormatPriceAsync(priceExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, false);
            }

            sb.AppendLine($"<td style=\"padding: 0.6em 0.4em;text-align: right;\">{priceStr}</td>");

            sb.AppendLine("</tr>");
        }

        if (vendorId == 0)
        {
            // We render checkout attributes and totals only for store owners (hide for vendors)

            if (!string.IsNullOrEmpty(order.CheckoutAttributeDescription))
            {
                sb.AppendLine("<tr><td style=\"text-align:right;\" colspan=\"1\">&nbsp;</td><td colspan=\"3\" style=\"text-align:right\">");
                sb.AppendLine(order.CheckoutAttributeDescription);
                sb.AppendLine("</td></tr>");
            }

            // Totals
            await WriteTotalsAsync(order, refundOrder, language, sb);
        }

        sb.AppendLine("</table>");
        var result = sb.ToString();
        return result;
    }

    /// <summary>
    /// Write order totals
    /// </summary>
    /// <param name="order">Order</param>
    /// <param name="refundOrder">Refund Order</param>
    /// <param name="language">Language</param>
    /// <param name="sb">StringBuilder</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    protected virtual async Task WriteTotalsAsync(Order order, RefundOrder refundOrder, Language language, StringBuilder sb)
    {
        // Subtotal
        string cusSubTotal;
        var displaySubTotalDiscount = false;
        var cusSubTotalDiscount = string.Empty;
        var languageId = language.Id;
        if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax && !_taxSettings.ForceTaxExclusionFromOrderSubtotal)
        {
            // Including tax

            // Subtotal
            var orderSubtotalInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(refundOrder.OrderSubtotalInclTax, order.CurrencyRate);
            cusSubTotal = await _priceFormatter.FormatPriceAsync(orderSubtotalInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, true);
            // Discount (applied to order subtotal)
            var orderSubTotalDiscountInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(refundOrder.OrderSubTotalDiscountInclTax, order.CurrencyRate);
            if (orderSubTotalDiscountInclTaxInCustomerCurrency > decimal.Zero)
            {
                cusSubTotalDiscount = await _priceFormatter.FormatPriceAsync(-orderSubTotalDiscountInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, true);
                displaySubTotalDiscount = true;
            }
        }
        else
        {
            // Excluding tax

            // Subtotal
            var orderSubtotalExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(refundOrder.OrderSubtotalExclTax, order.CurrencyRate);
            cusSubTotal = await _priceFormatter.FormatPriceAsync(orderSubtotalExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, false);
            // Discount (applied to order subtotal)
            var orderSubTotalDiscountExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(refundOrder.OrderSubTotalDiscountExclTax, order.CurrencyRate);
            if (orderSubTotalDiscountExclTaxInCustomerCurrency > decimal.Zero)
            {
                cusSubTotalDiscount = await _priceFormatter.FormatPriceAsync(-orderSubTotalDiscountExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, false);
                displaySubTotalDiscount = true;
            }
        }

        // Shipping, payment method fee
        string cusShipTotal;
        string cusPaymentMethodAdditionalFee;
        var taxRates = new SortedDictionary<decimal, decimal>();
        var cusTaxTotal = string.Empty;
        var cusDiscount = string.Empty;
        if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
        {
            // Including tax

            // Shipping
            var orderShippingInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(refundOrder.OrderShippingInclTax, order.CurrencyRate);
            cusShipTotal = await _priceFormatter.FormatShippingPriceAsync(orderShippingInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, true);
            // Payment method additional fee
            var paymentMethodAdditionalFeeInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(refundOrder.PaymentMethodAdditionalFeeInclTax, order.CurrencyRate);
            cusPaymentMethodAdditionalFee = await _priceFormatter.FormatPaymentMethodAdditionalFeeAsync(paymentMethodAdditionalFeeInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, true);
        }
        else
        {
            // Excluding tax

            // Shipping
            var orderShippingExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(refundOrder.OrderShippingExclTax, order.CurrencyRate);
            cusShipTotal = await _priceFormatter.FormatShippingPriceAsync(orderShippingExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, false);
            // Payment method additional fee
            var paymentMethodAdditionalFeeExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(refundOrder.PaymentMethodAdditionalFeeExclTax, order.CurrencyRate);
            cusPaymentMethodAdditionalFee = await _priceFormatter.FormatPaymentMethodAdditionalFeeAsync(paymentMethodAdditionalFeeExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, false);
        }

        // Shipping
        var displayShipping = order.ShippingStatus != ShippingStatus.ShippingNotRequired;

        // Payment method fee
        var displayPaymentMethodFee = refundOrder.PaymentMethodAdditionalFeeExclTax > decimal.Zero;

        // Tax
        bool displayTax;
        bool displayTaxRates;
        if (_taxSettings.HideTaxInOrderSummary && order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
        {
            displayTax = false;
            displayTaxRates = false;
        }
        else
        {
            if (refundOrder.OrderTax == 0 && _taxSettings.HideZeroTax)
            {
                displayTax = false;
                displayTaxRates = false;
            }
            else
            {
                taxRates = new SortedDictionary<decimal, decimal>();
                foreach (var tr in _orderService.ParseTaxRates(order, refundOrder.TaxRates))
                    taxRates.Add(tr.Key, _currencyService.ConvertCurrency(tr.Value, order.CurrencyRate));

                displayTaxRates = _taxSettings.DisplayTaxRates && taxRates.Count != 0;
                displayTax = !displayTaxRates;

                var orderTaxInCustomerCurrency = _currencyService.ConvertCurrency(refundOrder.OrderTax, order.CurrencyRate);
                var taxStr = await _priceFormatter.FormatPriceAsync(orderTaxInCustomerCurrency, true, order.CustomerCurrencyCode,
                    false, languageId);
                cusTaxTotal = taxStr;
            }
        }

        // Discount
        var displayDiscount = false;
        if (refundOrder.OrderDiscount > decimal.Zero)
        {
            var orderDiscountInCustomerCurrency = _currencyService.ConvertCurrency(refundOrder.OrderDiscount, order.CurrencyRate);
            cusDiscount = await _priceFormatter.FormatPriceAsync(-orderDiscountInCustomerCurrency, true, order.CustomerCurrencyCode, false, languageId);
            displayDiscount = true;
        }

        // Total
        var orderTotalInCustomerCurrency = _currencyService.ConvertCurrency(refundOrder.OrderTotal, order.CurrencyRate);
        var cusTotal = await _priceFormatter.FormatPriceAsync(orderTotalInCustomerCurrency, true, order.CustomerCurrencyCode, false, languageId);

        // Subtotal
        sb.AppendLine($"<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {_templatesSettings.Color3};padding:0.6em 0.4 em;\"><strong>{await _localizationService.GetResourceAsync("Messages.Order.SubTotal", languageId)}</strong></td> <td style=\"background-color: {_templatesSettings.Color3};padding:0.6em 0.4 em;\"><strong>{cusSubTotal}</strong></td></tr>");

        // Discount (applied to order subtotal)
        if (displaySubTotalDiscount)
        {
            sb.AppendLine($"<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {_templatesSettings.Color3};padding:0.6em 0.4 em;\"><strong>{await _localizationService.GetResourceAsync("Messages.Order.SubTotalDiscount", languageId)}</strong></td> <td style=\"background-color: {_templatesSettings.Color3};padding:0.6em 0.4 em;\"><strong>{cusSubTotalDiscount}</strong></td></tr>");
        }

        // Shipping
        if (displayShipping)
        {
            sb.AppendLine($"<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {_templatesSettings.Color3};padding:0.6em 0.4 em;\"><strong>{await _localizationService.GetResourceAsync("Messages.Order.Shipping", languageId)}</strong></td> <td style=\"background-color: {_templatesSettings.Color3};padding:0.6em 0.4 em;\"><strong>{cusShipTotal}</strong></td></tr>");
        }

        // Payment method fee
        if (displayPaymentMethodFee)
        {
            var paymentMethodFeeTitle = await _localizationService.GetResourceAsync("Messages.Order.PaymentMethodAdditionalFee", languageId);
            sb.AppendLine($"<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {_templatesSettings.Color3};padding:0.6em 0.4 em;\"><strong>{paymentMethodFeeTitle}</strong></td> <td style=\"background-color: {_templatesSettings.Color3};padding:0.6em 0.4 em;\"><strong>{cusPaymentMethodAdditionalFee}</strong></td></tr>");
        }

        // Tax
        if (displayTax)
        {
            sb.AppendLine($"<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {_templatesSettings.Color3};padding:0.6em 0.4 em;\"><strong>{await _localizationService.GetResourceAsync("Messages.Order.Tax", languageId)}</strong></td> <td style=\"background-color: {_templatesSettings.Color3};padding:0.6em 0.4 em;\"><strong>{cusTaxTotal}</strong></td></tr>");
        }

        if (displayTaxRates)
        {
            foreach (var item in taxRates)
            {
                var taxRate = string.Format(await _localizationService.GetResourceAsync("Messages.Order.TaxRateLine"),
                    _priceFormatter.FormatTaxRate(item.Key));
                var taxValue = await _priceFormatter.FormatPriceAsync(item.Value, true, order.CustomerCurrencyCode, false, languageId);
                sb.AppendLine($"<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {_templatesSettings.Color3};padding:0.6em 0.4 em;\"><strong>{taxRate}</strong></td> <td style=\"background-color: {_templatesSettings.Color3};padding:0.6em 0.4 em;\"><strong>{taxValue}</strong></td></tr>");
            }
        }

        // Discount
        if (displayDiscount)
        {
            sb.AppendLine($"<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {_templatesSettings.Color3};padding:0.6em 0.4 em;\"><strong>{await _localizationService.GetResourceAsync("Messages.Order.TotalDiscount", languageId)}</strong></td> <td style=\"background-color: {_templatesSettings.Color3};padding:0.6em 0.4 em;\"><strong>{cusDiscount}</strong></td></tr>");
        }

        // Gift cards
        foreach (var gcuh in await _giftCardService.GetGiftCardUsageHistoryAsync(order))
        {
            var giftCardText = string.Format(await _localizationService.GetResourceAsync("Messages.Order.GiftCardInfo", languageId),
                                             WebUtility.HtmlEncode((await _giftCardService.GetGiftCardByIdAsync(gcuh.GiftCardId))?.GiftCardCouponCode));
            var giftCardAmount = await _priceFormatter.FormatPriceAsync(-_currencyService.ConvertCurrency(gcuh.UsedValue, order.CurrencyRate),
                                                                        true, order.CustomerCurrencyCode, false,
                                                                        languageId);
            sb.AppendLine($"<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {_templatesSettings.Color3};padding:0.6em 0.4 em;\"><strong>{giftCardText}</strong></td> <td style=\"background-color: {_templatesSettings.Color3};padding:0.6em 0.4 em;\"><strong>{giftCardAmount}</strong></td></tr>");
        }

        // Reward points
        if (order.RedeemedRewardPointsEntryId.HasValue && await _rewardPointService.GetRewardPointsHistoryEntryByIdAsync(order.RedeemedRewardPointsEntryId.Value) is RewardPointsHistory redeemedRewardPointsEntry)
        {
            var rpTitle = string.Format(await _localizationService.GetResourceAsync("Messages.Order.RewardPoints", languageId),
                                        -redeemedRewardPointsEntry.Points);
            var rpAmount = await _priceFormatter.FormatPriceAsync(-_currencyService.ConvertCurrency(redeemedRewardPointsEntry.UsedAmount, order.CurrencyRate),
                                                                  true, order.CustomerCurrencyCode, false, languageId);
            sb.AppendLine($"<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {_templatesSettings.Color3};padding:0.6em 0.4 em;\"><strong>{rpTitle}</strong></td> <td style=\"background-color: {_templatesSettings.Color3};padding:0.6em 0.4 em;\"><strong>{rpAmount}</strong></td></tr>");
        }

        // Total
        sb.AppendLine($"<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {_templatesSettings.Color3};padding:0.6em 0.4 em;\"><strong>{await _localizationService.GetResourceAsync("Plugins.Tax.TaxJar.Fields.Messages.RefundOrder.RefundOrderTotal", languageId)}</strong></td> <td style=\"background-color: {_templatesSettings.Color3};padding:0.6em 0.4 em;\"><strong>{cusTotal}</strong></td></tr>");
    }

    /// <summary>
    /// Generates an absolute URL for the specified store, routeName and route values
    /// </summary>
    /// <param name="storeId">Store identifier; Pass 0 to load URL of the current store</param>
    /// <param name="routeName">The name of the route that is used to generate URL</param>
    /// <param name="routeValues">An object that contains route values</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the generated URL
    /// </returns>
    protected virtual async Task<string> RouteUrlAsync(int storeId = 0, string routeName = null, object routeValues = null)
    {
        // Try to get a store by the passed identifier
        var store = await _storeService.GetStoreByIdAsync(storeId)
                    ?? await _storeContext.GetCurrentStoreAsync()
                    ?? throw new Exception("No store could be loaded");

        // Ensure that the store URL is specified
        if (string.IsNullOrEmpty(store.Url))
            throw new Exception("URL cannot be null");

        // Generate the relative URL
        var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
        var url = urlHelper.RouteUrl(routeName, routeValues);

        // Compose the result
        return new Uri(new Uri(store.Url), url).AbsoluteUri;
    }

    /// <summary>
    /// Add order tokens
    /// </summary>
    /// <param name="tokens">List of already added tokens</param>
    /// <param name="order"></param>
    /// <param name="refundOrder">Refund Order</param>
    /// <param name="refundOrderItems">Refund Order Item</param>
    /// <param name="languageId">Language identifier</param>
    /// <param name="vendorId">Vendor identifier</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task AddOrderTokensAsync(IList<Token> tokens, Order order, RefundOrder refundOrder, List<RefundOrderItem> refundOrderItems, int languageId, int vendorId = 0)
    {
        // Lambda expression for choosing correct order address
        async Task<Address> orderAddress(Order o) => await _addressService.GetAddressByIdAsync((o.PickupInStore ? o.PickupAddressId : o.ShippingAddressId) ?? 0);

        var billingAddress = await _addressService.GetAddressByIdAsync(order.BillingAddressId);

        tokens.Add(new Token("Order.OrderId", order.Id));
        tokens.Add(new Token("Order.OrderNumber", order.CustomOrderNumber));

        tokens.Add(new Token("Order.CustomerFullName", $"{billingAddress.FirstName} {billingAddress.LastName}"));
        tokens.Add(new Token("Order.CustomerEmail", billingAddress.Email));

        tokens.Add(new Token("Order.BillingFirstName", billingAddress.FirstName));
        tokens.Add(new Token("Order.BillingLastName", billingAddress.LastName));
        tokens.Add(new Token("Order.BillingPhoneNumber", billingAddress.PhoneNumber));
        tokens.Add(new Token("Order.BillingEmail", billingAddress.Email));
        tokens.Add(new Token("Order.BillingFaxNumber", billingAddress.FaxNumber));
        tokens.Add(new Token("Order.BillingCompany", billingAddress.Company));
        tokens.Add(new Token("Order.BillingAddress1", billingAddress.Address1));
        tokens.Add(new Token("Order.BillingAddress2", billingAddress.Address2));
        tokens.Add(new Token("Order.BillingCity", billingAddress.City));
        tokens.Add(new Token("Order.BillingCounty", billingAddress.County));
        tokens.Add(new Token("Order.BillingStateProvince", await _stateProvinceService.GetStateProvinceByAddressAsync(billingAddress) is StateProvince billingStateProvince ? await _localizationService.GetLocalizedAsync(billingStateProvince, x => x.Name) : string.Empty));
        tokens.Add(new Token("Order.BillingZipPostalCode", billingAddress.ZipPostalCode));
        tokens.Add(new Token("Order.BillingCountry", await _countryService.GetCountryByAddressAsync(billingAddress) is Country billingCountry ? await _localizationService.GetLocalizedAsync(billingCountry, x => x.Name) : string.Empty));
        tokens.Add(new Token("Order.BillingCustomAttributes", await _addressAttributeFormatter.FormatAttributesAsync(billingAddress.CustomAttributes), true));

        tokens.Add(new Token("Order.Shippable", !string.IsNullOrEmpty(order.ShippingMethod)));
        tokens.Add(new Token("Order.ShippingMethod", order.ShippingMethod));
        tokens.Add(new Token("Order.PickupInStore", order.PickupInStore));
        tokens.Add(new Token("Order.ShippingFirstName", (await orderAddress(order))?.FirstName ?? string.Empty));
        tokens.Add(new Token("Order.ShippingLastName", (await orderAddress(order))?.LastName ?? string.Empty));
        tokens.Add(new Token("Order.ShippingPhoneNumber", (await orderAddress(order))?.PhoneNumber ?? string.Empty));
        tokens.Add(new Token("Order.ShippingEmail", (await orderAddress(order))?.Email ?? string.Empty));
        tokens.Add(new Token("Order.ShippingFaxNumber", (await orderAddress(order))?.FaxNumber ?? string.Empty));
        tokens.Add(new Token("Order.ShippingCompany", (await orderAddress(order))?.Company ?? string.Empty));
        tokens.Add(new Token("Order.ShippingAddress1", (await orderAddress(order))?.Address1 ?? string.Empty));
        tokens.Add(new Token("Order.ShippingAddress2", (await orderAddress(order))?.Address2 ?? string.Empty));
        tokens.Add(new Token("Order.ShippingCity", (await orderAddress(order))?.City ?? string.Empty));
        tokens.Add(new Token("Order.ShippingCounty", (await orderAddress(order))?.County ?? string.Empty));
        tokens.Add(new Token("Order.ShippingStateProvince", await _stateProvinceService.GetStateProvinceByAddressAsync(await orderAddress(order)) is StateProvince shippingStateProvince ? await _localizationService.GetLocalizedAsync(shippingStateProvince, x => x.Name) : string.Empty));
        tokens.Add(new Token("Order.ShippingZipPostalCode", (await orderAddress(order))?.ZipPostalCode ?? string.Empty));
        tokens.Add(new Token("Order.ShippingCountry", await _countryService.GetCountryByAddressAsync(await orderAddress(order)) is Country orderCountry ? await _localizationService.GetLocalizedAsync(orderCountry, x => x.Name) : string.Empty));
        tokens.Add(new Token("Order.ShippingCustomAttributes", await _addressAttributeFormatter.FormatAttributesAsync((await orderAddress(order))?.CustomAttributes ?? string.Empty), true));
        tokens.Add(new Token("Order.IsCompletelyShipped", !order.PickupInStore && order.ShippingStatus == ShippingStatus.Shipped));
        tokens.Add(new Token("Order.IsCompletelyReadyForPickup", order.PickupInStore && !await _orderService.HasItemsToAddToShipmentAsync(order) && !await _orderService.HasItemsToReadyForPickupAsync(order)));
        tokens.Add(new Token("Order.IsCompletelyDelivered", order.ShippingStatus == ShippingStatus.Delivered));

        var paymentMethod = await _paymentPluginManager.LoadPluginBySystemNameAsync(order.PaymentMethodSystemName);
        var paymentMethodName = paymentMethod != null ? await _localizationService.GetLocalizedFriendlyNameAsync(paymentMethod, (await _workContext.GetWorkingLanguageAsync()).Id) : order.PaymentMethodSystemName;
        tokens.Add(new Token("Order.PaymentMethod", paymentMethodName));
        tokens.Add(new Token("Order.VatNumber", order.VatNumber));
        var sbCustomValues = new StringBuilder();

        var customValues = new CustomValues();
        customValues.FillByXml(order.CustomValuesXml, true);
        if (customValues != null)
        {
            foreach (var item in customValues)
            {
                sbCustomValues.AppendFormat("{0}: {1}", WebUtility.HtmlEncode(item.Name), WebUtility.HtmlEncode(item.Value != null ? item.Value.ToString() : string.Empty));
                sbCustomValues.Append("<br />");
            }
        }

        tokens.Add(new Token("Order.CustomValues", sbCustomValues.ToString(), true));

        tokens.Add(new Token("Order.Product(s)", await ProductListToHtmlTableAsync(order, refundOrder, refundOrderItems, languageId, vendorId), true));

        var language = await _languageService.GetLanguageByIdAsync(languageId);
        if (language != null && !string.IsNullOrEmpty(language.LanguageCulture))
        {
            var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);
            var createdOn = _dateTimeHelper.ConvertToUserTime(order.CreatedOnUtc, TimeZoneInfo.Utc, await _dateTimeHelper.GetCustomerTimeZoneAsync(customer));
            tokens.Add(new Token("Order.CreatedOn", createdOn.ToString("D", new CultureInfo(language.LanguageCulture))));
        }
        else
        {
            tokens.Add(new Token("Order.CreatedOn", order.CreatedOnUtc.ToString("D")));
        }

        var orderUrl = await RouteUrlAsync(order.StoreId, "OrderDetails", new { orderId = order.Id });
        tokens.Add(new Token("Order.OrderURLForCustomer", orderUrl, true));

        // Event notification
        await _eventPublisher.EntityTokensAddedAsync(order, tokens);
    }

    /// <summary>
    /// Sends an order refunded notification to a customer
    /// </summary>
    /// <param name="order">Order instance</param>
    /// <param name="refundOrder">Refund Order</param>
    /// <param name="refundOrderItems">Refund Order Item</param>
    /// <param name="refundedAmount">Amount refunded</param>
    /// <param name="languageId">Message language identifier</param>
    /// <param name="attachmentFilePath">Attachment file path</param>
    /// <param name="attachmentFileName">Attachment file name. If specified, then this file name will be sent to a recipient. Otherwise, "AttachmentFilePath" name will be used.</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the queued email identifier
    /// </returns>
    protected virtual async Task<IList<int>> SendOrderRefundedCustomerNotificationAsync(Order order, RefundOrder refundOrder, List<RefundOrderItem> refundOrderItems, decimal refundedAmount, int languageId,
        string attachmentFilePath = null, string attachmentFileName = null)
    {
        ArgumentNullException.ThrowIfNull(order);

        var store = await _storeService.GetStoreByIdAsync(order.StoreId) ?? await _storeContext.GetCurrentStoreAsync();
        languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

        var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateSystemNames.ORDER_REFUNDED_CUSTOMER_NOTIFICATION, store.Id);
        if (!messageTemplates.Any())
            return new List<int>();

        // Tokens
        var commonTokens = new List<Token>();
        await AddOrderTokensAsync(commonTokens, order, refundOrder, refundOrderItems, languageId);
        await _messageTokenProvider.AddOrderRefundedTokensAsync(commonTokens, order, refundedAmount);
        await _messageTokenProvider.AddCustomerTokensAsync(commonTokens, order.CustomerId);

        return await messageTemplates.SelectAwait(async messageTemplate =>
        {
            // Email account
            var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

            var tokens = new List<Token>(commonTokens);
            await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount, (await _workContext.GetWorkingLanguageAsync()).Id);

            // Event notification
            await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

            var billingAddress = await _addressService.GetAddressByIdAsync(order.BillingAddressId);

            var toEmail = billingAddress.Email;
            var toName = $"{billingAddress.FirstName} {billingAddress.LastName}";

            return await _workflowMessageService.SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName,
                attachmentFilePath, attachmentFileName);
        }).ToListAsync();
    }

    /// <summary>
    /// Get billing address
    /// </summary>
    /// <param name="vendor">Vendor</param>
    /// <param name="lang">Language</param>
    /// <param name="order">Order</param>
    /// <returns>A task that contains address item</returns>
    protected virtual async Task<AddressItem> GetBillingAddressAsync(Vendor vendor, Language lang, Order order)
    {
        var addressResult = new AddressItem();

        var billingAddress = await _addressService.GetAddressByIdAsync(order.BillingAddressId);

        if (_addressSettings.CompanyEnabled && !string.IsNullOrEmpty(billingAddress.Company))
            addressResult.Company = billingAddress.Company;

        addressResult.Name = $"{billingAddress.FirstName} {billingAddress.LastName}";

        if (_addressSettings.PhoneEnabled)
            addressResult.Phone = billingAddress.PhoneNumber;

        if (_addressSettings.FaxEnabled && !string.IsNullOrEmpty(billingAddress.FaxNumber))
            addressResult.Fax = billingAddress.FaxNumber;

        if (_addressSettings.StreetAddressEnabled)
            addressResult.Address = billingAddress.Address1;

        if (_addressSettings.StreetAddress2Enabled && !string.IsNullOrEmpty(billingAddress.Address2))
            addressResult.Address2 = billingAddress.Address2;

        if (_addressSettings.CityEnabled || _addressSettings.StateProvinceEnabled ||
            _addressSettings.CountyEnabled || _addressSettings.ZipPostalCodeEnabled)
        {
            addressResult.AddressLine =
                $"{billingAddress.City}, " +
                $"{(!string.IsNullOrEmpty(billingAddress.County) ? $"{billingAddress.County}, " : string.Empty)}" +
                $"{(await _stateProvinceService.GetStateProvinceByAddressAsync(billingAddress) is StateProvince stateProvince ? await _localizationService.GetLocalizedAsync(stateProvince, x => x.Name, lang.Id) : string.Empty)} " +
                $"{billingAddress.ZipPostalCode}";
        }

        if (_addressSettings.CountryEnabled && await _countryService.GetCountryByAddressAsync(billingAddress) is Country country)
            addressResult.Country = await _localizationService.GetLocalizedAsync(country, x => x.Name, lang.Id);

        // VAT number
        if (!string.IsNullOrEmpty(order.VatNumber))
            addressResult.VATNumber = order.VatNumber;

        // Custom attributes
        var customBillingAddressAttributes = await _addressAttributeFormatter
            .FormatAttributesAsync(billingAddress.CustomAttributes, "<br />");

        if (!string.IsNullOrEmpty(customBillingAddressAttributes))
        {
            var text = _htmlFormatter.ConvertHtmlToPlainText(customBillingAddressAttributes, true, true);
            addressResult.AddressAttributes = [.. text.Split('\n')];
        }

        // Vendors payment details
        if (vendor is null)
        {
            // Payment method
            var paymentMethod = await _paymentPluginManager.LoadPluginBySystemNameAsync(order.PaymentMethodSystemName);
            var paymentMethodStr = paymentMethod != null
                ? await _localizationService.GetLocalizedFriendlyNameAsync(paymentMethod, lang.Id)
                : order.PaymentMethodSystemName;
            if (!string.IsNullOrEmpty(paymentMethodStr))
            {
                addressResult.PaymentMethod = paymentMethodStr;
            }

            // Billing address custom values
            var customValues = new CustomValues();
            customValues.FillByXml(order.CustomValuesXml, true);
            if (customValues != null)
                addressResult.CustomValues.AddRange(customValues.Where(value => value.DisplayLocation == CustomValueDisplayLocation.BillingAddress));
        }

        return addressResult;
    }

    /// <summary>
    /// Get shipping address
    /// </summary>
    /// <param name="lang">Language</param>
    /// <param name="order">Order</param>
    /// <returns>A task that contains address item</returns>
    protected virtual async Task<AddressItem> GetShippingAddressAsync(Language lang, Order order)
    {
        var addressResult = new AddressItem();

        if (order.ShippingStatus != ShippingStatus.ShippingNotRequired)
        {
            if (!order.PickupInStore)
            {
                if (order.ShippingAddressId == null || await _addressService.GetAddressByIdAsync(order.ShippingAddressId.Value) is not Address shippingAddress)
                    throw new NopException($"Shipping is required, but address is not available. Order ID = {order.Id}");

                if (!string.IsNullOrEmpty(shippingAddress.Company))
                    addressResult.Company = shippingAddress.Company;

                addressResult.Name = $"{shippingAddress.FirstName} {shippingAddress.LastName}";

                if (_addressSettings.PhoneEnabled)
                    addressResult.Phone = shippingAddress.PhoneNumber;

                if (_addressSettings.FaxEnabled && !string.IsNullOrEmpty(shippingAddress.FaxNumber))
                    addressResult.Fax = shippingAddress.FaxNumber;

                if (_addressSettings.StreetAddressEnabled)
                    addressResult.Address = shippingAddress.Address1;

                if (_addressSettings.StreetAddress2Enabled && !string.IsNullOrEmpty(shippingAddress.Address2))
                    addressResult.Address2 = shippingAddress.Address2;

                if (_addressSettings.CityEnabled || _addressSettings.StateProvinceEnabled ||
                    _addressSettings.CountyEnabled || _addressSettings.ZipPostalCodeEnabled)
                {
                    addressResult.AddressLine = $"{shippingAddress.City}, " +
                        $"{(!string.IsNullOrEmpty(shippingAddress.County) ? $"{shippingAddress.County}, " : string.Empty)}" +
                        $"{(await _stateProvinceService.GetStateProvinceByAddressAsync(shippingAddress) is StateProvince stateProvince ? await _localizationService.GetLocalizedAsync(stateProvince, x => x.Name, lang.Id) : string.Empty)} " +
                        $"{shippingAddress.ZipPostalCode}";
                }

                if (_addressSettings.CountryEnabled && await _countryService.GetCountryByAddressAsync(shippingAddress) is Country country)
                {
                    addressResult.Country = await _localizationService.GetLocalizedAsync(country, x => x.Name, lang.Id);
                }

                // Custom attributes
                var customShippingAddressAttributes = await _addressAttributeFormatter
                    .FormatAttributesAsync(shippingAddress.CustomAttributes, "<br />");
                if (!string.IsNullOrEmpty(customShippingAddressAttributes))
                {
                    var text = _htmlFormatter.ConvertHtmlToPlainText(customShippingAddressAttributes, true, true);
                    addressResult.AddressAttributes = [.. text.Split('\n')];
                }
            }
            else if (order.PickupAddressId.HasValue && await _addressService.GetAddressByIdAsync(order.PickupAddressId.Value) is Address pickupAddress)
            {
                if (!string.IsNullOrEmpty(pickupAddress.Address1))
                    addressResult.Address = pickupAddress.Address1;

                if (_addressSettings.CityEnabled || _addressSettings.StateProvinceEnabled ||
                    _addressSettings.CountyEnabled || _addressSettings.ZipPostalCodeEnabled)
                {
                    addressResult.AddressLine = $"{pickupAddress.City}, " +
                        $"{(!string.IsNullOrEmpty(pickupAddress.County) ? $"{pickupAddress.County}, " : string.Empty)}" +
                        $"{(await _stateProvinceService.GetStateProvinceByAddressAsync(pickupAddress) is StateProvince stateProvince ? await _localizationService.GetLocalizedAsync(stateProvince, x => x.Name, lang.Id) : string.Empty)} " +
                        $"{pickupAddress.ZipPostalCode}";
                }

                if (await _countryService.GetCountryByAddressAsync(pickupAddress) is Country country)
                    addressResult.Country = await _localizationService.GetLocalizedAsync(country, x => x.Name, lang.Id);
            }

            addressResult.ShippingMethod = order.ShippingMethod;
        }

        return addressResult;
    }

    /// <summary>
    /// Get order notes
    /// </summary>
    /// <param name="pdfSettingsByStore">PDF settings</param>
    /// <param name="order">Order</param>
    /// <param name="lang">Language</param>
    /// <returns>A task that contains collection of date/note pairs</returns>
    protected virtual async Task<List<(string, string)>> GetOrderNotesAsync(PdfSettings pdfSettingsByStore, Order order, Language lang)
    {
        var notesResult = new List<(string, string)>();

        if (!pdfSettingsByStore.RenderOrderNotes)
            return notesResult;

        var orderNotes = (await _orderService.GetOrderNotesByOrderIdAsync(order.Id, true))
            .OrderByDescending(on => on.CreatedOnUtc)
            .ToList();

        if (orderNotes.Count == 0)
            return notesResult;

        foreach (var orderNote in orderNotes)
        {
            var createdOn = (await _dateTimeHelper.ConvertToUserTimeAsync(orderNote.CreatedOnUtc, DateTimeKind.Utc)).ToString();
            var note = _htmlFormatter.ConvertHtmlToPlainText(_orderService.FormatOrderNoteText(orderNote), true, true);

            notesResult.Add((createdOn, note));

            // Should we display a link to downloadable files here?
            // I think, no. Anyway, PDFs are printable documents and links (files) are useful here
        }

        return notesResult;
    }

    /// <summary>
    /// Get product entries for document data source
    /// </summary>
    /// <param name="order">Order</param>
    /// <param name="orderItems">Collection of order items</param>
    /// <param name="language">Language</param>
    /// <param name="shipmentItems">Collection of shipment items; when using to prepare shipment items</param>
    /// <returns>A task that contains collection of product entries</returns>
    protected virtual async Task<List<ProductItem>> GetOrderProductItemsAsync(
        Order order,
        IList<OrderItem> orderItems,
        IList<RefundOrderItem> refundOrderItems,
        Language language,
        IList<ShipmentItem> shipmentItems = null)
    {
        var vendors = _vendorSettings.ShowVendorOnOrderDetailsPage ? await _vendorService.GetVendorsByProductIdsAsync(orderItems.Select(item => item.ProductId).ToArray()) : new List<Vendor>();
        var asspociatedVendors = _vendorSettings.ShowVendorOnOrderDetailsPage ? await _vendorService.GetVendorsByProductIdsAsync(refundOrderItems.Select(item => item.AssociatedProductId).ToArray()) : new List<Vendor>();
        var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);
        var result = new List<ProductItem>();

        foreach (var oi in refundOrderItems)
        {
            var productItem = new ProductItem();
            var product = await _productService.GetProductByIdAsync(oi.AssociatedProductId > 0 ? oi.AssociatedProductId : oi.ProductId);
            var realOrderItem = orderItems.Where(o => o.Id == oi.Id).FirstOrDefault();

            // Product name
            productItem.Name = await _localizationService.GetLocalizedAsync(product, x => x.Name, language.Id);

            // Attributes
            if (!string.IsNullOrEmpty(realOrderItem.AttributeDescription) && oi.AssociatedProductId == 0)
            {
                var renderAttributeValuePrices = await _settingService.GetSettingByKeyAsync<bool>("invoicepdfsettings.renderattributevalueprices", false,
                                                                                                  await _storeContext.GetActiveStoreScopeConfigurationAsync());
                if (!renderAttributeValuePrices)
                {
                    var orderStore = await _storeService.GetStoreByIdAsync(order.StoreId);
                    var attributeDescription = await _productAttributeFormatter.FormatAttributesAsync(product,
                        realOrderItem.AttributesXml, customer, orderStore, renderPrices: true);

                    var attributes = _htmlFormatter.ConvertHtmlToPlainText(attributeDescription, true, true);
                    productItem.ProductAttributes = [.. attributes.Split('\n')];
                }
                else
                {
                    var attributes = _htmlFormatter.ConvertHtmlToPlainText(realOrderItem.AttributeDescription, true, true);
                    productItem.ProductAttributes = [.. attributes.Split('\n')];
                }
            }

            // SKU
            if (_catalogSettings.ShowSkuOnProductDetailsPage)
                productItem.Sku = await _productService.FormatSkuAsync(product, oi.AssociatedProductId > 0 ? string.Empty : realOrderItem.AttributesXml);

            // Vendor name
            if (_vendorSettings.ShowVendorOnOrderDetailsPage)
            {
                productItem.VendorName = vendors.FirstOrDefault(v => v.Id == product.VendorId)?.Name ?? string.Empty;

                if (string.IsNullOrEmpty(productItem.VendorName))
                    productItem.VendorName = asspociatedVendors.FirstOrDefault(v => v.Id == product.VendorId)?.Name ?? string.Empty;
            }


            // Price
            string unitPrice;
            if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
            {
                // Including tax
                var unitPriceInclTaxInCustomerCurrency =
                    _currencyService.ConvertCurrency(oi.UnitPriceInclTax, order.CurrencyRate);
                unitPrice = await _priceFormatter.FormatPriceAsync(unitPriceInclTaxInCustomerCurrency, true,
                                                                   order.CustomerCurrencyCode, language.Id, true);
            }
            else
            {
                // Excluding tax
                var unitPriceExclTaxInCustomerCurrency =
                    _currencyService.ConvertCurrency(oi.UnitPriceExclTax, order.CurrencyRate);
                unitPrice = await _priceFormatter.FormatPriceAsync(unitPriceExclTaxInCustomerCurrency, true,
                                                                   order.CustomerCurrencyCode, language.Id, false);
            }

            productItem.Price = unitPrice;

            // Qty
            productItem.Quantity = shipmentItems is null 
                ? oi.Quantity.ToString() 
                : shipmentItems.FirstOrDefault(x => x.OrderItemId == oi.Id).Quantity.ToString();

            // Total
            string subTotal;
            if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
            {
                // Including tax
                var priceInclTaxInCustomerCurrency =
                    _currencyService.ConvertCurrency(oi.PriceInclTax, order.CurrencyRate);
                subTotal = await _priceFormatter.FormatPriceAsync(priceInclTaxInCustomerCurrency, true,
                                                                  order.CustomerCurrencyCode, language.Id, true);
            }
            else
            {
                // Excluding tax
                var priceExclTaxInCustomerCurrency =
                    _currencyService.ConvertCurrency(oi.PriceExclTax, order.CurrencyRate);
                subTotal = await _priceFormatter.FormatPriceAsync(priceExclTaxInCustomerCurrency, true,
                                                                  order.CustomerCurrencyCode, language.Id, false);
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
    protected virtual async Task<InvoiceTotals> GetTotalsAsync(Language lang, Order order, RefundOrder refundOrder)
    {
        var result = new InvoiceTotals();
        var languageId = lang.Id;

        // Order subtotal
        if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax
            && !_taxSettings.ForceTaxExclusionFromOrderSubtotal)
        {
            // Including tax
            var orderSubtotalInclTaxInCustomerCurrency =
                _currencyService.ConvertCurrency(refundOrder.OrderSubtotalInclTax, order.CurrencyRate);
            result.SubTotal = await _priceFormatter.FormatPriceAsync(orderSubtotalInclTaxInCustomerCurrency, true,
                                                                     order.CustomerCurrencyCode, languageId, true);
        }
        else
        {
            // Excluding tax
            var orderSubtotalExclTaxInCustomerCurrency =
                _currencyService.ConvertCurrency(refundOrder.OrderSubtotalExclTax, order.CurrencyRate);
            result.SubTotal = await _priceFormatter.FormatPriceAsync(orderSubtotalExclTaxInCustomerCurrency, true,
                                                                     order.CustomerCurrencyCode, languageId, false);
        }

        // Discount (applied to order subtotal)
        if (refundOrder.OrderSubTotalDiscountExclTax > decimal.Zero)
        {
            // Order subtotal
            if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax &&
                !_taxSettings.ForceTaxExclusionFromOrderSubtotal)
            {
                // Including tax
                var orderSubTotalDiscountInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(refundOrder.OrderSubTotalDiscountInclTax,
                                                                                                      order.CurrencyRate);
                result.Discount = await _priceFormatter.FormatPriceAsync(-orderSubTotalDiscountInclTaxInCustomerCurrency,
                                                                         true, order.CustomerCurrencyCode, languageId, true);
            }
            else
            {
                // Excluding tax
                var orderSubTotalDiscountExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(refundOrder.OrderSubTotalDiscountExclTax,
                                                                                                      order.CurrencyRate);
                result.Discount = await _priceFormatter.FormatPriceAsync(-orderSubTotalDiscountExclTaxInCustomerCurrency,
                                                                         true, order.CustomerCurrencyCode, languageId, false);
            }
        }

        // Shipping
        if (order.ShippingStatus != ShippingStatus.ShippingNotRequired)
        {
            if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
            {
                // Including tax
                var orderShippingInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(refundOrder.OrderShippingInclTax,
                                                                                              order.CurrencyRate);
                result.Shipping = await _priceFormatter.FormatShippingPriceAsync(orderShippingInclTaxInCustomerCurrency, true,
                                                                                 order.CustomerCurrencyCode, languageId, true);
            }
            else
            {
                // Excluding tax
                var orderShippingExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(refundOrder.OrderShippingExclTax,
                                                                                              order.CurrencyRate);
                result.Shipping = await _priceFormatter.FormatShippingPriceAsync(orderShippingExclTaxInCustomerCurrency, true, 
                                                                                 order.CustomerCurrencyCode, languageId, false);
            }
        }

        // Payment fee
        if (refundOrder.PaymentMethodAdditionalFeeExclTax > decimal.Zero)
        {
            if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
            {
                // Including tax
                var paymentMethodAdditionalFeeInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(refundOrder.PaymentMethodAdditionalFeeInclTax,
                                                                                                           order.CurrencyRate);
                result.PaymentMethodAdditionalFee = await _priceFormatter.FormatPaymentMethodAdditionalFeeAsync(
                    paymentMethodAdditionalFeeInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, true);
            }
            else
            {
                // Excluding tax
                var paymentMethodAdditionalFeeExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(refundOrder.PaymentMethodAdditionalFeeExclTax,
                                                                                                           order.CurrencyRate);
                result.PaymentMethodAdditionalFee = await _priceFormatter.FormatPaymentMethodAdditionalFeeAsync(
                    paymentMethodAdditionalFeeExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, false);
            }
        }

        // Tax
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
            if (refundOrder.OrderTax == 0 && _taxSettings.HideZeroTax)
            {
                displayTax = false;
                displayTaxRates = false;
            }
            else
            {
                taxRates = _orderService.ParseTaxRates(order, refundOrder.TaxRates);

                displayTaxRates = _taxSettings.DisplayTaxRates && taxRates.Count != 0;
                displayTax = !displayTaxRates;

                var orderTaxInCustomerCurrency = _currencyService.ConvertCurrency(refundOrder.OrderTax, order.CurrencyRate);
                taxStr = await _priceFormatter.FormatPriceAsync(orderTaxInCustomerCurrency, true,
                                                                order.CustomerCurrencyCode, false, languageId);
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
                var taxValue = await _priceFormatter.FormatPriceAsync(_currencyService.ConvertCurrency(item.Value, order.CurrencyRate),
                                                                      true, order.CustomerCurrencyCode, false, languageId);

                result.TaxRates.Add($"{taxRate} {taxValue}");
            }
        }

        // Discount (applied to order total)
        if (refundOrder.OrderDiscount > decimal.Zero)
        {
            var orderDiscountInCustomerCurrency = _currencyService.ConvertCurrency(refundOrder.OrderDiscount, order.CurrencyRate);
            result.Discount = await _priceFormatter.FormatPriceAsync(-orderDiscountInCustomerCurrency, true,
                                                                     order.CustomerCurrencyCode, false, languageId);
        }

        // Gift cards
        foreach (var gcuh in await _giftCardService.GetGiftCardUsageHistoryAsync(order))
        {
            var gcTitle = string.Format(await _localizationService.GetResourceAsync("Pdf.GiftCardInfo", languageId),
                                        (await _giftCardService.GetGiftCardByIdAsync(gcuh.GiftCardId))?.GiftCardCouponCode);
            var gcAmountStr = await _priceFormatter.FormatPriceAsync(-_currencyService.ConvertCurrency(gcuh.UsedValue, order.CurrencyRate),
                                                                     true, order.CustomerCurrencyCode, false, languageId);

            result.GiftCards.Add($"{gcTitle} {gcAmountStr}");
        }

        // Reward points
        if (order.RedeemedRewardPointsEntryId.HasValue
            && await _rewardPointService.GetRewardPointsHistoryEntryByIdAsync(order.RedeemedRewardPointsEntryId.Value) is RewardPointsHistory redeemedRewardPointsEntry)
        {
            var rpTitle = string.Format(await _localizationService.GetResourceAsync("Pdf.RewardPoints", languageId),
                                        -redeemedRewardPointsEntry.Points);
            var rpAmount = await _priceFormatter.FormatPriceAsync(-_currencyService.ConvertCurrency(redeemedRewardPointsEntry.UsedAmount, order.CurrencyRate),
                                                                  true, order.CustomerCurrencyCode, false, languageId);

            result.RewardPoints = $"{rpTitle} {rpAmount}";
        }

        // Order total
        var orderTotalInCustomerCurrency = _currencyService.ConvertCurrency(refundOrder.OrderTotal, order.CurrencyRate);
        var orderTotalStr = await _priceFormatter.FormatPriceAsync(orderTotalInCustomerCurrency, true, order.CustomerCurrencyCode, false, languageId);
        result.OrderTotal = $"{await _localizationService.GetResourceAsync("Plugins.Tax.TaxJar.Fields.PDFInvoice.RefundOrderTotal", languageId)} {orderTotalStr}";

        return result;
    }

    /// <summary>
    /// Print orders to PDF
    /// </summary>
    /// <param name="stream">Stream</param>
    /// <param name="order">Order</param>
    /// <param name="refundOrder">Refund Order</param>
    /// <param name="refundOrderItems">Refund Order Item</param>
    /// <param name="languageId">Language identifier; 0 to use a language used when placing an order</param>
    /// <param name="vendorId">Vendor identifier to limit products; 0 to print all products. If specified, then totals won't be printed</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    protected virtual async Task PrintRefundOrderToPdfAsync(Stream stream, Order order, RefundOrder refundOrder, List<RefundOrderItem> refundOrderItems, int languageId = 0, int vendorId = 0)
    {
        ArgumentNullException.ThrowIfNull(order);

        // Store info
        var store = await _storeContext.GetCurrentStoreAsync();

        var orderStore = order.StoreId == 0 || order.StoreId == store?.Id ?
            store : await _storeService.GetStoreByIdAsync(order.StoreId);

        // Language info
        var language = await _languageService.GetLanguageByIdAsync(languageId == 0 ? order.CustomerLanguageId : languageId);

        if (language?.Published != true)
            language = await _workContext.GetWorkingLanguageAsync();

        // By default _pdfSettings contains settings for the current active store
        // And we need PdfSettings for the store which was used to place an order
        // So let's load it based on a store of the current order
        var pdfSettingsByStore = await _settingService.LoadSettingAsync<PdfSettings>(orderStore.Id);

        byte[] logo = null;
        var logoPicture = await _pictureService.GetPictureByIdAsync(pdfSettingsByStore.LogoPictureId);
        if (logoPicture != null)
        {
            var logoFilePath = (await _pictureService.GetPictureUrlAsync(logoPicture, 0, false)).Url;
            logo = await _pictureService.LoadPictureBinaryAsync(logoPicture);

            if (logoPicture.MimeType == MimeTypes.ImageSvg)
            {
                await using var logoStream = new MemoryStream(logo);
                logo = await _pictureService.ConvertSvgToPngAsync(logoStream);
            }
            else
            {
                logo = await _fileProvider.ReadAllBytesAsync(logoFilePath);
            }
        }

        var date = await _dateTimeHelper.ConvertToUserTimeAsync(order.CreatedOnUtc, DateTimeKind.Utc);

        // A vendor should have access only to products
        var vendor = await _vendorService.GetVendorByIdAsync(vendorId);
        var orderItems = await _orderService.GetOrderItemsAsync(order.Id, vendorId: vendorId);

        var column1Lines = string.IsNullOrEmpty(pdfSettingsByStore.InvoiceFooterTextColumn1) 
            ? new List<string>()
            : [.. pdfSettingsByStore.InvoiceFooterTextColumn1.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)];

        var column2Lines = string.IsNullOrEmpty(pdfSettingsByStore.InvoiceFooterTextColumn2) 
            ? new List<string>()
            : [.. pdfSettingsByStore.InvoiceFooterTextColumn2.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)];

        var source = new InvoiceDocument
        {
            StoreUrl = orderStore.Url?.Trim('/'),
            Language = language,
            Font = ResolvePdfFont(language, pdfSettingsByStore),
            ImageTargetSize = pdfSettingsByStore.ImageTargetSize,
            OrderDateUser = date.ToString("D", new CultureInfo(language.LanguageCulture)),
            LogoData = logo,
            OrderNumberText = order.CustomOrderNumber,
            PageSize = pdfSettingsByStore.LetterPageSizeEnabled ? PdfPageSize.Letter : PdfPageSize.A4,
            BillingAddress = await GetBillingAddressAsync(vendor, language, order),
            ShippingAddress = await GetShippingAddressAsync(language, order),
            Products = await GetOrderProductItemsAsync(order, orderItems, refundOrderItems, language),
            ShowSkuInProductList = _catalogSettings.ShowSkuOnProductDetailsPage,
            ShowVendorInProductList = _vendorSettings.ShowVendorOnOrderDetailsPage,
            CheckoutAttributes = vendor is null ? order.CheckoutAttributeDescription : string.Empty, //vendors cannot see checkout attributes
            Totals = vendor is null ? await GetTotalsAsync(language, order, refundOrder) : new(), //vendors cannot see totals
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
    /// Resolve font for PDF document
    /// </summary>
    /// <param name="language">Language</param>
    /// <param name="settings">PDF settings</param>
    /// <returns>A font object</returns>
    protected virtual Font ResolvePdfFont(Language language, PdfSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var fontName = language?.Rtl == true
            ? !string.IsNullOrEmpty(settings.RtlFontName) ? settings.RtlFontName : NopCommonDefaults.PdfRtlFontName
            : !string.IsNullOrEmpty(settings.LtrFontName) ? settings.LtrFontName : NopCommonDefaults.PdfLtrFontName;

        var fontSize = settings.BaseFontSize >= 0 ? settings.BaseFontSize : 10;

        return PdfDocumentHelper.GetFont(fontName, fontSize);
    }

    /// <summary>
    /// Print an order to PDF
    /// </summary>
    /// <param name="order">Order</param>
    /// <param name="refundOrder">Refund Order</param>
    /// <param name="refundOrderItems">Refund Order Item</param>
    /// <param name="languageId">Language identifier; 0 to use a language used when placing an order</param>
    /// <param name="vendorId">Vendor identifier to limit products; 0 to print all products. If specified, then totals won't be printed</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains a path of generated file
    /// </returns>
    protected virtual async Task<string> PrintRefundOrderToPdfAsync(Order order, RefundOrder refundOrder, List<RefundOrderItem> refundOrderItems, int languageId = 0, int vendorId = 0)
    {
        ArgumentNullException.ThrowIfNull(order);

        var fileName = $"order_{order.Id}_refund_{CommonHelper.GenerateRandomDigitCode(4)}.pdf";
        var filePath = _fileProvider.Combine(_fileProvider.MapPath("~/wwwroot/files/exportimport"), fileName);
        await using var fileStream = new FileStream(filePath, FileMode.Create);

        await PrintRefundOrderToPdfAsync(fileStream, order, refundOrder, refundOrderItems, languageId, vendorId);

        return filePath;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Calculated price
    /// </summary>
    /// <param name="price">Price</param>
    /// <param name="percent">Percent</param>
    /// <param name="increase">Increase</param>
    /// <returns>New price</returns>
    public virtual decimal CalculatePrice(decimal price, decimal percent, bool increase)
    {
        if (percent == decimal.Zero)
            return price;

        decimal result;
        if (increase)
            result = price * (1 + percent / 100);
        else
            result = price - price / (100 + percent) * percent;

        return result;
    }

    /// <summary>
    /// Calculated rate
    /// </summary>
    /// <param name="price">Price</param>
    /// <param name="percent">Percent</param>
    /// <param name="increase">Increase</param>
    /// <returns>New price</returns>
    public virtual decimal CalculateRate(decimal price, decimal old)
    {
        if (old == decimal.Zero)
            return decimal.Zero;

        var increase = price - old;
        return increase / old * 100;
    }

    /// <summary>
    /// Print sales tax exemption certificate to PDF
    /// </summary>
    /// <param name="stream">Stream</param>
    /// <param name="customer">Customer</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task PrintSalesTaxExemptionCertificateToPdfAsync(Stream stream, Customer customer)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(customer);

        var filePath = _fileProvider.Combine(_fileProvider.MapPath(TaxJarDefaults.EXEMPTION_CERTIFICATE), TaxJarDefaults.EXEMPTION_CERTIFICATE_FILE_NAME);
        var fileStream = await _fileProvider.ReadAllBytesAsync(filePath);

        var pdfDoc = new PdfReader(fileStream);
        var pdfStamper = new PdfStamper(pdfDoc, stream);
        var pdfFormFields = pdfStamper.AcroFields;

        if (await _customerService.IsRegisteredAsync(customer))
        {
            // Set the field to bold
            var customerFullName = await _customerService.GetCustomerFullNameAsync(customer);
            pdfFormFields.SetField("Item.Your.Name", customerFullName);
            pdfFormFields.SetField("Item.PrintName", customerFullName);
            pdfFormFields.SetField("Item.Date", DateTime.UtcNow.ToShortDateString());
            pdfFormFields.SetField("Item.BusinessAddress", customer.StreetAddress);

            var stateProvince = await _stateProvinceService.GetStateProvinceByIdAsync(customer.StateProvinceId);
            pdfFormFields.SetField("Item.PurchaserState", stateProvince?.Abbreviation ?? string.Empty);
            pdfFormFields.SetField("Item.City", customer.City);
            pdfFormFields.SetField("Item.BusinessPostalcode", customer.ZipPostalCode);
        }
        else
        {
            var address = (await _customerService.GetAddressesByCustomerIdAsync(customer.Id)).LastOrDefault();
            if (address != null)
            {
                // Set the field to bold
                pdfFormFields.SetField("Item.Your.Name", $"{address.Address1} {address.Address2}");

                pdfFormFields.SetField("Item.PrintName", $"{address.Address1} {address.Address2}");
                pdfFormFields.SetField("Item.Date", DateTime.UtcNow.ToShortDateString());
                pdfFormFields.SetField("Item.BusinessAddress", address.Address1);

                var stateProvince = await _stateProvinceService.GetStateProvinceByIdAsync((int)address.StateProvinceId);
                pdfFormFields.SetField("Item.PurchaserState", stateProvince?.Abbreviation ?? string.Empty);
                pdfFormFields.SetField("Item.City", address.City);
                pdfFormFields.SetField("Item.BusinessPostalcode", address.ZipPostalCode);
            }
        }

        var attributeXml = customer.CustomCustomerAttributesXML;
        var attributs = await _customerAttributeService.GetAllAttributesAsync();
        foreach (var attribute in attributs)
        {
            switch (attribute.AttributeControlType)
            {
                case AttributeControlType.DropdownList:
                    {
                        var valuesStr = _customerAttributeParser.ParseValues(attributeXml, attribute.Id);
                        foreach (var valueStr in valuesStr)
                        {
                            if (int.TryParse(valueStr, out var attributeValueId))
                            {
                                var attributeValue = await _customerAttributeService.GetAttributeValueByIdAsync(attributeValueId);
                                if (attributeValue != null)
                                {
                                    if (attribute.Name.Contains("Reason"))
                                    {
                                        if (attributeValue.Name.Contains("Resale"))
                                        {
                                            pdfFormFields.SetField("Item.Checkbox2", "resale");
                                            pdfFormFields.SetField("Item.Checkbox", "Retail trade");
                                        }

                                        if (attributeValue.Name.Contains("Agriculture"))
                                        {
                                            pdfFormFields.SetField("Item.Checkbox2", "agricultural production");
                                            pdfFormFields.SetField("Item.Checkbox", "Agriculture");
                                        }

                                        if (attributeValue.Name.Contains("Government"))
                                        {
                                            pdfFormFields.SetField("Item.Checkbox2", "state or local gov");
                                            pdfFormFields.SetField("Item.Checkbox", "government");
                                        }
                                    }

                                    if (attribute.Name.Contains("State"))
                                        pdfFormFields.SetField("Item.BusinessState", attributeValue.Name);
                                }
                            }
                        }
                    }
                    break;
                case AttributeControlType.TextBox:
                    {
                        if (!string.IsNullOrEmpty(attributeXml))
                        {
                            var enteredText = _customerAttributeParser.ParseValues(attributeXml, attribute.Id);
                            if (enteredText.Any())
                            {
                                var enteredTextValue = enteredText[0];
                                if (attribute.Name.Contains("FEIN"))
                                    pdfFormFields.SetField("Item.FEIN", enteredTextValue);

                                if (attribute.Name.Contains("License"))
                                    pdfFormFields.SetField("Item.DriversLicenseNumber", enteredTextValue);

                                if (attribute.Name.Contains("Purchaser’s"))
                                    pdfFormFields.SetField("Item.TaxIDNumber", enteredTextValue);
                            }
                        }
                    }
                    break;
            }
        }

        //  close the pdf  
        pdfStamper.Close();
        pdfDoc?.Close();
    }

    /// <summary>
    /// Get order items from the passed form
    /// </summary>
    /// <param name="order">Order</param>
    /// <param name="form">Form values</param>
    /// <param name="orderShippingTotalInclTax">Shipping Total Incl Tax</param>
    /// <param name="orderShippingTotalExclTax">Shipping Total Excl Tax</param>
    /// <param name="paymentAdditionalFeeInclTax">Payment Additional Fee Incl Tax</param>
    /// <param name="paymentAdditionalFeeExclTax">Payment Additional Fee Excl Tax</param>
    /// <param name="errors">Errors</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the refund order. Refund order items. Tax rates (of order sub total)
    /// </returns>
    public virtual async Task<(RefundOrder refundOrder, List<RefundOrderItem> refundOrderItems, SortedDictionary<decimal, decimal> taxRates)> ParseRefundOrderAsync(
        Order order,
        IFormCollection form,
        decimal orderShippingTotalInclTax,
        decimal orderShippingTotalExclTax,
        decimal paymentAdditionalFeeInclTax,
        decimal paymentAdditionalFeeExclTax,
        List<string> errors)
    {
        ArgumentNullException.ThrowIfNull(order);
        ArgumentNullException.ThrowIfNull(form);

        var taxRates = new SortedDictionary<decimal, decimal>();
        var orderSubTotalTaxRates = new SortedDictionary<decimal, decimal>();
        var refundOrder = new RefundOrder() { Id = order.Id };

        var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);
        var primaryStoreCurrency = await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId);

        // Order items
        var refundOrderItems = new List<RefundOrderItem>();
        var orderItems = await _orderService.GetOrderItemsAsync(order.Id, vendorId: (await _workContext.GetCurrentVendorAsync())?.Id ?? 0);
        foreach (var item in orderItems)
        {
            var taxRate = CalculateRate(item.UnitPriceInclTax, item.UnitPriceExclTax);
            var controlId = $"orderitems_{item.Id}_0";
            var ctrlItems = form[controlId];
            if (!StringValues.IsNullOrEmpty(ctrlItems))
            {
                var selectedItemId = int.Parse(ctrlItems);
                if (selectedItemId > 0)
                {
                    if (!decimal.TryParse(form["pvUnitPriceInclTax" + selectedItemId], out var unitPriceInclTax))
                        unitPriceInclTax = item.UnitPriceInclTax;
                    if (!decimal.TryParse(form["pvUnitPriceExclTax" + selectedItemId], out var unitPriceExclTax))
                        unitPriceExclTax = item.UnitPriceExclTax;
                    if (!int.TryParse(form["pvQuantity" + selectedItemId], out var quantity))
                        quantity = item.Quantity;

                    var priceInclTax = unitPriceInclTax * quantity;
                    var priceExclTax = unitPriceExclTax * quantity;

                    if (_taxSettings.PricesIncludeTax)
                    {
                        unitPriceExclTax = CalculatePrice(unitPriceInclTax, taxRate, false);
                        priceExclTax = CalculatePrice(priceInclTax, taxRate, false);
                    }
                    else
                    {
                        unitPriceInclTax = CalculatePrice(unitPriceExclTax, taxRate, true);
                        priceInclTax = CalculatePrice(priceExclTax, taxRate, true);
                    }

                    // Tax rates
                    var sciTax = priceInclTax - priceExclTax;
                    if (taxRate > decimal.Zero || sciTax > decimal.Zero)
                    {
                        if (!orderSubTotalTaxRates.TryGetValue(taxRate, out decimal value))
                            orderSubTotalTaxRates.Add(taxRate, sciTax);
                        else
                            orderSubTotalTaxRates[taxRate] = value + sciTax;
                    }

                    var refundOrderItem = new RefundOrderItem()
                    {
                        Id = item.Id,
                        ProductId = item.ProductId,
                        UnitPriceInclTax = unitPriceInclTax,
                        UnitPriceExclTax = unitPriceExclTax,
                        Quantity = quantity,
                        PriceInclTax = priceInclTax,
                        PriceExclTax = priceExclTax,
                        OriginalProductCost = item.OriginalProductCost
                    };
                    refundOrderItems.Add(refundOrderItem);
                }
            }

            var attributeValues = (await _productAttributeParser.ParseProductAttributeValuesAsync(item.AttributesXml))
                .Where(a => a.AttributeValueType == AttributeValueType.AssociatedToProduct);
            foreach (var attributeValue in attributeValues)
            {
                controlId = $"orderitems_{item.Id}_{attributeValue.Id}";
                ctrlItems = form[controlId];
                if (!StringValues.IsNullOrEmpty(ctrlItems))
                {
                    var selectedAssociatedProductId = int.Parse(ctrlItems);
                    if (selectedAssociatedProductId > 0)
                    {
                        var associatedProduct = await _productService.GetProductByIdAsync(attributeValue.AssociatedProductId);
                        if (associatedProduct == null)
                            continue;

                        // Prices
                        var scUnitPrice = (await _priceCalculationService.GetFinalPriceAsync(associatedProduct, customer, await _storeService.GetStoreByIdAsync(order.StoreId))).finalPrice;
                        var scUnitPriceInclTax = CalculatePrice(scUnitPrice, taxRate, true);
                        var scUnitPriceExclTax = scUnitPrice;

                        var pvUnitPriceInclTax = $"pvUnitPriceInclTax{item.Id}_{selectedAssociatedProductId}";
                        if (!decimal.TryParse(form[pvUnitPriceInclTax], out var unitPriceInclTax))
                            unitPriceInclTax = scUnitPriceInclTax;

                        var pvUnitPriceExclTax = $"pvUnitPriceExclTax{item.Id}_{selectedAssociatedProductId}";
                        if (!decimal.TryParse(form[pvUnitPriceExclTax], out var unitPriceExclTax))
                            unitPriceExclTax = scUnitPriceExclTax;

                        var pvQuantity = $"pvQuantity{item.Id}_{selectedAssociatedProductId}";
                        if (!int.TryParse(form[pvQuantity], out var quantity))
                            quantity = item.Quantity * attributeValue.Quantity;

                        var priceInclTax = unitPriceInclTax * quantity;
                        var priceExclTax = unitPriceExclTax * quantity;

                        if (_taxSettings.PricesIncludeTax)
                        {
                            unitPriceExclTax = CalculatePrice(unitPriceInclTax, taxRate, false);
                            priceExclTax = CalculatePrice(priceInclTax, taxRate, false);
                        }
                        else
                        {
                            unitPriceInclTax = CalculatePrice(unitPriceExclTax, taxRate, true);
                            priceInclTax = CalculatePrice(priceExclTax, taxRate, true);
                        }

                        // Tax rates
                        var sciTax = priceInclTax - priceExclTax;
                        if (taxRate > decimal.Zero || sciTax > decimal.Zero)
                        {
                            if (!orderSubTotalTaxRates.TryGetValue(taxRate, out decimal value))
                                orderSubTotalTaxRates.Add(taxRate, sciTax);
                            else
                                orderSubTotalTaxRates[taxRate] = value + sciTax;
                        }

                        var refundOrderItem = new RefundOrderItem()
                        {
                            Id = item.Id,
                            ProductId = item.ProductId,
                            AssociatedProductId = attributeValue.AssociatedProductId,
                            UnitPriceInclTax = unitPriceInclTax,
                            UnitPriceExclTax = unitPriceExclTax,
                            Quantity = quantity,
                            PriceInclTax = priceInclTax,
                            PriceExclTax = priceExclTax,
                            OriginalProductCost = item.OriginalProductCost
                        };
                        refundOrderItems.Add(refundOrderItem);
                    }
                }
            }
        }

        // Sub totals
        var subTotalExclTaxWithoutDiscount = decimal.Zero;
        var subTotalInclTaxWithoutDiscount = decimal.Zero;
        var subtotalBase = decimal.Zero;
        subTotalInclTaxWithoutDiscount = refundOrderItems.Sum(r => r.PriceInclTax);
        subTotalExclTaxWithoutDiscount = refundOrderItems.Sum(r => r.PriceExclTax);

        // Sub total (incl tax)
        var (orderSubTotalDiscountAmount, subTotalWithoutDiscountBase, _, _) = await GetOrderSubTotalDiscountAsync(order, subTotalExclTaxWithoutDiscount, subTotalInclTaxWithoutDiscount, orderSubTotalTaxRates, true);
        refundOrder.OrderSubtotalInclTax = subTotalWithoutDiscountBase;
        refundOrder.OrderSubTotalDiscountInclTax = orderSubTotalDiscountAmount;

        // Sub total (excl tax)
        (orderSubTotalDiscountAmount, subTotalWithoutDiscountBase, subtotalBase, orderSubTotalTaxRates) = await GetOrderSubTotalDiscountAsync(order, subTotalExclTaxWithoutDiscount, subTotalInclTaxWithoutDiscount, orderSubTotalTaxRates, false);
        refundOrder.OrderSubtotalExclTax = subTotalWithoutDiscountBase;
        refundOrder.OrderSubTotalDiscountExclTax = orderSubTotalDiscountAmount;

        // Shipping cost
        var orderShippingTaxRate = CalculateRate(order.OrderShippingInclTax, order.OrderShippingExclTax);
        if (_taxSettings.PricesIncludeTax)
        {
            refundOrder.OrderShippingInclTax = orderShippingTotalInclTax;
            if (_taxSettings.ShippingIsTaxable)
                refundOrder.OrderShippingExclTax = CalculatePrice(orderShippingTotalInclTax, orderShippingTaxRate, false);
            else
                refundOrder.OrderShippingExclTax = orderShippingTotalInclTax;
        }
        else
        {
            refundOrder.OrderShippingExclTax = orderShippingTotalExclTax;
            if (_taxSettings.ShippingIsTaxable)
                refundOrder.OrderShippingInclTax = CalculatePrice(orderShippingTotalExclTax, orderShippingTaxRate, true);
            else
                refundOrder.OrderShippingInclTax = orderShippingTotalExclTax;
        }

        // Payment method handling changes
        var paymentAdditionalFeeTaxRate = CalculateRate(order.PaymentMethodAdditionalFeeInclTax, order.PaymentMethodAdditionalFeeExclTax);
        if (_taxSettings.PricesIncludeTax)
        {
            refundOrder.PaymentMethodAdditionalFeeInclTax = paymentAdditionalFeeInclTax;
            if (_taxSettings.PaymentMethodAdditionalFeeIsTaxable)
                refundOrder.PaymentMethodAdditionalFeeExclTax = CalculatePrice(paymentAdditionalFeeInclTax, paymentAdditionalFeeTaxRate, false);
            else
                refundOrder.PaymentMethodAdditionalFeeExclTax = paymentAdditionalFeeInclTax;
        }
        else
        {
            refundOrder.PaymentMethodAdditionalFeeExclTax = paymentAdditionalFeeExclTax;
            if (_taxSettings.PaymentMethodAdditionalFeeIsTaxable)
                refundOrder.PaymentMethodAdditionalFeeInclTax = CalculatePrice(paymentAdditionalFeeExclTax, paymentAdditionalFeeTaxRate, true);
            else
                refundOrder.PaymentMethodAdditionalFeeInclTax = paymentAdditionalFeeExclTax;
        }

        // Tax
        var taxTotal = decimal.Zero;
        var subTotalTaxTotal = decimal.Zero;
        foreach (var kvp in orderSubTotalTaxRates)
        {
            var taxRate = kvp.Key;
            var taxValue = kvp.Value;
            subTotalTaxTotal += taxValue;

            if (taxRate > decimal.Zero && taxValue > decimal.Zero)
            {
                if (!taxRates.TryGetValue(taxRate, out decimal value))
                    taxRates.Add(taxRate, taxValue);
                else
                    taxRates[taxRate] = value + taxValue;
            }
        }
        taxTotal += subTotalTaxTotal;

        // Shipping
        var shippingTax = decimal.Zero;
        if (_taxSettings.ShippingIsTaxable)
        {
            shippingTax = refundOrder.OrderShippingInclTax - refundOrder.OrderShippingExclTax;
            if (shippingTax < decimal.Zero)
                shippingTax = decimal.Zero;

            if (orderShippingTaxRate > decimal.Zero && shippingTax > decimal.Zero)
            {
                if (!taxRates.TryGetValue(orderShippingTaxRate, out decimal value))
                    taxRates.Add(orderShippingTaxRate, shippingTax);
                else
                    taxRates[orderShippingTaxRate] = value + shippingTax;
            }
        }
        taxTotal += shippingTax;

        // Payment method additional fee
        var paymentMethodAdditionalFeeTax = decimal.Zero;
        if (_taxSettings.PaymentMethodAdditionalFeeIsTaxable)
        {
            paymentMethodAdditionalFeeTax = refundOrder.PaymentMethodAdditionalFeeInclTax - refundOrder.PaymentMethodAdditionalFeeExclTax;
            if (paymentMethodAdditionalFeeTax < decimal.Zero)
                paymentMethodAdditionalFeeTax = decimal.Zero;

            if (paymentAdditionalFeeTaxRate > decimal.Zero && paymentMethodAdditionalFeeTax > decimal.Zero)
            {
                if (!taxRates.TryGetValue(paymentAdditionalFeeTaxRate, out decimal value))
                    taxRates.Add(paymentAdditionalFeeTaxRate, paymentMethodAdditionalFeeTax);
                else
                    taxRates[paymentAdditionalFeeTaxRate] = value + paymentMethodAdditionalFeeTax;
            }
        }
        taxTotal += paymentMethodAdditionalFeeTax;

        // Add at least one tax rate (0%)
        if (taxRates.Count == 0)
            taxRates.Add(decimal.Zero, decimal.Zero);

        if (taxTotal < decimal.Zero)
            taxTotal = decimal.Zero;

        refundOrder.OrderTax = taxTotal;
        refundOrder.TaxRates = taxRates.Aggregate(string.Empty, (current, next) =>
            $"{current}{next.Key.ToString(CultureInfo.InvariantCulture)}:{next.Value.ToString(CultureInfo.InvariantCulture)};   ");

        // Order discount & amount refund
        var resultTemp = decimal.Zero;
        resultTemp += subtotalBase;
        resultTemp += refundOrder.OrderShippingExclTax;
        resultTemp += refundOrder.PaymentMethodAdditionalFeeExclTax;
        resultTemp += refundOrder.OrderTax;
        if (_shoppingCartSettings.RoundPricesDuringCalculation)
            resultTemp = await _priceCalculationService.RoundPriceAsync(resultTemp);

        // Order total discount
        var allDiscounts = new List<Discount>();
        var duh = await _discountService.GetAllDiscountUsageHistoryAsync(orderId: order.Id);
        foreach (var d in duh)
        {
            var discount = await _discountService.GetDiscountByIdAsync(d.DiscountId);
            if (discount.DiscountType == DiscountType.AssignedToOrderTotal)
                allDiscounts.Add(discount);
        }

        var couponCodesToValidate = await _customerService.ParseAppliedDiscountCouponCodesAsync(customer);
        var allowedDiscounts = new List<Discount>();
        if (allDiscounts != null)
        {
            foreach (var discount in allDiscounts)
            {
                if (!_discountService.ContainsDiscount(allowedDiscounts, discount)
                    && (await _discountService.ValidateDiscountAsync(discount, customer, couponCodesToValidate)).IsValid)
                {
                    allowedDiscounts.Add(discount);
                }
            }
        }

        _discountService.GetPreferredDiscount(allowedDiscounts, resultTemp, out decimal discountTotalAmount);

        if (discountTotalAmount < decimal.Zero)
            discountTotalAmount = decimal.Zero;

        if (_shoppingCartSettings.RoundPricesDuringCalculation)
            discountTotalAmount = await _priceCalculationService.RoundPriceAsync(discountTotalAmount);

        // Sub totals with discount        
        if (resultTemp < discountTotalAmount)
            discountTotalAmount = resultTemp;

        // Reduce subtotal
        resultTemp -= discountTotalAmount;

        if (resultTemp < decimal.Zero)
            resultTemp = decimal.Zero;
        if (_shoppingCartSettings.RoundPricesDuringCalculation)
            resultTemp = await _priceCalculationService.RoundPriceAsync(resultTemp);

        if (resultTemp < decimal.Zero)
            resultTemp = decimal.Zero;
        if (_shoppingCartSettings.RoundPricesDuringCalculation)
            resultTemp = await _priceCalculationService.RoundPriceAsync(resultTemp);

        refundOrder.OrderTotal = resultTemp;

        return (refundOrder, refundOrderItems, taxRates);
    }

    /// <summary>
    /// Selected refunds an order (from admin panel)
    /// </summary>
    /// <param name="order">Order</param>
    /// <param name="refundOrder">Refund Order</param>
    /// <param name="refundOrderItems">Refund Order Item</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains a list of errors; empty list if no errors
    /// </returns>
    public virtual async Task<IList<string>> RefundSelectedAsync(Order order, RefundOrder refundOrder, List<RefundOrderItem> refundOrderItems)
    {
        ArgumentNullException.ThrowIfNull(order);

        if (!await _orderProcessingService.CanPartiallyRefundAsync(order, refundOrder.OrderTotal))
            throw new NopException("Cannot do refund selected items for order.");

        var request = new RefundPaymentRequest();
        RefundPaymentResult result = null;
        try
        {
            request.Order = order;
            request.AmountToRefund = refundOrder.OrderTotal;
            request.IsPartialRefund = true;

            result = await _paymentService.RefundAsync(request);

            if (result.Success)
            {
                // Total amount refunded
                var totalAmountRefunded = order.RefundedAmount + refundOrder.OrderTotal;

                // Update order info
                order.RefundedAmount = totalAmountRefunded;
                // Mark payment status as 'Refunded' if the order total amount is fully refunded
                order.PaymentStatus = order.OrderTotal == totalAmountRefunded && result.NewPaymentStatus == PaymentStatus.PartiallyRefunded ? PaymentStatus.Refunded : result.NewPaymentStatus;
                await _orderService.UpdateOrderAsync(order);

                // Check order status
                await _orderProcessingService.CheckOrderStatusAsync(order);

                // Notifications
                var orderRefundedStoreOwnerNotificationQueuedEmailIds = await _workflowMessageService
                    .SendOrderRefundedStoreOwnerNotificationAsync(order, refundOrder.OrderTotal,
                                                                  _localizationSettings.DefaultAdminLanguageId);

                if (orderRefundedStoreOwnerNotificationQueuedEmailIds.Any())
                    await AddOrderNoteAsync(order, $"\"Order refunded\" email (to store owner) has been queued. Queued email identifiers: {string.Join(", ", orderRefundedStoreOwnerNotificationQueuedEmailIds)}.");

                var orderRefundSelectedAttachmentFilePath = _taxJarSettings.AttachPdfInvoiceToOrderRefundSelectedEmail ? await PrintRefundOrderToPdfAsync(order, refundOrder, refundOrderItems) : null;
                var orderRefundSelectedAttachmentFileName = _taxJarSettings.AttachPdfInvoiceToOrderRefundSelectedEmail ? "refundorder.pdf" : null;
                var orderRefundedCustomerNotificationQueuedEmailIds = await SendOrderRefundedCustomerNotificationAsync(
                    order, refundOrder, refundOrderItems, refundOrder.OrderTotal, order.CustomerLanguageId,
                    orderRefundSelectedAttachmentFilePath, orderRefundSelectedAttachmentFileName);

                if (orderRefundedCustomerNotificationQueuedEmailIds.Any())
                    await AddOrderNoteAsync(order, $"\"Order refunded\" email (to customer) has been queued. Queued email identifiers: {string.Join(", ", orderRefundedCustomerNotificationQueuedEmailIds)}.");

                // Raise event       
                await _eventPublisher.PublishAsync(new OrderRefundedEvent(order, refundOrder.OrderTotal));
            }
        }
        catch (Exception exc)
        {
            result ??= new RefundPaymentResult();
            result.AddError($"Error: {exc.Message}. Full exception: {exc}");
        }

        // Process errors
        var error = string.Empty;
        for (var i = 0; i < result.Errors.Count; i++)
        {
            error += $"Error {i}: {result.Errors[i]}";
            if (i != result.Errors.Count - 1)
                error += ". ";
        }

        if (string.IsNullOrEmpty(error))
            return result.Errors;

        // Add a note
        await AddOrderNoteAsync(order, $"Unable to partially refund order. {error}");

        // Log it
        var logError = $"Error refunding order #{order.Id}. Error: {error}";
        await _logger.InsertLogAsync(LogLevel.Error, logError, logError);
        return result.Errors;
    }

    /// <summary>
    /// Selected refunds an order (offline)
    /// </summary>
    /// <param name="order">Order</param>
    /// <param name="refundOrder">Refund Order</param>
    /// <param name="refundOrderItems">Refund Order Item</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task RefundSelectedOfflineAsync(Order order, RefundOrder refundOrder, List<RefundOrderItem> refundOrderItems)
    {
        ArgumentNullException.ThrowIfNull(order);

        if (!_orderProcessingService.CanPartiallyRefundOffline(order, refundOrder.OrderTotal))
            throw new NopException("You can't refund selected items (offline) this order");

        // Total amount refunded
        var totalAmountRefunded = order.RefundedAmount + refundOrder.OrderTotal;

        // Update order info
        order.RefundedAmount = totalAmountRefunded;
        // Mark payment status as 'Refunded' if the order total amount is fully refunded
        order.PaymentStatus = order.OrderTotal == totalAmountRefunded ? PaymentStatus.Refunded : PaymentStatus.PartiallyRefunded;
        await _orderService.UpdateOrderAsync(order);
        
        foreach (var item in refundOrderItems)
        {
            var product = await _productService.GetProductByIdAsync(item.ProductId);

            // Add a note
            await AddOrderNoteAsync(order, $"Order item {product.Name} has been partially refunded. Amount = {refundOrder.OrderTotal}");
        }

        // Check order status
        await _orderProcessingService.CheckOrderStatusAsync(order);

        // Notifications
        var orderRefundedStoreOwnerNotificationQueuedEmailIds = await _workflowMessageService.SendOrderRefundedStoreOwnerNotificationAsync(order, refundOrder.OrderTotal, _localizationSettings.DefaultAdminLanguageId);
        if (orderRefundedStoreOwnerNotificationQueuedEmailIds.Any())
            await AddOrderNoteAsync(order, $"\"Order refunded\" email (to store owner) has been queued. Queued email identifiers: {string.Join(", ", orderRefundedStoreOwnerNotificationQueuedEmailIds)}.");

        var orderRefundSelectedAttachmentFilePath = _taxJarSettings.AttachPdfInvoiceToOrderRefundSelectedEmail ? await PrintRefundOrderToPdfAsync(order, refundOrder, refundOrderItems) : null;
        var orderRefundSelectedAttachmentFileName = _taxJarSettings.AttachPdfInvoiceToOrderRefundSelectedEmail ? "refundorder.pdf" : null;
        var orderRefundedCustomerNotificationQueuedEmailIds = await SendOrderRefundedCustomerNotificationAsync(order, refundOrder, refundOrderItems, refundOrder.OrderTotal, order.CustomerLanguageId,
                    orderRefundSelectedAttachmentFilePath, orderRefundSelectedAttachmentFileName);
        if (orderRefundedCustomerNotificationQueuedEmailIds.Any())
            await AddOrderNoteAsync(order, $"\"Order refunded\" email (to customer) has been queued. Queued email identifiers: {string.Join(", ", orderRefundedCustomerNotificationQueuedEmailIds)}.");

        // Raise event       
        await _eventPublisher.PublishAsync(new OrderRefundedEvent(order, refundOrder.OrderTotal));
    }

    /// <summary>
    /// Resend invoice to customer
    /// </summary>
    /// <param name="order">Order</param>
    /// <param name="email">Email to resend invoice</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task ResendOrderInvoiceAsync(Order order, string email)
    {
        ArgumentNullException.ThrowIfNull(order);

        var orderPlacedAttachmentFilePath = await _pdfService.SaveOrderPdfToDiskAsync(order);
        var orderPlacedAttachmentFileName = "order.pdf";

        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var attachOrderId = await _settingService.GetSettingByKeyAsync<bool>("invoicepdfsettings.attachorderid", false, storeScope);
        var useOrderMask = await _settingService.GetSettingByKeyAsync<bool>("invoicepdfsettings.useordermask", false, storeScope);
        if (!string.IsNullOrEmpty(orderPlacedAttachmentFileName))
        {
            if (attachOrderId)
            {
                if (useOrderMask)
                    orderPlacedAttachmentFileName = $"order_{order.CustomOrderNumber}.pdf";
                else
                    orderPlacedAttachmentFileName = $"order_{order.Id}.pdf";
            }
        }

        var store = await _storeService.GetStoreByIdAsync(order.StoreId) ?? await _storeContext.GetCurrentStoreAsync();
        var languageId = await EnsureLanguageIsActiveAsync(order.CustomerLanguageId, store.Id);

        var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateSystemNames.ORDER_PLACED_CUSTOMER_NOTIFICATION, store.Id);
        if (!messageTemplates.Any())
            return;

        // Tokens
        var commonTokens = new List<Token>();
        await _messageTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId);
        await _messageTokenProvider.AddCustomerTokensAsync(commonTokens, order.CustomerId);

        await messageTemplates.SelectAwait(async messageTemplate =>
        {
            // Email account
            var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

            var tokens = new List<Token>(commonTokens);
            await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount, languageId);

            // Event notification
            await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

            var billingAddress = await _addressService.GetAddressByIdAsync(order.BillingAddressId);

            var toEmail = email;
            var toName = $"{billingAddress.FirstName} {billingAddress.LastName}";

            return await _workflowMessageService.SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName,
                orderPlacedAttachmentFilePath, orderPlacedAttachmentFileName);
        }).ToListAsync();
    }
    #endregion
}
