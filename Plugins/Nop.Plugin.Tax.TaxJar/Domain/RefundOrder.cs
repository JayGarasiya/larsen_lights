using Nop.Core;

namespace Nop.Plugin.Tax.TaxJar.Domain;

/// <summary>
/// Represents a refund order
/// </summary>
public partial class RefundOrder : BaseEntity
{
    #region Properties
    /// <summary>
    /// Gets or sets the order subtotal (include tax)
    /// </summary>
    public decimal OrderSubtotalInclTax { get; set; }

    /// <summary>
    /// Gets or sets the order subtotal (exclude tax)
    /// </summary>
    public decimal OrderSubtotalExclTax { get; set; }

    /// <summary>
    /// Gets or sets the order subtotal discount (include tax)
    /// </summary>
    public decimal OrderSubTotalDiscountInclTax { get; set; }

    /// <summary>
    /// Gets or sets the order subtotal discount (exclude tax)
    /// </summary>
    public decimal OrderSubTotalDiscountExclTax { get; set; }

    /// <summary>
    /// Gets or sets the order shipping (include tax)
    /// </summary>
    public decimal OrderShippingInclTax { get; set; }

    /// <summary>
    /// Gets or sets the order shipping (exclude tax)
    /// </summary>
    public decimal OrderShippingExclTax { get; set; }

    /// <summary>
    /// Gets or sets the payment method additional fee (incl tax)
    /// </summary>
    public decimal PaymentMethodAdditionalFeeInclTax { get; set; }

    /// <summary>
    /// Gets or sets the payment method additional fee (exclude tax)
    /// </summary>
    public decimal PaymentMethodAdditionalFeeExclTax { get; set; }

    /// <summary>
    /// Gets or sets the order tax
    /// </summary>
    public decimal OrderTax { get; set; }

    /// <summary>
    /// Gets or sets the tax rates
    /// </summary>
    public string TaxRates { get; set; }

    /// <summary>
    /// Gets or sets the order discount (applied to order total)
    /// </summary>
    public decimal OrderDiscount { get; set; }

    /// <summary>
    /// Gets or sets the order total
    /// </summary>
    public decimal OrderTotal { get; set; }

    #endregion
}