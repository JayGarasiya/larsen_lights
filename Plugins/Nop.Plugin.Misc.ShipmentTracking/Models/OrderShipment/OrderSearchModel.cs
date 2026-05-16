using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.ShipmentTracking.Models.OrderShipment
{
    /// <summary>
    /// Represents an order search model
    /// </summary>
    public record OrderSearchModel: Web.Areas.Admin.Models.Orders.OrderSearchModel
    {
        #region Properties

        [NopResourceDisplayName("Plugins.Misc.ShipmentTracking.Orders.PurchaseOrderNumber")]
        public string PurchaseOrderNumber { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ShipmentTracking.Orders.BillingZipPostalCode")]
        public string BillingZipPostalCode { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ShipmentTracking.Orders.TrackOrder")]
        public string TrackOrder { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ShipmentTracking.Orders.List.SearchCompany")]
        public string SearchCompany { get; set; }
        public bool CompanyEnabled { get; set; }

        #endregion
    }
}
