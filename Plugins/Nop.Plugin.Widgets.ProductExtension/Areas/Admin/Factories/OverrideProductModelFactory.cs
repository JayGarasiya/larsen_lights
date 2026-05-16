using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Vendors;
using Nop.Plugin.Widgets.ProductExtension.Services;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.FilterLevels;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Factories;
using Nop.Web.Framework.Models.Extensions;
using System.Net;

namespace Nop.Plugin.Widgets.ProductExtension.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the override product model factory implementation
    /// </summary>
    public class OverrideProductModelFactory : ProductModelFactory
    {
        #region Fields

        protected readonly IProductExtendService _productExtendService;

        #endregion

        #region Ctor
        public OverrideProductModelFactory(CatalogSettings catalogSettings, 
            CurrencySettings currencySettings, 
            IAddressService addressService, 
            IBaseAdminModelFactory baseAdminModelFactory, 
            ICategoryService categoryService, 
            ICurrencyService currencyService, 
            ICustomerService customerService, 
            IDateTimeHelper dateTimeHelper, 
            IDiscountService discountService, 
            IDiscountSupportedModelFactory discountSupportedModelFactory, 
            IFilterLevelValueService filterLevelValueService, 
            ILanguageService languageService, 
            ILocalizationService localizationService, 
            ILocalizedModelFactory localizedModelFactory, 
            IManufacturerService manufacturerService, 
            IMeasureService measureService, 
            IOrderService orderService, 
            IPictureService pictureService, 
            IPriceFormatter priceFormatter, 
            IProductAttributeFormatter productAttributeFormatter, 
            IProductAttributeParser productAttributeParser, 
            IProductAttributeService productAttributeService, 
            IProductService productService, 
            IProductTagService productTagService, 
            IProductTemplateService productTemplateService, 
            ISettingModelFactory settingModelFactory, 
            ISettingService settingService, 
            IShipmentService shipmentService, 
            IShoppingCartService shoppingCartService, 
            ISpecificationAttributeService specificationAttributeService, 
            IStoreMappingSupportedModelFactory storeMappingSupportedModelFactory, 
            IStoreContext storeContext, 
            IStoreService storeService, 
            IUrlRecordService urlRecordService, 
            IVideoService videoService, 
            IWarehouseService warehouseService, 
            IWorkContext workContext, 
            MeasureSettings measureSettings, 
            NopHttpClient nopHttpClient, 
            TaxSettings taxSettings, 
            VendorSettings vendorSettings,
            IProductExtendService productExtendService) : base(
                catalogSettings, 
                currencySettings, 
                addressService, 
                baseAdminModelFactory, 
                categoryService, 
                currencyService, 
                customerService, 
                dateTimeHelper, 
                discountService, 
                discountSupportedModelFactory, 
                filterLevelValueService, 
                languageService, 
                localizationService, 
                localizedModelFactory, 
                manufacturerService, 
                measureService, 
                orderService, 
                pictureService, 
                priceFormatter, 
                productAttributeFormatter, 
                productAttributeParser, 
                productAttributeService, 
                productService, 
                productTagService, 
                productTemplateService, 
                settingModelFactory, 
                settingService, 
                shipmentService, 
                shoppingCartService, 
                specificationAttributeService, 
                storeMappingSupportedModelFactory, 
                storeContext, 
                storeService, 
                urlRecordService, 
                videoService, 
                warehouseService, 
                workContext, 
                measureSettings, 
                nopHttpClient, 
                taxSettings, 
                vendorSettings)
        {
            _productExtendService = productExtendService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Prepare paged product attribute value list model
        /// </summary>
        /// <param name="searchModel">Product attribute value search model</param>
        /// <param name="productAttributeMapping">Product attribute mapping</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product attribute value list model
        /// </returns>
        public override async Task<ProductAttributeValueListModel> PrepareProductAttributeValueListModelAsync(ProductAttributeValueSearchModel searchModel,
            ProductAttributeMapping productAttributeMapping)
        {
            ArgumentNullException.ThrowIfNull(searchModel);

            ArgumentNullException.ThrowIfNull(productAttributeMapping);

            //get product attribute values
            var productAttributeValues = (await _productAttributeService
                .GetProductAttributeValuesAsync(productAttributeMapping.Id)).ToPagedList(searchModel);

            //prepare list model
            var model = await new ProductAttributeValueListModel().PrepareToGridAsync(searchModel, productAttributeValues, () =>
            {
                return productAttributeValues.SelectAwait(async value =>
                {
                    //fill in model values from the entity
                    var productAttributeValueModel = value.ToModel<ProductAttributeValueModel>();

                    //fill in additional values (not existing in the entity)
                    productAttributeValueModel.AttributeValueTypeName = await _localizationService.GetLocalizedEnumAsync(value.AttributeValueType);

                    productAttributeValueModel.Name = productAttributeMapping.AttributeControlType != AttributeControlType.ColorSquares
                        ? value.Name : $"{value.Name} - {value.ColorSquaresRgb}";
                    if (value.AttributeValueType == AttributeValueType.Simple)
                    {
                        productAttributeValueModel.PriceAdjustmentStr = value.PriceAdjustment.ToString("G29");
                        if (value.PriceAdjustmentUsePercentage)
                            productAttributeValueModel.PriceAdjustmentStr += " %";
                        productAttributeValueModel.WeightAdjustmentStr = value.WeightAdjustment.ToString("G29");
                    }

                    if (value.AttributeValueType == AttributeValueType.AssociatedToProduct)
                    {
                        productAttributeValueModel.AssociatedProductName = (await _productService.GetProductByIdAsync(value.AssociatedProductId))?.Name ?? string.Empty;
                    }

                    var pavPictures = await _productAttributeService.GetProductAttributeValuePicturesAsync(value.Id);
                    var pictureId = pavPictures.FirstOrDefault()?.PictureId ?? 0;
                    var pictureThumbnailUrl = pictureId > 0? await _pictureService.GetPictureUrlAsync(pictureId, 75, false): await _pictureService.GetDefaultPictureUrlAsync(targetSize: 1);

                    //little hack here. Grid is rendered wrong way with <img> without "src" attribute
                    if (string.IsNullOrEmpty(pictureThumbnailUrl))
                        pictureThumbnailUrl = await _pictureService.GetDefaultPictureUrlAsync(targetSize: 1);

                    productAttributeValueModel.PictureThumbnailUrl = pictureThumbnailUrl;

                    //check for custom condtions 
                    productAttributeValueModel.CustomProperties.Add("AllowCondition", (productAttributeMapping.AttributeControlType == AttributeControlType.Checkboxes).ToString());

                    if (productAttributeMapping.AttributeControlType != AttributeControlType.Checkboxes)
                        return productAttributeValueModel;

                    //get all product attribute value condtions
                    var pavConditionString = string.Empty;
                    var pavConditions = await _productExtendService.GetAllProductAttributeValueConditionByValueIdAsync(value.Id);
                    if (!pavConditions.Any())
                        return productAttributeValueModel;

                    foreach (var pavCondition in pavConditions)
                    {
                        var pavConditionValue = await _productAttributeService.GetProductAttributeValueByIdAsync(pavCondition.ProductAttributeValueId2);
                        pavConditionString += string.IsNullOrEmpty(pavConditionString) ? $"{WebUtility.HtmlEncode(pavConditionValue.Name)}" : $", {WebUtility.HtmlEncode(pavConditionValue.Name)}";
                    }

                    if (!string.IsNullOrEmpty(pavConditionString))
                        productAttributeValueModel.CustomProperties.Add("ConditionString", string.Format(await _localizationService.GetResourceAsync("Plugins.Widgets.ProductExtension.ProductAttributeValue.ConditionString"), pavConditionString));

                    return productAttributeValueModel;
                });
            });

            return model;
        }

        #endregion

    }
}
