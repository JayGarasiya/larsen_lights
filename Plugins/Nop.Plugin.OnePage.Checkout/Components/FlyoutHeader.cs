using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Tax;
using Nop.Services.Catalog;
using Nop.Services.Directory;
using Nop.Services.Orders;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.OnePage.Checkout.Components
{
    /// <summary>
    /// Represents view component for flyout Header
    /// </summary>
    public class FlyoutHeaderViewComponent : NopViewComponent
    {
        #region Fields

        protected readonly IWorkContext _workContext;
        protected readonly IStoreContext _storeContext;
        protected readonly IShoppingCartService _shoppingCartService;
        protected readonly TaxSettings _taxSettings;
        protected readonly IOrderTotalCalculationService _orderTotalCalculationService;
        protected readonly ICurrencyService _currencyService;
        protected readonly IPriceFormatter _priceFormatter;

        #endregion

        #region Ctor

        public FlyoutHeaderViewComponent(IWorkContext workContext,
            IStoreContext storeContext,
            IShoppingCartService shoppingCartService,
            TaxSettings taxSettings,
            IOrderTotalCalculationService orderTotalCalculationService,
            ICurrencyService currencyService,
            IPriceFormatter priceFormatter)
        {
            _workContext = workContext;
            _storeContext = storeContext;
            _shoppingCartService = shoppingCartService;
            _taxSettings = taxSettings;
            _orderTotalCalculationService = orderTotalCalculationService;
            _currencyService = currencyService;
            _priceFormatter = priceFormatter;
        }

        #endregion

        #region Methods

        /// <summary>
        ///  Invoke view component
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the view component result
        /// </returns>
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var currency = await _workContext.GetWorkingCurrencyAsync();
            //performance optimization (use "HasShoppingCartItems" property)
            if (customer.HasShoppingCartItems)
            {
                var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);
                if (cart.Any())
                {
                    //subtotal
                    var subTotalIncludingTax = await _workContext.GetTaxDisplayTypeAsync() == TaxDisplayType.IncludingTax && !_taxSettings.ForceTaxExclusionFromOrderSubtotal;
                    var (_, _, _, subTotalWithoutDiscountBase, _) = await _orderTotalCalculationService.GetShoppingCartSubTotalAsync(cart, subTotalIncludingTax);
                    var subtotal = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(subTotalWithoutDiscountBase, currency);
                    var totalString = await _priceFormatter.FormatPriceAsync(subtotal, false, currency, (await _workContext.GetWorkingLanguageAsync()).Id, subTotalIncludingTax);

                    return View("~/Plugins/OnePage.Checkout/Views/Shared/Components/FlyoutHeader/Default.cshtml", totalString);
                }
            }
            return Content(string.Empty);
        }

        #endregion
    }
}
