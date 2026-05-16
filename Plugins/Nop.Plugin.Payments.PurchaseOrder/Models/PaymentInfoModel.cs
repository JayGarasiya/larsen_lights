using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Payments.PurchaseOrder.Models
{
    /// <summary>
    /// Represents Payment Info model
    /// </summary>
    public record PaymentInfoModel : BaseNopModel
    {
        [NopResourceDisplayName("Plugins.Payment.PurchaseOrder.PurchaseOrderNumber")]
        public string PurchaseOrderNumber { get; set; }
    }
}