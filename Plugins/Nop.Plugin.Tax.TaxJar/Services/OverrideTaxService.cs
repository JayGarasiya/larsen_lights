using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Core.Events;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Logging;
using Nop.Services.Tax;
using Nop.Services.Localization;
using Nop.Services.Attributes;

namespace Nop.Plugin.Tax.TaxJar.Services;

/// <summary>
/// Tax Service override
/// </summary>
public class OverrideTaxService : TaxService
{
    #region Fields
    protected readonly IAttributeService<CustomerAttribute, CustomerAttributeValue> _customerAttributeService;
    protected readonly IAttributeParser<CustomerAttribute, CustomerAttributeValue> _customerAttributeParser;
    protected readonly TaxJarSettings _taxJarSettings;
    protected readonly ILocalizationService _localizationService;
    #endregion

    #region Ctor
    public OverrideTaxService(
        AddressSettings addressSettings,
        CustomerSettings customerSettings,
        IAddressService addressService,
        ICheckVatService checkVatService,
        ICountryService countryService,
        ICustomerService customerService,
        IEventPublisher eventPublisher,
        IGenericAttributeService genericAttributeService,
        IGeoLookupService geoLookupService,
        ILogger logger,
        IStateProvinceService stateProvinceService,
        IStoreContext storeContext,
        ITaxPluginManager taxPluginManager,
        IWebHelper webHelper,
        IWorkContext workContext,
        ShippingSettings shippingSettings,
        TaxSettings taxSettings,
        IAttributeService<CustomerAttribute, CustomerAttributeValue> customerAttributeService,
        IAttributeParser<CustomerAttribute, CustomerAttributeValue> customerAttributeParser,
        TaxJarSettings taxJarSettings,
        ILocalizationService localizationService) : base(
            addressSettings,
            customerSettings,
            addressService,
            checkVatService,
            countryService,
            customerService,
            eventPublisher,
            genericAttributeService,
            geoLookupService,
            logger,
            stateProvinceService,
            storeContext,
            taxPluginManager,
            webHelper,
            workContext,
            shippingSettings,
            taxSettings)
    {
        _customerAttributeService = customerAttributeService;
        _customerAttributeParser = customerAttributeParser;
        _taxJarSettings = taxJarSettings;
        _localizationService = localizationService;
    }
    #endregion

    #region Utilites 
    /// <summary>
    /// Gets a value indicating whether a customer is tax exempt
    /// </summary>
    /// <param name="customer">Customer</param>
    /// <param name="overrideAttributesXml">Overridden customer attributes in XML format; pass null to use CustomCustomerAttributes of customer</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains a value indicating whether a customer is tax exempt
    /// </returns>
    protected virtual async Task<(bool, string)> IsTaxExemptCustomerAsync(Customer customer, string overrideAttributesXml = "")
    {
        ArgumentNullException.ThrowIfNull(customer);

        var taxExamptTextValue = new List<int>();
        var taxExamptValue = new List<int>();
        var taxExamptValid = string.Empty;

        // Set already selected attributes
        var selectedAttributesXml = !string.IsNullOrEmpty(overrideAttributesXml) ? overrideAttributesXml : string.Empty;

        var customerAttributes = await _customerAttributeService.GetAllAttributesAsync();
        foreach (var attribute in customerAttributes)
        {
            switch (attribute.AttributeControlType)
            {
                case AttributeControlType.DropdownList:
                case AttributeControlType.RadioList:
                case AttributeControlType.Checkboxes:
                    {
                        if (!string.IsNullOrEmpty(selectedAttributesXml))
                        {
                            // Select new values
                            var selectedValues = await _customerAttributeParser.ParseAttributeValuesAsync(selectedAttributesXml);
                            foreach (var attributeValue in selectedValues.Where(sv => sv.AttributeId == attribute.Id))
                            {
                                if (_taxJarSettings.IsTaxExempt.Contains(attribute.Id) && !taxExamptValue.Contains(attribute.Id))
                                    taxExamptValue.Add(attribute.Id);
                            }
                        }
                    }
                    break;
                case AttributeControlType.TextBox:
                case AttributeControlType.MultilineTextbox:
                    {
                        if (!string.IsNullOrEmpty(selectedAttributesXml))
                        {
                            var enteredText = _customerAttributeParser.ParseValues(selectedAttributesXml, attribute.Id);
                            if (enteredText.Any() && _taxJarSettings.IsTaxExempt.Contains(attribute.Id) && !taxExamptTextValue.Contains(attribute.Id))
                            {
                                var errorStr = await _localizationService.GetResourceAsync("Plugins.Tax.TaxJar.TaxExempt.Invalid");
                                var attrLocStr = await _localizationService.GetLocalizedAsync(attribute, x => x.Name, (await _workContext.GetWorkingLanguageAsync()).Id);
                                taxExamptValid = enteredText.FirstOrDefault().Length != 0 && enteredText.FirstOrDefault().Length < 7 ? string.Format(errorStr, attrLocStr.Split('-').FirstOrDefault()) : string.Empty;
                                taxExamptTextValue.Add(attribute.Id);
                            }
                        }
                    }
                    break;
                case AttributeControlType.ColorSquares:
                case AttributeControlType.ImageSquares:
                case AttributeControlType.Datepicker:
                case AttributeControlType.FileUpload:
                case AttributeControlType.ReadonlyCheckboxes:
                default:
                    // Not supported attribute control types
                    break;
            }
        }

        if (taxExamptTextValue.Count != 0 && taxExamptValue.Count != 0 && string.IsNullOrEmpty(taxExamptValid))
            return (true, taxExamptValid);

        return (false, taxExamptValid);
    }
    #endregion

    #region Methods
    /// <summary>
    /// Gets a value indicating whether a product is tax exempt
    /// </summary>
    /// <param name="product">Product</param>
    /// <param name="customer">Customer</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains a value indicating whether a product is tax exempt
    /// </returns>
    public override async Task<bool> IsTaxExemptAsync(Product product, Customer customer)
    {
        if (customer != null)
        {
            if (customer.IsTaxExempt)
            {
                // Save customer attributes and mark as tax exampt or not.
                var (isTaxExempt, taxExamptValid) = await IsTaxExemptCustomerAsync(customer, customer.CustomCustomerAttributesXML);
                customer.IsTaxExempt = isTaxExempt;
                await _customerService.UpdateCustomerAsync(customer);
                
                return isTaxExempt;
            }

            if ((await _customerService.GetCustomerRolesAsync(customer)).Any(cr => cr.TaxExempt))
                return true;
        }

        if (product == null)
            return false;

        if (product.IsTaxExempt)
            return true;

        return false;
    }
    
    /// <summary>
    /// Gets shipping price
    /// </summary>
    /// <param name="price">Price</param>
    /// <param name="includingTax">A value indicating whether calculated price should include tax</param>
    /// <param name="customer">Customer</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the price. Tax rate
    /// </returns>
    public override async Task<(decimal price, decimal taxRate)> GetShippingPriceAsync(decimal price, bool includingTax, Customer customer)
    {
        var taxRate = decimal.Zero;

        if (!_taxSettings.ShippingIsTaxable)
            return (price, taxRate);

        // Check is state of shipping address on tax exclude
        if (_taxSettings.TaxBasedOn == TaxBasedOn.ShippingAddress && customer.ShippingAddressId != null)
        {
            var shippingAddress = await _customerService.GetCustomerShippingAddressAsync(customer);
            if (_taxJarSettings.ExcludeTaxOnStateProvinceIds.Contains(shippingAddress?.StateProvinceId ?? 0))
                return (price, taxRate);
        }

        var taxClassId = _taxSettings.ShippingTaxClassId;
        var priceIncludesTax = _taxSettings.ShippingPriceIncludesTax;

        return await GetProductPriceAsync(null, taxClassId, price, includingTax, customer, priceIncludesTax);
    }
    #endregion
}
