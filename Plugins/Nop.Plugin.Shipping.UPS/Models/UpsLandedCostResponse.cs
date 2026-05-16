namespace Nop.Plugin.Shipping.UPS.Models
{
    /// <summary>
    /// Represents a ups landed cost response model
    /// </summary>
    public class UpsLandedCostResponse
    {
        public Shipment shipment { get; set; }
        public int alversion { get; set; }
        public string dpversion { get; set; }
        public string transID { get; set; }
        public object error { get; set; }
        public PerfStats perfStats { get; set; }
    }

    public class Shipment
    {
        public string currencyCode { get; set; }
        public string id { get; set; }

        public List<BrokerageFeeItem> brokerageFeeItems { get; set; }

        public decimal totalBrokerageFees { get; set; }
        public decimal totalDuties { get; set; }
        public decimal totalCommodityLevelTaxesAndFees { get; set; }
        public decimal totalShipmentLevelTaxesAndFees { get; set; }
        public decimal totalVAT { get; set; }
        public decimal totalDutyandTax { get; set; }
        public decimal grandTotal { get; set; }

        public string importCountryCode { get; set; }

        public List<ShipmentItem> shipmentItems { get; set; }
    }

    public class BrokerageFeeItem
    {
        public string chargeName { get; set; }
        public decimal chargeAmount { get; set; }
    }

    public class ShipmentItem
    {
        public string commodityId { get; set; }
        public decimal commodityDuty { get; set; }
        public decimal totalCommodityTaxesAndFees { get; set; }
        public decimal commodityVAT { get; set; }
        public decimal totalCommodityDutyandTax { get; set; }
        public string commodityCurrencyCode { get; set; }
        public bool isCalculable { get; set; }
        public string hsCode { get; set; }
    }

    public class PerfStats
    {
        public string absLayerTime { get; set; }
        public string fulfillTime { get; set; }
        public string receiptTime { get; set; }
    }
}
