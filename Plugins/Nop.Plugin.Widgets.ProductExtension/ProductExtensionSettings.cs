using Nop.Core.Configuration;

namespace Nop.Plugin.Widgets.ProductExtension
{
    /// <summary>
    /// Represents plugin Product extension settings
    /// </summary>
    public class ProductExtensionSettings : ISettings
    {
        public ProductExtensionSettings()
        {
            DealerRoleIds = new List<int>();
        }

        /// <summary>
        /// Gets or sets dealer role Ids
        /// </summary>
        public List<int> DealerRoleIds { get; set; }
    }
}
