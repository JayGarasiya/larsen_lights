using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Messages;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Misc.InvoicePDF.Areas.Admin.Controllers
{
    public class OverrideMessageTemplateController : MessageTemplateController
    {
        #region Fields
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ICustomerService _customerService;
        #endregion

        #region Ctor
        public OverrideMessageTemplateController(ICustomerActivityService customerActivityService,
            ILocalizationService localizationService,
            ILocalizedEntityService localizedEntityService,
            IMessageTemplateModelFactory messageTemplateModelFactory,
            IMessageTemplateService messageTemplateService,
            INotificationService notificationService,
            IStoreMappingService storeMappingService,
            IWorkflowMessageService workflowMessageService,
            IGenericAttributeService genericAttributeService,
            ICustomerService customerService) : base(customerActivityService,
                localizationService,
                localizedEntityService,
                messageTemplateModelFactory,
                messageTemplateService,
                notificationService,
                storeMappingService,
                workflowMessageService)
        {
            _genericAttributeService = genericAttributeService;
            _customerService = customerService;
        }
        #endregion

        #region Methods
        [CheckPermission(StandardPermission.ContentManagement.MESSAGE_TEMPLATES_VIEW)]
        public override async Task<IActionResult> Edit(int id)
        {
            //try to get a message template with the specified id
            var messageTemplate = await _messageTemplateService.GetMessageTemplateByIdAsync(id);
            if (messageTemplate == null)
                return RedirectToAction("List");

            //Custom Code For Order Placed Customer Notification
            if (messageTemplate.Name == MessageTemplateSystemNames.ORDER_PLACED_CUSTOMER_NOTIFICATION)
            {
                var customerRoleId = await _genericAttributeService.GetAttributeAsync<int>(messageTemplate, InvoicePDFDefaults.FOR_CUSTOMER_ROLE);
                if (customerRoleId > 0)
                    ViewBag.RoleName = $"- {(await _customerService.GetCustomerRoleByIdAsync(customerRoleId))?.Name}";
            }

            //prepare model
            var model = await _messageTemplateModelFactory.PrepareMessageTemplateModelAsync(null, messageTemplate);

            return View(model);
        }
        #endregion
    }
}
