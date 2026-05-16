using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Cms;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Widgets.QuickOrder.Factories;
using Nop.Plugin.Widgets.QuickOrder.Models;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Widgets.QuickOrder.Controllers;

public class QuickOrderController : BaseAdminController
{
    #region Fields

    protected readonly ILocalizationService _localizationService;
    protected readonly INotificationService _notificationService;
    protected readonly ISettingService _settingService;
    protected readonly IStoreContext _storeContext;
    protected readonly ICustomerService _customerService;
    protected readonly ICustomerActivityService _customerActivityService;
    protected readonly IWorkContext _workContext;
    protected readonly IGenericAttributeService _genericAttributeService;
    protected readonly IQuickOrderModelFactory _quickOrderModelFactory;

    #endregion

    #region Ctor

    public QuickOrderController(
        ILocalizationService localizationService,
        INotificationService notificationService,
        ISettingService settingService,
        IStoreContext storeContext,
        ICustomerService customerService,
        ICustomerActivityService customerActivityService,
        IWorkContext workContext,
        IGenericAttributeService genericAttributeService,
        IQuickOrderModelFactory quickOrderModelFactory)
    {
        _localizationService = localizationService;
        _notificationService = notificationService;
        _settingService = settingService;
        _storeContext = storeContext;
        _customerService = customerService;
        _customerActivityService = customerActivityService;
        _workContext = workContext;
        _genericAttributeService = genericAttributeService;
        _quickOrderModelFactory = quickOrderModelFactory;
    }

    #endregion

    #region Methods

    [CheckPermission(StandardPermission.Configuration.MANAGE_WIDGETS)]
    public async Task<IActionResult> Configure()
    {
        // Fetch widget settings for the current store
        var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var widgetSettings = await _settingService.LoadSettingAsync<WidgetSettings>(storeId);

        // Prepare the configuration model to display the widget's status
        var model = new ConfigurationModel
        {
            Enabled = widgetSettings.ActiveWidgetSystemNames.Contains(QuickOrderDefaults.SystemName),
        };

        return View("~/Plugins/Widgets.QuickOrder/Views/Configure.cshtml", model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Configuration.MANAGE_WIDGETS)]
    public async Task<IActionResult> Configure(ConfigurationModel model)
    {
        // If the model is not valid, re-render the configuration view
        if (!ModelState.IsValid)
            return await Configure();

        var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var widgetSettings = await _settingService.LoadSettingAsync<WidgetSettings>(storeId);

        // Update the widget's status (enable/disable) based on the configuration model
        if (model.Enabled && !widgetSettings.ActiveWidgetSystemNames.Contains(QuickOrderDefaults.SystemName))
            widgetSettings.ActiveWidgetSystemNames.Add(QuickOrderDefaults.SystemName);
        if (!model.Enabled && widgetSettings.ActiveWidgetSystemNames.Contains(QuickOrderDefaults.SystemName))
            widgetSettings.ActiveWidgetSystemNames.Remove(QuickOrderDefaults.SystemName);

        // Save the updated widget settings and clear cache
        await _settingService.SaveSettingAsync(widgetSettings);
        await _settingService.ClearCacheAsync();

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

        return await Configure();
    }

    [CheckPermission(StandardPermission.Customers.CUSTOMERS_IMPERSONATION)]
    [CheckPermission(StandardPermission.Security.ACCESS_ADMIN_PANEL)]
    [CheckPermission(StandardPermission.Customers.CUSTOMERS_CREATE_EDIT_DELETE)]
    public virtual async Task<IActionResult> List()
    {
        // Prepare the search model for displaying customers in the QuickOrder widget
        var model = await _quickOrderModelFactory.PrepareCustomerSearchModelAsync(new QuickOrderSearchModel());

        return View("~/Plugins/Widgets.QuickOrder/Views/List.cshtml", model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Customers.CUSTOMERS_IMPERSONATION)]
    [CheckPermission(StandardPermission.Security.ACCESS_ADMIN_PANEL)]
    [CheckPermission(StandardPermission.Customers.CUSTOMERS_CREATE_EDIT_DELETE)]
    public virtual async Task<IActionResult> CustomerList(QuickOrderSearchModel searchModel)
    {
        // Prepare the list model based on the search parameters
        var model = await _quickOrderModelFactory.PrepareCustomerListModelAsync(searchModel);

        return Json(model);
    }

    [CheckPermission(StandardPermission.Customers.CUSTOMERS_CREATE_EDIT_DELETE)]
    public virtual async Task<IActionResult> Impersonate(int id)
    {
        // Try to fetch the customer by the provided ID
        var customer = await _customerService.GetCustomerByIdAsync(id);
        if (customer == null) // If customer is not found, redirect to the customer list
            return RedirectToAction("List");

        // Check if the customer is active; if not, show a warning notification
        if (!customer.Active)
        {
            _notificationService.WarningNotification(
                await _localizationService.GetResourceAsync("Admin.Customers.Customers.Impersonate.Inactive"));
            return RedirectToAction("List", customer.Id);
        }

        // Ensure that a non-admin user cannot impersonate as an Administrator
        // Otherwise, that user can simply impersonate as an Administrator and gain additional Administrative Privileges
        if (!await _customerService.IsAdminAsync(await _workContext.GetCurrentCustomerAsync()) && await _customerService.IsAdminAsync(customer))
        {
            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Customers.Customers.NonAdminNotImpersonateAsAdminError"));
            return RedirectToAction("List", customer.Id);
        }

        // Log the activity of impersonation
        await _customerActivityService.InsertActivityAsync("Impersonation.Started",
            string.Format(await _localizationService.GetResourceAsync("ActivityLog.Impersonation.Started.StoreOwner"), customer.Email, customer.Id), customer);
        await _customerActivityService.InsertActivityAsync(customer, "Impersonation.Started",
            string.Format(await _localizationService.GetResourceAsync("ActivityLog.Impersonation.Started.Customer"), (await _workContext.GetCurrentCustomerAsync()).Email, (await _workContext.GetCurrentCustomerAsync()).Id), await _workContext.GetCurrentCustomerAsync());

        // Set customer login requirements to false to allow impersonation
        customer.RequireReLogin = false;
        await _customerService.UpdateCustomerAsync(customer);

        // Save the impersonated customer ID and QuickOrder attribute for the current user
        await _genericAttributeService.SaveAttributeAsync<int?>(await _workContext.GetCurrentCustomerAsync(), NopCustomerDefaults.ImpersonatedCustomerIdAttribute, customer.Id);
        await _genericAttributeService.SaveAttributeAsync<bool>(customer, "QuickOrder", true);

        return RedirectToAction("Index", "Home", new { area = string.Empty });
    }

    #endregion
}
