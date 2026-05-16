using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Orders;
using Nop.Services.Tax;
using Nop.Web.Areas.Admin.Models.Orders;
using Nop.Web.Framework.Components;
using Nop.Web.Framework.Infrastructure;

namespace Nop.Plugin.Tax.TaxJar.Areas.Admin.Components;

/// <summary>
/// Represents a view component to add the resend invoice buttons on Order details page
/// </summary>
public class ResendInvoiceViewComponent : NopViewComponent
{
    #region Fields
    private readonly ITaxPluginManager _taxPluginManager;
    private readonly IWorkContext _workContext;
    private readonly IOrderService _orderService;
    #endregion

    #region Ctor
    public ResendInvoiceViewComponent(
        ITaxPluginManager taxPluginManager,
        IWorkContext workContext,
        IOrderService orderService)
    {
        _taxPluginManager = taxPluginManager;
        _workContext = workContext;
        _orderService = orderService;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Invoke the widget view component
    /// </summary>
    /// <param name="widgetZone">The name of the widget zone where the widget will be displayed.</param>
    /// <param name="additionalData">Any additional data that may be passed to the component.</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the view component result
    /// </returns>
    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
    {
        // Ensure that tax jar tax provider is active
        if (!await _taxPluginManager.IsPluginActiveAsync(TaxJarDefaults.SystemName, await _workContext.GetCurrentCustomerAsync()))
            return Content(string.Empty);

        // Ensure that it's a proper widget zone
        if (!widgetZone.Equals(AdminWidgetZones.OrderDetailsButtons))
            return Content(string.Empty);

        if (additionalData is not OrderModel model)
            return Content(string.Empty);

        // Try to get an order with the specified id
        var order = await _orderService.GetOrderByIdAsync(model.Id);
        if (order == null || order.Deleted)
            return Content(string.Empty);

        // Let's ensure that at least 3600 seconds passed after order is placed
        // P.S. there's no any particular reason for that. we just do it
        if ((DateTime.UtcNow - order.CreatedOnUtc).TotalSeconds < 3600)
            return Content(string.Empty);

        return View("~/Plugins/Tax.TaxJar/Areas/Admin/Views/Order/ResendInvoice.cshtml", model);
    }
    #endregion
}