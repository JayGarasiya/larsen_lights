using Nop.Core;

namespace Nop.Plugin.Tax.TaxJar.Domain;

/// <summary>
/// Represents a refund order model
/// </summary>
public partial class RefundOrderItem : BaseEntity
{
    #region Properties
    /// <summary>
    /// Gets or sets the product identifier
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Gets or sets the associated product identifier
    /// </summary>
    public int AssociatedProductId { get; set; }

    /// <summary>
    /// Gets or sets the quantity
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Gets or sets the unit price in primary store currency (include tax)
    /// </summary>
    public decimal UnitPriceInclTax { get; set; }

    /// <summary>
    /// Gets or sets the unit price in primary store currency (exclude tax)
    /// </summary>
    public decimal UnitPriceExclTax { get; set; }

    /// <summary>
    /// Gets or sets the price in primary store currency (include tax)
    /// </summary>
    public decimal PriceInclTax { get; set; }

    /// <summary>
    /// Gets or sets the price in primary store currency (exclude tax)
    /// </summary>
    public decimal PriceExclTax { get; set; }

    /// <summary>
    /// Gets or sets the original cost of this order item (when an order was placed), qty 1
    /// </summary>
    public decimal OriginalProductCost { get; set; }
    #endregion
}
