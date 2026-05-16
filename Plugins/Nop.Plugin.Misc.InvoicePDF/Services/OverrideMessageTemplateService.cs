using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Messages;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Stores;

namespace Nop.Plugin.Misc.InvoicePDF.Services
{
    /// <summary>
    /// Gets message templates Services
    /// </summary>
    public class OverrideMessageTemplateService : MessageTemplateService
    {
        #region Fields
        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;
        private readonly IMailTemplateCustomerRoleService _mailTemplateCustomerRoleService;
        #endregion

        #region Ctor
        public OverrideMessageTemplateService(IStaticCacheManager staticCacheManager,
            ILanguageService languageService,
            ILocalizationService localizationService,
            ILocalizedEntityService localizedEntityService,
            IRepository<MessageTemplate> messageTemplateRepository,
            IStoreMappingService storeMappingService,
            IWorkContext workContext,
            ICustomerService customerService,
            IMailTemplateCustomerRoleService mailTemplateCustomerRoleService) : base(staticCacheManager,
                languageService,
                localizationService,
                localizedEntityService,
                messageTemplateRepository,
                storeMappingService)
        {
            _workContext = workContext;
            _customerService = customerService;
            _mailTemplateCustomerRoleService = mailTemplateCustomerRoleService;
        }
        #endregion

        #region Utilities
        /// <summary>
        /// Gets message templates by the name
        /// </summary>
        /// <param name="messageTemplateName">Message template name</param>
        /// <param name="storeId">Store identifier; pass null to load all records</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the list of message templates
        /// </returns>
        public override async Task<IList<MessageTemplate>> GetMessageTemplatesByNameAsync(string messageTemplateName, int? storeId = null)
        {
            if (string.IsNullOrWhiteSpace(messageTemplateName))
                throw new ArgumentException(nameof(messageTemplateName));

            #region Custom Code
            if (messageTemplateName.Equals(MessageTemplateSystemNames.ORDER_PLACED_CUSTOMER_NOTIFICATION))
            {
                var getMessageTemplatesIds = await _mailTemplateCustomerRoleService.GetMessageTemplateIdsByCustomerRoleIdsAsync(await _customerService.GetCustomerRoleIdsAsync(await _workContext.GetCurrentCustomerAsync()));
                if (getMessageTemplatesIds.Any())
                {
                    //Get only first or default mail template
                    var messageTemplateList = new List<MessageTemplate>();
                    messageTemplateList.Add(await _mailTemplateCustomerRoleService.GetActiveMessageTemplatesByIdsAsync(getMessageTemplatesIds));
                    return messageTemplateList;
                }
            }
            #endregion

            var key = _staticCacheManager.PrepareKeyForDefaultCache(NopMessageDefaults.MessageTemplatesByNameCacheKey, messageTemplateName, storeId);
            return await _staticCacheManager.GetAsync(key, async () =>
            {
                //get message templates with the passed name
                var templatesQuery = _messageTemplateRepository.Table
                    .Where(messageTemplate => messageTemplate.Name.Equals(messageTemplateName));

                if (storeId.HasValue && storeId.Value > 0)
                {
                    templatesQuery = await _storeMappingService.ApplyStoreMapping(templatesQuery, storeId.Value);
                }

                return await templatesQuery.OrderBy(messageTemplate => messageTemplate.Id)
                .ToListAsync();
            });
        }
        #endregion
    }
}
