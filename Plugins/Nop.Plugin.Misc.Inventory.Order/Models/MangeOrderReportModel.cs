using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Inventory.Order.Models
{
    /// <summary>
    /// Represents manage order report model
    /// </summary>
    public record MangeOrderReportModel : BaseNopEntityModel
    {
        public int ManufactureId { get; set; }

        public int ProductId { get; set; }

        public string Manufacture { get; set; }

        public string Sku { get; set; }

        public string Name { get; set; }

        public int InStock { get; set; }

        public int QTY { get; set; }

        public int Needed { get; set; }

        public string OrderedQty { get; set; }

        public decimal BoxVolume { get; set; }

        public decimal TotalCartoon { get; set; }

        public decimal ProductCost { get; set; }

        public int CategoryId { get; set; }

        public bool Published { get; set; }
    }
}
