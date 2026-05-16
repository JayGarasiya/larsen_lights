using Nop.Core.Configuration;

namespace Nop.Plugin.OnePage.Checkout
{
    /// <summary>
    /// Represents a plugin settings
    /// </summary>
    public class OnePageCheckoutSettings : ISettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether to use enables/disables the one page checkout
        /// </summary>
        public bool EnableOnePageCheckout { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use enables/disables the login/register popup
        /// </summary>
        public bool LoginRegisterPopUp { get; set; }
    }
}
