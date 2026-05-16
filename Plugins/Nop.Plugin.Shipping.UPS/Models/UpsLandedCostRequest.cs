namespace Nop.Plugin.Shipping.UPS.Models
{
    /// <summary>
    /// Represents a ups landed cost request model
    /// </summary>
    public class UpsLandedCostRequest
    {
        public UpsLandedCostRequest()
        {
            shipment = new ShipmentRequest();
        }

        public string currencyCode { get; set; }
        public string transID { get; set; }
        public bool allowPartialLandedCostResult { get; set; }
        public ShipmentRequest shipment { get; set; }
    }

    public class ShipmentRequest
    {
        public ShipmentRequest()
        {
            shipmentItems = new List<ShipmentItemRequest>();
        }

        public string id { get; set; }
        public string importCountryCode { get; set; }
        public string exportCountryCode { get; set; }
        public string shipmentType { get; set; }
        public List<ShipmentItemRequest> shipmentItems { get; set; }
    }

    public class ShipmentItemRequest
    {
        public string commodityId { get; set; }
        public string priceEach { get; set; }
        public string hsCode { get; set; }
        public int quantity { get; set; }
        public string uom { get; set; }
        public string originCountryCode { get; set; }
        public string commodityCurrencyCode { get; set; }
    }
}
