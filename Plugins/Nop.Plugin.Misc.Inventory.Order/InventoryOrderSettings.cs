using Nop.Core.Configuration;

namespace Nop.Plugin.Misc.Inventory.Order
{
    /// <summary>
    /// Represents plugin settings
    /// </summary>
    public class InventoryOrderSettings : ISettings
    {
        /// <summary>
        /// Gets or sets allowed manufacturers identifiers
        /// </summary>
        public List<int> AllowedManufacturers { get; set; }
    }
}
