using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Tax;
using Nop.Plugin.Tax.TaxJar.Models;
using Nop.Plugin.Tax.TaxJar.Services;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Stores;
using Nop.Services.Vendors;
using Nop.Web.Areas.Admin.Models.Orders;

namespace Nop.Plugin.Tax.TaxJar.Factories;

/// <summary>
/// Factory for generating TaxJar model data related to orders and refund processing.
/// </summary>
public partial class TaxJarModelFactory : ITaxJarModelFactory
{
    #region Fields
    private readonly IWorkContext _workContext;
    private readonly IOrderService _orderService;
    private readonly ICurrencyService _currencyService;
    private readonly CurrencySettings _currencySettings;
    private readonly IPriceFormatter _priceFormatter;
    private readonly IProductService _productService;
    private readonly IVendorService _vendorService;
    private readonly IPictureService _pictureService;
    private readonly OrderSettings _orderSettings;
    private readonly TaxSettings _taxSettings;
    private readonly IProductAttributeParser _productAttributeParser;
    private readonly IPriceCalculationService _priceCalculationService;
    private readonly ICustomerService _customerService;
    private readonly ITaxJarService _taxJarService;
    private readonly IStoreService _storeService;
    #endregion

    #region Ctor
    public TaxJarModelFactory(
        IWorkContext workContext,
        IOrderService orderService,
        ICurrencyService currencyService,
        CurrencySettings currencySettings,
        IPriceFormatter priceFormatter,
        IProductService productService,
        IVendorService vendorService,
        IPictureService pictureService,
        OrderSettings orderSettings,
        TaxSettings taxSettings,
        IProductAttributeParser productAttributeParser,
        IPriceCalculationService priceCalculationService,
        ICustomerService customerService,
        ITaxJarService taxJarService,
        IStoreService storeService)
    {
        _workContext = workContext;
        _orderService = orderService;
        _currencyService = currencyService;
        _currencySettings = currencySettings;
        _priceFormatter = priceFormatter;
        _productService = productService;
        _vendorService = vendorService;
        _pictureService = pictureService;
        _orderSettings = orderSettings;
        _taxSettings = taxSettings;
        _productAttributeParser = productAttributeParser;
        _priceCalculationService = priceCalculationService;
        _customerService = customerService;
        _taxJarService = taxJarService;
        _storeService = storeService;
    }
    #endregion

    #region Utilities
    /// <summary>
    /// Prepares the associated item models for order items.
    /// </summary>
    /// <param name="item">Order item model to associate items with</param>
    /// <param name="attributeValue">Product attribute value for associated product</param>
    /// <param name="order">The order object</param>
    /// <param name="customer">The customer who placed the order</param>
    /// <param name="primaryStoreCurrency">Currency of the store</param>
    /// <param name="quantity">Quantity of the associated product</param>
    /// <param name="storeId">ID of the store</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    protected virtual async Task PrepareOrderItemAssociatedItemModelsAsync(
        RefundOrderModel.ItemsModel item,
        ProductAttributeValue attributeValue,
        Order order,
        Customer customer,
        Currency primaryStoreCurrency,
        int quantity,
        int storeId)
    {
        var associatedProduct = await _productService.GetProductByIdAsync(attributeValue.AssociatedProductId);
        if (associatedProduct == null)
            return;

        // Fetching store details and tax rates
        var store = await _storeService.GetStoreByIdAsync(storeId);

        // Calculating prices and discounts
        var taxRate = _taxJarService.CalculateRate(item.Item.UnitPriceInclTaxValue, item.Item.UnitPriceExclTaxValue);
        var (_, scUnitPrice, discountAmount, _) = await _priceCalculationService.GetFinalPriceAsync(associatedProduct, customer, store);
        var scUnitPriceInclTax = _taxJarService.CalculatePrice(scUnitPrice, taxRate, true);
        var scUnitPriceExclTax = scUnitPrice;
        var discountAmountInclTax = _taxJarService.CalculatePrice(discountAmount, taxRate, true);
        var discountAmountExclTax = discountAmount;
        var scSubTotalInclTax = scUnitPriceInclTax * quantity;
        var scSubTotalExclTax = scUnitPriceExclTax * quantity;

        // Populating the associated order item model with calculated values
        var orderItemAssociatedModel = new OrderItemModel
        {
            Id = attributeValue.Id,
            ProductId = attributeValue.AssociatedProductId,
            ProductName = associatedProduct.Name,
            Quantity = quantity,
            UnitPriceInclTaxValue = scUnitPriceInclTax,
            UnitPriceExclTaxValue = scUnitPriceExclTax,
            DiscountInclTaxValue = discountAmountInclTax,
            DiscountExclTaxValue = discountAmountExclTax,
            SubTotalInclTaxValue = scSubTotalInclTax,
            SubTotalExclTaxValue = scSubTotalExclTax,
            //fill in additional values (not existing in the entity)
            Sku = await _productService.FormatSkuAsync(associatedProduct, string.Empty),
            VendorName = (await _vendorService.GetVendorByIdAsync(associatedProduct.VendorId))?.Name
        };

        // Adding picture URL to the model
        var orderItemPicture = await _pictureService.GetProductPictureAsync(associatedProduct, string.Empty);
        (orderItemAssociatedModel.PictureThumbnailUrl, _) = await _pictureService.GetPictureUrlAsync(orderItemPicture, 75);

        // Formatting prices for display
        var languageId = (await _workContext.GetWorkingLanguageAsync()).Id;

        // Unit price formattings
        orderItemAssociatedModel.UnitPriceInclTax = await _priceFormatter
            .FormatOrderPriceAsync(scUnitPriceInclTax, order.CurrencyRate, order.CustomerCurrencyCode,
                                   _orderSettings.DisplayCustomerCurrencyOnOrders, primaryStoreCurrency, languageId,
                                   true, true);
        orderItemAssociatedModel.UnitPriceExclTax = await _priceFormatter
            .FormatOrderPriceAsync(scUnitPriceExclTax, order.CurrencyRate, order.CustomerCurrencyCode,
                                   _orderSettings.DisplayCustomerCurrencyOnOrders, primaryStoreCurrency, languageId,
                                   false, true);

        // Discount formatting
        orderItemAssociatedModel.DiscountInclTax = await _priceFormatter
            .FormatOrderPriceAsync(discountAmountInclTax, order.CurrencyRate, order.CustomerCurrencyCode,
                                   _orderSettings.DisplayCustomerCurrencyOnOrders, primaryStoreCurrency, languageId,
                                   true, true);
        orderItemAssociatedModel.DiscountExclTax = await _priceFormatter
            .FormatOrderPriceAsync(discountAmountExclTax, order.CurrencyRate, order.CustomerCurrencyCode,
                                   _orderSettings.DisplayCustomerCurrencyOnOrders, primaryStoreCurrency, languageId,
                                   false, true);

        // Subtotal formatting
        orderItemAssociatedModel.SubTotalInclTax = await _priceFormatter
            .FormatOrderPriceAsync(scSubTotalInclTax, order.CurrencyRate, order.CustomerCurrencyCode,
                                   _orderSettings.DisplayCustomerCurrencyOnOrders, primaryStoreCurrency, languageId,
                                   true, true);
        orderItemAssociatedModel.SubTotalExclTax = await _priceFormatter
            .FormatOrderPriceAsync(scSubTotalExclTax, order.CurrencyRate, order.CustomerCurrencyCode,
                                   _orderSettings.DisplayCustomerCurrencyOnOrders, primaryStoreCurrency, languageId,
                                   false, true);

        item.AssociatedItems.Add(orderItemAssociatedModel);
    }

    /// <summary>
    /// Prepares the main order item models for processing.
    /// </summary>
    /// <param name="models">List of order item models</param>
    /// <param name="order">The order object</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    protected virtual async Task PrepareOrderItemModelsAsync(IList<RefundOrderModel.ItemsModel> models, Order order)
    {
        ArgumentNullException.ThrowIfNull(models);
        ArgumentNullException.ThrowIfNull(order);

        // Fetching customer and store currency details
        var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);
        var primaryStoreCurrency = await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId);

        // Fetching order items
        var orderItems = await _orderService.GetOrderItemsAsync(order.Id, vendorId: (await _workContext.GetCurrentVendorAsync())?.Id ?? 0);
        foreach (var orderItem in orderItems)
        {
            var product = await _productService.GetProductByIdAsync(orderItem.ProductId);

            // Populate the order item model with data
            var orderItemModel = new OrderItemModel
            {
                Id = orderItem.Id,
                ProductId = orderItem.ProductId,
                ProductName = product.Name,
                Quantity = orderItem.Quantity,
                UnitPriceInclTaxValue = orderItem.UnitPriceInclTax,
                UnitPriceExclTaxValue = orderItem.UnitPriceExclTax,
                DiscountInclTaxValue = orderItem.DiscountAmountInclTax,
                DiscountExclTaxValue = orderItem.DiscountAmountExclTax,
                SubTotalInclTaxValue = orderItem.PriceInclTax,
                SubTotalExclTaxValue = orderItem.PriceExclTax,
                AttributeInfo = orderItem.AttributeDescription,
                //fill in additional values (not existing in the entity)
                Sku = await _productService.FormatSkuAsync(product, orderItem.AttributesXml),
                VendorName = (await _vendorService.GetVendorByIdAsync(product.VendorId))?.Name
            };

            // Fetching product image for display
            var orderItemPicture = await _pictureService.GetProductPictureAsync(product, orderItem.AttributesXml);
            (orderItemModel.PictureThumbnailUrl, _) = await _pictureService.GetPictureUrlAsync(orderItemPicture, 75);

            var languageId = (await _workContext.GetWorkingLanguageAsync()).Id;

            // Formatting the prices for display
            orderItemModel.UnitPriceInclTax = await _priceFormatter
                .FormatOrderPriceAsync(orderItem.UnitPriceInclTax, order.CurrencyRate, order.CustomerCurrencyCode,
                                       _orderSettings.DisplayCustomerCurrencyOnOrders, primaryStoreCurrency, languageId,
                                       true, true);
            orderItemModel.UnitPriceExclTax = await _priceFormatter
                .FormatOrderPriceAsync(orderItem.UnitPriceExclTax, order.CurrencyRate, order.CustomerCurrencyCode,
                                       _orderSettings.DisplayCustomerCurrencyOnOrders, primaryStoreCurrency, languageId,
                                       false, true);

            // Discount formatting
            orderItemModel.DiscountInclTax = await _priceFormatter
                .FormatOrderPriceAsync(orderItem.DiscountAmountInclTax, order.CurrencyRate, order.CustomerCurrencyCode,
                                       _orderSettings.DisplayCustomerCurrencyOnOrders, primaryStoreCurrency, languageId,
                                       true, true);
            orderItemModel.DiscountExclTax = await _priceFormatter
                .FormatOrderPriceAsync(orderItem.DiscountAmountExclTax, order.CurrencyRate, order.CustomerCurrencyCode,
                                       _orderSettings.DisplayCustomerCurrencyOnOrders, primaryStoreCurrency, languageId,
                                       false, true);

            // Subtotal formatting
            orderItemModel.SubTotalInclTax = await _priceFormatter
                .FormatOrderPriceAsync(orderItem.PriceInclTax, order.CurrencyRate, order.CustomerCurrencyCode,
                                       _orderSettings.DisplayCustomerCurrencyOnOrders, primaryStoreCurrency, languageId,
                                       true, true);
            orderItemModel.SubTotalExclTax = await _priceFormatter
                .FormatOrderPriceAsync(orderItem.PriceExclTax, order.CurrencyRate, order.CustomerCurrencyCode,
                                       _orderSettings.DisplayCustomerCurrencyOnOrders, primaryStoreCurrency, languageId,
                                       false, true);

            var item = new RefundOrderModel.ItemsModel() { Item = orderItemModel };

            // Check for associated products
            var attributeValues = (await _productAttributeParser.ParseProductAttributeValuesAsync(orderItem.AttributesXml))
                .Where(a => a.AttributeValueType == AttributeValueType.AssociatedToProduct);
            item.HasAssociatedProducts = attributeValues.Any();
            foreach (var attributeValue in attributeValues)
                await PrepareOrderItemAssociatedItemModelsAsync(item, attributeValue, order, customer,
                                                                primaryStoreCurrency,
                                                                orderItem.Quantity * attributeValue.Quantity,
                                                                order.StoreId);

            if (item.HasAssociatedProducts)
            {
                var associatedProductTotal = await _priceCalculationService.RoundPriceAsync(_taxSettings.PricesIncludeTax 
                    ? item.AssociatedItems.Sum(p => p.SubTotalInclTaxValue) - item.Item.DiscountInclTaxValue 
                    : item.AssociatedItems.Sum(p => p.SubTotalExclTaxValue) - item.Item.DiscountExclTaxValue, primaryStoreCurrency);
                
                var itemSubtotal = _taxSettings.PricesIncludeTax ? item.Item.SubTotalInclTaxValue : item.Item.SubTotalExclTaxValue;
                if (!associatedProductTotal.Equals(itemSubtotal))
                {
                    item.HasAmountMissMatch = true;
                    item.AmountMissMatch = await _priceFormatter
                        .FormatOrderPriceAsync(itemSubtotal - associatedProductTotal, order.CurrencyRate,
                                               order.CustomerCurrencyCode,
                                               _orderSettings.DisplayCustomerCurrencyOnOrders, primaryStoreCurrency,
                                               languageId, false, false);
                    item.AssociatedProductsTotal = await _priceFormatter
                        .FormatOrderPriceAsync(associatedProductTotal, order.CurrencyRate, order.CustomerCurrencyCode,
                                               _orderSettings.DisplayCustomerCurrencyOnOrders, primaryStoreCurrency,
                                               languageId, false, false);
                }
            }

            models.Add(item);
        }
    }

    /// <summary>
    /// Prepares totals and additional information for the refund order model.
    /// </summary>
    /// <param name="model">Refund order model</param>
    /// <param name="order">The order object</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    protected virtual async Task PrepareRefundOrderModelTotalsAsync(RefundOrderModel model, Order order)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(order);

        var primaryStoreCurrency = await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId);
        var languageId = (await _workContext.GetWorkingLanguageAsync()).Id;

        // Populate shipping and payment method fee details
        model.OrderShippingInclTaxValue = order.OrderShippingInclTax;
        model.OrderShippingExclTaxValue = order.OrderShippingExclTax;
        model.PaymentMethodAdditionalFeeInclTaxValue = order.PaymentMethodAdditionalFeeInclTax;
        model.PaymentMethodAdditionalFeeExclTaxValue = order.PaymentMethodAdditionalFeeExclTax;

        // Parse and display tax rates
        var taxRates = _orderService.ParseTaxRates(order, order.TaxRates);
        foreach (var tr in taxRates)
            model.TaxRates.Add(new RefundOrderModel.TaxRate { Rate = _priceFormatter.FormatTaxRate(tr.Key) });

        var displayTaxRates = _taxSettings.DisplayTaxRates && taxRates.Any();
        model.DisplayTaxRates = displayTaxRates;
        model.PricesIncludeTax = _taxSettings.PricesIncludeTax;

        // Total price of the order
        model.OrderTotal = await _priceFormatter
            .FormatOrderPriceAsync(order.OrderTotal, order.CurrencyRate, order.CustomerCurrencyCode,
                                   _orderSettings.DisplayCustomerCurrencyOnOrders, primaryStoreCurrency, languageId,
                                   null, false);
        model.OrderTotalValue = order.OrderTotal;
        model.OrderTotalDiscountValue = order.OrderDiscount;

        // Refund details
        model.PrimaryStoreCurrencyCode = primaryStoreCurrency?.CurrencyCode;
        model.MaxAmountToRefund = order.OrderTotal - order.RefundedAmount;
        if (order.RefundedAmount > decimal.Zero)
            model.RefundedAmount = await _priceFormatter.FormatPriceAsync(order.RefundedAmount, true, false);
    }
    #endregion

    #region Methods
    /// <summary>
    /// Prepares a refund order model asynchronously.
    /// </summary>
    /// <param name="model">Refund order model</param>
    /// <param name="order">The order object</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the refund order model
    /// </returns>
    public virtual async Task<RefundOrderModel> PrepareRefundOrderModelAsync(RefundOrderModel model, Order order)
    {
        if (order != null)
        {
            // Fill in model values from the entity
            model ??= new RefundOrderModel { Id = order.Id };
            model.CustomOrderNumber = order.CustomOrderNumber;

            // Prepare totals and order items
            await PrepareRefundOrderModelTotalsAsync(model, order);
            await PrepareOrderItemModelsAsync(model.Items, order);
        }

        // Set associated product flags and vendor login status
        model.HasAssociatedProducts = model.Items.Any(x => x.HasAssociatedProducts);
        model.IsLoggedInAsVendor = await _workContext.GetCurrentVendorAsync() != null;

        return model;
    }
    #endregion
}
