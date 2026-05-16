using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Orders;
using Nop.Core.Events;
using Nop.Services.Affiliates;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Html;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Stores;

namespace Nop.Plugin.Widgets.MakeTypeModel.Areas.Admin.Services
{
    /// <summary>
    /// Override workflow message service
    /// </summary>
    public class OverrideWorkflowMessageService : WorkflowMessageService
    {
        #region Fields

        protected readonly ICustomReturnRequestService _customReturnRequestService;
        protected readonly IHtmlFormatter _htmlFormatter;

        #endregion

        #region Ctor

        public OverrideWorkflowMessageService(CommonSettings commonSettings,
            EmailAccountSettings emailAccountSettings,
            IAddressService addressService,
            IAffiliateService affiliateService,
            ICustomerService customerService,
            IEmailAccountService emailAccountService,
            IEventPublisher eventPublisher,
            ILanguageService languageService,
            ILocalizationService localizationService,
            IMessageTemplateService messageTemplateService,
            IMessageTokenProvider messageTokenProvider,
            IOrderService orderService,
            IProductService productService,
            IQueuedEmailService queuedEmailService,
            IStoreContext storeContext,
            IStoreService storeService,
            ITokenizer tokenizer,
            MessagesSettings messagesSettings,
            ICustomReturnRequestService customReturnRequestService,
            IHtmlFormatter htmlFormatter)
            : base(commonSettings,
                emailAccountSettings,
                addressService,
                affiliateService,
                customerService,
                emailAccountService,
                eventPublisher,
                languageService,
                localizationService,
                messageTemplateService,
                messageTokenProvider,
                orderService,
                productService,
                queuedEmailService,
                storeContext,
                storeService,
                tokenizer,
                messagesSettings)
        {
            _customReturnRequestService = customReturnRequestService;
            _htmlFormatter = htmlFormatter;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Add return request tokens
        /// </summary>
        /// <param name="tokens">List of already added tokens</param>
        /// <param name="returnRequest">Return request</param>
        /// <param name="orderItem">Order item</param>
        /// <param name="languageId">Language identifier</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task AddReturnRequestTokensAsync(IList<Token> tokens, ReturnRequest returnRequest, OrderItem orderItem, int languageId)
        {
            var product = await _productService.GetProductByIdAsync(orderItem.ProductId);

            var note = await _customReturnRequestService.GetReturnRequestNoteByIdAsync(returnRequest.Id);
            var formattedNote = _htmlFormatter.FormatText(note?.Notes ?? string.Empty, false, true, false, false, false, false);

            tokens.Add(new Token("ReturnRequest.CustomNumber", returnRequest.CustomNumber));
            tokens.Add(new Token("ReturnRequest.OrderId", orderItem.OrderId));
            tokens.Add(new Token("ReturnRequest.Product.Quantity", returnRequest.Quantity));
            tokens.Add(new Token("ReturnRequest.Product.Name", await _localizationService.GetLocalizedAsync(product, x => x.Name, languageId)));
            tokens.Add(new Token("ReturnRequest.Reason", returnRequest.ReasonForReturn));
            tokens.Add(new Token("ReturnRequest.RequestedAction", returnRequest.RequestedAction));
            tokens.Add(new Token("ReturnRequest.CustomerComment", _htmlFormatter.FormatText(returnRequest.CustomerComments, false, true, false, false, false, false), true));
            tokens.Add(new Token("ReturnRequest.StaffNotes", _htmlFormatter.FormatText(returnRequest.StaffNotes, false, true, false, false, false, false), true));
            tokens.Add(new Token("ReturnRequest.Status", await _localizationService.GetLocalizedEnumAsync(returnRequest.ReturnRequestStatus, languageId)));
            tokens.Add(new Token(MakeTypeModelDefaults.ReturnRequestNoteToken, formattedNote, true));

            //event notification
            await _eventPublisher.EntityTokensAddedAsync(returnRequest, tokens);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Sends 'Return Request status changed' message to a customer
        /// </summary>
        /// <param name="returnRequest">Return request</param>
        /// <param name="orderItem">Order item</param>
        /// <param name="order">Order</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the queued email identifier
        /// </returns>
        public override async Task<IList<int>> SendReturnRequestStatusChangedCustomerNotificationAsync(ReturnRequest returnRequest, OrderItem orderItem, Order order)
        {
            ArgumentNullException.ThrowIfNull(returnRequest);

            ArgumentNullException.ThrowIfNull(orderItem);

            ArgumentNullException.ThrowIfNull(order);

            var store = await _storeService.GetStoreByIdAsync(order.StoreId) ?? await _storeContext.GetCurrentStoreAsync();
            var languageId = await EnsureLanguageIsActiveAsync(order.CustomerLanguageId, store.Id);

            var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateSystemNames.RETURN_REQUEST_STATUS_CHANGED_CUSTOMER_NOTIFICATION, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            var customer = await _customerService.GetCustomerByIdAsync(returnRequest.CustomerId);

            //tokens
            var commonTokens = new List<Token>();
            await _messageTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId);
            await _messageTokenProvider.AddCustomerTokensAsync(commonTokens, customer);
            await AddReturnRequestTokensAsync(commonTokens, returnRequest, orderItem, languageId);

            return await messageTemplates.SelectAwait(async messageTemplate =>
            {
                //email account
                var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount,languageId);

                //event notification
                await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

                var billingAddress = await _addressService.GetAddressByIdAsync(order.BillingAddressId);

                var toEmail = (await _customerService.IsGuestAsync(customer))
                    ? billingAddress.Email
                    : customer.Email; 
                var toName = (await _customerService.IsGuestAsync(customer))
                    ? billingAddress.FirstName
                    : await _customerService.GetCustomerFullNameAsync(customer);

                return await SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
            }).ToListAsync();
        }
        
        #endregion

    }
}
