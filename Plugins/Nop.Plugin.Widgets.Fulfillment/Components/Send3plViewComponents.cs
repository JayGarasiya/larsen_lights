using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Widgets.Fulfillment.Domain;
using Nop.Plugin.Widgets.Fulfillment.Services.ThreePl;
using Nop.Services.Orders;
using Nop.Web.Areas.Admin.Models.Orders;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Widgets.Fulfillment.Components
{  
    /// <summary>
    /// Represents the view component to place a widget into pages
    /// </summary>
    public class Send3plViewComponents : NopViewComponent
    {
        #region Fields

        protected readonly IOrderService _orderService;
        protected readonly IThreePlService _threePlService;

        #endregion

        #region Ctor

        public Send3plViewComponents(IOrderService orderService,
            IThreePlService threePlService)
        {
            _orderService = orderService;
            _threePlService = threePlService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Invoke view component
        /// </summary>
        /// <param name="widgetZone">Widget zone name</param>
        /// <param name="additionalData">Additional data</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the view component result
        /// </returns>
        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            if (!(additionalData is OrderModel model))
                return Content(string.Empty);

            //try to get an order with the specified id
            var order = await _orderService.GetOrderByIdAsync(model.Id);
            if (order == null || order.Deleted)
                return Content(string.Empty);

            //check payment status
            if(order.PaymentStatus != PaymentStatus.Paid && !order.PaymentMethodSystemName.Equals("Payments.PurchaseOrder"))
                return Content(string.Empty);

            //check plugin was enable to sync data with 3PL
            if (!await _threePlService.PluginActiveAsync())
                return Content(string.Empty);

            var threePlRecord = await _threePlService.GetThreePlRecordByOrderIdAsync(order.Id);
            if(threePlRecord != null)
            {
                if (threePlRecord.ThreePlStutus == ThreePlStutus.Sent || threePlRecord.ThreePlStutus == ThreePlStutus.PartiallySent)
                    model.CustomProperties.Add("Synced", "true");

                if (threePlRecord.ThreePlStutus == ThreePlStutus.NotRequire || threePlRecord.ThreePlStutus == ThreePlStutus.Cancel)
                    return Content(string.Empty);
            }

            return View("~/Plugins/Widgets.Fulfillment/Views/Send3pl.cshtml", model);
        }

        #endregion
    }
}
