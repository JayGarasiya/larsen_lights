using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Widgets.ProductExtension.Services;
using Nop.Web.Framework.Components;
using Nop.Core;
using Nop.Plugin.Widgets.ProductExtension.Models;
using Nop.Services.Common;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Services.Localization;
using Nop.Services.Customers;

namespace Nop.Plugin.Widgets.ProductExtension.Components
{
    /// <summary>
    /// Represents the view component to place a widget into pages
    /// </summary>
    public class DealerPriceSelectorViewComponent : NopViewComponent
    {
        #region Fields

        protected readonly IProductExtendService _productExtendService;
        protected readonly IWorkContext _workContext;
        protected readonly IStoreContext _storeContext;
        protected readonly IGenericAttributeService _genericAttributeService;
        protected readonly ILocalizationService _localizationService;
        protected readonly ICustomerService _customerService;
        protected readonly ProductExtensionSettings _productExtensionSettings;

        #endregion

        #region Ctor

        public DealerPriceSelectorViewComponent(IProductExtendService productExtendService,
            IWorkContext workContext,
            IStoreContext storeContext,
            IGenericAttributeService genericAttributeService,
            ILocalizationService localizationService,
            ICustomerService customerService,
            ProductExtensionSettings productExtensionSettings)
        {
            _productExtendService = productExtendService;
            _workContext = workContext;
            _storeContext = storeContext;
            _genericAttributeService = genericAttributeService;
            _localizationService = localizationService;
            _customerService = customerService;
            _productExtensionSettings = productExtensionSettings;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Invoke view component
        /// </summary>
        /// <param name="widgetZone">Widget zone name</param>
        /// <param name="additionalData">Additional data</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the view component result
        /// </returns>
        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            if (!await _productExtendService.PluginActiveAsync())
                return Content(string.Empty);

            var store = await _storeContext.GetCurrentStoreAsync();
            if(!_productExtensionSettings.DealerRoleIds.Any())
                return Content(string.Empty);

            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!(await _customerService.GetCustomerRolesAsync(customer)).Any(role => _productExtensionSettings.DealerRoleIds.Any(dr => dr == role.Id)))
                return Content(string.Empty);

            var model = new DealerPriceSelectorModel();

            model.CurrentPriceId = await _genericAttributeService.GetAttributeAsync(customer, ProductExtensionDefaults.DealerPriceAttribute, store.Id, 1);
            var currentPrice = string.Format(await _localizationService.GetResourceAsync("Plugins.Widgets.ProductExtension.Fields.Dealer.AllPrices"), (await _workContext.GetWorkingCurrencyAsync()).CurrencyCode);

            model.AvailablePrices.Add(new SelectListItem {
                Text = await _localizationService.GetResourceAsync("Plugins.Widgets.ProductExtension.Fields.Dealer.DealerPricing"),
                Value = "1",
                Selected = model.CurrentPriceId == 1
            });
            model.AvailablePrices.Add(new SelectListItem {
                Text = await _localizationService.GetResourceAsync("Plugins.Widgets.ProductExtension.Fields.Dealer.ListPricing"),
                Value = "2",
                Selected = model.CurrentPriceId == 2
            });

            model.AvailablePrices.Add(new SelectListItem
            {
                Text = currentPrice,
                Value = "",
                Disabled = true
            });

            return View("~/Plugins/Widgets.ProductExtension/Views/DealerPrice.cshtml", model);
        }

        #endregion
    }
}
