using Nop.Web.Areas.Admin.Models.Orders;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Tax.TaxJar.Models;

/// <summary>
/// Represents an refund order model
/// </summary>
public partial record RefundOrderModel : BaseNopEntityModel
{
    #region Ctor
    public RefundOrderModel()
    {
        Items = new List<ItemsModel>();
        TaxRates = new List<TaxRate>();
    }
    #endregion

    #region Properties
    public bool IsLoggedInAsVendor { get; set; }

    //identifiers
    public override int Id { get; set; }
    public string CustomOrderNumber { get; set; }

    [NopResourceDisplayName("Admin.Orders.Fields.RefundedAmount")]
    public string RefundedAmount { get; set; }

    //edit totals
    [NopResourceDisplayName("Admin.Orders.Fields.Edit.OrderSubtotal")]
    public decimal OrderSubtotalInclTaxValue { get; set; }
    [NopResourceDisplayName("Admin.Orders.Fields.Edit.OrderSubtotal")]
    public decimal OrderSubtotalExclTaxValue { get; set; }
    [NopResourceDisplayName("Admin.Orders.Fields.Edit.OrderSubTotalDiscount")]
    public decimal OrderSubTotalDiscountInclTaxValue { get; set; }
    [NopResourceDisplayName("Admin.Orders.Fields.Edit.OrderSubTotalDiscount")]
    public decimal OrderSubTotalDiscountExclTaxValue { get; set; }
    [NopResourceDisplayName("Admin.Orders.Fields.Edit.OrderShipping")]
    public decimal OrderShippingInclTaxValue { get; set; }
    [NopResourceDisplayName("Admin.Orders.Fields.Edit.OrderShipping")]
    public decimal OrderShippingExclTaxValue { get; set; }
    [NopResourceDisplayName("Admin.Orders.Fields.Edit.PaymentMethodAdditionalFee")]
    public decimal PaymentMethodAdditionalFeeInclTaxValue { get; set; }
    [NopResourceDisplayName("Admin.Orders.Fields.Edit.PaymentMethodAdditionalFee")]
    public decimal PaymentMethodAdditionalFeeExclTaxValue { get; set; }
    [NopResourceDisplayName("Admin.Orders.Fields.Edit.Tax")]
    public IList<TaxRate> TaxRates { get; set; }
    public bool DisplayTaxRates { get; set; }
    public decimal TaxValue { get; set; }
    public bool PricesIncludeTax { get; set; }

    [NopResourceDisplayName("Admin.Orders.Fields.Edit.OrderTotalDiscount")]
    public decimal OrderTotalDiscountValue { get; set; }
    [NopResourceDisplayName("Admin.Orders.Fields.OrderTotal")]
    public string OrderTotal { get; set; }
    [NopResourceDisplayName("Admin.Orders.Fields.Edit.OrderTotal")]
    public decimal OrderTotalValue { get; set; }

    //items
    public IList<ItemsModel> Items { get; set; }

    //refund info
    [NopResourceDisplayName("Admin.Orders.Fields.PartialRefund.AmountToRefund")]
    public decimal AmountToRefund { get; set; }
    public decimal MaxAmountToRefund { get; set; }
    public string PrimaryStoreCurrencyCode { get; set; }
    public bool OnlineRefund { get; set; }
    public bool HasAssociatedProducts { get; set; }

    #region Nested Classes
    public partial record ItemsModel : BaseNopModel
    {
        public ItemsModel()
        {
            Item = new OrderItemModel();
            AssociatedItems = new List<OrderItemModel>();
        }

        public OrderItemModel Item { get; set; }

        public bool HasAssociatedProducts { get; set; }
        public IList<OrderItemModel> AssociatedItems { get; set; }

        public bool HasAmountMissMatch { get; set; }
        public string AmountMissMatch { get; set; }
        public string AssociatedProductsTotal { get; set; }
    }
    #endregion
    #endregion

    #region Nested Classes
    public partial record TaxRate : BaseNopModel
    {
        public string Rate { get; set; }
    }
    #endregion
}
