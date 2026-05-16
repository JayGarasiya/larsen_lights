using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Reports.ChartsAndTables.Models
{
    public record TableReportModel : BaseNopModel
    {
        #region Properties

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Table.Menu.OrderId")]
        public int OrderId { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Table.Menu.CustomerId")]
        public int CustomerId { get; set; }
        public string CustomerEmail { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Table.Menu.ProductId")]
        public int ProductId { get; set; }
        public string ProductName { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Table.Menu.Sku")]
        public string Sku { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Table.Menu.Quantity")]
        public int Quantity { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Table.Menu.UnitPriceExclTax")]
        public string UnitPriceExclTax { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Table.Menu.LineTotalExclTax")]
        public string LineTotalExclTax { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Table.Menu.PaymentMethod")]
        public string PaymentMethod { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Table.Menu.ShippingMethod")]
        public string ShippingMethod { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Table.Menu.BillingCountry")]
        public string BillingCountry { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Table.Menu.BillingCity")]
        public string BillingCity { get; set; }

        [NopResourceDisplayName("Plugins.Reports.ChartsAndTables.Table.Menu.YearMonth")]
        public string YearMonth { get; set; }

        #endregion
    }
}
