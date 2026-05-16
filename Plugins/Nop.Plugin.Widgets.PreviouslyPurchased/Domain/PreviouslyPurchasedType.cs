namespace Nop.Plugin.Widgets.PreviouslyPurchased.Domain;

/// <summary>
/// Represents the type of previously purchased products to be displayed.
/// </summary>
public enum PreviouslyPurchasedType
{
    /// <summary>
    /// Display products based on the customer's first purchase.
    /// </summary>
    FirstPurchase = 10,

    /// <summary>
    /// Display products based on the customer's last purchase.
    /// </summary>
    LastPurchase = 20,

    /// <summary>
    /// Display products in a random order.
    /// </summary>
    RandomNumber = 30
}
