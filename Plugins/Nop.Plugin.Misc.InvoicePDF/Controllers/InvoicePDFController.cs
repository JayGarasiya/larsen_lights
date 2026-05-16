using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Misc.InvoicePDF.Models;
using Nop.Plugin.Misc.InvoicePDF.Services;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Areas.Admin.Models.Customers;
using Nop.Web.Framework.Models.Extensions;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.InvoicePDF.Controllers
{
    public class InvoicePDFController : BaseAdminController
    {
        #region Fields
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IMessageTemplateService _messageTemplateService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IMailTemplateCustomerRoleService _mailTemplateCustomerRoleService;
        private readonly ICustomerService _customerService;
        #endregion

        #region Ctor
        public InvoicePDFController(ILocalizationService localizationService,
            INotificationService notificationService,
            ISettingService settingService,
            IStoreContext storeContext,
            IMessageTemplateService messageTemplateService,
            IGenericAttributeService genericAttributeService,
            IMailTemplateCustomerRoleService mailTemplateCustomerRoleService,
            ICustomerService customerService)
        {
            _localizationService = localizationService;
            _notificationService = notificationService;
            _settingService = settingService;
            _storeContext = storeContext;
            _messageTemplateService = messageTemplateService;
            _genericAttributeService = genericAttributeService;
            _mailTemplateCustomerRoleService = mailTemplateCustomerRoleService;
            _customerService = customerService;
        }
        #endregion

        #region Methods

        #region Configuration
        [CheckPermission(StandardPermission.Configuration.MANAGE_WIDGETS)]
        public async Task<IActionResult> Configure()
        {
            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var invoiceSettings = await _settingService.LoadSettingAsync<InvoicePDFSettings>(storeScope);
            var shoppingCartSettings = await _settingService.LoadSettingAsync<ShoppingCartSettings>();

            var model = new ConfigurationModel
            {
                AttachOrderId = invoiceSettings.AttachOrderId,
                UseOrderMask = invoiceSettings.UseOrderMask,
                VendorCost = invoiceSettings.VendorCost,
                RenderAttributeValuePrices = invoiceSettings.RenderAttributeValuePrices,
                ReturnAddress = invoiceSettings.ReturnAddress,
                RenderAssociatedAttributeValueQuantity = shoppingCartSettings.RenderAssociatedAttributeValueQuantity,
                ActiveStoreScopeConfiguration = storeScope
            };

            if (storeScope > 0)
            {
                model.AttachOrderId_OverrideForStore = await _settingService.SettingExistsAsync(invoiceSettings, x => x.AttachOrderId, storeScope);
                model.UseOrderMask_OverrideForStore = await _settingService.SettingExistsAsync(invoiceSettings, x => x.UseOrderMask, storeScope);
                model.VendorCost_OverrideForStore = await _settingService.SettingExistsAsync(invoiceSettings, x => x.VendorCost, storeScope);
                model.RenderAttributeValuePrices_OverrideForStore = await _settingService.SettingExistsAsync(invoiceSettings, x => x.RenderAttributeValuePrices, storeScope);
                model.ReturnAddress_OverrideForStore = await _settingService.SettingExistsAsync(invoiceSettings, x => x.ReturnAddress, storeScope);
            }

            //prepare search model
            model.CustomerSearchModel = new CustomerSearchModel();

            //prepare page parameters
            model.CustomerSearchModel.SetGridPageSize();

            return View("~/Plugins/Misc.InvoicePDF/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Configuration.MANAGE_WIDGETS)]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var invoiceSettings = await _settingService.LoadSettingAsync<InvoicePDFSettings>(storeScope);
            var shoppingCartSettings = await _settingService.LoadSettingAsync<ShoppingCartSettings>();

            invoiceSettings.AttachOrderId = model.AttachOrderId;
            invoiceSettings.UseOrderMask = model.UseOrderMask;
            invoiceSettings.VendorCost = model.VendorCost;
            invoiceSettings.RenderAttributeValuePrices = model.RenderAttributeValuePrices;
            invoiceSettings.ReturnAddress = model.ReturnAddress;

            shoppingCartSettings.RenderAssociatedAttributeValueQuantity = model.RenderAssociatedAttributeValueQuantity;
            await _settingService.SaveSettingAsync(shoppingCartSettings);

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            await _settingService.SaveSettingOverridablePerStoreAsync(invoiceSettings, x => x.AttachOrderId, model.AttachOrderId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(invoiceSettings, x => x.UseOrderMask, model.UseOrderMask_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(invoiceSettings, x => x.VendorCost, model.VendorCost_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(invoiceSettings, x => x.RenderAttributeValuePrices, model.RenderAttributeValuePrices_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(invoiceSettings, x => x.ReturnAddress, model.ReturnAddress_OverrideForStore, storeScope, false);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

            return await Configure();
        }

        #endregion

        #region Message template 

        [HttpPost]
        [CheckPermission(StandardPermission.ContentManagement.MESSAGE_TEMPLATES_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> CopyTemplate(IFormCollection form)
        {
            int.TryParse(form["Id"], out int messageTemplateId);
            int.TryParse(form["CustomerRoleId"], out int customerRoleId);

            //try to get a message template with the specified id
            var messageTemplate = await _messageTemplateService.GetMessageTemplateByIdAsync(messageTemplateId);
            if (messageTemplate == null)
                return RedirectToAction("List", "MessageTemplate");

            try
            {
                #region In active previous mail templates

                var getDuplicateIds = await _mailTemplateCustomerRoleService.GetMessageTemplateIdsByCustomerRoleIdAsync(customerRoleId);
                if (getDuplicateIds.Any())
                    await _mailTemplateCustomerRoleService.UpdateMessageTemplatesAsync(getDuplicateIds);

                #endregion

                #region Copy new template

                var newMessageTemplate = await _messageTemplateService.CopyMessageTemplateAsync(messageTemplate);
                await _genericAttributeService.SaveAttributeAsync(newMessageTemplate, InvoicePDFDefaults.FOR_CUSTOMER_ROLE, customerRoleId);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.ContentManagement.MessageTemplates.Copied"));

                #endregion

                return RedirectToAction("Edit", "MessageTemplate", new { id = newMessageTemplate.Id });
            }
            catch (Exception exc)
            {
                _notificationService.ErrorNotification(exc.Message);
                return RedirectToAction("Edit", "MessageTemplate", new { id = messageTemplateId });
            }
        }

        #endregion

        #region PDF Invoices 'No mailed invoice needed'

        [HttpPost]
        [CheckPermission(StandardPermission.Customers.CUSTOMERS_VIEW)]
        public virtual async Task<IActionResult> ExcludedCustomerList(CustomerSearchModel searchModel)
        {
            ArgumentNullException.ThrowIfNull(searchModel);

            //load settings for a chosen store scope
            var invoiceSettings = await _settingService.LoadSettingAsync<InvoicePDFSettings>();
            var allCustomers = await _customerService.GetCustomersByIdsAsync(invoiceSettings.ExcludedCustomerIds.ToArray());

            var customers = allCustomers.ToPagedList(searchModel);

            //prepare list model
            var model = await new CustomerListModel().PrepareToGridAsync(searchModel, customers, () =>
            {
                return customers.SelectAwait(async customer =>
                {
                    //fill in model values from the entity
                    return new CustomerModel
                    {
                        Id = customer.Id,
                        Email = (await _customerService.IsRegisteredAsync(customer))
                            ? customer.Email
                            : await _localizationService.GetResourceAsync("Admin.Customers.Guest")
                    };
                });
            });

            return Json(model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Customers.CUSTOMERS_VIEW)]
        public virtual async Task<IActionResult> ExcludedCustomerAdd(int customerId)
        {
            if (customerId == 0)
                ModelState.AddModelError("", "No customer found with the specified email address");

            if (!ModelState.IsValid)
            {
                return ErrorJson(ModelState.SerializeErrors());
            }

            //load settings for a chosen store scope
            var invoiceSettings = await _settingService.LoadSettingAsync<InvoicePDFSettings>();
            var customer = await _customerService.GetCustomerByIdAsync(customerId);

            if (!invoiceSettings.ExcludedCustomerIds.Contains(customer.Id))
            {
                invoiceSettings.ExcludedCustomerIds.Add(customer.Id);

                await _settingService.SaveSettingAsync(invoiceSettings);
            }
            else
            {
                return ErrorJson(string.Format(await _localizationService.GetResourceAsync("Plugins.Misc.InvoicePDF.ExcludedCustomer.CustomerAlreadyExists"), customer.Email));
            }

            return Json(new { Result = true });
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Configuration.MANAGE_LANGUAGES)]
        public virtual async Task<IActionResult> ExcludedCustomerDelete(int id)
        {
            //try to get a customer with the specified id
            var customer = await _customerService.GetCustomerByIdAsync(id)
                ?? throw new ArgumentException("No customer found with the specified id", nameof(id));

            //load settings for a chosen store scope
            var invoiceSettings = await _settingService.LoadSettingAsync<InvoicePDFSettings>();
            invoiceSettings.ExcludedCustomerIds.Remove(customer.Id);

            await _settingService.SaveSettingAsync(invoiceSettings);

            return new NullJsonResult();
        }

        [CheckPermission(StandardPermission.Security.ACCESS_ADMIN_PANEL)]
        public virtual async Task<IActionResult> SearchCustomerAutoComplete(string term)
        {
            const int searchTermMinimumLength = 3;
            if (string.IsNullOrWhiteSpace(term) || term.Length < searchTermMinimumLength)
                return Content(string.Empty);

            //customers
            const int customerNumber = 15;
            var customers = await _customerService.GetAllCustomersAsync(email: term, pageSize: customerNumber);

            var result = (from c in customers
                          select new
                          {
                              label = c.Email,
                              customerid = c.Id
                          }).ToList();

            return Json(result);
        }

        #endregion

        #endregion
    }
}
