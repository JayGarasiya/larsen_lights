using Nop.Services.Orders;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Services.Shipping.Date;
using Nop.Services.Stores;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Services.Attributes;
using Nop.Core.Events;

namespace Nop.Plugin.Widgets.Fulfillment.Services.ShoppingCart
{
    /// <summary>
    /// override shopping cart service
    /// </summary>
    public partial class OverrideShoppingCartService : ShoppingCartService
    {
        #region Ctor
        public OverrideShoppingCartService(CatalogSettings catalogSettings, 
            IAclService aclService, 
            IActionContextAccessor actionContextAccessor, 
            IAttributeParser<CheckoutAttribute, CheckoutAttributeValue> checkoutAttributeParser, 
            IAttributeService<CheckoutAttribute, CheckoutAttributeValue> checkoutAttributeService, 
            ICurrencyService currencyService, 
            ICustomerService customerService, 
            IDateRangeService dateRangeService, 
            IDateTimeHelper dateTimeHelper, 
            IEventPublisher eventPublisher, 
            IGenericAttributeService genericAttributeService, 
            IGiftCardService giftCardService, 
            ILocalizationService localizationService, 
            IPermissionService permissionService, 
            IPriceCalculationService priceCalculationService, 
            IPriceFormatter priceFormatter, 
            IProductAttributeParser productAttributeParser, 
            IProductAttributeService productAttributeService, 
            IProductService productService, 
            IRepository<ShoppingCartItem> sciRepository, 
            IShippingService shippingService, 
            IShortTermCacheManager shortTermCacheManager, 
            IStaticCacheManager staticCacheManager, 
            IStoreContext storeContext, 
            IStoreService storeService, 
            IStoreMappingService storeMappingService, 
            IUrlHelperFactory urlHelperFactory, 
            IUrlRecordService urlRecordService, 
            IWorkContext workContext, 
            OrderSettings orderSettings, 
            ShoppingCartSettings shoppingCartSettings) : base(
                catalogSettings, 
                aclService, 
                actionContextAccessor, 
                checkoutAttributeParser, 
                checkoutAttributeService, 
                currencyService, 
                customerService, 
                dateRangeService, 
                dateTimeHelper, 
                eventPublisher, 
                genericAttributeService, 
                giftCardService, 
                localizationService, 
                permissionService, 
                priceCalculationService, 
                priceFormatter, 
                productAttributeParser, 
                productAttributeService, 
                productService, 
                sciRepository, 
                shippingService, 
                shortTermCacheManager, 
                staticCacheManager, 
                storeContext, 
                storeService, 
                storeMappingService, 
                urlHelperFactory, 
                urlRecordService, 
                workContext, 
                orderSettings, 
                shoppingCartSettings)
        {
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Validates a product for standard properties
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="shoppingCartType">Shopping cart type</param>
        /// <param name="product">Product</param>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <param name="customerEnteredPrice">Customer entered price</param>
        /// <param name="quantity">Quantity</param>
        /// <param name="shoppingCartItemId">Shopping cart identifier; pass 0 if it's a new item</param>
        /// <param name="storeId">Store identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the warnings
        /// </returns>
        protected override async Task<IList<string>> GetStandardWarningsAsync(Customer customer, ShoppingCartType shoppingCartType, Product product,
            string attributesXml, decimal customerEnteredPrice, int quantity, int shoppingCartItemId, int storeId)
        {
            ArgumentNullException.ThrowIfNull(customer);

            ArgumentNullException.ThrowIfNull(product);

            var warnings = new List<string>();

            //deleted
            if (product.Deleted)
            {
                warnings.Add(await _localizationService.GetResourceAsync("ShoppingCart.ProductDeleted"));
                return warnings;
            }

            //published
            if (!product.Published)
            {
                warnings.Add(await _localizationService.GetResourceAsync("ShoppingCart.ProductUnpublished"));
            }

            //we can add only simple products
            if (product.ProductType != ProductType.SimpleProduct)
            {
                warnings.Add("This is not simple product");
            }

            //ACL
            if (!await _aclService.AuthorizeAsync(product, customer))
            {
                warnings.Add(await _localizationService.GetResourceAsync("ShoppingCart.ProductUnpublished"));
            }

            //Store mapping
            if (!await _storeMappingService.AuthorizeAsync(product, storeId))
            {
                warnings.Add(await _localizationService.GetResourceAsync("ShoppingCart.ProductUnpublished"));
            }

            //disabled "add to cart" button
            if (shoppingCartType == ShoppingCartType.ShoppingCart && product.DisableBuyButton)
            {
                warnings.Add(await _localizationService.GetResourceAsync("ShoppingCart.BuyingDisabled"));
            }

            //disabled "add to wishlist" button
            if (shoppingCartType == ShoppingCartType.Wishlist && product.DisableWishlistButton)
            {
                warnings.Add(await _localizationService.GetResourceAsync("ShoppingCart.WishlistDisabled"));
            }

            //call for price
            if (shoppingCartType == ShoppingCartType.ShoppingCart && product.CallForPrice &&
                //also check whether the current user is impersonated
                (!_orderSettings.AllowAdminsToBuyCallForPriceProducts || _workContext.OriginalCustomerIfImpersonated == null))
            {
                warnings.Add(await _localizationService.GetResourceAsync("Products.CallForPrice"));
            }

            //customer entered price
            if (product.CustomerEntersPrice)
            {
                if (customerEnteredPrice < product.MinimumCustomerEnteredPrice ||
                    customerEnteredPrice > product.MaximumCustomerEnteredPrice)
                {
                    var currentCurrency = await _workContext.GetWorkingCurrencyAsync();
                    var minimumCustomerEnteredPrice = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(product.MinimumCustomerEnteredPrice, currentCurrency);
                    var maximumCustomerEnteredPrice = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(product.MaximumCustomerEnteredPrice, currentCurrency);
                    warnings.Add(string.Format(await _localizationService.GetResourceAsync("ShoppingCart.CustomerEnteredPrice.RangeError"),
                        await _priceFormatter.FormatPriceAsync(minimumCustomerEnteredPrice, false, false),
                        await _priceFormatter.FormatPriceAsync(maximumCustomerEnteredPrice, false, false)));
                }
            }

            //quantity validation
            var hasQtyWarnings = false;
            if (quantity < product.OrderMinimumQuantity)
            {
                warnings.Add(string.Format(await _localizationService.GetResourceAsync("ShoppingCart.MinimumQuantity"), product.OrderMinimumQuantity));
                hasQtyWarnings = true;
            }

            if (quantity > product.OrderMaximumQuantity)
            {
                warnings.Add(string.Format(await _localizationService.GetResourceAsync("ShoppingCart.MaximumQuantity"), product.OrderMaximumQuantity));
                hasQtyWarnings = true;
            }

            var allowedQuantities = _productService.ParseAllowedQuantities(product);
            if (allowedQuantities.Length > 0 && !allowedQuantities.Contains(quantity))
            {
                warnings.Add(string.Format(await _localizationService.GetResourceAsync("ShoppingCart.AllowedQuantities"), string.Join(", ", allowedQuantities)));
            }

            var validateOutOfStock = shoppingCartType == ShoppingCartType.ShoppingCart || !_shoppingCartSettings.AllowOutOfStockItemsToBeAddedToWishlist;
            if (validateOutOfStock && !hasQtyWarnings)
            {
                switch (product.ManageInventoryMethod)
                {
                    case ManageInventoryMethod.DontManageStock:
                        //do nothing
                        break;
                    case ManageInventoryMethod.ManageStock:
                        if (product.BackorderMode == BackorderMode.NoBackorders)
                        {
                            var maximumQuantityCanBeAdded = await _productService.GetTotalStockQuantityAsync(product);

                            //custom code
                            if (_workContext.OriginalCustomerIfImpersonated == null)
                            {
                                warnings.AddRange(await GetQuantityProductWarningsAsync(product, quantity, maximumQuantityCanBeAdded));
                                if (warnings.Any())
                                    return warnings;
                            }
                            else
                            {
                                //if customer is impersonat avoid attribute validation
                                var httpContext = _actionContextAccessor.ActionContext.HttpContext;
                                var controllerName = httpContext.GetRouteValue(NopRoutingDefaults.RouteValue.Controller).ToString();
                                var actionName = httpContext.GetRouteValue(NopRoutingDefaults.RouteValue.Action).ToString();

                                var impersonatedValidation = await _genericAttributeService.GetAttributeAsync<bool>(customer, "ImpersonatedValidation");

                                if ((controllerName.Equals("Product", StringComparison.InvariantCultureIgnoreCase) &&
                                    actionName.Equals("ProductDetails", StringComparison.InvariantCultureIgnoreCase)) || impersonatedValidation)
                                {
                                    warnings.AddRange(await GetQuantityProductWarningsAsync(product, quantity, maximumQuantityCanBeAdded));
                                    if (warnings.Any())
                                        return warnings;
                                }
                            }

                            //validate product quantity with non combinable product attributes
                            var productAttributeMappings = await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(product.Id);
                            if (productAttributeMappings?.Any() == true)
                            {
                                var onlyCombinableAttributes = productAttributeMappings.All(mapping => !mapping.IsNonCombinable());
                                if (!onlyCombinableAttributes)
                                {
                                    var cart = await GetShoppingCartAsync(customer, shoppingCartType, storeId);
                                    var totalAddedQuantity = cart
                                        .Where(item => item.ProductId == product.Id && item.Id != shoppingCartItemId)
                                        .Sum(product => product.Quantity);

                                    totalAddedQuantity += quantity;

                                    //counting a product into bundles
                                    foreach (var bundle in cart.Where(x => x.Id != shoppingCartItemId && !string.IsNullOrEmpty(x.AttributesXml)))
                                    {
                                        var attributeValues = await _productAttributeParser.ParseProductAttributeValuesAsync(bundle.AttributesXml);
                                        foreach (var attributeValue in attributeValues)
                                        {
                                            if (attributeValue.AttributeValueType == AttributeValueType.AssociatedToProduct && attributeValue.AssociatedProductId == product.Id)
                                                totalAddedQuantity += bundle.Quantity * attributeValue.Quantity;
                                        }
                                    }

                                    warnings.AddRange(await GetQuantityProductWarningsAsync(product, totalAddedQuantity, maximumQuantityCanBeAdded));
                                }
                            }

                            if (warnings.Any())
                                return warnings;

                            //validate product quantity and product quantity into bundles
                            if (string.IsNullOrEmpty(attributesXml))
                            {
                                var cart = await GetShoppingCartAsync(customer, shoppingCartType, storeId);
                                var totalQuantityInCart = cart.Where(item => item.ProductId == product.Id && item.Id != shoppingCartItemId && string.IsNullOrEmpty(item.AttributesXml))
                                    .Sum(product => product.Quantity);

                                totalQuantityInCart += quantity;

                                foreach (var bundle in cart.Where(x => x.Id != shoppingCartItemId && !string.IsNullOrEmpty(x.AttributesXml)))
                                {
                                    var attributeValues = await _productAttributeParser.ParseProductAttributeValuesAsync(bundle.AttributesXml);
                                    foreach (var attributeValue in attributeValues)
                                    {
                                        if (attributeValue.AttributeValueType == AttributeValueType.AssociatedToProduct && attributeValue.AssociatedProductId == product.Id)
                                            totalQuantityInCart += bundle.Quantity * attributeValue.Quantity;
                                    }
                                }

                                //custom code
                                if (_workContext.OriginalCustomerIfImpersonated == null)
                                    warnings.AddRange(await GetQuantityProductWarningsAsync(product, totalQuantityInCart, maximumQuantityCanBeAdded));
                            }
                        }

                        break;
                    case ManageInventoryMethod.ManageStockByAttributes:
                        var combination = await _productAttributeParser.FindProductAttributeCombinationAsync(product, attributesXml);
                        if (combination != null)
                        {
                            //combination exists
                            //let's check stock level
                            if (!combination.AllowOutOfStockOrders)
                                warnings.AddRange(await GetQuantityProductWarningsAsync(product, quantity, combination.StockQuantity));
                        }
                        else
                        {
                            //combination doesn't exist
                            if (product.AllowAddingOnlyExistingAttributeCombinations)
                            {
                                //maybe, is it better  to display something like "No such product/combination" message?
                                var productAvailabilityRange = await _dateRangeService.GetProductAvailabilityRangeByIdAsync(product.ProductAvailabilityRangeId);
                                var warning = productAvailabilityRange == null ? await _localizationService.GetResourceAsync("ShoppingCart.OutOfStock")
                                    : string.Format(await _localizationService.GetResourceAsync("ShoppingCart.AvailabilityRange"),
                                        await _localizationService.GetLocalizedAsync(productAvailabilityRange, range => range.Name));
                                warnings.Add(warning);
                            }
                        }

                        break;
                    default:
                        break;
                }
            }

            //availability dates
            var availableStartDateError = false;
            if (product.AvailableStartDateTimeUtc.HasValue)
            {
                var availableStartDateTime = DateTime.SpecifyKind(product.AvailableStartDateTimeUtc.Value, DateTimeKind.Utc);
                if (availableStartDateTime.CompareTo(DateTime.UtcNow) > 0)
                {
                    warnings.Add(await _localizationService.GetResourceAsync("ShoppingCart.NotAvailable"));
                    availableStartDateError = true;
                }
            }

            if (product.AgeVerification && product.MinimumAgeToPurchase > 0)
            {
                if (!customer.DateOfBirth.HasValue)
                    warnings.Add(await _localizationService.GetResourceAsync("ShoppingCart.DateOfBirthRequired"));
                else if (CommonHelper.GetDifferenceInYears(customer.DateOfBirth.Value, DateTime.Today) < product.MinimumAgeToPurchase)
                    warnings.Add(string.Format(await _localizationService.GetResourceAsync("ShoppingCart.MinimumAgeToPurchase"), product.MinimumAgeToPurchase));
            }

            if (!product.AvailableEndDateTimeUtc.HasValue || availableStartDateError)
                return warnings;

            var availableEndDateTime = DateTime.SpecifyKind(product.AvailableEndDateTimeUtc.Value, DateTimeKind.Utc);
            if (availableEndDateTime.CompareTo(DateTime.UtcNow) < 0)
            {
                warnings.Add(await _localizationService.GetResourceAsync("ShoppingCart.NotAvailable"));
            }

            return warnings;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Validates shopping cart item attributes
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="shoppingCartType">Shopping cart type</param>
        /// <param name="product">Product</param>
        /// <param name="quantity">Quantity</param>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <param name="ignoreNonCombinableAttributes">A value indicating whether we should ignore non-combinable attributes</param>
        /// <param name="ignoreConditionMet">A value indicating whether we should ignore filtering by "is condition met" property</param>
        /// <param name="ignoreBundledProducts">A value indicating whether we should ignore bundled (associated) products</param>
        /// <param name="shoppingCartItemId">Shopping cart identifier; pass 0 if it's a new item</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the warnings
        /// </returns>
        public override async Task<IList<string>> GetShoppingCartItemAttributeWarningsAsync(Customer customer,
            ShoppingCartType shoppingCartType,
            Product product,
            int quantity = 1,
            string attributesXml = "",
            bool ignoreNonCombinableAttributes = false,
            bool ignoreConditionMet = false,
            bool ignoreBundledProducts = false,
            int shoppingCartItemId = 0)
        {
            ArgumentNullException.ThrowIfNull(product);

            var warnings = new List<string>();

            //ensure it's our attributes
            var attributes1 = await _productAttributeParser.ParseProductAttributeMappingsAsync(attributesXml);
            if (ignoreNonCombinableAttributes)
            {
                attributes1 = attributes1.Where(x => !x.IsNonCombinable()).ToList();
            }

            foreach (var attribute in attributes1)
            {
                if (attribute.ProductId == 0)
                {
                    warnings.Add("Attribute error");
                    return warnings;
                }

                if (attribute.ProductId != product.Id)
                {
                    warnings.Add("Attribute error");
                }
            }

            //validate required product attributes (whether they're chosen/selected/entered)
            var attributes2 = await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(product.Id);
            if (ignoreNonCombinableAttributes)
            {
                attributes2 = attributes2.Where(x => !x.IsNonCombinable()).ToList();
            }

            //validate conditional attributes only (if specified)
            if (!ignoreConditionMet)
            {
                attributes2 = await attributes2.WhereAwait(async x =>
                {
                    var conditionMet = await _productAttributeParser.IsConditionMetAsync(x, attributesXml);
                    return !conditionMet.HasValue || conditionMet.Value;
                }).ToListAsync();
            }

            foreach (var a2 in attributes2)
            {
                var productAttributeValues = await _productAttributeService.GetProductAttributeValuesAsync(a2.Id);

                if (a2.IsRequired)
                {
                    var found = false;
                    //selected product attributes
                    foreach (var a1 in attributes1)
                    {
                        if (a1.Id != a2.Id)
                            continue;

                        var attributeValuesStr = _productAttributeParser.ParseValues(attributesXml, a1.Id);

                        if (a2.ShouldHaveValues() && productAttributeValues.Any() && !productAttributeValues.Any(x => attributeValuesStr.Contains(x.Id.ToString())))
                            break;

                        foreach (var str1 in attributeValuesStr)
                        {
                            if (string.IsNullOrEmpty(str1.Trim()))
                                continue;

                            found = true;
                            break;
                        }
                    }

                    //if not found
                    if (!found)
                    {
                        var productAttribute = await _productAttributeService.GetProductAttributeByIdAsync(a2.ProductAttributeId);

                        var textPrompt = await _localizationService.GetLocalizedAsync(a2, x => x.TextPrompt);
                        var notFoundWarning = !string.IsNullOrEmpty(textPrompt) ?
                            textPrompt :
                            string.Format(await _localizationService.GetResourceAsync("ShoppingCart.SelectAttribute"), await _localizationService.GetLocalizedAsync(productAttribute, a => a.Name));

                        warnings.Add(notFoundWarning);
                    }
                }

                if (a2.AttributeControlType != AttributeControlType.ReadonlyCheckboxes)
                    continue;

                //customers cannot edit read-only attributes
                var allowedReadOnlyValueIds = productAttributeValues
                    .Where(x => x.IsPreSelected)
                    .Select(x => x.Id)
                    .ToArray();

                var selectedReadOnlyValueIds = (await _productAttributeParser.ParseProductAttributeValuesAsync(attributesXml))
                    .Where(x => x.ProductAttributeMappingId == a2.Id)
                    .Select(x => x.Id)
                    .ToArray();

                if (!CommonHelper.ArraysEqual(allowedReadOnlyValueIds, selectedReadOnlyValueIds))
                {
                    warnings.Add("You cannot change read-only values");
                }
            }

            //validation rules
            foreach (var pam in attributes2)
            {
                if (!pam.ValidationRulesAllowed())
                    continue;

                string enteredText;
                int enteredTextLength;

                var productAttribute = await _productAttributeService.GetProductAttributeByIdAsync(pam.ProductAttributeId);

                //minimum length
                if (pam.ValidationMinLength.HasValue)
                {
                    if (pam.AttributeControlType == AttributeControlType.TextBox ||
                        pam.AttributeControlType == AttributeControlType.MultilineTextbox)
                    {
                        enteredText = _productAttributeParser.ParseValues(attributesXml, pam.Id).FirstOrDefault();
                        enteredTextLength = string.IsNullOrEmpty(enteredText) ? 0 : enteredText.Length;

                        if (pam.ValidationMinLength.Value > enteredTextLength)
                        {
                            warnings.Add(string.Format(await _localizationService.GetResourceAsync("ShoppingCart.TextboxMinimumLength"), await _localizationService.GetLocalizedAsync(productAttribute, a => a.Name), pam.ValidationMinLength.Value));
                        }
                    }
                }

                //maximum length
                if (!pam.ValidationMaxLength.HasValue)
                    continue;

                if (pam.AttributeControlType != AttributeControlType.TextBox && pam.AttributeControlType != AttributeControlType.MultilineTextbox)
                    continue;

                enteredText = _productAttributeParser.ParseValues(attributesXml, pam.Id).FirstOrDefault();
                enteredTextLength = string.IsNullOrEmpty(enteredText) ? 0 : enteredText.Length;

                if (pam.ValidationMaxLength.Value < enteredTextLength)
                {
                    warnings.Add(string.Format(await _localizationService.GetResourceAsync("ShoppingCart.TextboxMaximumLength"), await _localizationService.GetLocalizedAsync(productAttribute, a => a.Name), pam.ValidationMaxLength.Value));
                }
            }

            if (warnings.Any() || ignoreBundledProducts)
                return warnings;

            //validate bundled products
            var attributeValues = await _productAttributeParser.ParseProductAttributeValuesAsync(attributesXml);
            foreach (var attributeValue in attributeValues)
            {
                if (attributeValue.AttributeValueType != AttributeValueType.AssociatedToProduct)
                    continue;

                var productAttributeMapping = await _productAttributeService.GetProductAttributeMappingByIdAsync(attributeValue.ProductAttributeMappingId);

                if (productAttributeMapping == null)
                    continue;

                if (ignoreNonCombinableAttributes && productAttributeMapping.IsNonCombinable())
                    continue;

                //custom code
                //if customer is impersonat avoid attribute validation
                if (_workContext.OriginalCustomerIfImpersonated != null)
                {
                    var impersonatedValidation = await _genericAttributeService.GetAttributeAsync<bool>(customer, "ImpersonatedValidation");
                    if (!impersonatedValidation)
                        continue;
                }

                //associated product (bundle)
                var associatedProduct = await _productService.GetProductByIdAsync(attributeValue.AssociatedProductId);
                if (associatedProduct != null)
                {
                    var store = await _storeContext.GetCurrentStoreAsync();
                    var totalQty = quantity * attributeValue.Quantity;
                    var associatedProductWarnings = await GetShoppingCartItemWarningsAsync(customer,
                        shoppingCartType, associatedProduct, store.Id,
                        string.Empty, decimal.Zero, null, null, totalQty, false, shoppingCartItemId);

                    var productAttribute = await _productAttributeService.GetProductAttributeByIdAsync(productAttributeMapping.ProductAttributeId);

                    foreach (var associatedProductWarning in associatedProductWarnings)
                    {
                        var attributeName = await _localizationService.GetLocalizedAsync(productAttribute, a => a.Name);
                        var attributeValueName = await _localizationService.GetLocalizedAsync(attributeValue, a => a.Name);
                        warnings.Add(string.Format(
                            await _localizationService.GetResourceAsync("ShoppingCart.AssociatedAttributeWarning"),
                            attributeName, attributeValueName, associatedProductWarning));
                    }
                }
                else
                    warnings.Add($"Associated product cannot be loaded - {attributeValue.AssociatedProductId}");
            }

            return warnings;
        }

        #endregion
    }
}
