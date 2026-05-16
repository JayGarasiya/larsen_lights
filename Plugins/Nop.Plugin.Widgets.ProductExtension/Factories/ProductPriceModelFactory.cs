using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Seo;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Stores;
using Nop.Core.Domain.Vendors;
using Nop.Data;
using Nop.Plugin.Widgets.ProductExtension.Services;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Shipping.Date;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Services.Vendors;
using Nop.Web.Factories;
using Nop.Web.Infrastructure.Cache;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.Media;
using System.Globalization;

namespace Nop.Plugin.Widgets.ProductExtension.Factories
{
    /// <summary>
    /// Represents the product price model factory implementation
    /// </summary>
    public class ProductPriceModelFactory : ProductModelFactory
    {
        #region Fields
        protected readonly IProductExtendService _productExtendService;
        protected readonly INopDataProvider _dataProvider;

        #endregion

        #region Ctor
        public ProductPriceModelFactory(CaptchaSettings captchaSettings, 
            CatalogSettings catalogSettings, 
            CustomerSettings customerSettings, 
            ICategoryService categoryService, 
            ICurrencyService currencyService, 
            ICustomerService customerService, 
            ICustomWishlistService customWishlistService, 
            IDateRangeService dateRangeService, 
            IDateTimeHelper dateTimeHelper, 
            IDownloadService downloadService, 
            IGenericAttributeService genericAttributeService, 
            IJsonLdModelFactory jsonLdModelFactory, 
            ILocalizationService localizationService, 
            IManufacturerService manufacturerService, 
            IPermissionService permissionService, 
            IPictureService pictureService, 
            IPriceCalculationService priceCalculationService, 
            IPriceFormatter priceFormatter, 
            IProductAttributeParser productAttributeParser, 
            IProductAttributeService productAttributeService, 
            IProductReviewService productReviewService, 
            IProductService productService, 
            IProductTagService productTagService, 
            IProductTemplateService productTemplateService, 
            IReviewTypeService reviewTypeService, 
            IShoppingCartService shoppingCartService, 
            ISpecificationAttributeService specificationAttributeService, 
            IStaticCacheManager staticCacheManager, 
            IStoreContext storeContext, 
            IStoreService storeService, 
            IShoppingCartModelFactory shoppingCartModelFactory, 
            ITaxService taxService, 
            IUrlRecordService urlRecordService, 
            IVendorService vendorService, 
            IVideoService videoService, 
            IWebHelper webHelper, 
            IWorkContext workContext, 
            MediaSettings mediaSettings, 
            OrderSettings orderSettings, 
            SeoSettings seoSettings, 
            ShippingSettings shippingSettings, 
            VendorSettings vendorSettings, 
            IProductExtendService productExtendService,
            INopDataProvider dataProvider) : base(
                captchaSettings, 
                catalogSettings, 
                customerSettings, 
                categoryService, 
                currencyService, 
                customerService, 
                customWishlistService, 
                dateRangeService, 
                dateTimeHelper, 
                downloadService, 
                genericAttributeService, 
                jsonLdModelFactory, 
                localizationService, 
                manufacturerService, 
                permissionService, 
                pictureService, 
                priceCalculationService, 
                priceFormatter, 
                productAttributeParser, 
                productAttributeService, 
                productReviewService, 
                productService, 
                productTagService, 
                productTemplateService, 
                reviewTypeService, 
                shoppingCartService, 
                specificationAttributeService, 
                staticCacheManager, 
                storeContext, 
                storeService, 
                shoppingCartModelFactory, 
                taxService, 
                urlRecordService, 
                vendorService, 
                videoService, 
                webHelper, 
                workContext, 
                mediaSettings, 
                orderSettings, 
                seoSettings, 
                shippingSettings, 
                vendorSettings)
        {
            _productExtendService = productExtendService;
            _dataProvider = dataProvider;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Prepare the associated product price
        /// </summary>
        /// <param name="product">Product</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the associated product price
        /// </returns>
        protected virtual async Task<(decimal, string)> PrepareAssociatedProductPriceAsync(Product product, Customer customer, Store store)
        {
            if (product == null)
                return (decimal.Zero, string.Empty);

            var priceAdjustment = decimal.Zero;
            var attributesXml = string.Empty;
            var productAttributeMappings = await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(product.Id);
            var productAttributeMappingsWithoutCondition = productAttributeMappings
                .Where(p => p.IsRequired && string.IsNullOrEmpty(p.ConditionAttributeXml));
            var validateAttributeConditions = productAttributeMappings.Any(p => !string.IsNullOrEmpty(p.ConditionAttributeXml));
            foreach (var attribute in productAttributeMappingsWithoutCondition)
            {
                var attributeValues = (await _productAttributeService.GetProductAttributeValuesAsync(attribute.Id)) .Where(p => p.IsPreSelected);
                foreach (var attributeValue in attributeValues)
                {
                    var valuePriceAdjustment = await _priceCalculationService.GetProductAttributeValuePriceAdjustmentAsync(product, attributeValue,
                        customer, store);

                    priceAdjustment += valuePriceAdjustment;

                    if(validateAttributeConditions)
                        attributesXml = _productAttributeParser.AddProductAttribute(attributesXml, attribute, attributeValue.Id.ToString(), null);
                }
            }

            if(!string.IsNullOrEmpty(attributesXml) && validateAttributeConditions)
            {
                var attributes = await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(product.Id);
                foreach (var attribute in productAttributeMappings)
                {
                    var conditionMet = await _productAttributeParser.IsConditionMetAsync(attribute, attributesXml);
                    if (conditionMet.HasValue)
                    {
                        if (conditionMet.Value)
                        {
                            var attributeValues = (await _productAttributeService.GetProductAttributeValuesAsync(attribute.Id)).Where(p => p.IsPreSelected);
                            foreach (var attributeValue in attributeValues)
                            {
                                var valuePriceAdjustment = await _priceCalculationService.GetProductAttributeValuePriceAdjustmentAsync(product, attributeValue,
                                    customer, store);

                                priceAdjustment += valuePriceAdjustment;

                                if (validateAttributeConditions)
                                    attributesXml = _productAttributeParser.AddProductAttribute(attributesXml, attribute, attributeValue.Id.ToString(), null);
                            }
                        }
                    }
                }
            }

            return (priceAdjustment, attributesXml);
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        protected override async Task PrepareGroupedProductPriceModelAsync(Product product, ProductPriceModel priceModel)
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            var associatedProducts = await _productService.GetAssociatedProductsAsync(product.Id,
                store.Id);

            //add to cart button (ignore "DisableBuyButton" property for grouped products)
            priceModel.DisableBuyButton =
                !await _permissionService.AuthorizeAsync(StandardPermission.PublicStore.ENABLE_SHOPPING_CART) ||
                !await _permissionService.AuthorizeAsync(StandardPermission.PublicStore.DISPLAY_PRICES);

            //add to wishlist button (ignore "DisableWishlistButton" property for grouped products)
            priceModel.DisableWishlistButton =
                !await _permissionService.AuthorizeAsync(StandardPermission.PublicStore.ENABLE_SHOPPING_CART) ||
                !await _permissionService.AuthorizeAsync(StandardPermission.PublicStore.DISPLAY_PRICES);

            //compare products
            priceModel.DisableAddToCompareListButton = !_catalogSettings.CompareProductsEnabled;
            if (!associatedProducts.Any())
                return;

            //we have at least one associated product
            if (await _permissionService.AuthorizeAsync(StandardPermission.PublicStore.DISPLAY_PRICES))
            {
                //find a minimum possible price
                decimal? minPossiblePrice = null;
                Product minPriceProduct = null;
                var customer = await _workContext.GetCurrentCustomerAsync();
                foreach (var associatedProduct in associatedProducts)
                {
                    var (_, tmpMinPossiblePrice, _, _) = await _priceCalculationService.GetFinalPriceAsync(associatedProduct, customer, store);
                    var tierPrices = await _productService.GetTierPricesByProductAsync(associatedProduct.Id);
                    if (tierPrices.Any())
                    {
                        //calculate price for the maximum quantity if we have tier prices, and choose minimal
                        tmpMinPossiblePrice = Math.Min(tmpMinPossiblePrice,
                            (await _priceCalculationService.GetFinalPriceAsync(associatedProduct, customer, store, quantity: int.MaxValue)).finalPrice);
                    }

                    var (priceAdjustment, _) = await PrepareAssociatedProductPriceAsync(associatedProduct, customer, store);
                    if (priceAdjustment > decimal.Zero)
                        tmpMinPossiblePrice += priceAdjustment;

                    if (minPossiblePrice.HasValue && tmpMinPossiblePrice >= minPossiblePrice.Value)
                        continue;
                    minPriceProduct = associatedProduct;
                    minPossiblePrice = tmpMinPossiblePrice;
                }

                if (minPriceProduct == null || minPriceProduct.CustomerEntersPrice)
                    return;

                if (minPriceProduct.CallForPrice &&
                    //also check whether the current user is impersonated
                    (!_orderSettings.AllowAdminsToBuyCallForPriceProducts ||
                     _workContext.OriginalCustomerIfImpersonated == null))
                {
                    priceModel.OldPrice = null;
                    priceModel.OldPriceValue = null;
                    priceModel.Price = await _localizationService.GetResourceAsync("Products.CallForPrice");
                    priceModel.PriceValue = null;
                }
                else
                {
                    //calculate prices
                    var (finalPriceBase, _) = await _taxService.GetProductPriceAsync(minPriceProduct, minPossiblePrice.Value);
                    var finalPrice = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(finalPriceBase, await _workContext.GetWorkingCurrencyAsync());

                    priceModel.OldPrice = null;
                    priceModel.OldPriceValue = null;
                    priceModel.Price = string.Format(await _localizationService.GetResourceAsync("Products.PriceRangeFrom"), await _priceFormatter.FormatPriceAsync(finalPrice));
                    priceModel.PriceValue = finalPrice;

                    //PAngV default baseprice (used in Germany)
                    priceModel.BasePricePAngV = await _priceFormatter.FormatBasePriceAsync(product, finalPriceBase);
                    priceModel.BasePricePAngVValue = finalPriceBase;
                }
            }
            else
            {
                //hide prices
                priceModel.OldPrice = null;
                priceModel.OldPriceValue = null;
                priceModel.Price = null;
                priceModel.PriceValue = null;
            }
        }

        /// <summary>
        /// Prepare the product attribute models
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="updatecartitem">Updated shopping cart item</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the list of product attribute model
        /// </returns>
        protected override async Task<IList<ProductDetailsModel.ProductAttributeModel>> PrepareProductAttributeModelsAsync(Product product, ShoppingCartItem updatecartitem)
        {
            ArgumentNullException.ThrowIfNull(product);

            var model = new List<ProductDetailsModel.ProductAttributeModel>();
            var store = updatecartitem != null ? await _storeService.GetStoreByIdAsync(updatecartitem.StoreId) : await _storeContext.GetCurrentStoreAsync();
            var currentCustomer = await _workContext.GetCurrentCustomerAsync();
            var customer = updatecartitem?.CustomerId is null ? currentCustomer : await _customerService.GetCustomerByIdAsync(updatecartitem.CustomerId);

            var productAttributeMapping = await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(product.Id);
            foreach (var attribute in productAttributeMapping)
            {
                var productAttrubute = await _productAttributeService.GetProductAttributeByIdAsync(attribute.ProductAttributeId);

                var attributeModel = new ProductDetailsModel.ProductAttributeModel
                {
                    Id = attribute.Id,
                    ProductId = product.Id,
                    ProductAttributeId = attribute.ProductAttributeId,
                    Name = await _localizationService.GetLocalizedAsync(productAttrubute, x => x.Name),
                    Description = await _localizationService.GetLocalizedAsync(productAttrubute, x => x.Description),
                    TextPrompt = await _localizationService.GetLocalizedAsync(attribute, x => x.TextPrompt),
                    IsRequired = attribute.IsRequired,
                    AttributeControlType = attribute.AttributeControlType,
                    DefaultValue = updatecartitem != null ? null : await _localizationService.GetLocalizedAsync(attribute, x => x.DefaultValue),
                    HasCondition = !string.IsNullOrEmpty(attribute.ConditionAttributeXml)
                };
                if (!string.IsNullOrEmpty(attribute.ValidationFileAllowedExtensions))
                {
                    attributeModel.AllowedFileExtensions = attribute.ValidationFileAllowedExtensions
                        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .ToList();
                }

                if (attribute.ShouldHaveValues())
                {
                    //values
                    var attributeValues = await _productAttributeService.GetProductAttributeValuesAsync(attribute.Id);
                    foreach (var attributeValue in attributeValues)
                    {
                        var valueModel = new ProductDetailsModel.ProductAttributeValueModel
                        {
                            Id = attributeValue.Id,
                            Name = await _localizationService.GetLocalizedAsync(attributeValue, x => x.Name),
                            ColorSquaresRgb = attributeValue.ColorSquaresRgb, //used with "Color squares" attribute type
                            IsPreSelected = attributeValue.IsPreSelected,
                            CustomerEntersQty = attributeValue.CustomerEntersQty,
                            Quantity = attributeValue.Quantity
                        };

                        //check has condition on product attribute value
                        if (attribute.AttributeControlType == AttributeControlType.Checkboxes)
                            valueModel.CustomProperties.Add("HasCondition", (await _productExtendService.ShouldHasConditionOnAttributeValueAsync(attributeValue.Id)).ToString());

                        attributeModel.Values.Add(valueModel);

                        //validate product attribute value
                        if (attributeValue.AttributeValueType == AttributeValueType.AssociatedToProduct)
                        {
                            var associatedProduct = await _productService.GetProductByIdAsync(attributeValue.AssociatedProductId);
                            if (associatedProduct != null)
                            {
                                var totalQty = product.OrderMinimumQuantity * attributeValue.Quantity;
                                var associatedProductWarnings = await _shoppingCartService.GetShoppingCartItemWarningsAsync(customer,
                                    ShoppingCartType.ShoppingCart, associatedProduct, store.Id,
                                    string.Empty, decimal.Zero, null, null, totalQty, false, 0, true, false, false, false, false);
                                if (!associatedProductWarnings.Any())
                                {
                                    var stockQuantity = await _productService.GetTotalStockQuantityAsync(associatedProduct);
                                    if (stockQuantity <= 0)
                                    {
                                        if(associatedProduct.BackorderMode == BackorderMode.AllowQtyBelow0AndNotifyCustomer)
                                        {
                                            var availableDate = (await _dataProvider.QueryAsync<DateTime?>(@$"
                                                SELECT [AvailableDateOnUTC]
                                                FROM [PoOrderItem] oi
                                                LEFT JOIN [PoOrder] o ON oi.PoOrderId = o.Id
                                                WHERE [OrderedQty] > 0 AND [ProductId] = {associatedProduct.Id} AND [AvailableDateOnUTC] IS NOT NULL AND [AvailableDateOnUTC] > GETDATE()
                                                ORDER BY [AvailableDateOnUTC]"))?.FirstOrDefault();

                                            if (availableDate != null && availableDate.HasValue)
                                            {
                                                var userDate = await _dateTimeHelper.ConvertToUserTimeAsync(availableDate.Value, DateTimeKind.Utc);
                                                var stockMessage = availableDate == null
                                                    ? await _localizationService.GetResourceAsync("Products.Availability.Backordering")
                                                    : string.Format(await _localizationService.GetResourceAsync("Products.Availability.BackorderingWithDate"),
                                                        userDate.ToShortDateString());

                                                valueModel.Name = $"{await _localizationService.GetLocalizedAsync(attributeValue, x => x.Name)} - {stockMessage}";
                                            }
                                        }
                                    }
                                }
                                else
                                    valueModel.Name = $"{await _localizationService.GetLocalizedAsync(attributeValue, x => x.Name)} - {await _localizationService.GetResourceAsync("Products.Availability.OutOfStock")}";
                            }
                        }

                        //display price if allowed
                        if (await _permissionService.AuthorizeAsync(StandardPermission.PublicStore.DISPLAY_PRICES))
                        {

                            var attributeValuePriceAdjustment = await _priceCalculationService.GetProductAttributeValuePriceAdjustmentAsync(product, attributeValue, customer, store, quantity: updatecartitem?.Quantity ?? 1);
                            var (priceAdjustmentBase, _) = await _taxService.GetProductPriceAsync(product, attributeValuePriceAdjustment);
                            var priceAdjustment = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(priceAdjustmentBase, await _workContext.GetWorkingCurrencyAsync());

                            if (attributeValue.PriceAdjustmentUsePercentage)
                            {
                                var priceAdjustmentStr = attributeValue.PriceAdjustment.ToString("G29");
                                if (attributeValue.PriceAdjustment > decimal.Zero)
                                    valueModel.PriceAdjustment = "+";
                                valueModel.PriceAdjustment += priceAdjustmentStr + "%";
                            }
                            else
                            {
                                if (priceAdjustmentBase > decimal.Zero)
                                    valueModel.PriceAdjustment = "+" + await _priceFormatter.FormatPriceAsync(priceAdjustment, false, false);
                                else if (priceAdjustmentBase < decimal.Zero)
                                    valueModel.PriceAdjustment = "-" + await _priceFormatter.FormatPriceAsync(-priceAdjustment, false, false);
                            }

                            valueModel.PriceAdjustmentValue = priceAdjustment;
                        }

                        //"image square" picture (with with "image squares" attribute type only)
                        if (attributeValue.ImageSquaresPictureId > 0)
                        {
                            var productAttributeImageSquarePictureCacheKey = _staticCacheManager.PrepareKeyForDefaultCache(NopModelCacheDefaults.ProductAttributeImageSquarePictureModelKey
                                , attributeValue.ImageSquaresPictureId,
                                    _webHelper.IsCurrentConnectionSecured(),
                                    await _storeContext.GetCurrentStoreAsync());
                            valueModel.ImageSquaresPictureModel = await _staticCacheManager.GetAsync(productAttributeImageSquarePictureCacheKey, async () =>
                            {
                                var imageSquaresPicture = await _pictureService.GetPictureByIdAsync(attributeValue.ImageSquaresPictureId);

                                (var fullSizeImageUrl, imageSquaresPicture) = await _pictureService.GetPictureUrlAsync(imageSquaresPicture);
                                (var imageUrl, imageSquaresPicture) = await _pictureService.GetPictureUrlAsync(imageSquaresPicture, _mediaSettings.ImageSquarePictureSize);

                                if (imageSquaresPicture != null)
                                {

                                    return new PictureModel
                                    {
                                        FullSizeImageUrl = fullSizeImageUrl,
                                        ImageUrl = imageUrl
                                    };
                                }

                                return new PictureModel();
                            });
                        }

                        //picture of a product attribute value
                        var pictures = await _productAttributeService.GetProductAttributeValuePicturesAsync(attributeValue.Id);
                        var picture = pictures.FirstOrDefault();
                        if (picture != null)
                        {
                            valueModel.PictureId = picture.PictureId;
                        }
                        else
                        {
                            valueModel.PictureId = 0;
                        }
                    }
                }

                //set already selected attributes (if we're going to update the existing shopping cart item)
                if (updatecartitem != null)
                {
                    switch (attribute.AttributeControlType)
                    {
                        case AttributeControlType.DropdownList:
                        case AttributeControlType.RadioList:
                        case AttributeControlType.Checkboxes:
                        case AttributeControlType.ColorSquares:
                        case AttributeControlType.ImageSquares:
                            {
                                if (!string.IsNullOrEmpty(updatecartitem.AttributesXml))
                                {
                                    //clear default selection
                                    foreach (var item in attributeModel.Values)
                                        item.IsPreSelected = false;

                                    //select new values
                                    var selectedValues = await _productAttributeParser.ParseProductAttributeValuesAsync(updatecartitem.AttributesXml);
                                    foreach (var attributeValue in selectedValues)
                                        foreach (var item in attributeModel.Values)
                                            if (attributeValue.Id == item.Id)
                                            {
                                                item.IsPreSelected = true;

                                                //set customer entered quantity
                                                if (attributeValue.CustomerEntersQty)
                                                    item.Quantity = attributeValue.Quantity;
                                            }
                                }
                            }

                            break;
                        case AttributeControlType.ReadonlyCheckboxes:
                            {
                                //values are already pre-set

                                //set customer entered quantity
                                if (!string.IsNullOrEmpty(updatecartitem.AttributesXml))
                                {
                                    foreach (var attributeValue in (await _productAttributeParser.ParseProductAttributeValuesAsync(updatecartitem.AttributesXml))
                                        .Where(value => value.CustomerEntersQty))
                                    {
                                        var item = attributeModel.Values.FirstOrDefault(value => value.Id == attributeValue.Id);
                                        if (item != null)
                                            item.Quantity = attributeValue.Quantity;
                                    }
                                }
                            }

                            break;
                        case AttributeControlType.TextBox:
                        case AttributeControlType.MultilineTextbox:
                            {
                                if (!string.IsNullOrEmpty(updatecartitem.AttributesXml))
                                {
                                    var enteredText = _productAttributeParser.ParseValues(updatecartitem.AttributesXml, attribute.Id);
                                    if (enteredText.Any())
                                        attributeModel.DefaultValue = enteredText[0];
                                }
                            }

                            break;
                        case AttributeControlType.Datepicker:
                            {
                                //keep in mind my that the code below works only in the current culture
                                var selectedDateStr = _productAttributeParser.ParseValues(updatecartitem.AttributesXml, attribute.Id);
                                if (selectedDateStr.Any())
                                {
                                    if (DateTime.TryParseExact(selectedDateStr[0], "D", CultureInfo.CurrentCulture, DateTimeStyles.None, out var selectedDate))
                                    {
                                        //successfully parsed
                                        attributeModel.SelectedDay = selectedDate.Day;
                                        attributeModel.SelectedMonth = selectedDate.Month;
                                        attributeModel.SelectedYear = selectedDate.Year;
                                    }
                                }
                            }

                            break;
                        case AttributeControlType.FileUpload:
                            {
                                if (!string.IsNullOrEmpty(updatecartitem.AttributesXml))
                                {
                                    var downloadGuidStr = _productAttributeParser.ParseValues(updatecartitem.AttributesXml, attribute.Id).FirstOrDefault();
                                    _ = Guid.TryParse(downloadGuidStr, out var downloadGuid);
                                    var download = await _downloadService.GetDownloadByGuidAsync(downloadGuid);
                                    if (download != null)
                                        attributeModel.DefaultValue = download.DownloadGuid.ToString();
                                }
                            }

                            break;
                        default:
                            break;
                    }
                }

                model.Add(attributeModel);
            }

            return model;
        }

        /// <summary>
        /// Prepare the product details picture model
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="isAssociatedProduct">Whether the product is associated</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the picture model for the default picture; All picture models
        /// </returns>
        protected override async Task<(PictureModel pictureModel, IList<PictureModel> allPictureModels, IList<VideoModel> allVideoModels)> PrepareProductDetailsPictureModelAsync(Product product, bool isAssociatedProduct)
        {
            ArgumentNullException.ThrowIfNull(product);

            //default picture size
            var defaultPictureSize = isAssociatedProduct ?
                _mediaSettings.AssociatedProductPictureSize :
                _mediaSettings.ProductDetailsPictureSize;

            //prepare picture models
            var productPicturesCacheKey = _staticCacheManager.PrepareKeyForDefaultCache(NopModelCacheDefaults.ProductDetailsPicturesModelKey
                , product, defaultPictureSize, isAssociatedProduct,
                await _workContext.GetWorkingLanguageAsync(), _webHelper.IsCurrentConnectionSecured(), await _storeContext.GetCurrentStoreAsync());
            var cachedPictures = await _staticCacheManager.GetAsync(productPicturesCacheKey, async () =>
            {
                var productName = await _localizationService.GetLocalizedAsync(product, x => x.Name);

                var pictures = await _pictureService.GetPicturesByProductIdAsync(product.Id);
                var attibutePictures = await _productExtendService.GetProductAttributeValuesByProductIdAsync(product.Id);
                var defaultPicture = pictures.FirstOrDefault();

                (var fullSizeImageUrl, defaultPicture) = await _pictureService.GetPictureUrlAsync(defaultPicture, 0, !isAssociatedProduct);
                (var imageUrl, defaultPicture) = await _pictureService.GetPictureUrlAsync(defaultPicture, defaultPictureSize, !isAssociatedProduct);

                var defaultPictureModel = new PictureModel
                {
                    ImageUrl = imageUrl,
                    FullSizeImageUrl = fullSizeImageUrl,
                    //"title" attribute
                    Title = (defaultPicture != null && !string.IsNullOrEmpty(defaultPicture.TitleAttribute)) ?
                        defaultPicture.TitleAttribute :
                        string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageLinkTitleFormat.Details"), productName),
                    //"alt" attribute
                    AlternateText = (defaultPicture != null && !string.IsNullOrEmpty(defaultPicture.AltAttribute)) ?
                        defaultPicture.AltAttribute :
                        string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageAlternateTextFormat.Details"), productName)
                };

                //all pictures
                var pictureModels = new List<PictureModel>();
                for (var i = 0; i < pictures.Count; i++)
                {
                    var picture = pictures[i];

                    if (attibutePictures.Contains(picture.Id))
                        continue;

                    (fullSizeImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture);
                    (imageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture, defaultPictureSize, !isAssociatedProduct);
                    (var thumbImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture, _mediaSettings.ProductThumbPictureSizeOnProductDetailsPage);

                    var pictureModel = new PictureModel
                    {
                        ImageUrl = imageUrl,
                        ThumbImageUrl = thumbImageUrl,
                        FullSizeImageUrl = fullSizeImageUrl,
                        Title = string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageLinkTitleFormat.Details"), productName),
                        AlternateText = string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageAlternateTextFormat.Details"), productName),
                    };
                    //"title" attribute
                    pictureModel.Title = !string.IsNullOrEmpty(picture.TitleAttribute) ?
                        picture.TitleAttribute :
                        string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageLinkTitleFormat.Details"), productName);
                    //"alt" attribute
                    pictureModel.AlternateText = !string.IsNullOrEmpty(picture.AltAttribute) ?
                        picture.AltAttribute :
                        string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageAlternateTextFormat.Details"), productName);

                    pictureModels.Add(pictureModel);
                }

                return new { DefaultPictureModel = defaultPictureModel, PictureModels = pictureModels };
            });

            var allPictureModels = cachedPictures.PictureModels;

            //all videos
            var allvideoModels = new List<VideoModel>();
            var videos = await _videoService.GetVideosByProductIdAsync(product.Id);
            foreach (var video in videos)
            {
                var videoModel = new VideoModel
                {
                    VideoUrl = video.VideoUrl,
                    Allow = _mediaSettings.VideoIframeAllow,
                    Width = _mediaSettings.VideoIframeWidth,
                    Height = _mediaSettings.VideoIframeHeight
                };

                allvideoModels.Add(videoModel);
            }
            return (cachedPictures.DefaultPictureModel, allPictureModels, allvideoModels);
        }

        /// <summary>
        /// Prepare the product price model
        /// </summary>
        /// <param name="product">Product</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product price model
        /// </returns>
        protected override async Task<ProductPriceModel> PrepareProductPriceModelAsync(Product product, bool addPriceRangeFrom = false, bool forceRedirectionAfterAddingToCart = false)
        {
            ArgumentNullException.ThrowIfNull(product);

            var model = new ProductPriceModel
            {
                ProductId = product.Id
            };

            if (await _permissionService.AuthorizeAsync(StandardPermission.PublicStore.DISPLAY_PRICES))
            {
                model.HidePrices = false;

                if (product.CustomerEntersPrice)
                {
                    model.CustomerEntersPrice = true;
                }
                else
                {
                    if (product.CallForPrice &&
                        (!_orderSettings.AllowAdminsToBuyCallForPriceProducts ||
                         _workContext.OriginalCustomerIfImpersonated == null))
                    {
                        model.CallForPrice = true;
                    }
                    else
                    {
                        var customer = await _workContext.GetCurrentCustomerAsync();
                        var store = await _storeContext.GetCurrentStoreAsync();
                        var currentCurrency = await _workContext.GetWorkingCurrencyAsync();

                        var (oldPriceBase, _) =
                            await _taxService.GetProductPriceAsync(product, product.OldPrice);

                        var (priceAdjustment, _) =
                            await PrepareAssociatedProductPriceAsync(product, customer, store);

                        var (finalPriceWithoutDiscountBase, _) =
                            await _taxService.GetProductPriceAsync(
                                product,
                                (await _priceCalculationService.GetFinalPriceAsync(
                                    product, customer, store, priceAdjustment, includeDiscounts: false)).finalPrice);

                        var (finalPriceWithDiscountBase, _) =
                            await _taxService.GetProductPriceAsync(
                                product,
                                (await _priceCalculationService.GetFinalPriceAsync(
                                    product, customer, store, priceAdjustment)).finalPrice);


                        var hasMultiplePrices = false;
                        var minPossiblePriceWithoutDiscount = finalPriceWithoutDiscountBase;
                        var minPossiblePriceWithDiscount = finalPriceWithDiscountBase;

                        if (addPriceRangeFrom)
                        {
                            (hasMultiplePrices,
                             minPossiblePriceWithoutDiscount,
                             minPossiblePriceWithDiscount) =
                                await GetFromPriceAsync(product, customer, store);
                        }

                        var tierPrices = await _productService.GetTierPricesAsync(product, customer, store);

                        // When there is just one tier price with quantity 1, there is no real tier pricing benefit
                        if (tierPrices.Any() && !(tierPrices.Count == 1 && tierPrices[0].Quantity <= 1))
                        {
                            var (tierWithoutDiscount, tierWithDiscount, _, _) =
                                await _priceCalculationService.GetFinalPriceAsync(
                                    product, customer, store, quantity: int.MaxValue);

                            minPossiblePriceWithoutDiscount =
                                Math.Min(minPossiblePriceWithoutDiscount, tierWithoutDiscount);

                            minPossiblePriceWithDiscount =
                                Math.Min(minPossiblePriceWithDiscount, tierWithDiscount);

                            hasMultiplePrices = true;
                        }


                        if (priceAdjustment > decimal.Zero)
                        {
                            minPossiblePriceWithoutDiscount += priceAdjustment;
                            minPossiblePriceWithDiscount += priceAdjustment;
                        }

                        var oldPrice =
                            await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(
                                oldPriceBase, currentCurrency);

                        var finalPrice =
                            await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(
                                minPossiblePriceWithDiscount, currentCurrency);

                        if (minPossiblePriceWithoutDiscount != oldPriceBase && oldPriceBase > decimal.Zero)
                        {
                            model.OldPrice = await _priceFormatter.FormatPriceAsync(oldPrice);
                            model.OldPriceValue = oldPrice;
                        }

                        model.Price = await _priceFormatter.FormatPriceAsync(finalPrice);
                        model.PriceValue = finalPrice;

                        if (minPossiblePriceWithoutDiscount != minPossiblePriceWithDiscount)
                        {
                            model.PriceWithDiscount = model.Price;
                            model.PriceWithDiscountValue = finalPrice;
                        }

                        if (addPriceRangeFrom && hasMultiplePrices)
                        {
                            model.Price = string.Format(
                                await _localizationService.GetResourceAsync("Products.PriceRangeFrom"),
                                model.Price);
                        }

                        model.DisplayTaxShippingInfo =
                            _catalogSettings.DisplayTaxShippingInfoProductDetailsPage &&
                            product.IsShipEnabled &&
                            !product.IsFreeShipping;

                        model.BasePricePAngV =
                            await _priceFormatter.FormatBasePriceAsync(
                                product, minPossiblePriceWithDiscount);

                        model.BasePricePAngVValue = minPossiblePriceWithDiscount;
                        model.CurrencyCode = currentCurrency.CurrencyCode;

                        if (product.IsRental)
                        {
                            model.IsRental = true;
                            var priceStr = await _priceFormatter.FormatPriceAsync(finalPrice);
                            model.RentalPrice =
                                await _priceFormatter.FormatRentalProductPeriodAsync(product, priceStr);
                            model.RentalPriceValue = finalPrice;
                        }
                    }
                }
            }
            else
            {
                model.HidePrices = true;
                model.OldPrice = null;
                model.OldPriceValue = null;
                model.Price = null;
            }

            return model;
        }


        #endregion

        #region Methods

        /// <summary>
        /// Prepare the product details model
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="updatecartitem">Updated shopping cart item</param>
        /// <param name="isAssociatedProduct">Whether the product is associated</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product details model
        /// </returns>
        public override async Task<ProductDetailsModel> PrepareProductDetailsModelAsync(Product product,
            ShoppingCartItem updatecartitem = null, bool isAssociatedProduct = false)
        {
            ArgumentNullException.ThrowIfNull(product);

            var store = await _storeContext.GetCurrentStoreAsync();
            var customer = await _workContext.GetCurrentCustomerAsync();
            var (priceAdjustment, attributesXml) = await PrepareAssociatedProductPriceAsync(product, customer, store);

            //standard properties
            var model = new ProductDetailsModel
            {
                Id = product.Id,
                Name = await _localizationService.GetLocalizedAsync(product, x => x.Name),
                ShortDescription = await _localizationService.GetLocalizedAsync(product, x => x.ShortDescription),
                FullDescription = await _localizationService.GetLocalizedAsync(product, x => x.FullDescription),
                MetaKeywords = await _localizationService.GetLocalizedAsync(product, x => x.MetaKeywords),
                MetaDescription = await _localizationService.GetLocalizedAsync(product, x => x.MetaDescription),
                MetaTitle = await _localizationService.GetLocalizedAsync(product, x => x.MetaTitle),
                SeName = await _urlRecordService.GetSeNameAsync(product),
                ProductType = product.ProductType,
                ShowSku = _catalogSettings.ShowSkuOnProductDetailsPage,
                Sku = product.Sku,
                ShowManufacturerPartNumber = _catalogSettings.ShowManufacturerPartNumber,
                FreeShippingNotificationEnabled = _catalogSettings.ShowFreeShippingNotification,
                ManufacturerPartNumber = product.ManufacturerPartNumber,
                ShowGtin = _catalogSettings.ShowGtin,
                Gtin = product.Gtin,
                ManageInventoryMethod = product.ManageInventoryMethod,
                StockAvailability = await _productService.FormatStockMessageAsync(product, attributesXml),
                HasSampleDownload = product.IsDownload && product.HasSampleDownload,
                DisplayDiscontinuedMessage = !product.Published && _catalogSettings.DisplayDiscontinuedMessageForUnpublishedProducts,
                AvailableEndDate = product.AvailableEndDateTimeUtc,
                VisibleIndividually = product.VisibleIndividually,
                AllowAddingOnlyExistingAttributeCombinations = product.AllowAddingOnlyExistingAttributeCombinations
            };

            //automatically generate product description?
            if (_seoSettings.GenerateProductMetaDescription && string.IsNullOrEmpty(model.MetaDescription))
            {
                //based on short description
                model.MetaDescription = model.ShortDescription;
            }

            //shipping info
            model.IsShipEnabled = product.IsShipEnabled;
            if (product.IsShipEnabled)
            {
                model.IsFreeShipping = product.IsFreeShipping;
                //delivery date
                var deliveryDate = await _dateRangeService.GetDeliveryDateByIdAsync(product.DeliveryDateId);
                if (deliveryDate != null)
                {
                    model.DeliveryDate = await _localizationService.GetLocalizedAsync(deliveryDate, dd => dd.Name);
                }
            }

            //email a friend
            model.EmailAFriendEnabled = _catalogSettings.EmailAFriendEnabled;
            //compare products
            model.CompareProductsEnabled = _catalogSettings.CompareProductsEnabled;
            //store name
            model.CurrentStoreName = await _localizationService.GetLocalizedAsync(store, x => x.Name);

            //vendor details
            if (_vendorSettings.ShowVendorOnProductDetailsPage)
            {
                var vendor = await _vendorService.GetVendorByIdAsync(product.VendorId);
                if (vendor != null && !vendor.Deleted && vendor.Active)
                {
                    model.ShowVendor = true;

                    model.VendorModel = new VendorBriefInfoModel
                    {
                        Id = vendor.Id,
                        Name = await _localizationService.GetLocalizedAsync(vendor, x => x.Name),
                        SeName = await _urlRecordService.GetSeNameAsync(vendor),
                    };
                }
            }

            //page sharing
            if (_catalogSettings.ShowShareButton && !string.IsNullOrEmpty(_catalogSettings.PageShareCode))
            {
                var shareCode = _catalogSettings.PageShareCode;
                if (_webHelper.IsCurrentConnectionSecured())
                {
                    //need to change the add this link to be https linked when the page is, so that the page doesn't ask about mixed mode when viewed in https...
                    shareCode = shareCode.Replace("http://", "https://");
                }

                model.PageShareCode = shareCode;
            }

            switch (product.ManageInventoryMethod)
            {
                case ManageInventoryMethod.DontManageStock:
                    model.InStock = true;
                    break;

                case ManageInventoryMethod.ManageStock:
                    model.InStock = product.BackorderMode != BackorderMode.NoBackorders
                        || await _productService.GetTotalStockQuantityAsync(product) > 0;
                    model.DisplayBackInStockSubscription = !model.InStock && product.AllowBackInStockSubscriptions;
                    break;

                case ManageInventoryMethod.ManageStockByAttributes:
                    model.InStock = (await _productAttributeService
                        .GetAllProductAttributeCombinationsAsync(product.Id))
                        ?.Any(c => c.StockQuantity > 0 || c.AllowOutOfStockOrders)
                        ?? false;

                    if (!model.StockAvailability.Contains("stock-error"))
                        model.InStock = true;

                    break;
            }

            //breadcrumb
            //do not prepare this model for the associated products. anyway it's not used
            if (_catalogSettings.CategoryBreadcrumbEnabled && !isAssociatedProduct)
            {
                model.Breadcrumb = await PrepareProductBreadcrumbModelAsync(product);
            }

            //product tags
            //do not prepare this model for the associated products. anyway it's not used
            if (!isAssociatedProduct)
            {
                model.ProductTags = await PrepareProductTagModelsAsync(product);
            }

            //pictures and videos
            model.DefaultPictureZoomEnabled = _mediaSettings.DefaultPictureZoomEnabled;
            IList<PictureModel> allPictureModels;
            IList<VideoModel> allVideoModels;
            (model.DefaultPictureModel, allPictureModels, allVideoModels) = await PrepareProductDetailsPictureModelAsync(product, isAssociatedProduct);
            model.PictureModels = allPictureModels;
            model.VideoModels = allVideoModels;

            //price
            model.ProductPrice = await PrepareProductPriceModelAsync(product);

            //'Add to cart' model
            model.AddToCart = await PrepareProductAddToCartModelAsync(product, updatecartitem);
            
            //gift card
            if (product.IsGiftCard)
            {
                model.GiftCard.IsGiftCard = true;
                model.GiftCard.GiftCardType = product.GiftCardType;

                if (updatecartitem == null)
                {
                    model.GiftCard.SenderName = await _customerService.GetCustomerFullNameAsync(customer);
                    model.GiftCard.SenderEmail = customer.Email;
                }
                else
                {
                    _productAttributeParser.GetGiftCardAttribute(updatecartitem.AttributesXml,
                        out var giftCardRecipientName, out var giftCardRecipientEmail,
                        out var giftCardSenderName, out var giftCardSenderEmail, out var giftCardMessage);

                    model.GiftCard.RecipientName = giftCardRecipientName;
                    model.GiftCard.RecipientEmail = giftCardRecipientEmail;
                    model.GiftCard.SenderName = giftCardSenderName;
                    model.GiftCard.SenderEmail = giftCardSenderEmail;
                    model.GiftCard.Message = giftCardMessage;
                }
            }

            //product attributes
            model.ProductAttributes = await PrepareProductAttributeModelsAsync(product, updatecartitem);

            //no need to show old price if there was kit product
            if(model.ProductAttributes.Any())
            {
                model.ProductPrice.OldPrice = string.Empty;
                model.ProductPrice.OldPriceValue = null;

                if (!string.IsNullOrWhiteSpace(model.ProductPrice.PriceWithDiscount))
                    model.ProductPrice.Price = string.Empty;
            }

            //product specifications
            //do not prepare this model for the associated products. anyway it's not used
            if (!isAssociatedProduct)
            {
                model.ProductSpecificationModel = await PrepareProductSpecificationModelAsync(product);
            }

            //product review overview
            model.ProductReviewOverview = await PrepareProductReviewOverviewModelAsync(product);

            //tier prices
            var tierPrices = await _productService.GetTierPricesByProductAsync(product.Id);

            if (tierPrices.Any() &&
                await _permissionService.AuthorizeAsync(StandardPermission.PublicStore.DISPLAY_PRICES))
            {
                model.TierPrices = await PrepareProductTierPriceModelsAsync(product);
            }


            //manufacturers
            model.ProductManufacturers = await PrepareProductManufacturerModelsAsync(product);

            //rental products
            if (product.IsRental)
            {
                model.IsRental = true;
                //set already entered dates attributes (if we're going to update the existing shopping cart item)
                if (updatecartitem != null)
                {
                    model.RentalStartDate = updatecartitem.RentalStartDateUtc;
                    model.RentalEndDate = updatecartitem.RentalEndDateUtc;
                }
            }

            //estimate shipping
            if (_shippingSettings.EstimateShippingProductPageEnabled && !model.IsFreeShipping)
            {
                var wrappedProduct = new ShoppingCartItem
                {
                    StoreId = store.Id,
                    ShoppingCartTypeId = (int)ShoppingCartType.ShoppingCart,
                    CustomerId = customer.Id,
                    ProductId = product.Id,
                    CreatedOnUtc = DateTime.UtcNow
                };

                var estimateShippingModel = await _shoppingCartModelFactory.PrepareEstimateShippingModelAsync(new[] { wrappedProduct });

                model.ProductEstimateShipping.ProductId = product.Id;
                model.ProductEstimateShipping.RequestDelay = estimateShippingModel.RequestDelay;
                model.ProductEstimateShipping.Enabled = estimateShippingModel.Enabled;
                model.ProductEstimateShipping.CountryId = estimateShippingModel.CountryId;
                model.ProductEstimateShipping.StateProvinceId = estimateShippingModel.StateProvinceId;
                model.ProductEstimateShipping.ZipPostalCode = estimateShippingModel.ZipPostalCode;
                model.ProductEstimateShipping.UseCity = estimateShippingModel.UseCity;
                model.ProductEstimateShipping.City = estimateShippingModel.City;
                model.ProductEstimateShipping.AvailableCountries = estimateShippingModel.AvailableCountries;
                model.ProductEstimateShipping.AvailableStates = estimateShippingModel.AvailableStates;
            }

            //associated products
            if (product.ProductType == ProductType.GroupedProduct)
            {
                //ensure no circular references
                if (!isAssociatedProduct)
                {
                    var associatedProducts = await _productService.GetAssociatedProductsAsync(product.Id, store.Id);
                    foreach (var associatedProduct in associatedProducts)
                        model.AssociatedProducts.Add(await PrepareProductDetailsModelAsync(associatedProduct, null, true));
                }
                model.InStock = model.AssociatedProducts.Any(associatedProduct => associatedProduct.InStock);
            }

            return model;
        }

        #endregion
    }
}
