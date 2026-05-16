using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Plugin.Tax.TaxJar.Domain;
using Nop.Services.Attributes;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Logging;
using Nop.Services.Tax;
using Taxjar;

namespace Nop.Plugin.Tax.TaxJar.Services;

/// <summary>
/// Represents the manager that operates with requests to the TaxJar Tax Manager
/// </summary>
public class TaxJarTaxManager : IDisposable
{
    #region Fields
    private readonly TaxJarSettings _taxJarSettings;
    private readonly IAddressService _addressService;
    private readonly ICountryService _countryService;
    private readonly ICustomerService _customerService;
    private readonly IGenericAttributeService _genericAttributeService;
    private readonly ILogger _logger;
    private readonly IStateProvinceService _stateProvinceService;
    private readonly IStaticCacheManager _staticCacheManager;
    private readonly IWorkContext _workContext;
    private readonly ShippingSettings _shippingSettings;
    private readonly TaxSettings _taxSettings;
    private readonly IAttributeParser<CustomerAttribute, CustomerAttributeValue> _customerAttributeParser;
    private readonly IPriceCalculationService _priceCalculationService;
    private readonly TaxJarRequestLogService _taxJarRequestLogService;

    private TaxjarApi _serviceClient;
    private bool _disposed;
    #endregion

    #region Ctor
    public TaxJarTaxManager(
        TaxJarSettings taxJarSettings,
        IAddressService addressService,
        ICountryService countryService,
        ICustomerService customerService,
        IGenericAttributeService genericAttributeService,
        ILogger logger,
        IStateProvinceService stateProvinceService,
        IStaticCacheManager staticCacheManager,
        IWorkContext workContext,
        ShippingSettings shippingSettings,
        TaxSettings taxSettings,
        IAttributeParser<CustomerAttribute, CustomerAttributeValue> customerAttributeParser,
        IPriceCalculationService priceCalculationService,
        TaxJarRequestLogService taxJarRequestLogService)
    {
        _taxJarSettings = taxJarSettings;
        _addressService = addressService;
        _countryService = countryService;
        _customerService = customerService;
        _genericAttributeService = genericAttributeService;
        _logger = logger;
        _stateProvinceService = stateProvinceService;
        _staticCacheManager = staticCacheManager;
        _workContext = workContext;
        _shippingSettings = shippingSettings;
        _taxSettings = taxSettings;
        _customerAttributeParser = customerAttributeParser;
        _priceCalculationService = priceCalculationService;
        _taxJarRequestLogService = taxJarRequestLogService;
    }
    #endregion

    #region Properties

    /// <summary>
    /// Gets client that connects to Avalara services
    /// </summary>
    private TaxjarApi ServiceClient
    {
        get
        {
            if (_serviceClient == null)
            {
                // Create a client with credentials
                if (_taxJarSettings.UseSandbox)
                    _serviceClient = new TaxjarApi(_taxJarSettings.ApiToken, new { apiUrl = "https://api.sandbox.taxjar.com" });
                else
                    _serviceClient = new TaxjarApi(_taxJarSettings.ApiToken);
            }

            return _serviceClient;
        }
    }

    #endregion

    #region Utilities
    #region Common
    /// <summary>
    /// Check that tax provider is configured
    /// </summary>
    /// <returns>True if it's configured; otherwise false</returns>
    private bool IsConfigured()
    {
        return !string.IsNullOrEmpty(_taxJarSettings.ApiToken);
    }

    /// <summary>
    /// Handle function and get result
    /// </summary>
    /// <typeparam name="TResult">Result type</typeparam>
    /// <param name="function">Function</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the result
    /// </returns>
    private async Task<TResult> HandleFunctionAsync<TResult>(Func<Task<TResult>> function, bool recall = false)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();

        try
        {
            // Ensure that Avalara tax provider is configured
            if (!IsConfigured())
                throw new NopException("Tax provider is not configured");

            var response = await function();

            // Log request results
            await _taxJarRequestLogService.InsertTaxJarRequestLogAsync(new TaxJarRequestLogs
            {
                StatusCode = 200,
                Url = function.Method.Name.Replace("<", "").Substring(0, function.Method.Name.IndexOf('>') - 1),
                RequestMessage = JsonConvert.SerializeObject(function.Target),
                ResponseMessage = JsonConvert.SerializeObject(response).Replace(@"""<>4__this"":{},", ""),
                CustomerId = customer.Id,
                CreatedDateUtc = DateTime.UtcNow
            });

            return response;
        }
        catch (Exception exception)
        {
            // Recall method if unable to return rate
            if (recall)
                await HandleFunctionAsync(function);

            // Compose an error message
            var statusCode = 400;
            var errorMessage = exception.Message;
            if (exception is TaxjarException taxjarError && taxjarError.TaxjarError.Error != null)
            {
                statusCode = (int)taxjarError.HttpStatusCode;
                var errorInfo = taxjarError.TaxjarError;
                if (errorInfo != null)
                {
                    errorMessage = $"{errorInfo.StatusCode} - {errorInfo.Detail}{Environment.NewLine}";
                    if (!string.IsNullOrEmpty(errorInfo.Detail))
                        errorMessage = $"{errorMessage} Details: {errorInfo.Detail}";
                }
            }

            // Log errors
            await _logger.ErrorAsync($"{TaxJarDefaults.SystemName} error. {errorMessage}", exception, await _workContext.GetCurrentCustomerAsync());

            // Log request results
            await _taxJarRequestLogService.InsertTaxJarRequestLogAsync(new TaxJarRequestLogs
            {
                StatusCode = statusCode,
                Url = function.Method.Name.Replace("<", "").Substring(0, function.Method.Name.IndexOf('>') - 1),
                RequestMessage = JsonConvert.SerializeObject(function.Target),
                ResponseMessage = JsonConvert.SerializeObject(exception),
                CustomerId = customer.Id,
                CreatedDateUtc = DateTime.UtcNow
            });

            return default;
        }
    }
    #endregion

    #region Tax calculation
    /// <summary>
    /// Get a tax address of the passed order
    /// </summary>
    /// <param name="order">Order</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the address
    /// </returns>
    private async Task<Core.Domain.Common.Address> GetTaxAddressAsync(Core.Domain.Orders.Order order)
    {
        Core.Domain.Common.Address address = null;

        // Tax is based on billing address
        if (_taxSettings.TaxBasedOn == TaxBasedOn.BillingAddress
            && await _addressService.GetAddressByIdAsync(order.BillingAddressId) is Core.Domain.Common.Address billingAddress)
        {
            address = billingAddress;
        }

        // Tax is based on shipping address
        if (_taxSettings.TaxBasedOn == TaxBasedOn.ShippingAddress
            && order.ShippingAddressId.HasValue
            && await _addressService.GetAddressByIdAsync(order.ShippingAddressId.Value) is Core.Domain.Common.Address shippingAddress)
        {
            address = shippingAddress;
        }

        // Tax is based on pickup point address
        if (_taxSettings.TaxBasedOnPickupPointAddress
            && order.PickupAddressId.HasValue
            && await _addressService.GetAddressByIdAsync(order.PickupAddressId.Value) is Core.Domain.Common.Address pickupAddress)
        {
            address = pickupAddress;
        }

        // Or use default address for tax calculation
        address ??= await _addressService.GetAddressByIdAsync(_taxSettings.DefaultTaxAddressId);

        return address;
    }

    /// <summary>
    /// Map address model
    /// </summary>
    /// <param name="address">Address</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the address model
    /// </returns>
    private async Task<Address> MapAddressAsync(Core.Domain.Common.Address address)
    {
        var country = await _countryService.GetCountryByAddressAsync(address);
        var state = await _stateProvinceService.GetStateProvinceByAddressAsync(address);

        return await Task.FromResult(address == null ? default : new Address
        {
            Country = CommonHelper.EnsureMaximumLength(country?.TwoLetterIsoCode, 2),
            State = CommonHelper.EnsureMaximumLength(state?.Abbreviation, 2),
            Zip = CommonHelper.EnsureMaximumLength(address.ZipPostalCode, 11),
            City = CommonHelper.EnsureMaximumLength(address.City, 50),
            Street = CommonHelper.EnsureMaximumLength(address.Address1, 50)
        });
    }
    #endregion
    #endregion

    #region Methods
    #region Configuration
    /// <summary>
    /// Ping service (test conection)
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the ping result
    /// </returns>
    public async Task<List<Category>> PingAsync()
    {
        return await HandleFunctionAsync(() => Task.FromResult(ServiceClient.Categories()) ?? throw new NopException("No response from the service"));
    }
    #endregion

    #region Validation
    /// <summary>
    /// Resolve the passed address against Avalara's address-validation system
    /// </summary>
    /// <param name="address">Address to validate</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the validated address
    /// </returns>
    public async Task<List<Address>> ValidateAddressAsync(Core.Domain.Common.Address address)
    {
        if (address.CountryId == _taxJarSettings.CountryId)
        {
            var country = await _countryService.GetCountryByAddressAsync(address);
            var state = await _stateProvinceService.GetStateProvinceByAddressAsync(address);

            return (await HandleFunctionAsync(() => Task.FromResult(ServiceClient.ValidateAddress(new
            {
                country = CommonHelper.EnsureMaximumLength(country?.TwoLetterIsoCode, 2),
                state = CommonHelper.EnsureMaximumLength(state?.Abbreviation, 2),
                zip = CommonHelper.EnsureMaximumLength(address.ZipPostalCode, 11),
                city = CommonHelper.EnsureMaximumLength(address.City, 50),
                street = CommonHelper.EnsureMaximumLength(string.Format("{0}, {1}", address.Address1, address.Address2), 50),
            }) ?? throw new NopException("No response from the service"))));
        }

        return default;
    }
    #endregion

    #region Tax calculation
    /// <summary>
    /// Create test tax transaction
    /// </summary>
    /// <param name="address">Tax address</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the ransaction
    /// </returns>
    public async Task<TaxResponseAttributes> GetTaxRateAsync(Core.Domain.Common.Address address)
    {
        return await HandleFunctionAsync(async () =>
        {
            var originAddress = _taxJarSettings.TaxOriginAddressType switch
            {
                TaxOriginAddressType.ShippingOrigin => await _addressService.GetAddressByIdAsync(_shippingSettings.ShippingOriginAddressId),
                TaxOriginAddressType.DefaultTaxAddress => await _addressService.GetAddressByIdAsync(_taxSettings.DefaultTaxAddressId),
                _ => null
            };
            var fromAddress = await MapAddressAsync(originAddress);
            var toAddress = await MapAddressAsync(address);

            // Create tax transaction for a simplified item and without saving 
            var transaction = ServiceClient.TaxForOrder(new
            {
                from_country = fromAddress.Country,
                from_zip = fromAddress.Zip,
                from_city = fromAddress.City,
                from_state = fromAddress.State,
                to_country = toAddress.Country,
                to_zip = toAddress.Zip,
                to_state = toAddress.State,
                shipping = 0,
                line_items = new[] { new { unit_price = 100, discount = 0 } }
            }) ?? throw new NopException("No response from the service");

            return transaction;
        }, true);
    }

    /// <summary>
    /// Create transaction to get tax rate
    /// </summary>
    /// <param name="taxRateRequest">Tax rate request</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the ransaction
    /// </returns>
    public async Task<decimal?> GetTaxRateAsync(TaxRateRequest taxRateRequest)
    {
        // Prepare cache key
        var address = await _addressService.GetAddressByIdAsync(taxRateRequest.Address.Id);
        var customer = taxRateRequest.Customer ?? await _workContext.GetCurrentCustomerAsync();
        var taxCategoryId = taxRateRequest.TaxCategoryId > 0
            ? taxRateRequest.TaxCategoryId
            : taxRateRequest.Product?.TaxCategoryId ?? 0;
        var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(TaxJarDefaults.TaxRateCacheKey,
            customer,
            taxRateRequest.Address.Address1,
            taxRateRequest.Address.City,
            taxRateRequest.Address.StateProvinceId ?? 0,
            taxRateRequest.Address.CountryId ?? 0,
            taxRateRequest.Address.ZipPostalCode);
        if (_taxJarSettings.TaxRateByAddressCacheTime > 0)
            cacheKey.CacheTime = _taxJarSettings.TaxRateByAddressCacheTime;

        // Get tax rate
        return await _staticCacheManager.GetAsync(cacheKey, async () =>
        {
            // Create tax transaction for a single item and without saving
            var transaction = await GetTaxRateAsync(address) ?? default;

            // We return the tax total, since we used the amount of 100 when requesting, so the total is the same as the rate
            return transaction?.Rate * 100M;
        });
    }

    /// <summary>
    /// Create and Update Customer in taxjar
    /// </summary>
    /// <param name="customer">order customer details</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the ransaction
    /// </returns>
    public virtual async Task CreateOrUpdateTaxCustomerAsync(int customerId, int shippingAddressId)
    {
        var customer = await _customerService.GetCustomerByIdAsync(customerId) ?? throw new NopException("No customer avaliable");
        if(customer is null)
            return;

        await HandleFunctionAsync(async () =>
        {
            if (customer.IsTaxExempt)
            {
                var shippingAddress = await _addressService.GetAddressByIdAsync(shippingAddressId) ?? throw new NopException("No shipping address avaliable");
                var customerAttributeValues = await _customerAttributeParser.ParseAttributeValuesAsync(customer.CustomCustomerAttributesXML);

                var exemptionType = string.Empty;
                foreach (var attribute in customerAttributeValues)
                {
                    exemptionType = attribute.Name switch
                    {
                        "Agriculture" => "other",
                        "Resale" => "wholesale",
                        "Government" => "government",
                        _ => "non_exempt",
                    };
                }

                if (!string.IsNullOrEmpty(exemptionType))
                {
                    // Customer identifier
                    var customerId = customer.Id.ToString();
                    var customerFullName = await _customerService.GetCustomerFullNameAsync(customer);
                    if (string.IsNullOrEmpty(customerFullName))
                        customerFullName = string.Join(" ", new string[] { shippingAddress?.FirstName, shippingAddress?.LastName });
                    if (string.IsNullOrEmpty(customerFullName))
                        customerFullName = string.Format("Customer# {0}", customerId);

                    var taxjarCustomer = new Taxjar.Customer
                    {
                        CustomerId = customerId,
                        ExemptionType = exemptionType,
                        ExemptRegions = new List<ExemptRegion>(),
                        Name = customerFullName,
                        Zip = customer.ZipPostalCode,
                        Street = customer.StreetAddress,
                        City = customer.City,
                    };

                    var shippingcountry = await _countryService.GetCountryByIdAsync((int)shippingAddress?.CountryId) ?? throw new NopException("No country avaliable");
                    if (shippingcountry is not null)
                        taxjarCustomer.Country = shippingcountry.TwoLetterIsoCode;
                    
                    var shippingstate = await _stateProvinceService.GetStateProvinceByIdAsync((int)shippingAddress?.StateProvinceId) ?? throw new NopException("No state avaliable");
                    if (shippingstate is not null)
                        taxjarCustomer.State = shippingstate.Abbreviation;

                    if (string.IsNullOrEmpty(taxjarCustomer.Country))
                    {
                        var country = await _countryService.GetCountryByIdAsync(customer.CountryId) ?? throw new NopException("No country avaliable");
                        if (country is not null)
                            taxjarCustomer.Country = country.TwoLetterIsoCode;
                    }

                    if (string.IsNullOrEmpty(taxjarCustomer.State))
                    {
                        var state = await _stateProvinceService.GetStateProvinceByIdAsync(customer.StateProvinceId) ?? throw new NopException("No state avaliable");
                        if (state is not null)
                            taxjarCustomer.State = state.Abbreviation;
                    }

                    // Lists existing customers created through the API.
                    var taxjarCustomers = await ServiceClient.ListCustomersAsync();
                    
                    // If customer is already in taxjar and exemptionType is not euqle to current customer exemptionType update customer 
                    if (taxjarCustomers.Any(t => t.Equals(customerId)))
                    {
                        var existingCustomer = await ServiceClient.ShowCustomerAsync(customerId);
                        if(existingCustomer is not null && existingCustomer?.ExemptionType != exemptionType)
                            return Task.FromResult(await ServiceClient.UpdateCustomerAsync(taxjarCustomer) ?? throw new NopException("No response from the service"));
                    }

                    // Create customer in taxjar
                    if (!taxjarCustomers.Any(t => t.Equals(customerId)))
                        return Task.FromResult(await ServiceClient.CreateCustomerAsync(taxjarCustomer));
                }
            }

            return default;
        });
    }

    /// <summary>
    /// Create tax transaction for the placed order
    /// </summary>
    /// <param name="order">Order</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the ransaction
    /// </returns>
    public async Task CreateOrderTaxTransactionAsync(Core.Domain.Orders.Order order)
    {
        if (order == null)
            return;

        var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId) ?? throw new NopException("No customer avaliable");

        // Create or update customer in taxjar if is tax exempt
        if (customer.IsTaxExempt)
            await CreateOrUpdateTaxCustomerAsync(order.CustomerId, order.ShippingAddressId ?? customer.ShippingAddressId ?? 0);

        await HandleFunctionAsync(async () =>
        {
            // Prepare transaction model
            var originAddress = _taxJarSettings.TaxOriginAddressType switch
            {
                TaxOriginAddressType.ShippingOrigin => await _addressService.GetAddressByIdAsync(_shippingSettings.ShippingOriginAddressId),
                TaxOriginAddressType.DefaultTaxAddress => await _addressService.GetAddressByIdAsync(_taxSettings.DefaultTaxAddressId),
                _ => null
            };

            var fromAddress = await MapAddressAsync(originAddress);

            var address = await GetTaxAddressAsync(order);
            var toAddress = await MapAddressAsync(address);

            var subtotal = await _priceCalculationService.RoundPriceAsync(order.OrderSubtotalExclTax);
            var shippingamount = await _priceCalculationService.RoundPriceAsync(order.OrderShippingExclTax);
            var amount = decimal.Add(subtotal, shippingamount);
            var discountAmt = decimal.Subtract(decimal.Add(amount, order.OrderTax), order.OrderTotal);

            var orderSync = await ServiceClient.CreateOrderAsync(new
            {
                transaction_id = order.Id,
                transaction_date = order.PaidDateUtc.HasValue ? order.PaidDateUtc : order.CreatedOnUtc,
                from_country = fromAddress.Country,
                from_zip = fromAddress.Zip,
                from_city = fromAddress.City,
                from_state = fromAddress.State,
                to_country = toAddress.Country,
                to_zip = toAddress.Zip,
                to_state = toAddress.State,
                amount = await _priceCalculationService.RoundPriceAsync(decimal.Subtract(amount, discountAmt)),
                shipping = shippingamount,
                sales_tax = order.OrderTax,
                customer_id = order.CustomerId,
                line_items = new[] {
                  new { unit_price = await _priceCalculationService.RoundPriceAsync(subtotal), discount = discountAmt }
                }
            }) ?? throw new NopException("No response from the service");

            return orderSync;
        });
    }

    /// <summary>
    /// Delete tax transaction
    /// </summary>
    /// <param name="order">Order</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public async Task DeleteTaxTransactionAsync(Core.Domain.Orders.Order order)
    {
        var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId) ?? throw new NopException("No customer avaliable");

        await HandleFunctionAsync(async () =>
        {
            var deleteOrderSync = ServiceClient.DeleteOrder(order.Id.ToString()) ?? throw new NopException("No response from the service");
            var transaction = string.Empty;
            var refundTransactions = await _genericAttributeService.GetAttributeAsync<List<string>>(order, "OrderRefundTransaction") ?? new List<string>();
            foreach (var refundTransaction in refundTransactions)
                ServiceClient.DeleteRefund(refundTransaction);

            // Remove order refund transactions
            var removeValue = (await _genericAttributeService.GetAttributesForEntityAsync(order.Id, order.GetType().Name))
                .Where(x => x.Key.Equals("OrderRefundTransaction"))
                .FirstOrDefault();
            if (removeValue != null)
                await _genericAttributeService.DeleteAttributeAsync(removeValue);

            return Task.FromResult(deleteOrderSync);
        });
    }

    /// <summary>
    /// Refund tax transaction
    /// </summary>
    /// <param name="order">Order</param>
    /// <param name="amountToRefund">Amount to refund</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public async Task RefundTaxTransactionAsync(Core.Domain.Orders.Order order, decimal amountToRefund)
    {
        var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId) ?? throw new NopException("No customer avaliable");

        await HandleFunctionAsync(async () =>
        {
            var transactionId = string.Empty;

            // Whether it's a partial refund
            var isPartialRefund = !order.OrderTotal.Equals(amountToRefund);
            if (isPartialRefund)
            {
                var partiallyRefundCount = await _genericAttributeService.GetAttributeAsync<int>(order, "PartiallyRefundCount");
                if (partiallyRefundCount == 0)
                    await _genericAttributeService.SaveAttributeAsync(order, "PartiallyRefundCount", 1);

                var refundCount = partiallyRefundCount + 1;
                await _genericAttributeService.SaveAttributeAsync(order, "PartiallyRefundCount", refundCount);
                transactionId = $"Partially Refund_{order.Id}-{refundCount}";
            }

            var originAddress = _taxJarSettings.TaxOriginAddressType switch
            {
                TaxOriginAddressType.ShippingOrigin => await _addressService.GetAddressByIdAsync(_shippingSettings.ShippingOriginAddressId),
                TaxOriginAddressType.DefaultTaxAddress => await _addressService.GetAddressByIdAsync(_taxSettings.DefaultTaxAddressId),
                _ => null
            };
            var fromAddress = await MapAddressAsync(originAddress);

            var address = await GetTaxAddressAsync(order);
            var toAddress = await MapAddressAsync(address);

            var taxRef = decimal.Zero;
            var refundAmountDiff = decimal.Zero;
            
            if(order.OrderTotal.Equals(amountToRefund))
            {
                taxRef = order.OrderTax;
                refundAmountDiff = await _priceCalculationService.RoundPriceAsync(amountToRefund - taxRef);
            }
            else
            {
                taxRef = await _priceCalculationService.RoundPriceAsync(amountToRefund * order.OrderTax / order.OrderTotal);
                refundAmountDiff = await _priceCalculationService.RoundPriceAsync(amountToRefund - taxRef);
            }

            var refund = ServiceClient.CreateRefund(new
            {
                transaction_id = !string.IsNullOrEmpty(transactionId) ? transactionId : string.Format("Refund_{0}", order.Id),
                transaction_reference_id = order.Id,
                transaction_date = order.PaidDateUtc.HasValue ? order.PaidDateUtc : order.CreatedOnUtc,
                from_country = fromAddress.Country,
                from_zip = fromAddress.Zip,
                from_state = fromAddress.State,
                to_country = toAddress.Country,
                to_zip = toAddress.Zip,
                to_state = toAddress.State,
                amount = -refundAmountDiff,
                shipping = 0,
                sales_tax = -taxRef,
                line_items = new[] {
                    new {   description = string.Format("Order Id: {0}, Order Status: {1}",order.Id ,order.PaymentStatus),
                            discount = 0,
                            sales_tax = 0,
                            unit_price = -(refundAmountDiff)
                        }
                }
            }) ?? throw new NopException("No response from the service");

            // Track refund transaction to delete while order cancle or delete
            var refundTransactions = await _genericAttributeService.GetAttributeAsync<List<string>>(order, "OrderRefundTransaction") ?? new List<string>();
            refundTransactions.Add(refund.TransactionId);
            await _genericAttributeService.SaveAttributeAsync(order, "OrderRefundTransaction", refundTransactions);

            return Task.FromResult(refund);
        });
    }


    /// <summary>
    /// Re-Sync tax transaction for the existing order
    /// </summary>
    /// <param name="order">Order</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the ransaction
    /// </returns>
    public async Task ResyncOrderTaxTransactionAsync(Core.Domain.Orders.Order order)
    {
        // Delete already synced transaction
        await DeleteTaxTransactionAsync(order);

        // Create new transaction
        await CreateOrderTaxTransactionAsync(order);

        // Check if order is refunded then need to create refund
        if (order.RefundedAmount > 0)
            await RefundTaxTransactionAsync(order, order.RefundedAmount);
    }
    #endregion

    /// <summary>
    /// Dispose object
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    // Protected implementation of Dispose pattern.
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        _disposed = true;
    }
    #endregion
}