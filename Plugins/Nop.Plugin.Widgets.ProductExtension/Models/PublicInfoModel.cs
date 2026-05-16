using Nop.Web.Framework.Models;

namespace Nop.Plugin.Widgets.ProductExtension.Models
{
    /// <summary>
    /// Represents a ProductNote model
    /// </summary>
    public partial record PublicInfoModel : BaseNopEntityModel
    {
        #region Properties

        public string Name { get; set; }

        public string Description { get; set; }

        public bool ShowDisplayName { get; set; }
        
        #endregion
    }
}