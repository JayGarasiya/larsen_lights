using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Messages;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Messages;
using Nop.Web.Framework.Factories;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Plugin.Misc.InvoicePDF.Areas.Admin.Factories
{
    public class OverrideMessageTemplateModelFactory : MessageTemplateModelFactory
    {
        #region Fields
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ICustomerService _customerService;
        #endregion

        #region Ctor
        public OverrideMessageTemplateModelFactory(CatalogSettings catalogSettings,
            IBaseAdminModelFactory baseAdminModelFactory,
            ILocalizationService localizationService,
            ILocalizedModelFactory localizedModelFactory,
            IMessageTemplateService messageTemplateService,
            IMessageTokenProvider messageTokenProvider,
            IStoreMappingSupportedModelFactory storeMappingSupportedModelFactory,
            IStoreService storeService,
            IGenericAttributeService genericAttributeService,
            ICustomerService customerService) : base(catalogSettings,
                baseAdminModelFactory,
                localizationService,
                localizedModelFactory,
                messageTemplateService,
                messageTokenProvider,
                storeMappingSupportedModelFactory,
                storeService)
        {
            _genericAttributeService = genericAttributeService;
            _customerService = customerService;
        }
        #endregion

        #region Method
        /// <summary>
        /// Prepare paged message template list model
        /// </summary>
        /// <param name="searchModel">Message template search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the message template list model
        /// </returns>
        public override async Task<MessageTemplateListModel> PrepareMessageTemplateListModelAsync(MessageTemplateSearchModel searchModel)
        {
            ArgumentNullException.ThrowIfNull(searchModel);

            var isActive = searchModel.IsActiveId == 0 ? null : (bool?)(searchModel.IsActiveId == 1);

            //get message templates
            var messageTemplates = (await _messageTemplateService
                .GetAllMessageTemplatesAsync(searchModel.SearchStoreId, searchModel.SearchKeywords, isActive, searchModel.EmailAccountId)).ToPagedList(searchModel);

            //prepare store names (to avoid loading for each message template)
            var stores = (await _storeService.GetAllStoresAsync()).Select(store => new { store.Id, store.Name }).ToList();

            //prepare list model
            var model = await new MessageTemplateListModel().PrepareToGridAsync(searchModel, messageTemplates, () =>
            {
                return messageTemplates.SelectAwait(async messageTemplate =>
                {
                    //fill in model values from the entity
                    var messageTemplateModel = messageTemplate.ToModel<MessageTemplateModel>();

                    //fill in additional values (not existing in the entity)
                    if (messageTemplate.LimitedToStores)
                    {
                        await _storeMappingSupportedModelFactory.PrepareModelStoresAsync(messageTemplateModel, messageTemplate, false);
                        var storeNames = stores
                            .Where(store => messageTemplateModel.SelectedStoreIds.Contains(store.Id)).Select(store => store.Name);
                        messageTemplateModel.ListOfStores = string.Join(", ", storeNames);
                    }
                    else
                    {
                        var allstores = await _localizationService.GetResourceAsync("Admin.Configuration.Settings.AllSettings.Fields.StoreName.AllStores");
                        messageTemplateModel.ListOfStores = allstores;
                    }

                    //Custom Code For Order Placed Customer Notification
                    if (messageTemplateModel.Name == MessageTemplateSystemNames.ORDER_PLACED_CUSTOMER_NOTIFICATION)
                    {
                        var entity = messageTemplateModel.ToEntity<MessageTemplate>();
                        var customerRoleId = await _genericAttributeService.GetAttributeAsync<int>(entity, InvoicePDFDefaults.FOR_CUSTOMER_ROLE);
                        if (customerRoleId > 0)
                            messageTemplateModel.Name = $"{messageTemplateModel.Name} - {(await _customerService.GetCustomerRoleByIdAsync(customerRoleId))?.Name}";
                    }

                    return messageTemplateModel;
                });
            });

            return model;
        }
        #endregion
    }
}
