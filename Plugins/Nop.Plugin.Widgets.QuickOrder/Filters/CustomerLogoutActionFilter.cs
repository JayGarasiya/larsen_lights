using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Data;
using Nop.Plugin.Widgets.QuickOrder.Services;
using Nop.Services.Common;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Web.Controllers;
using Nop.Web.Framework;
using System.Net;

namespace Nop.Plugin.Widgets.QuickOrder.Filters;

/// <summary>
/// A custom action filter to handle logic related to customer logout, specifically for impersonated customers
/// in the Quick Order plugin.
/// </summary>
public class CustomerLogoutActionFilter : IAsyncActionFilter
{
    #region Fields

    protected readonly IQuickOrderService _quickOrderService;
    protected readonly IWorkContext _workContext;
    protected readonly IGenericAttributeService _genericAttributeService;
    protected readonly ICustomerActivityService _customerActivityService;
    protected readonly ILocalizationService _localizationService;

    #endregion

    #region Ctor
    public CustomerLogoutActionFilter(
        IQuickOrderService quickOrderService,
        IWorkContext workContext,
        IGenericAttributeService genericAttributeService,
        ICustomerActivityService customerActivityService,
        ILocalizationService localizationService)
    {
        _quickOrderService = quickOrderService;
        _workContext = workContext;
        _genericAttributeService = genericAttributeService;
        _customerActivityService = customerActivityService;
        _localizationService = localizationService;
    }

    #endregion

    #region Utilities

    /// <summary>
    /// Checks if a customer is impersonating another and handles necessary actions on logout.
    /// Logs activities and ensures that Quick Order settings are reset if needed.
    /// </summary>
    /// <param name="context">The action executing context.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task CheckQuickOrderImpersonatedLoginAsync(ActionExecutingContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.HttpContext.Request == null)
            return;

        // Skip if the database is not installed
        if (!DataSettingsManager.IsDatabaseInstalled())
            return;

        // Ensure the Quick Order plugin is active
        if (!await _quickOrderService.PluginActiveAsync())
            return;

        // Only proceed for GET requests
        if (!context.HttpContext.Request.Method.Equals(WebRequestMethods.Http.Get, StringComparison.InvariantCultureIgnoreCase))
            return;

        // Get the action and controller names to ensure it's the logout action of CustomerController
        if (!(context.ActionDescriptor is ControllerActionDescriptor actionDescriptor))
            return;

        var actionName = actionDescriptor?.ActionName;
        var controllerName = actionDescriptor?.ControllerName;
        if (string.IsNullOrEmpty(actionName) || string.IsNullOrEmpty(controllerName))
            return;

        if (actionDescriptor.ControllerTypeInfo != typeof(CustomerController) || actionDescriptor.ActionName != "Logout")
            return;

        // Check if the current customer is impersonating another
        if (_workContext.OriginalCustomerIfImpersonated is not null)
        {
            // Log activity for finishing impersonation
            await _customerActivityService.InsertActivityAsync(
                _workContext.OriginalCustomerIfImpersonated,
                "Impersonation.Finished",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.Impersonation.Finished.StoreOwner"),
                (await _workContext.GetCurrentCustomerAsync()).Email, (await _workContext.GetCurrentCustomerAsync()).Id),
                await _workContext.GetCurrentCustomerAsync());

            await _customerActivityService.InsertActivityAsync(
                "Impersonation.Finished",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.Impersonation.Finished.Customer"),
                _workContext.OriginalCustomerIfImpersonated.Email, _workContext.OriginalCustomerIfImpersonated.Id),
                _workContext.OriginalCustomerIfImpersonated);

            // Reset the Quick Order setting for the customer if necessary
            var quickOrder = await _genericAttributeService
                .GetAttributeAsync<bool>(await _workContext.GetCurrentCustomerAsync(), "QuickOrder");

            if (quickOrder)
            {
                await _genericAttributeService
                    .SaveAttributeAsync<bool>(await _workContext.GetCurrentCustomerAsync(), "QuickOrder", false);
            }

            // Clear the impersonation attribute
            await _genericAttributeService
                .SaveAttributeAsync<int?>(_workContext.OriginalCustomerIfImpersonated, NopCustomerDefaults.ImpersonatedCustomerIdAttribute, null);

            // Redirect back to the customer details page if Quick Order was enabled
            if (quickOrder)
                context.Result = new RedirectToActionResult("List", "QuickOrder", new { area = AreaNames.ADMIN });
        }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Executes before the action is executed. This is where we check for impersonation and handle necessary logic.
    /// </summary>
    /// <param name="context">The action executing context.</param>
    /// <param name="next">The next delegate in the filter pipeline.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Perform the check for impersonation and Quick Order-related actions
        await CheckQuickOrderImpersonatedLoginAsync(context);

        // Continue with the action if no redirect is set
        if (context.Result == null)
            await next();
    }

    #endregion
}
