using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Security;
using Nop.Core.Events;
using Nop.Plugin.Widgets.ProductExtension.Services;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Web.Controllers;
using Nop.Web.Factories;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Models.Catalog;
using Nop.Core.Http;

namespace Nop.Plugin.Widgets.ProductExtension.Contollers
{
    public partial class ProductReviewController : BasePublicController
    {
        #region Fields

        protected readonly CaptchaSettings _captchaSettings;
        protected readonly CatalogSettings _catalogSettings;
        protected readonly ICustomerActivityService _customerActivityService;
        protected readonly ICustomerService _customerService;
        protected readonly IEventPublisher _eventPublisher;
        protected readonly ILocalizationService _localizationService;
        protected readonly IOrderService _orderService;
        protected readonly IProductModelFactory _productModelFactory;
        protected readonly IProductService _productService;
        protected readonly IReviewTypeService _reviewTypeService;
        protected readonly IStoreContext _storeContext;
        protected readonly IWorkContext _workContext;
        protected readonly IWorkflowMessageService _workflowMessageService;
        protected readonly LocalizationSettings _localizationSettings;
        protected readonly IPermissionService _permissionService;
        protected readonly IShoppingCartService _shoppingCartService;
        protected readonly IQuoteService _quoteService;
        protected readonly OrderSettings _orderSettings;
        protected readonly IPdfService _pdfService;
        protected readonly IDateTimeHelper _dateTimeHelper;
        protected readonly INotificationService _notificationService;
        protected readonly IGenericAttributeService _genericAttributeService;
        protected readonly ProductExtensionSettings _productExtensionSettings;
        protected readonly IProductReviewService _productReviewService;

        #endregion

        #region Ctor

        public ProductReviewController(CaptchaSettings captchaSettings,
            CatalogSettings catalogSettings,
            ICustomerActivityService customerActivityService,
            ICustomerService customerService,
            IEventPublisher eventPublisher,
            ILocalizationService localizationService,
            IOrderService orderService,
            IProductModelFactory productModelFactory,
            IProductService productService,
            IReviewTypeService reviewTypeService,
            IStoreContext storeContext,
            IWorkContext workContext,
            IWorkflowMessageService workflowMessageService,
            LocalizationSettings localizationSettings,
            IPermissionService permissionService,
            IShoppingCartService shoppingCartService,
            IQuoteService quoteService,
            OrderSettings orderSettings,
            IPdfService pdfService,
            IDateTimeHelper dateTimeHelper,
            INotificationService notificationService,
            IGenericAttributeService genericAttributeService,
            ProductExtensionSettings productExtensionSettings,
            IProductReviewService productReviewService)
        {
            _captchaSettings = captchaSettings;
            _catalogSettings = catalogSettings;
            _customerActivityService = customerActivityService;
            _customerService = customerService;
            _eventPublisher = eventPublisher;
            _localizationService = localizationService;
            _orderService = orderService;
            _productModelFactory = productModelFactory;
            _productService = productService;
            _reviewTypeService = reviewTypeService;
            _storeContext = storeContext;
            _workContext = workContext;
            _workflowMessageService = workflowMessageService;
            _localizationSettings = localizationSettings;
            _permissionService = permissionService;
            _shoppingCartService = shoppingCartService;
            _quoteService = quoteService;
            _orderSettings = orderSettings;
            _pdfService = pdfService;
            _dateTimeHelper = dateTimeHelper;
            _notificationService = notificationService;
            _genericAttributeService = genericAttributeService;
            _productExtensionSettings = productExtensionSettings;
            _productReviewService = productReviewService;
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

        #region Product Reviews

        [HttpPost, ActionName("ProductReviewsAdd")]
        [ValidateCaptcha]
        [IgnoreAntiforgeryToken]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> ProductReviewsAdd(int id, ProductReviewsModel model, bool captchaValid)
        {
            var product = await _productService.GetProductByIdAsync(id);
            var storeId = (await _storeContext.GetCurrentStoreAsync()).Id;
            if (product == null || product.Deleted || !product.Published || !product.AllowCustomerReviews ||
                !await _productReviewService.CanAddReviewAsync(product.Id,storeId))
                return Content(string.Empty);

            // validate CAPTCHA
            if (_captchaSettings.Enabled && _captchaSettings.ShowOnProductReviewPage && !captchaValid)
            {
                ModelState.AddModelError(string.Empty,
                    await _localizationService.GetResourceAsync("Common.WrongCaptchaMessage"));
            }

            await ValidateProductReviewAvailabilityAsync(product);

            if (ModelState.IsValid)
            {
                // save review
                var rating = model.AddProductReview.Rating;
                if (rating < 1 || rating > 5)
                    rating = _catalogSettings.DefaultProductRatingValue;

                var isApproved = !_catalogSettings.ProductReviewsMustBeApproved;

                var productReview = new ProductReview
                {
                    ProductId = product.Id,
                    CustomerId = (await _workContext.GetCurrentCustomerAsync()).Id,
                    Title = model.AddProductReview.Title,
                    ReviewText = model.AddProductReview.ReviewText,
                    Rating = rating,
                    HelpfulYesTotal = 0,
                    HelpfulNoTotal = 0,
                    IsApproved = isApproved,
                    CreatedOnUtc = DateTime.UtcNow,
                    StoreId = storeId
                };

                await _productReviewService.InsertProductReviewAsync(productReview);

                // add additional review type mappings
                foreach (var additionalReview in model.AddAdditionalProductReviewList)
                {
                    await _reviewTypeService.InsertProductReviewReviewTypeMappingsAsync(
                        new ProductReviewReviewTypeMapping
                        {
                            ProductReviewId = productReview.Id,
                            ReviewTypeId = additionalReview.ReviewTypeId,
                            Rating = additionalReview.Rating
                        });
                }

                // update product totals
                await _productReviewService.UpdateProductReviewTotalsAsync(product);

                // notify store owner
                if (_catalogSettings.NotifyStoreOwnerAboutNewProductReviews)
                {
                    await _workflowMessageService.SendProductReviewStoreOwnerNotificationMessageAsync(productReview,_localizationSettings.DefaultAdminLanguageId);
                }

                // activity log
                await _customerActivityService.InsertActivityAsync("PublicStore.AddProductReview", string.Format(
                    await _localizationService.GetResourceAsync("ActivityLog.PublicStore.AddProductReview"), product.Name), product);

                // raise event
                if (productReview.IsApproved)
                    await _eventPublisher.PublishAsync(new ProductReviewApprovedEvent(productReview));
            }

            model = await _productModelFactory.PrepareProductReviewsModelAsync(product);

            return View("ProductDetailReviews", model);
        }


        #endregion

        #region Download Cart Quote Pdf
        [CheckPermission(StandardPermission.PublicStore.ENABLE_WISHLIST)]
        public virtual async Task<IActionResult> CartQuote(Guid? customerGuid)
        {
            var customer = customerGuid.HasValue ?
                await _customerService.GetCustomerByGuidAsync(customerGuid.Value)
                : await _workContext.GetCurrentCustomerAsync();
            if (customer == null)
                return RedirectToRoute("Homepage");

             var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

            byte[] bytes;
            await using (var stream = new MemoryStream())
            {
                await _quoteService.PrintCartToPdfAsync(stream, cart);
                bytes = stream.ToArray();
            }
            return File(bytes, MimeTypes.ApplicationPdf, $"Cart_{customer.CustomerGuid}.pdf");
        }

        #endregion

        #region Pricing 

        //available even when navigation is not allowed
        [CheckAccessPublicStore(ignore: true)]
        public virtual async Task<IActionResult> SetPrice(int customerPrice, string returnUrl = "")
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            if ((await _customerService.GetCustomerRolesAsync(customer)).Any(role => _productExtensionSettings.DealerRoleIds.Any(dr => dr == role.Id)))
            {
                //set customer price
                await _genericAttributeService.SaveAttributeAsync(customer, ProductExtensionDefaults.DealerPriceAttribute, customerPrice == 0 ? 1 : customerPrice, (await _storeContext.GetCurrentStoreAsync()).Id);            //[1= Dealer price, 2= List price]
            }

            //home page
            if (string.IsNullOrEmpty(returnUrl))
                returnUrl = Url.RouteUrl(NopRouteNames.General.HOMEPAGE);

            //prevent open redirection attack
            if (!Url.IsLocalUrl(returnUrl))
                returnUrl = Url.RouteUrl(NopRouteNames.General.HOMEPAGE);

            return Redirect(returnUrl);
        }

        #endregion

        #endregion
    }
} 
