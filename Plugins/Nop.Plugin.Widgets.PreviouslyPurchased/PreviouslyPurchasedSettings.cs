using Nop.Core.Configuration;

namespace Nop.Plugin.Widgets.PreviouslyPurchased;

/// <summary>
/// Represents settings for the Previously Purchased Products plugin.
/// </summary>
public class PreviouslyPurchasedSettings : ISettings
{
    /// <summary>
    /// Gets or sets the product type to be displayed (First purchase, Last purchase, or Random).
    /// </summary>
    public int ProductsType { get; set; }

    /// <summary>
    /// Gets or sets the number of previously purchased products to display.
    /// </summary>
    public int PreviouslyPurchasedProductsNumber { get; set; }

    /// <summary>
    /// Gets or sets the widget zone where the widget will be displayed.
    /// </summary>
    public string WidgetZone { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to display the 'Previously Purchased Products' link on the my account page.
    /// </summary>
    public bool MyAccountNavigation { get; set; }
}