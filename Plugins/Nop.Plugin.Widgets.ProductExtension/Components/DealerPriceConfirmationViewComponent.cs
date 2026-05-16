using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Widgets.ProductExtension.Models;
using Nop.Plugin.Widgets.ProductExtension.Services;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Web.Framework.Components;
using Nop.Web.Models.ShoppingCart;

namespace Nop.Plugin.Widgets.ProductExtension.Components
{
    /// <summary>
    /// Represents the view component to place a widget into pages
    /// </summary>
    public class DealerPriceConfirmationViewComponent : NopViewComponent
    {
        #region Fields

        protected readonly IProductExtendService _productExtendService;
        protected readonly IWorkContext _workContext;
        protected readonly IStoreContext _storeContext;
        protected readonly IGenericAttributeService _genericAttributeService;
        protected readonly ICustomerService _customerService;
        protected readonly ProductExtensionSettings _productExtensionSettings;

        #endregion

        #region Ctor

        public DealerPriceConfirmationViewComponent(IProductExtendService productExtendService,
            IWorkContext workContext,
            IStoreContext storeContext,
            IGenericAttributeService genericAttributeService,
            ICustomerService customerService,
            ProductExtensionSettings productExtensionSettings)
        {
            _productExtendService = productExtendService;
            _workContext = workContext;
            _storeContext = storeContext;
            _genericAttributeService = genericAttributeService;
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

            if (!(additionalData is ShoppingCartModel cartModel))
                return Content(string.Empty);

            if(!cartModel.IsEditable)
                return Content(string.Empty);

            var store = await _storeContext.GetCurrentStoreAsync();
            if (!_productExtensionSettings.DealerRoleIds.Any())
                return Content(string.Empty);

            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!(await _customerService.GetCustomerRolesAsync(customer)).Any(role => _productExtensionSettings.DealerRoleIds.Any(dr => dr == role.Id)))
                return Content(string.Empty);

            var model = new DealerPriceSelectorModel();

            model.CurrentPriceId = await _genericAttributeService.GetAttributeAsync(customer, ProductExtensionDefaults.DealerPriceAttribute, store.Id, 1);
            if (model.CurrentPriceId == 2)              //[1= Dealer price, 2= List price]
                model.ShowConfirmation = true;

            return View("~/Plugins/Widgets.ProductExtension/Views/DealerPriceConfirmationPopUp.cshtml", model);
        }

        #endregion
    }
}
