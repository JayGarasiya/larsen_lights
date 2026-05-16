using Nop.Core.Domain.Discounts;
using Nop.Core;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Core.Domain.Orders;
using Nop.Services.Configuration;
using Nop.Services.Orders;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Services.Common;
using Nop.Services.Payments;
using Nop.Services.Shipping;
using Nop.Services.Tax;
using Nop.Services.Attributes;

namespace Nop.Plugin.Misc.InvoicePDF.Services
{
    /// <summary>
    /// Discount service
    /// </summary>
    public partial class OverrideOrderTotalCalculationService : OrderTotalCalculationService
    {
        #region Fields
        private readonly ISettingService _settingService;
        #endregion

        #region Ctor
        public OverrideOrderTotalCalculationService(CatalogSettings catalogSettings,
            IAddressService addressService,
            IAttributeParser<CheckoutAttribute, CheckoutAttributeValue> checkoutAttributeParser,
            ICustomerService customerService,
            IDiscountService discountService,
            IGenericAttributeService genericAttributeService,
            IGiftCardService giftCardService,
            IOrderService orderService,
            IPaymentService paymentService,
            IPriceCalculationService priceCalculationService,
            IProductService productService,
            IRewardPointService rewardPointService,
            IShippingPluginManager shippingPluginManager,
            IShippingService shippingService,
            IShoppingCartService shoppingCartService,
            IStoreContext storeContext,
            ITaxService taxService,
            IWorkContext workContext,
            RewardPointsSettings rewardPointsSettings,
            ShippingSettings shippingSettings,
            ShoppingCartSettings shoppingCartSettings,
            TaxSettings taxSettings,
            ISettingService settingService) : base(catalogSettings,
                addressService,
                checkoutAttributeParser,
                customerService,
                discountService,
                genericAttributeService,
                giftCardService,
                orderService,
                paymentService,
                priceCalculationService,
                productService,
                rewardPointService,
                shippingPluginManager,
                shippingService,
                shoppingCartService,
                storeContext,
                taxService,
                workContext,
                rewardPointsSettings,
                shippingSettings,
                shoppingCartSettings,
                taxSettings)
        {
            _settingService = settingService;
        }
        #endregion

        #region Utilities

        /// <summary>
        /// Get preferred discount (with maximum discount value)
        /// </summary>
        /// <param name="discounts">A list of discounts to check</param>
        /// <param name="vendorDiscounts">A dictionary of vendor amount reduce by discounts</param>
        /// <param name="amount">Amount (initial value)</param>
        /// <param name="discountAmount">Discount amount</param>
        /// <returns>Preferred discount</returns>
        protected virtual List<Discount> GetPreferredDiscount(IList<Discount> discounts, IDictionary<int, decimal> vendorDiscounts,
            decimal amount, out decimal discountAmount)
        {
            ArgumentNullException.ThrowIfNull(discounts);

            var result = new List<Discount>();
            discountAmount = decimal.Zero;
            if (!discounts.Any())
                return result;

            //first we check simple discounts
            foreach (var discount in discounts)
            {
                var vendorDiscount = vendorDiscounts.ContainsKey(discount.Id) ? vendorDiscounts[discount.Id] : decimal.Zero;
                var currentDiscountValue = _discountService.GetDiscountAmount(discount, vendorDiscount > decimal.Zero ? (amount - vendorDiscount) : amount);
                if (currentDiscountValue <= discountAmount)
                    continue;

                discountAmount = currentDiscountValue;

                result.Clear();
                result.Add(discount);
            }
            //now let's check cumulative discounts
            //right now we calculate discount values based on the original amount value
            //please keep it in mind if you're going to use discounts with "percentage"
            var cumulativeDiscounts = discounts.Where(x => x.IsCumulative).OrderBy(x => x.Name).ToList();
            if (cumulativeDiscounts.Count <= 1)
                return result;

            var cumulativeDiscountAmount = cumulativeDiscounts.Sum(d =>
            {
                var vendorDiscount = vendorDiscounts.ContainsKey(d.Id) ? vendorDiscounts[d.Id] : decimal.Zero;
                return _discountService.GetDiscountAmount(d, vendorDiscount > decimal.Zero ? (amount - vendorDiscount) : amount);
            });

            if (cumulativeDiscountAmount <= discountAmount)
                return result;

            discountAmount = cumulativeDiscountAmount;

            result.Clear();
            result.AddRange(cumulativeDiscounts);

            return result;
        }

        /// <summary>
        /// Gets an order discount (applied to order subtotal)
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="orderSubTotal">Order subtotal</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the order discount, Applied discounts
        /// </returns>
        protected override async Task<(decimal orderDiscount, List<Discount> appliedDiscounts)> GetOrderSubtotalDiscountAsync(Customer customer,
            decimal orderSubTotal)
        {
            var discountAmount = decimal.Zero;
            if (_catalogSettings.IgnoreDiscounts)
                return (discountAmount, new List<Discount>());

            var allDiscounts = await _discountService.GetAllDiscountsAsync(DiscountType.AssignedToOrderSubTotal);
            var vendorDiscounts = new Dictionary<int, decimal>();
            var allowedDiscounts = new List<Discount>();
            if (allDiscounts?.Any() == true)
            {
                var couponCodesToValidate = await _customerService.ParseAppliedDiscountCouponCodesAsync(customer);
                foreach (var discount in allDiscounts)
                {
                    if (!_discountService.ContainsDiscount(allowedDiscounts, discount) &&
                        (await _discountService.ValidateDiscountAsync(discount, customer, couponCodesToValidate)).IsValid)
                    {

                        //exclude vendor products subtotal while discount type is assigned to order subtotal
                        if (discount.DiscountType == DiscountType.AssignedToOrderSubTotal)
                        {
                            var vendorAmount = decimal.Zero;
                            var vendorsRequirement = string.Empty;
                            var spendAmountOverRequirements = (await _discountService.GetAllDiscountRequirementsAsync(discount.Id))
                                .Where(d => !d.IsGroup && d.DiscountRequirementRuleSystemName == "DiscountRequirement.SpendAmountOver")
                                .FirstOrDefault();

                            if (spendAmountOverRequirements != null)
                                vendorsRequirement = _settingService.GetSettingByKey<string>(string.Format("DiscountRequirement.SpendAmountOverExcludedVendors-{0}", spendAmountOverRequirements.Id));

                            var restrictedvendorIds = !string.IsNullOrWhiteSpace(vendorsRequirement) ? vendorsRequirement.Split(',').Select(int.Parse).ToList() : new List<int>();
                            if (restrictedvendorIds.Any())
                            {
                                var shoppingCart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);
                                foreach (var sci in shoppingCart)
                                {
                                    var product = await _productService.GetProductByIdAsync(sci.ProductId);
                                    if (!restrictedvendorIds.Any(rv => rv == product.VendorId))
                                        continue;

                                    var (subtotal, _, _, _) = await _shoppingCartService.GetSubTotalAsync(sci, true);
                                    vendorAmount += subtotal;
                                }
                            }

                            vendorDiscounts.Add(discount.Id, vendorAmount);
                        }

                        allowedDiscounts.Add(discount);
                    }
                }
            }

            var appliedDiscounts = _discountService.GetPreferredDiscount(allowedDiscounts, orderSubTotal, out discountAmount);
            if (discountAmount < decimal.Zero)
                discountAmount = decimal.Zero;

            return (discountAmount, appliedDiscounts);
        }

        #endregion
    }
}
