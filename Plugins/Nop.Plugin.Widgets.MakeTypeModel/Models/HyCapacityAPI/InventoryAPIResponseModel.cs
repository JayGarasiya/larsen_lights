namespace Nop.Plugin.Widgets.MakeTypeModel.Models.HyCapacityAPI
{
    /// <summary>
    /// Represents an inventory API response model
    /// </summary>
    public class InventoryAPIResponseModel
    {
        public List<InventoryItemModel> Payload { get; set; }
    }

    /// <summary>
    /// Represents an inventory item model
    /// </summary>
    public class InventoryItemModel
    {
        public string HyCapStockNumber { get; set; }
        public decimal DealerPrice { get; set; }
    }

}
