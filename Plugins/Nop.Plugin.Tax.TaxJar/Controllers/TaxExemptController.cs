using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Services.Attributes;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Web.Components;
using Nop.Web.Controllers;

namespace Nop.Plugin.Tax.TaxJar.Controllers;

public class TaxExemptController : BasePublicController
{
    #region Fields
    private readonly IWorkContext _workContext;
    private readonly ICustomerService _customerService;
    private readonly IAttributeParser<CustomerAttribute, CustomerAttributeValue> _customerAttributeParser;
    private readonly IAttributeService<CustomerAttribute, CustomerAttributeValue> _customerAttributeService;
    private readonly TaxJarSettings _taxJarSettings;
    private readonly ILocalizationService _localizationService;
    #endregion

    #region Ctor
    public TaxExemptController(
        IWorkContext workContext,
        ICustomerService customerService,
        IAttributeService<CustomerAttribute, CustomerAttributeValue> customerAttributeService,
        IAttributeParser<CustomerAttribute, CustomerAttributeValue> customerAttributeParser,
        TaxJarSettings taxJarSettings,
        ILocalizationService localizationService)
    {
        _workContext = workContext;
        _customerService = customerService;
        _customerAttributeService = customerAttributeService;
        _customerAttributeParser = customerAttributeParser;
        _taxJarSettings = taxJarSettings;
        _localizationService = localizationService;
    }
    #endregion

    #region Utilities
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

        //set already selected attributes
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
                            //select new values
                            var selectedValues = await _customerAttributeParser.ParseAttributeValuesAsync(selectedAttributesXml);
                            foreach (var attributeValue in selectedValues.Where(sv => sv.AttributeId == attribute.Id))
                                if (_taxJarSettings.IsTaxExempt.Contains(attribute.Id) && !taxExamptValue.Contains(attribute.Id))
                                    taxExamptValue.Add(attribute.Id);
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
                    //not supported attribute control types
                    break;
            }
        }

        if (taxExamptTextValue.Count != 0 && taxExamptValue.Count != 0 && string.IsNullOrEmpty(taxExamptValid))
            return (true, taxExamptValid);

        return (false, taxExamptValid);
    }

    /// <summary>
    /// Get custom customer attributes from the passed form
    /// </summary>
    /// <param name="form">Form values</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the attributes in XML format
    /// </returns>
    protected virtual async Task<string> ParseCustomCustomerAttributesAsync(IFormCollection form)
    {
        ArgumentNullException.ThrowIfNull(form);

        var attributesXml = "";
        var attributes = await _customerAttributeService.GetAllAttributesAsync();
        foreach (var attribute in attributes)
        {
            var controlId = $"{NopCustomerServicesDefaults.CustomerAttributePrefix}{attribute.Id}";
            switch (attribute.AttributeControlType)
            {
                case AttributeControlType.DropdownList:
                case AttributeControlType.RadioList:
                    {
                        var ctrlAttributes = form[controlId];
                        if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                        {
                            var selectedAttributeId = int.Parse(ctrlAttributes);
                            if (selectedAttributeId > 0)
                                attributesXml = _customerAttributeParser.AddAttribute(attributesXml,
                                    attribute, selectedAttributeId.ToString());
                        }
                    }
                    break;
                case AttributeControlType.Checkboxes:
                    {
                        var cblAttributes = form[controlId];
                        if (!StringValues.IsNullOrEmpty(cblAttributes))
                        {
                            foreach (var item in cblAttributes.ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                            {
                                var selectedAttributeId = int.Parse(item);
                                if (selectedAttributeId > 0)
                                    attributesXml = _customerAttributeParser.AddAttribute(attributesXml,
                                        attribute, selectedAttributeId.ToString());
                            }
                        }
                    }
                    break;
                case AttributeControlType.ReadonlyCheckboxes:
                    {
                        //load read-only (already server-side selected) values
                        var attributeValues = await _customerAttributeService.GetAttributeValuesAsync(attribute.Id);
                        foreach (var selectedAttributeId in attributeValues
                            .Where(v => v.IsPreSelected)
                            .Select(v => v.Id)
                            .ToList())
                        {
                            attributesXml = _customerAttributeParser.AddAttribute(attributesXml,
                                attribute, selectedAttributeId.ToString());
                        }
                    }
                    break;
                case AttributeControlType.TextBox:
                case AttributeControlType.MultilineTextbox:
                    {
                        var ctrlAttributes = form[controlId];
                        if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                        {
                            var enteredText = ctrlAttributes.ToString().Trim();
                            attributesXml = _customerAttributeParser.AddAttribute(attributesXml,
                                attribute, enteredText);
                        }
                    }
                    break;
                case AttributeControlType.Datepicker:
                case AttributeControlType.ColorSquares:
                case AttributeControlType.ImageSquares:
                case AttributeControlType.FileUpload:
                //not supported customer attributes
                default:
                    break;
            }
        }

        return attributesXml;
    }
    #endregion

    #region Methods
    [HttpPost]
    public async Task<IActionResult> TaxExemptDetalis(IFormCollection form)
    {
        ArgumentNullException.ThrowIfNull(form);

        var customer = await _workContext.GetCurrentCustomerAsync();
        var customerAttributesXml = await ParseCustomCustomerAttributesAsync(form);

        //save customer attributes and mark as tax exampt or not.
        var (isTaxExempt, taxExamptValid) = await IsTaxExemptCustomerAsync(customer, customerAttributesXml);
        customer.IsTaxExempt = isTaxExempt;
        customer.CustomCustomerAttributesXML = customerAttributesXml;
        await _customerService.UpdateCustomerAsync(customer);

        var ordertotalsectionhtml = await RenderViewComponentToStringAsync(typeof(OrderTotalsViewComponent), new { isEditable = false });

        //nothing to return
        return Json(new
        {
            isTaxexempt = customer.IsTaxExempt,
            taxExamptValid,
            ordertotalsectionhtml
        });
    }
    #endregion
}
