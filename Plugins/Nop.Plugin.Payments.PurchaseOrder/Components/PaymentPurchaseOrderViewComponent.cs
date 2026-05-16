using System.Net;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Payments.PurchaseOrder.Models;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Payments.PurchaseOrder.Components
{
    /// <summary>
    /// Represents the view component to display payment info in the public store
    /// </summary>
    public class PaymentPurchaseOrderViewComponent : NopViewComponent
    {
        /// <summary>
        /// Invoke view component
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the view component result
        /// </returns>
        public IViewComponentResult Invoke()
        {
            var model = new PaymentInfoModel();
            //set postback values (we cannot access "Form" with "GET" requests)
            if (Request.Method != WebRequestMethods.Http.Get)
            {
                model.PurchaseOrderNumber = HttpContext.Request.Form["PurchaseOrderNumber"];
            }

            return View("~/Plugins/Payments.PurchaseOrder/Views/PaymentInfo.cshtml", model);
        }
    }
}
