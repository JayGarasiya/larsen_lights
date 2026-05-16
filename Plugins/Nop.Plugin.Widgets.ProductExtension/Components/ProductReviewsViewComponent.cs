using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Widgets.ProductExtension.Services;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Web.Factories;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Widgets.ProductExtension.Components
{
    /// <summary>
    /// Represents the view component to place a widget into pages
    /// </summary>
    [ViewComponent(Name = ProductExtensionDefaults.VIEW_COMPONENT_PRODUCTREVIEW)]
    public class ProductReviewsViewComponent : NopViewComponent
    {
        #region Fields

        protected readonly IProductExtendService _productExtendService;
        protected readonly IProductService _productService;
        protected readonly IProductModelFactory _productModelFactory;
        protected readonly CatalogSettings _catalogSettings;
        protected readonly IWorkContext _workContext;
        protected readonly ICustomerService _customerService;
        protected readonly ILocalizationService _localizationService;
        protected readonly IOrderService _orderService;
        protected readonly ISettingService _settingService;

        #endregion

        #region Ctor

        public ProductReviewsViewComponent(IProductExtendService productExtendService,
            IProductService productService,
            IProductModelFactory productModelFactory,
            CatalogSettings catalogSettings,
            IWorkContext workContext,
            ICustomerService customerService,
            ILocalizationService localizationService,
            IOrderService orderService,
            ISettingService settingService)
        {
            _productExtendService = productExtendService;
            _productService = productService;
            _productModelFactory = productModelFactory;
            _catalogSettings = catalogSettings;
            _workContext = workContext;
            _customerService = customerService;
            _localizationService = localizationService;
            _orderService = orderService;
            _settingService = settingService;
        }

        #endregion

        #region Utilities

        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task ValidateProductReviewAvailabilityAsync(Product product)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (await _customerService.IsGuestAsync(customer) && !_catalogSettings.AllowAnonymousUsersToReviewProduct)
                ModelState.AddModelError(string.Empty, await _localizationService.GetResourceAsync("Reviews.OnlyRegisteredUsersCanWriteReviews"));

            if (!_catalogSettings.ProductReviewPossibleOnlyAfterPurchasing)
                return;

            var hasCompletedOrders = product.ProductType == ProductType.SimpleProduct
                ? await HasCompletedOrdersAsync(product)
                : await (await _productService.GetAssociatedProductsAsync(product.Id)).AnyAwaitAsync(HasCompletedOrdersAsync);

            if (!hasCompletedOrders)
                ModelState.AddModelError(string.Empty, await _localizationService.GetResourceAsync("Reviews.ProductReviewPossibleOnlyAfterPurchasing"));
        }

        protected virtual async ValueTask<bool> HasCompletedOrdersAsync(Product product)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            return (await _orderService.SearchOrdersAsync(customerId: customer.Id,
                productId: product.Id,
                osIds: new List<int> { (int)OrderStatus.Complete },
                pageSize: 1)).Any();
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
            if (await _settingService.GetSettingByKeyAsync("tabsettings.enableproductreviewstab", false))
                return Content(string.Empty);

            if (!await _productExtendService.PluginActiveAsync())
                return Content(string.Empty);

            if (!(additionalData is int productId))
                return Content(string.Empty);

            //load product
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null || product.Deleted || !product.Published || !product.AllowCustomerReviews)
                return Content(string.Empty);

            var model = await _productModelFactory.PrepareProductReviewsModelAsync(product);
            await ValidateProductReviewAvailabilityAsync(product);

            //default value
            model.AddProductReview.Rating = _catalogSettings.DefaultProductRatingValue;

            //default value for all additional review types
            if (model.ReviewTypeList.Count > 0)
                foreach (var additionalProductReview in model.AddAdditionalProductReviewList)
                {
                    additionalProductReview.Rating = additionalProductReview.IsRequired ? _catalogSettings.DefaultProductRatingValue : 0;
                }

            return View(model);
        }

        #endregion
    }
}