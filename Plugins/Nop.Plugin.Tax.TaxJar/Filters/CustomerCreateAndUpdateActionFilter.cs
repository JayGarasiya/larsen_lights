using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Data;
using Nop.Services.Attributes;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Tax;
using Nop.Web.Areas.Admin.Models.Customers;
using Nop.Web.Controllers;
using Nop.Web.Framework.Models;
using System.Net;

namespace Nop.Plugin.Tax.TaxJar.Filters;

public class CustomerCreateAndUpdateActionFilter : IAsyncActionFilter
{
    #region Fields
    private readonly TaxJarSettings _taxJarSettings;
    private readonly ITaxPluginManager _taxPluginManager;
    private readonly ICustomerService _customerService;
    private readonly IWorkContext _workContext;
    private readonly IAttributeService<CustomerAttribute, CustomerAttributeValue> _customerAttributeService;
    private readonly IAttributeParser<CustomerAttribute, CustomerAttributeValue> _customerAttributeParser;
    private readonly ILocalizationService _localizationService;
    #endregion

    #region Ctor
    public CustomerCreateAndUpdateActionFilter(
        TaxJarSettings taxJarSettings,
        ITaxPluginManager taxPluginManager,
        ICustomerService customerService,
        IWorkContext workContext,
        IAttributeService<CustomerAttribute, CustomerAttributeValue> customerAttributeService,
        IAttributeParser<CustomerAttribute, CustomerAttributeValue> customerAttributeParser,
        ILocalizationService localizationService)
    {
        _taxJarSettings = taxJarSettings;
        _taxPluginManager = taxPluginManager;
        _customerService = customerService;
        _workContext = workContext;
        _customerAttributeService = customerAttributeService;
        _customerAttributeParser = customerAttributeParser;
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
    private async Task<(bool, string)> IsTaxExemptCustomerAsync(Customer customer, string overrideAttributesXml = "")
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
                            if (enteredText.Count != 0
                                && _taxJarSettings.IsTaxExempt.Contains(attribute.Id)
                                && !taxExamptTextValue.Contains(attribute.Id))
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

    /// <summary>
    /// Get custom customer attributes from the passed form
    /// </summary>
    /// <param name="form">Form values</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the attributes in XML format
    /// </returns>
    private async Task<string> ParseCustomCustomerAttributesAsync(IFormCollection form)
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
                        // Load read-only (already server-side selected) values
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
                // Not supported customer attributes
                default:
                    break;
            }
        }

        return attributesXml;
    }

    /// <summary>
    /// Called asynchronously before the action, after model binding is complete.
    /// </summary>
    /// <param name="context">A context for action filters</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    private async Task CheckProductSetupWithOpticalAttributesAsync(ActionExecutingContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.HttpContext.Request == null)
            return;

        if (!DataSettingsManager.IsDatabaseInstalled())
            return;

        // Ensure that taxjar tax provider is active
        if (!await _taxPluginManager.IsPluginActiveAsync(TaxJarDefaults.SystemName))
            return;

        // Ensure that in configure setup tax exempt attributes
        if (_taxJarSettings.IsTaxExempt.Count == 0)
            return;

        // Only in Post requests
        if (!context.HttpContext.Request.Method.Equals(WebRequestMethods.Http.Post, StringComparison.InvariantCultureIgnoreCase))
            return;

        // Get action and controller names
        var actionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;
        var actionName = actionDescriptor?.ActionName;
        var controllerName = actionDescriptor?.ControllerName;
        if (string.IsNullOrEmpty(actionName) || string.IsNullOrEmpty(controllerName))
            return;

        var areaExist = actionDescriptor.RouteValues.TryGetValue("area", out string value)
                        && (value?.ToString() ?? string.Empty).Equals("Admin", StringComparison.InvariantCultureIgnoreCase);
        if (areaExist)
        {
            var isvalid = actionDescriptor.ControllerTypeInfo == typeof(Nop.Web.Areas.Admin.Controllers.CustomerController) && (actionName == "Create" || actionName == "Edit");
            if (!isvalid)
                return;
        }
        else
        {
            if (actionDescriptor.ControllerTypeInfo != typeof(CustomerController) && actionName != "Info")
                return;
        }

        if (!context.ModelState.IsValid)
            return;

        // Custom customer attributes
        var form = context.HttpContext.Request.Form;
        var customer = await _workContext.GetCurrentCustomerAsync();
        var customerAttributesXml = await ParseCustomCustomerAttributesAsync(form);

        var (isTaxExempt, taxExamptValid) = await IsTaxExemptCustomerAsync(customer, customerAttributesXml);
        if (isTaxExempt)
        {
            if (areaExist)
            {
                var model = context.ActionArguments.Values.OfType<BaseNopModel>().FirstOrDefault() as CustomerModel;
                if (model != null)
                    model.IsTaxExempt = true;
            }
            else
                customer.IsTaxExempt = true;
        }
        else
            customer.IsTaxExempt = false;

        await _customerService.UpdateCustomerAsync(customer);

        // Retrun error result for not valid ID's
        if (!string.IsNullOrEmpty(taxExamptValid))
            context.ModelState.AddModelError("", taxExamptValid);
    }
    #endregion

    #region Methods

    /// <summary>
    /// Called asynchronously before the action, after model binding is complete.
    /// </summary>
    /// <param name="context">A context for action filters</param>
    /// <param name="next">A delegate invoked to execute the next action filter or the action itself</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        await CheckProductSetupWithOpticalAttributesAsync(context);
        if (context.Result == null)
            await next();
    }
    #endregion
}
