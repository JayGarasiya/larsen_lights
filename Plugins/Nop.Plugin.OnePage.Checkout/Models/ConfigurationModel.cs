using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.OnePage.Checkout.Models
{
    /// <summary>
    /// Represents configuration model
    /// </summary>
    public record ConfigurationModel : BaseNopModel
    {
        #region Properties

        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Plugins.OnePage.Checkout.Fields.EnableOnePageCheckout")]
        public bool EnableOnePageCheckout { get; set; }
        public bool EnableOnePageCheckout_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.OnePage.Checkout.Fields.LoginRegisterPopUp")]
        public bool LoginRegisterPopUp { get; set; }
        public bool LoginRegisterPopUp_OverrideForStore { get; set; }

        #endregion
    }
}
