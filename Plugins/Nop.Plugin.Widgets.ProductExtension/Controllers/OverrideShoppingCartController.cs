using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Shipping;
using Nop.Core.Infrastructure;
using Nop.Plugin.Widgets.ProductExtension.Services;
using Nop.Services.Attributes;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Html;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Web.Controllers;
using Nop.Web.Factories;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Infrastructure.Cache;
using Nop.Web.Models.Media;

namespace Nop.Plugin.Widgets.ProductExtension.Contollers
{
    public class OverrideShoppingCartController : ShoppingCartController
    {
        #region Fields

        protected readonly IProductExtendService _productExtendService;

        #endregion

        #region Ctor
        public OverrideShoppingCartController(CaptchaSettings captchaSettings, 
            CustomerSettings customerSettings, 
            IAttributeParser<CheckoutAttribute, CheckoutAttributeValue> checkoutAttributeParser, 
            IAttributeService<CheckoutAttribute, CheckoutAttributeValue> checkoutAttributeService, 
            ICurrencyService currencyService, 
            ICustomerActivityService customerActivityService, 
            ICustomerService customerService, 
            ICustomWishlistService customWishlistService, 
            IDiscountService discountService, 
            IDownloadService downloadService, 
            IGenericAttributeService genericAttributeService, 
            IGiftCardService giftCardService, 
            IHtmlFormatter htmlFormatter, 
            ILocalizationService localizationService, 
            INopFileProvider fileProvider, 
            INopUrlHelper nopUrlHelper, 
            INotificationService notificationService, 
            IPermissionService permissionService, 
            IPictureService pictureService, 
            IPriceFormatter priceFormatter, 
            IProductAttributeParser productAttributeParser, 
            IProductAttributeService productAttributeService, 
            IProductService productService, 
            IShippingService shippingService, 
            IShoppingCartModelFactory shoppingCartModelFactory, 
            IShoppingCartService shoppingCartService, 
            IStaticCacheManager staticCacheManager, 
            IStoreContext storeContext, 
            IStoreMappingService storeMappingService, 
            ITaxService taxService, 
            IWebHelper webHelper, 
            IWorkContext workContext, 
            IWorkflowMessageService workflowMessageService, 
            MediaSettings mediaSettings, 
            OrderSettings orderSettings, 
            ShoppingCartSettings shoppingCartSettings, 
            ShippingSettings shippingSettings,
            IProductExtendService productExtendService) : base(
                captchaSettings, 
                customerSettings, 
                checkoutAttributeParser, 
                checkoutAttributeService, 
                currencyService, 
                customerActivityService, 
                customerService, 
                customWishlistService, 
                discountService, 
                downloadService, 
                genericAttributeService, 
                giftCardService, 
                htmlFormatter, 
                localizationService, 
                fileProvider, 
                nopUrlHelper, 
                notificationService, 
                permissionService, 
                pictureService, 
                priceFormatter, 
                productAttributeParser, 
                productAttributeService, 
                productService, 
                shippingService, 
                shoppingCartModelFactory, 
                shoppingCartService, 
                staticCacheManager, 
                storeContext, 
                storeMappingService, 
                taxService, 
                webHelper, 
                workContext, 
                workflowMessageService, 
                mediaSettings, 
                orderSettings, 
                shoppingCartSettings, 
                shippingSettings)
        {
            _productExtendService = productExtendService;
        }

        #endregion

        #region Methods

        //handle product attribute selection event. this way we return new price, overridden gtin/sku/mpn
        //currently we use this method on the product details pages
        [HttpPost]
        public override async Task<IActionResult> ProductDetails_AttributeChange(int productId, bool validateAttributeConditions,
            bool loadPicture, IFormCollection form)
        {
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null)
                return new NullJsonResult();

            var errors = new List<string>();
            var attributeXml = await _productAttributeParser.ParseProductAttributesAsync(product, form, errors);

            //rental attributes
            DateTime? rentalStartDate = null;
            DateTime? rentalEndDate = null;
            if (product.IsRental)
            {
                _productAttributeParser.ParseRentalDates(product, form, out rentalStartDate, out rentalEndDate);
            }

            //sku, mpn, gtin
            var sku = await _productService.FormatSkuAsync(product, attributeXml);
            var mpn = await _productService.FormatMpnAsync(product, attributeXml);
            var gtin = await _productService.FormatGtinAsync(product, attributeXml);

            //conditional attributes
            var selectAttributeValueIds = new List<string>();
            var unSelectAttributeValueIds = new List<string>();

            // calculating weight adjustment
            var attributeValues = await _productAttributeParser.ParseProductAttributeValuesAsync(attributeXml);
            var totalWeight = product.BasepriceAmount;

            foreach (var attributeValue in attributeValues)
            {
                switch (attributeValue.AttributeValueType)
                {
                    case AttributeValueType.Simple:
                        //simple attribute
                        totalWeight += attributeValue.WeightAdjustment;
                        break;
                    case AttributeValueType.AssociatedToProduct:
                        //bundled product
                        var associatedProduct = await _productService.GetProductByIdAsync(attributeValue.AssociatedProductId);
                        if (associatedProduct != null)
                            totalWeight += associatedProduct.BasepriceAmount * attributeValue.Quantity;
                        break;
                }

                var conditionalValues = await _productExtendService.GetAllProductAttributeValueConditionByValueIdAsync(attributeValue.Id);
                foreach (var item in conditionalValues)
                {
                    if (!attributeValues.Any(av => av.Id.Equals(item.ProductAttributeValueId2)))
                        selectAttributeValueIds.Add(string.Format("{0}_{1}", attributeValue.ProductAttributeMappingId, item.ProductAttributeValueId2));
                }
            }

            var ctrlAttributes = form[$"nonformchanges_{product.Id}"];
            if (!StringValues.IsNullOrEmpty(ctrlAttributes))
            {
                var selectedAttributeId = int.Parse(ctrlAttributes);
                if (selectedAttributeId > 0)
                {
                    if (!attributeValues.Any(av => av.Id.Equals(selectedAttributeId)))
                    {
                        var conditionalValues = await _productExtendService.GetAllProductAttributeValueConditionByValueIdAsync(selectedAttributeId);
                        foreach (var item in conditionalValues)
                        {
                            var conditionalVal = attributeValues.FirstOrDefault(av => av.Id.Equals(item.ProductAttributeValueId2));
                            if (conditionalVal != null)
                                unSelectAttributeValueIds.Add(string.Format("{0}_{1}", conditionalVal.ProductAttributeMappingId, item.ProductAttributeValueId2));
                        }
                    }
                }
            }

            //price
            var price = string.Empty;
            //base price
            var basepricepangv = string.Empty;
            if (!product.CustomerEntersPrice && await _permissionService.AuthorizeAsync(StandardPermission.PublicStore.DISPLAY_PRICES))
            {
                var currentStore = await _storeContext.GetCurrentStoreAsync();
                var currentCustomer = await _workContext.GetCurrentCustomerAsync();

                //get quantity entered by customer
                var quantity = 1;
                var quantityStr = form[$"addtocart_{product.Id}.EnteredQuantity"];
                if (!StringValues.IsNullOrEmpty(quantityStr) && (!int.TryParse(quantityStr, out quantity) || quantity < 1))
                    quantity = 1;

                //we do not calculate price of "customer enters price" option is enabled
                var (finalPrice, _, _) = await _shoppingCartService.GetUnitPriceAsync(product,
                    currentCustomer,
                    currentStore,
                    ShoppingCartType.ShoppingCart,
                    quantity, attributeXml, 0,
                    rentalStartDate, rentalEndDate, true);
                var (finalPriceWithDiscountBase, _) = await _taxService.GetProductPriceAsync(product, finalPrice);
                var finalPriceWithDiscount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(finalPriceWithDiscountBase, await _workContext.GetWorkingCurrencyAsync());
                price = await _priceFormatter.FormatPriceAsync(finalPriceWithDiscount);
                basepricepangv = await _priceFormatter.FormatBasePriceAsync(product, finalPriceWithDiscountBase, totalWeight);
            }

            //stock
            var stockAvailability = await _productService.FormatStockMessageAsync(product, attributeXml);

            //conditional attributes
            var enabledAttributeMappingIds = new List<int>();
            var disabledAttributeMappingIds = new List<int>();
            if (validateAttributeConditions)
            {
                var attributes = await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(product.Id);
                foreach (var attribute in attributes)
                {
                    var conditionMet = await _productAttributeParser.IsConditionMetAsync(attribute, attributeXml);
                    if (conditionMet.HasValue)
                    {
                        if (conditionMet.Value)
                            enabledAttributeMappingIds.Add(attribute.Id);
                        else
                            disabledAttributeMappingIds.Add(attribute.Id);
                    }
                }
            }

            //picture. used when we want to override a default product picture when some attribute is selected
            var pictureFullSizeUrl = string.Empty;
            var pictureDefaultSizeUrl = string.Empty;
            var pictureIds = new List<int>();
            if (loadPicture)
            {
                //first, try to get product attribute combination picture
                var pictureId = 0;
                var combination = await _productAttributeParser.FindProductAttributeCombinationAsync(product, attributeXml);
                if (combination != null)
                {
                    var combinationPictures = await _productAttributeService.GetProductAttributeCombinationPicturesAsync(combination.Id);
                    pictureIds = combinationPictures.Select(cp => cp.PictureId).ToList();
                    pictureId = combinationPictures.FirstOrDefault()?.PictureId ?? 0;
                }

                //then, let's see whether we have attribute values with pictures
                if (pictureId == 0)
                {
                    var valuePictures = await (await _productAttributeParser.ParseProductAttributeValuesAsync(attributeXml))
                        .SelectManyAwait(async attributeValue => await _productAttributeService.GetProductAttributeValuePicturesAsync(attributeValue.Id))
                        .ToListAsync();
                    pictureIds = valuePictures.Select(vp => vp.PictureId).ToList();
                    pictureId = valuePictures.FirstOrDefault()?.PictureId ?? 0;
                }

                if (pictureId > 0)
                {
                    var productAttributePictureCacheKey = _staticCacheManager.PrepareKeyForDefaultCache(NopModelCacheDefaults.ProductAttributePictureModelKey,
                        pictureId, _webHelper.IsCurrentConnectionSecured(), await _storeContext.GetCurrentStoreAsync());
                    var pictureModel = await _staticCacheManager.GetAsync(productAttributePictureCacheKey, async () =>
                    {
                        var picture = await _pictureService.GetPictureByIdAsync(pictureId);
                        string fullSizeImageUrl, imageUrl;

                        (fullSizeImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture);
                        (imageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture, _mediaSettings.ProductDetailsPictureSize);

                        return picture == null ? new PictureModel() : new PictureModel
                        {
                            FullSizeImageUrl = fullSizeImageUrl,
                            ImageUrl = imageUrl
                        };
                    });
                    pictureFullSizeUrl = pictureModel.FullSizeImageUrl;
                    pictureDefaultSizeUrl = pictureModel.ImageUrl;
                }
                else
                {
                    //then, let's load default product with pictures
                    var pictures = await _pictureService.GetPicturesByProductIdAsync(product.Id);
                    var defaultPicture = pictures.FirstOrDefault();
                    if (defaultPicture != null)
                    {
                        (pictureFullSizeUrl, defaultPicture) = await _pictureService.GetPictureUrlAsync(defaultPicture);
                        (pictureDefaultSizeUrl, defaultPicture) = await _pictureService.GetPictureUrlAsync(defaultPicture, _mediaSettings.ProductDetailsPictureSize);
                    }
                }
            }

            var isFreeShipping = product.IsFreeShipping;
            if (isFreeShipping && !string.IsNullOrEmpty(attributeXml))
            {
                isFreeShipping = await (await _productAttributeParser.ParseProductAttributeValuesAsync(attributeXml))
                    .Where(attributeValue => attributeValue.AttributeValueType == AttributeValueType.AssociatedToProduct)
                    .SelectAwait(async attributeValue => await _productService.GetProductByIdAsync(attributeValue.AssociatedProductId))
                    .AllAsync(associatedProduct => associatedProduct == null || !associatedProduct.IsShipEnabled || associatedProduct.IsFreeShipping);
            }

            return Json(new
            {
                productId,
                gtin,
                mpn,
                sku,
                price,
                basepricepangv,
                stockAvailability,
                enabledattributemappingids = enabledAttributeMappingIds.ToArray(),
                disabledattributemappingids = disabledAttributeMappingIds.ToArray(),
                pictureFullSizeUrl,
                pictureDefaultSizeUrl,
                isFreeShipping,
                message = errors.Any() ? errors.ToArray() : null,
                selectattributevalueids = selectAttributeValueIds.ToArray(),
                unselectattributevalueids = unSelectAttributeValueIds.ToArray()
            });
        }

        #endregion
    }
}
