using Microsoft.AspNetCore.Mvc.Infrastructure;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Orders;
using Nop.Core.Events;
using Nop.Services.Attributes;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Events;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Tax;
using Nop.Web.Framework.Events;
using Nop.Web.Framework.UI;

namespace Nop.Plugin.Tax.TaxJar.Services;

public class EventConsumer : IConsumer<OrderPlacedEvent>,
    IConsumer<OrderStatusChangedEvent>,
    IConsumer<EntityDeletedEvent<Order>>,
    IConsumer<OrderRefundedEvent>,
    IConsumer<OrderPaidEvent>,
    IConsumer<EntityInsertedEvent<QueuedEmail>>,
    IConsumer<PageRenderingEvent>,
    IConsumer<CustomerRegisteredEvent>
{
    #region Fields
    private readonly TaxJarSettings _taxJarSettings;
    private readonly ITaxPluginManager _taxPluginManager;
    private readonly TaxJarTaxManager _taxJarTaxManager;
    private readonly IMessageTemplateService _messageTemplateService;
    private readonly IStoreContext _storeContext;
    private readonly IAttributeParser<AddressAttribute, AddressAttributeValue> _addressAttributeParser;
    private readonly IOrderService _orderService;
    private readonly IAddressService _addressService;
    private readonly IQueuedEmailService _queuedEmailService;
    private readonly ICustomerService _customerService;
    private readonly IAttributeService<CustomerAttribute, CustomerAttributeValue> _customerAttributeService;
    private readonly IAttributeParser<CustomerAttribute, CustomerAttributeValue> _customerAttributeParser;
    private readonly IActionContextAccessor _actionContextAccessor;
    #endregion

    #region Ctor
    public EventConsumer(
        TaxJarSettings taxJarSettings,
        ITaxPluginManager taxPluginManager,
        TaxJarTaxManager taxJarTaxManager,
        IMessageTemplateService messageTemplateService,
        IStoreContext storeContext,
        IAttributeParser<AddressAttribute, AddressAttributeValue> addressAttributeParser,
        IOrderService orderService,
        IAddressService addressService,
        IQueuedEmailService queuedEmailService,
        ICustomerService customerService,
        IAttributeService<CustomerAttribute, CustomerAttributeValue> customerAttributeService,
        IAttributeParser<CustomerAttribute, CustomerAttributeValue> customerAttributeParser,
        IActionContextAccessor actionContextAccessor)
    {
        _taxJarSettings = taxJarSettings;
        _taxPluginManager = taxPluginManager;
        _taxJarTaxManager = taxJarTaxManager;
        _messageTemplateService = messageTemplateService;
        _storeContext = storeContext;
        _addressAttributeParser = addressAttributeParser;
        _orderService = orderService;
        _addressService = addressService;
        _queuedEmailService = queuedEmailService;
        _customerService = customerService;
        _customerAttributeService = customerAttributeService;
        _customerAttributeParser = customerAttributeParser;
        _actionContextAccessor = actionContextAccessor;
    }
    #endregion

    #region Utilities
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
    /// Gets a value indicating whether a customer is tax exempt
    /// </summary>
    /// <param name="customer">Customer</param>
    /// <param name="overrideAttributesXml">Overridden customer attributes in XML format; pass null to use CustomCustomerAttributes of customer</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains a value indicating whether a customer is tax exempt
    /// </returns>
    protected virtual async Task<bool> IsTaxExemptCustomerAsync(Customer customer, string overrideAttributesXml = "")
    {
        ArgumentNullException.ThrowIfNull(customer);

        var taxExamptTextValue = new List<int>();
        var taxExamptValue = new List<int>();
        var taxExamptValid = false;

        // Set already selected attributes
        var selectedAttributesXml = !string.IsNullOrEmpty(overrideAttributesXml) ? overrideAttributesXml : customer.CustomCustomerAttributesXML;

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
                            if (enteredText.Count != 0 && _taxJarSettings.IsTaxExempt.Contains(attribute.Id) && !taxExamptTextValue.Contains(attribute.Id))
                            {
                                taxExamptValid = enteredText.FirstOrDefault().Length != 0 && enteredText.FirstOrDefault().Length < 7 ? true : false;
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

        if (taxExamptTextValue.Count != 0 && taxExamptValue.Count != 0 && !taxExamptValid)
            return true;

        return false;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Handle order placed event
    /// </summary>
    /// <param name="eventMessage">Event message</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public async Task HandleEventAsync(OrderPlacedEvent eventMessage)
    {
        if (eventMessage.Order == null)
            return;

        // Ensure that Avalara tax provider is active
        if (!await _taxPluginManager.IsPluginActiveAsync(TaxJarDefaults.SystemName))
            return;

        // Ensure that plugin allow to commit transactions
        if (!_taxJarSettings.CommitTransactions)
            return;

        var paymentmethodList = new List<string>();
        if (!string.IsNullOrEmpty(_taxJarSettings.PaymentMethods))
            paymentmethodList = [.. _taxJarSettings.PaymentMethods.Split(',')];

        if (!paymentmethodList.Contains(eventMessage.Order.PaymentMethodSystemName))
            await _taxJarTaxManager.CreateOrderTaxTransactionAsync(eventMessage.Order);
    }

    /// <summary>
    /// Handle order cancelled event
    /// </summary>
    /// <param name="eventMessage">Event message</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public async Task HandleEventAsync(OrderStatusChangedEvent eventMessage)
    {
        if (eventMessage.Order == null)
            return;

        // Ensure that Avalara tax provider is active
        if (!await _taxPluginManager.IsPluginActiveAsync(TaxJarDefaults.SystemName))
            return;

        // Ensure that plugin allow to commit transactions
        if (!_taxJarSettings.CommitTransactions)
            return;

        // Async Task tax transaction
        var order = eventMessage.Order;
        if (order.OrderStatus == OrderStatus.Cancelled)
            await _taxJarTaxManager.DeleteTaxTransactionAsync(order);
    }

    /// <summary>
    /// Handle order deleted event
    /// </summary>
    /// <param name="eventMessage">Event message</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public async Task HandleEventAsync(EntityDeletedEvent<Order> eventMessage)
    {
        if (eventMessage.Entity == null)
            return;

        // Ensure that Avalara tax provider is active
        if (!await _taxPluginManager.IsPluginActiveAsync(TaxJarDefaults.SystemName))
            return;

        // Ensure that plugin allow to commit transactions
        if (!_taxJarSettings.CommitTransactions)
            return;

        // Async Task tax transaction
        await _taxJarTaxManager.DeleteTaxTransactionAsync(eventMessage.Entity);
    }

    /// <summary>
    /// Handle order refunded event
    /// </summary>
    /// <param name="eventMessage">Event message</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public async Task HandleEventAsync(OrderRefundedEvent eventMessage)
    {
        if (eventMessage.Order == null)
            return;

        // Ensure that Avalara tax provider is active
        if (!await _taxPluginManager.IsPluginActiveAsync(TaxJarDefaults.SystemName))
            return;

        // Ensure that plugin allow to commit transactions
        if (!_taxJarSettings.CommitTransactions)
            return;

        // Async Task tax transaction
        await _taxJarTaxManager.RefundTaxTransactionAsync(eventMessage.Order, eventMessage.Amount);
    }

    /// <summary>
    /// Handle order paid event
    /// </summary>
    /// <param name="eventMessage">Event message</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public async Task HandleEventAsync(OrderPaidEvent eventMessage)
    {
        if (eventMessage.Order == null)
            return;

        // Ensure that Avalara tax provider is active
        if (!await _taxPluginManager.IsPluginActiveAsync(TaxJarDefaults.SystemName))
            return;

        // Ensure that plugin allow to commit transactions
        if (!_taxJarSettings.CommitTransactions)
            return;

        var paymentmethodList = new List<string>();
        if (!string.IsNullOrEmpty(_taxJarSettings.PaymentMethods))
            paymentmethodList = [.. _taxJarSettings.PaymentMethods.Split(',')];

        if (paymentmethodList.Contains(eventMessage.Order.PaymentMethodSystemName))
            await _taxJarTaxManager.CreateOrderTaxTransactionAsync(eventMessage.Order);
    }

    /// <summary>
    /// Handle the insert queued email event
    /// </summary>
    /// <param name="eventMessage">The event message.</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public async Task HandleEventAsync(EntityInsertedEvent<QueuedEmail> eventMessage)
    {
        // Handle event
        var queuedEmail = eventMessage.Entity;
        if (!queuedEmail.Subject.Contains('#'))
            return;

        var store = await _storeContext.GetCurrentStoreAsync();
        var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateSystemNames.ORDER_PLACED_CUSTOMER_NOTIFICATION, store.Id);

        if (!messageTemplates.Any())
            return;

        var qesubj = queuedEmail.Subject.Split("#");
        var subjtitle = qesubj.FirstOrDefault();
        if (messageTemplates.Where(x => x.Subject.Split("#").FirstOrDefault()?.Trim()?.Contains(subjtitle.Trim()) ?? false).Any() && qesubj.Length > 1)
        {
            var subjHash = qesubj.LastOrDefault().Split(" ").FirstOrDefault();
            var order = await _orderService.GetOrderByIdAsync(Convert.ToInt32(subjHash));
            if (order != null)
            {
                var address = await _addressService.GetAddressByIdAsync(order?.BillingAddressId ?? 0);
                if (address != null && _taxJarSettings.CCAttributeId > 0)
                {
                    var customAttributeValue = _addressAttributeParser.ParseValues(address?.CustomAttributes ?? string.Empty, _taxJarSettings.CCAttributeId);
                    if (customAttributeValue.Any())
                        queuedEmail.CC = customAttributeValue[0];
                }

                // Let's ensure that at least 3600 seconds passed after resend invoice with update Bcc
                // P.S. there's no any particular reason for that. we just do it
                if ((DateTime.UtcNow - order.CreatedOnUtc).TotalSeconds > 3600)
                    queuedEmail.Bcc = string.Empty;

                await _queuedEmailService.UpdateQueuedEmailAsync(queuedEmail);
            }
        }
    }

    /// <summary>
    /// Handle page rendering event
    /// </summary>
    /// <param name="eventMessage">Event message</param>
    public async Task HandleEventAsync(PageRenderingEvent eventMessage)
    {
        // Check is admin area request
        var routeValues = _actionContextAccessor.ActionContext.RouteData.Values;
        var areaExist = routeValues.ContainsKey("area")
                        && (routeValues["area"]?.ToString() ?? string.Empty).Equals("Admin", StringComparison.InvariantCultureIgnoreCase);
        if (areaExist)
            return;

        // Ensure that taxjar tax provider is active
        if (!await _taxPluginManager.IsPluginActiveAsync(TaxJarDefaults.SystemName))
            return;

        // Get the route name associated with the request rendering this page
        var routeName = eventMessage.GetRouteName() ?? string.Empty;
        if (string.IsNullOrEmpty(routeName) || string.IsNullOrWhiteSpace(routeName))
            return;

        // Add css to one page checkout
        if (routeName.Equals("RealOnePageCheckout") || routeName.Equals("CheckoutOnePage") || routeName.Equals("CheckoutBillingAddress") ||
            routeName.Equals("CheckoutShippingAddress") || routeName.Equals("CheckoutConfirm") || routeName.Equals("ShoppingCart"))
        {
            eventMessage.Helper.AddScriptParts(ResourceLocation.Footer, "~/lib_npm/magnific-popup/jquery.magnific-popup.min.js");
            eventMessage.Helper.AddCssFileParts("~/lib_npm/magnific-popup/magnific-popup.css");
            eventMessage.Helper.AddCssFileParts("~/Plugins/Tax.TaxJar/Content/styles.css");
        }
    }

    /// <summary>
    /// Handle customer registered event
    /// </summary>
    /// <param name="eventMessage">Event message</param>
    public async Task HandleEventAsync(CustomerRegisteredEvent eventMessage)
    {
        // Ensure that taxjar tax provider is active
        if (!await _taxPluginManager.IsPluginActiveAsync(TaxJarDefaults.SystemName))
            return;

        // Handle event
        var customer = eventMessage.Customer;

        // Check customer is tax exempt or not and update it.
        var isTaxExempt = await IsTaxExemptCustomerAsync(customer);
        customer.IsTaxExempt = isTaxExempt;
        await _customerService.UpdateCustomerAsync(customer);
    }
    #endregion
}
