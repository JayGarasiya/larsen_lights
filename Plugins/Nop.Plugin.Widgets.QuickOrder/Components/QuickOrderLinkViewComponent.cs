using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Widgets.QuickOrder.Services;
using Nop.Services.Security;
using Nop.Web.Framework.Components;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Widgets.QuickOrder.Components;

/// <summary>
/// Represents the view component that renders the quick order widget in specified zones.
/// </summary>
public class QuickOrderLinkViewComponent : NopViewComponent
{
    #region Fields

    protected readonly IWorkContext _workContext;
    protected readonly IQuickOrderService _quickOrderService;

    #endregion

    #region Ctor

    public QuickOrderLinkViewComponent(
        IWorkContext workContext,
        IQuickOrderService quickOrderService)
    {
        _workContext = workContext;
        _quickOrderService = quickOrderService;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Invokes the view component to determine if the widget should be rendered.
    /// Checks for plugin activation and user permissions.
    /// </summary>
    /// <param name="widgetZone">The name of the widget zone where the widget will be displayed.</param>
    /// <param name="additionalData">Any additional data that may be passed to the component.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains the view component result.
    /// </returns>
    [CheckPermission(StandardPermission.Security.ACCESS_ADMIN_PANEL)]
    [CheckPermission(StandardPermission.Customers.CUSTOMERS_CREATE_EDIT_DELETE)]
    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
    {
        // If the Quick Order plugin is not active, return an empty result (no widget displayed)
        if (!await _quickOrderService.PluginActiveAsync())
            return Content(string.Empty);

        // If the user is impersonating another customer, hide the widget
        if (_workContext.OriginalCustomerIfImpersonated is not null)
            return Content(string.Empty);

        // If the user is a vendor, hide the widget (vendors should not see this widget)
        if (await _workContext.GetCurrentVendorAsync() is not null)
            return Content(string.Empty);

        return View("~/Plugins/Widgets.QuickOrder/Views/PublicInfo.cshtml");
    }

    #endregion
}
