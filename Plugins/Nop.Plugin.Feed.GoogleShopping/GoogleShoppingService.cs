using System.Globalization;
using System.Net;
using System.Xml;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Stores;
using Nop.Core.Infrastructure;
using Nop.Plugin.Feed.GoogleShopping.Services;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Plugins;
using Nop.Services.ScheduleTasks;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Tax;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Feed.GoogleShopping;

public class GoogleShoppingService : BasePlugin, IMiscPlugin
{
    #region Fields
    private readonly CurrencySettings _currencySettings;
    private readonly GoogleShoppingSettings _googleShoppingSettings;
    private readonly IActionContextAccessor _actionContextAccessor;
    private readonly ICategoryService _categoryService;
    private readonly ICurrencyService _currencyService;
    private readonly IGoogleService _googleService;
    private readonly ILanguageService _languageService;
    private readonly ILocalizationService _localizationService;
    private readonly IManufacturerService _manufacturerService;
    private readonly IMeasureService _measureService;
    private readonly INopFileProvider _nopFileProvider;
    private readonly IPictureService _pictureService;
    private readonly IPriceCalculationService _priceCalculationService;
    private readonly IProductService _productService;
    private readonly ISettingService _settingService;
    private readonly IStoreContext _storeContext;
    private readonly ITaxService _taxService;
    private readonly IUrlHelperFactory _urlHelperFactory;
    private readonly IUrlRecordService _urlRecordService;
    private readonly IWebHelper _webHelper;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IWorkContext _workContext;
    private readonly MeasureSettings _measureSettings;
    private readonly IScheduleTaskService _scheduleTaskService;
    private readonly IAclService _aclService;
    private readonly IProductAttributeService _productAttributeService;
    private readonly IProductAttributeParser _productAttributeParser;
    #endregion

    #region Ctor
    public GoogleShoppingService(CurrencySettings currencySettings,
        GoogleShoppingSettings googleShoppingSettings,
        IActionContextAccessor actionContextAccessor,
        ICategoryService categoryService,
        ICurrencyService currencyService,
        IGoogleService googleService,
        ILanguageService languageService,
        ILocalizationService localizationService,
        IManufacturerService manufacturerService,
        IMeasureService measureService,
        INopFileProvider nopFileProvider,
        IPictureService pictureService,
        IPriceCalculationService priceCalculationService,
        IProductService productService,
        ISettingService settingService,
        IStoreContext storeContext,
        ITaxService taxService,
        IUrlHelperFactory urlHelperFactory,
        IUrlRecordService urlRecordService,
        IWebHelper webHelper,
        IWebHostEnvironment webHostEnvironment,
        IWorkContext workContext,
        MeasureSettings measureSettings,
        IScheduleTaskService scheduleTaskService,
        IAclService aclService,
        IProductAttributeService productAttributeService,
        IProductAttributeParser productAttributeParser)
    {
        _actionContextAccessor = actionContextAccessor;
        _categoryService = categoryService;
        _currencyService = currencyService;
        _currencySettings = currencySettings;
        _googleService = googleService;
        _googleShoppingSettings = googleShoppingSettings;
        _languageService = languageService;
        _localizationService = localizationService;
        _manufacturerService = manufacturerService;
        _measureService = measureService;
        _measureSettings = measureSettings;
        _nopFileProvider = nopFileProvider;
        _pictureService = pictureService;
        _priceCalculationService = priceCalculationService;
        _productService = productService;
        _settingService = settingService;
        _storeContext = storeContext;
        _taxService = taxService;
        _urlHelperFactory = urlHelperFactory;
        _urlRecordService = urlRecordService;
        _webHelper = webHelper;
        _webHostEnvironment = webHostEnvironment;
        _workContext = workContext;
        _scheduleTaskService = scheduleTaskService;
        _aclService = aclService;
        _productAttributeService = productAttributeService;
        _productAttributeParser = productAttributeParser;
    }
    #endregion

    #region Utilities
    /// <summary>
    /// Removes invalid characters
    /// </summary>
    /// <param name="input">Input string</param>
    /// <param name="isHtmlEncoded">A value indicating whether input string is HTML encoded</param>
    /// <returns>Valid string</returns>
    protected virtual string StripInvalidChars(string input, bool isHtmlEncoded)
    {
        if (string.IsNullOrWhiteSpace(input))
            return input;

        // Microsoft uses a proprietary encoding (called CP-1252) for the bullet symbol and some other special characters, 
        // whereas most websites and data feeds use UTF-8. When you copy-paste from a Microsoft product into a website, 
        // some characters may appear as junk. Our system generates data feeds in the UTF-8 character encoding, 
        // which many shopping engines now require.

        //http://www.atensoftware.com/p90.php?q=182

        if (isHtmlEncoded)
            input = WebUtility.HtmlDecode(input);

        input = input.Replace("¼", "");
        input = input.Replace("½", "");
        input = input.Replace("¾", "");
        //input = input.Replace("•", "");
        //input = input.Replace("”", "");
        //input = input.Replace("“", "");
        //input = input.Replace("’", "");
        //input = input.Replace("‘", "");
        //input = input.Replace("™", "");
        //input = input.Replace("®", "");
        //input = input.Replace("°", "");

        if (!isHtmlEncoded)
            input = WebUtility.HtmlEncode(input);

        return input;
    }

    /// <summary>
    /// Get used currency
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the Currency
    /// </returns>
    protected virtual async Task<Currency> GetUsedCurrencyAsync()
    {
        var currency = await _currencyService.GetCurrencyByIdAsync(_googleShoppingSettings.CurrencyId);
        if (currency == null || !currency.Published)
            currency = await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId);
        return currency;
    }

    /// <summary>
    /// Get UrlHelper
    /// </summary>
    /// <returns>UrlHelper</returns>
    protected virtual IUrlHelper GetUrlHelper()
    {
        return _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
    }

    /// <summary>
    /// Get HTTP protocol
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the Protocol name
    /// </returns>
    protected virtual async Task<string> GetHttpProtocolAsync()
    {
        return (await _storeContext.GetCurrentStoreAsync()).SslEnabled ? Uri.UriSchemeHttps : Uri.UriSchemeHttp;
    }

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

        if (!string.IsNullOrEmpty(attributesXml) && validateAttributeConditions)
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
    #endregion

    #region Methods
    /// <summary>
    /// Gets a configuration page URL
    /// </summary>
    public override string GetConfigurationPageUrl()
    {
        return $"{_webHelper.GetStoreLocation()}Admin/FeedGoogleShopping/Configure";
    }

    public async Task<bool> GenerateGoogleShoppingFeeds(Store store, List<Product> products, string filePath)
    {
        var result = await WriteProductsToXml(store, products, filePath);
        return result;
    }

    // This method is your existing XML generation logic
    private async Task<bool> WriteProductsToXml(Store store, List<Product> products, string filePath)
    {
        const string googleBaseNamespace = "http://base.google.com/ns/1.0";

        var googleShoppingSettings = await _settingService.LoadSettingAsync<GoogleShoppingSettings>(store.Id);

        //language
        var languageId = 0;
        var languages = await _languageService.GetAllLanguagesAsync(storeId: store.Id);
        //if we have only one language, let's use it
        if (languages.Count() == 1)
        {
            //let's use the first one
            var language = languages.FirstOrDefault();
            languageId = language != null ? language.Id : 0;
        }
        //otherwise, use the current one
        if (languageId == 0)
            languageId = (await _workContext.GetWorkingLanguageAsync()).Id;

        //we load all Google products here using one SQL request (performance optimization)
        var allGoogleProducts = await _googleService.GetAllAsync();

        using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
        using (var writer = XmlWriter.Create(fs, new XmlWriterSettings { Indent = true }))
        {
            //Generate feed according to the following specs: http://www.google.com/support/merchants/bin/answer.py?answer=188494&expand=GB
            writer.WriteStartDocument();
            writer.WriteStartElement("rss");
            writer.WriteAttributeString("version", "2.0");
            writer.WriteAttributeString("xmlns", "g", null, googleBaseNamespace);


            writer.WriteStartElement("channel");
            writer.WriteElementString("title", "Google Base feed");
            writer.WriteElementString("link", "http://base.google.com/base/");
            writer.WriteElementString("description", "Information about products");

            if (products.Any())
            {
                foreach (var product in products)
                {
                    if (string.IsNullOrEmpty(product.Sku))
                        continue;

                    var customerRoleIds = await _aclService.GetCustomerRoleIdsWithAccessAsync(product.Id, nameof(Product));
                    if (customerRoleIds.Any())
                        continue;

                    writer.WriteStartElement("item");

                    #region Basic Product Information

                    //id [id]- An identifier of the item
                    writer.WriteElementString("g", "id", googleBaseNamespace, product.Sku);

                    //title [title] - Title of the item
                    writer.WriteStartElement("title");
                    var title = await _localizationService.GetLocalizedAsync(product, x => x.Name, languageId);
                    //title should be not longer than 70 characters
                    if (title.Length > 70)
                        title = title[..70];
                    writer.WriteCData(title);
                    writer.WriteEndElement(); // title

                    //description [description] - Description of the item
                    writer.WriteStartElement("description");
                    var description = await _localizationService.GetLocalizedAsync(product, x => x.FullDescription, languageId);
                    if (string.IsNullOrEmpty(description))
                        description = await _localizationService.GetLocalizedAsync(product, x => x.ShortDescription, languageId);
                    if (string.IsNullOrEmpty(description))
                        description = await _localizationService.GetLocalizedAsync(product, x => x.Name, languageId); //description is required
                                                                                                                      //resolving character encoding issues in your data feed
                    description = StripInvalidChars(description, true);
                    writer.WriteCData(description);
                    writer.WriteEndElement(); // description

                    //google product category [google_product_category] - Google's category of the item
                    //the category of the product according to Google’s product taxonomy. http://www.google.com/support/merchants/bin/answer.py?answer=160081
                    var googleProductCategory = "";
                    //var googleProduct = _googleService.GetByProductId(product.Id);
                    var googleProduct = allGoogleProducts.FirstOrDefault(x => x.ProductId == product.Id);
                    if (googleProduct != null)
                        googleProductCategory = googleProduct.Taxonomy;
                    if (string.IsNullOrEmpty(googleProductCategory))
                        googleProductCategory = googleShoppingSettings.DefaultGoogleCategory;
                    if (string.IsNullOrEmpty(googleProductCategory))
                        throw new NopException("Default Google category is not set");
                    writer.WriteStartElement("g", "google_product_category", googleBaseNamespace);
                    writer.WriteCData(googleProductCategory);
                    writer.WriteFullEndElement(); // g:google_product_category

                    //product type [product_type] - Your category of the item
                    var defaultProductCategory = (await _categoryService
                        .GetProductCategoriesByProductIdAsync(product.Id))
                        .FirstOrDefault();
                    if (defaultProductCategory != null)
                    {
                        //TODO localize categories
                        var category = await _categoryService.GetFormattedBreadCrumbAsync(
                            category: await _categoryService.GetCategoryByIdAsync(defaultProductCategory.CategoryId),
                        separator: ">",
                            languageId: languageId);
                        if (!string.IsNullOrEmpty(category))
                        {
                            writer.WriteStartElement("g", "product_type", googleBaseNamespace);
                            writer.WriteCData(category);
                            writer.WriteFullEndElement(); // g:product_type
                        }
                    }

                    //link [link] - URL directly linking to your item's page on your website
                    var productUrl = GetUrlHelper().RouteUrl<Product>(new { SeName = await _urlRecordService.GetSeNameAsync(product) }, await GetHttpProtocolAsync());
                    writer.WriteElementString("link", productUrl);

                    //image link [image_link] - URL of an image of the item
                    //additional images [additional_image_link]
                    //up to 10 pictures
                    const int maximumPictures = 10;
                    var storeLocation = _webHelper.GetStoreLocation();
                    var pictures = await _pictureService.GetPicturesByProductIdAsync(product.Id, maximumPictures);
                    for (var i = 0; i < pictures.Count; i++)
                    {
                        var picture = pictures[i];
                        var imageUrl = await _pictureService.GetPictureUrlAsync(picture.Id,
                            googleShoppingSettings.ProductPictureSize,
                            storeLocation: storeLocation);

                        if (i == 0)
                        {
                            //default image
                            writer.WriteElementString("g", "image_link", googleBaseNamespace, imageUrl);
                        }
                        else
                        {
                            //additional image
                            writer.WriteElementString("g", "additional_image_link", googleBaseNamespace, imageUrl);
                        }
                    }
                    if (!pictures.Any())
                    {
                        //no picture? submit a default one
                        var imageUrl = await _pictureService.GetDefaultPictureUrlAsync(googleShoppingSettings.ProductPictureSize, storeLocation: storeLocation);
                        writer.WriteElementString("g", "image_link", googleBaseNamespace, imageUrl);
                    }

                    //condition [condition] - Condition or state of the item
                    writer.WriteElementString("g", "condition", googleBaseNamespace, "new");

                    writer.WriteElementString("g", "expiration_date", googleBaseNamespace, DateTime.Now.AddDays(googleShoppingSettings.ExpirationNumberOfDays).ToString("yyyy-MM-dd"));

                    #endregion

                    #region Availability & Price

                    //availability [availability] - Availability status of the item
                    var availability = "in stock"; //in stock by default
                    if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStock
                        && product.BackorderMode == BackorderMode.NoBackorders
                        && await _productService.GetTotalStockQuantityAsync(product) <= 0)
                    {
                        availability = "out of stock";
                    }

                    var priceAdjustment = decimal.Zero;
                    var attributesXml = string.Empty;
                    var currentCustomer = await _workContext.GetCurrentCustomerAsync();
                    if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStockByAttributes)
                    {
                        (priceAdjustment, attributesXml) = await PrepareAssociatedProductPriceAsync(product, currentCustomer, store);
                        var stockAvailability = await _productService.FormatStockMessageAsync(product, attributesXml);
                        if (stockAvailability.Contains("stock-error"))
                            availability = "out of stock";
                        else
                            availability = "in stock";
                    }

                    //uncomment th code below in order to support "preorder" value for "availability"
                    //if (product.AvailableForPreOrder &&
                    //    (!product.PreOrderAvailabilityStartDateTimeUtc.HasValue || 
                    //    product.PreOrderAvailabilityStartDateTimeUtc.Value >= DateTime.UtcNow))
                    //{
                    //    availability = "preorder";
                    //}
                    writer.WriteElementString("g", "availability", googleBaseNamespace, availability);

                    //price [price] - Price of the item
                    var currency = await GetUsedCurrencyAsync();
                    decimal finalPriceBase;
                    if (googleShoppingSettings.PricesConsiderPromotions)
                    {
                        var (minPossiblePriceWithoutDiscount, minPossiblePrice, _, _) =
                        await _priceCalculationService.GetFinalPriceAsync(product,currentCustomer,store);

                        if (priceAdjustment > decimal.Zero)
                        {
                            var newPrice = minPossiblePriceWithoutDiscount + priceAdjustment;

                            minPossiblePrice = (await _priceCalculationService.GetFinalPriceAsync(
                                product,
                                currentCustomer,
                                store,
                                newPrice,
                                0,
                                true,
                                1,
                                null,
                                null
                            )).finalPrice;
                        }

                        finalPriceBase = (await _taxService.GetProductPriceAsync(product,minPossiblePrice)).price;

                    }
                    else
                    {
                        finalPriceBase = product.Price;
                    }
                    var price = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(finalPriceBase, currency);
                    //round price now so it matches the product details page
                    price = await _priceCalculationService.RoundPriceAsync(price);

                    writer.WriteElementString("g", "price", googleBaseNamespace,
                                              price.ToString(new CultureInfo("en-US", false).NumberFormat) + " " +
                                              currency.CurrencyCode);

                    #endregion

                    #region Unique Product Identifiers

                    /* Unique product identifiers such as UPC, EAN, JAN or ISBN allow us to show your listing on the appropriate product page. If you don't provide the required unique product identifiers, your store may not appear on product pages, and all your items may be removed from Product Search.
                     * We require unique product identifiers for all products - except for custom made goods. For apparel, you must submit the 'brand' attribute. For media (such as books, movies, music and video games), you must submit the 'gtin' attribute. In all cases, we recommend you submit all three attributes.
                     * You need to submit at least two attributes of 'brand', 'gtin' and 'mpn', but we recommend that you submit all three if available. For media (such as books, movies, music and video games), you must submit the 'gtin' attribute, but we recommend that you include 'brand' and 'mpn' if available.
                    */

                    //GTIN [gtin] - GTIN
                    var gtin = product.Gtin;
                    if (!string.IsNullOrEmpty(gtin))
                    {
                        writer.WriteStartElement("g", "gtin", googleBaseNamespace);
                        writer.WriteCData(gtin);
                        writer.WriteFullEndElement(); // g:gtin
                    }

                    //brand [brand] - Brand of the item
                    var defaultManufacturer = (await _manufacturerService.GetProductManufacturersByProductIdAsync(product.Id)).FirstOrDefault();
                    if (defaultManufacturer != null)
                    {
                        writer.WriteStartElement("g", "brand", googleBaseNamespace);
                        writer.WriteCData((await _manufacturerService.GetManufacturerByIdAsync(defaultManufacturer.ManufacturerId))?.Name);
                        writer.WriteFullEndElement(); // g:brand
                    }

                    //mpn [mpn] - Manufacturer Part Number (MPN) of the item
                    var mpn = product.ManufacturerPartNumber;
                    if (!string.IsNullOrEmpty(mpn))
                    {
                        writer.WriteStartElement("g", "mpn", googleBaseNamespace);
                        writer.WriteCData(mpn);
                        writer.WriteFullEndElement(); // g:mpn
                    }

                    //identifier exists [identifier_exists] - Submit custom goods
                    if (googleProduct != null && googleProduct.CustomGoods)
                    {
                        writer.WriteElementString("g", "identifier_exists", googleBaseNamespace, "FALSE");
                    }

                    #endregion

                    #region Apparel Products

                    /* Apparel includes all products that fall under 'Apparel & Accessories' (including all sub-categories)
                     * in Google’s product taxonomy.
                    */

                    //gender [gender] - Gender of the item
                    if (googleProduct != null && !string.IsNullOrEmpty(googleProduct.Gender))
                    {
                        writer.WriteStartElement("g", "gender", googleBaseNamespace);
                        writer.WriteCData(googleProduct.Gender);
                        writer.WriteFullEndElement(); // g:gender
                    }

                    //age group [age_group] - Target age group of the item
                    if (googleProduct != null && !string.IsNullOrEmpty(googleProduct.AgeGroup))
                    {
                        writer.WriteStartElement("g", "age_group", googleBaseNamespace);
                        writer.WriteCData(googleProduct.AgeGroup);
                        writer.WriteFullEndElement(); // g:age_group
                    }

                    //color [color] - Color of the item
                    if (googleProduct != null && !string.IsNullOrEmpty(googleProduct.Color))
                    {
                        writer.WriteStartElement("g", "color", googleBaseNamespace);
                        writer.WriteCData(googleProduct.Color);
                        writer.WriteFullEndElement(); // g:color
                    }

                    //size [size] - Size of the item
                    if (googleProduct != null && !string.IsNullOrEmpty(googleProduct.Size))
                    {
                        writer.WriteStartElement("g", "size", googleBaseNamespace);
                        writer.WriteCData(googleProduct.Size);
                        writer.WriteFullEndElement(); // g:size
                    }

                    #endregion

                    #region Tax & Shipping

                    //tax [tax]
                    //The tax attribute is an item-level override for merchant-level tax settings as defined in your Google Merchant Center account. This attribute is only accepted in the US, if your feed targets a country outside of the US, please do not use this attribute.
                    //IMPORTANT NOTE: Set tax in your Google Merchant Center account settings

                    //IMPORTANT NOTE: Set shipping in your Google Merchant Center account settings

                    //shipping weight [shipping_weight] - Weight of the item for shipping
                    //We accept only the following units of weight: lb, oz, g, kg.
                    if (googleShoppingSettings.PassShippingInfoWeight)
                    {
                        var shippingWeight = product.Weight;
                        var weightSystemName = (await _measureService.GetMeasureWeightByIdAsync(_measureSettings.BaseWeightId)).SystemKeyword;
                        var weightName = weightSystemName switch
                        {
                            "ounce" => "oz",
                            "lb" => "lb",
                            "grams" => "g",
                            "kg" => "kg",
                            _ => throw new Exception("Not supported weight. Google accepts the following units: lb, oz, g, kg."),
                        };
                        writer.WriteElementString("g", "shipping_weight", googleBaseNamespace, string.Format(CultureInfo.InvariantCulture, "{0} {1}", shippingWeight.ToString(new CultureInfo("en-US", false).NumberFormat), weightName));
                    }

                    //shipping length [shipping_length] - Length of the item for shipping
                    //shipping width [shipping_width] - Width of the item for shipping
                    //shipping height [shipping_height] - Height of the item for shipping
                    //We accept only the following units of length: in, cm
                    if (googleShoppingSettings.PassShippingInfoDimensions)
                    {
                        var length = product.Length;
                        var width = product.Width;
                        var height = product.Height;
                        var dimensionSystemName = (await _measureService.GetMeasureDimensionByIdAsync(_measureSettings.BaseDimensionId)).SystemKeyword;
                        var dimensionName = dimensionSystemName switch
                        {
                            "inches" => "in",
                            //TODO support other dimensions (convert to cm)
                            _ => throw new Exception("Not supported dimension. Google accepts the following units: in, cm."),//unknown dimension 
                        };
                        writer.WriteElementString("g", "shipping_length", googleBaseNamespace, string.Format(CultureInfo.InvariantCulture, "{0} {1}", length.ToString(new CultureInfo("en-US", false).NumberFormat), dimensionName));
                        writer.WriteElementString("g", "shipping_width", googleBaseNamespace, string.Format(CultureInfo.InvariantCulture, "{0} {1}", width.ToString(new CultureInfo("en-US", false).NumberFormat), dimensionName));
                        writer.WriteElementString("g", "shipping_height", googleBaseNamespace, string.Format(CultureInfo.InvariantCulture, "{0} {1}", height.ToString(new CultureInfo("en-US", false).NumberFormat), dimensionName));
                    }

                    #endregion

                    writer.WriteEndElement();
                }// item
                _googleShoppingSettings.FileCounter++;
                await _settingService.SaveSettingAsync(_googleShoppingSettings);
                return true;
            }
            return false;
        }
    }

    /// <summary>
    /// Generate a feed
    /// </summary>
    /// <param name="stream">Stream</param>
    /// <param name="store">Store</param>
    /// <returns>Generated feed</returns>
    public async Task<bool> GenerateFeedAsync(Store store)
    {
        string directoryPath = Path.Combine(_nopFileProvider.MapPath("~/wwwroot/files/exportimport/googleFeedXmls/"));
        if (!_nopFileProvider.DirectoryExists(directoryPath))
        {
            _nopFileProvider.CreateDirectory(directoryPath);
        }

        int fileCounter = _googleShoppingSettings.FileCounter;
        int pageSize = _googleShoppingSettings.PageSizeForGoogleXmlProducts;
        var baseFilePath = _nopFileProvider.Combine(_webHostEnvironment.WebRootPath, "files", "exportimport", "googleFeedXmls", $"{store.Name}");
        string filePath = $"{baseFilePath}_part_{fileCounter + 1}_{_googleShoppingSettings.StaticFileName}";

        FileInfo fi = new FileInfo(filePath);
        if (fi.Exists && fi.LastWriteTime.Date == DateTime.Now.Date)
            return false;

        var products = await _productService.SearchProductsAsync(storeId: store.Id,
            visibleIndividuallyOnly: true, pageIndex: fileCounter++, pageSize: pageSize);
        var productsToProcess = new List<Product>();

        if (!products.Any())
        {
            _googleShoppingSettings.FileCounter = 0;
            await _settingService.SaveSettingAsync(_googleShoppingSettings);
            return false;
        }

        //simple product doesn't have child products
        productsToProcess.AddRange(products.Where(p => p.ProductType == ProductType.SimpleProduct).ToList());

        //grouped products could have several child products
        foreach (var product in products.Where(p => p.ProductType == ProductType.GroupedProduct).ToList())
        {
            var associatedProducts = await _productService.GetAssociatedProductsAsync(product.Id, store.Id);
            productsToProcess.AddRange(associatedProducts);
        }

        var result = await GenerateGoogleShoppingFeeds(store, productsToProcess, filePath);
        return result;
    }

    // <summary>
    /// Install the plugin
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    public override async Task InstallAsync()
    {

        if (await _scheduleTaskService.GetTaskByTypeAsync("Nop.Plugin.Feed.GoogleShopping.ScheduleTask.GoogleFeedGenerate") == null)
        {
            await _scheduleTaskService.InsertTaskAsync(new Nop.Core.Domain.ScheduleTasks.ScheduleTask
            {
                Enabled = true,
                LastEnabledUtc = DateTime.UtcNow,
                Seconds = 86400,
                Name = "Google Feed Generate",
                Type = "Nop.Plugin.Feed.GoogleShopping.ScheduleTask.GoogleFeedGenerate",
            });
        }

        //settings
        var settings = new GoogleShoppingSettings
        {
            PricesConsiderPromotions = false,
            ProductPictureSize = 125,
            PassShippingInfoWeight = false,
            PassShippingInfoDimensions = false,
            StaticFileName = $"googleshopping_{CommonHelper.GenerateRandomDigitCode(10)}.xml",
            ExpirationNumberOfDays = 28,
            FileCounter = 0,
            PageSizeForGoogleXmlProducts = 5000
        };
        await _settingService.SaveSettingAsync(settings);

        //locales
        await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
        {
            ["Plugins.Feed.GoogleShopping.Store"] = "Store",
            ["Plugins.Feed.GoogleShopping.Store.Hint"] = "Select the store that will be used to generate the feed.",
            ["Plugins.Feed.GoogleShopping.Currency"] = "Currency",
            ["Plugins.Feed.GoogleShopping.Currency.Hint"] = "Select the default currency that will be used to generate the feed.",
            ["Plugins.Feed.GoogleShopping.DefaultGoogleCategory"] = "Default Google category",
            ["Plugins.Feed.GoogleShopping.DefaultGoogleCategory.Hint"] = "The default Google category to use if one is not specified.",
            ["Plugins.Feed.GoogleShopping.ExceptionLoadPlugin"] = "Cannot load the plugin",
            ["Plugins.Feed.GoogleShopping.General"] = "General",
            ["Plugins.Feed.GoogleShopping.GeneralInstructions"] = "<p><ul><li>At least two unique product identifiers are required. So each of your product should have manufacturer (brand) and MPN (manufacturer part number) specified</li><li>Specify default tax values in your Google Merchant Center account settings</li><li>Specify default shipping values in your Google Merchant Center account settings</li><li>In order to get more info about required fields look at the following article <a href=\"http://www.google.com/support/merchants/bin/answer.py?answer=188494\" target=\"_blank\">http://www.google.com/support/merchants/bin/answer.py?answer=188494</a></li></ul></p>",
            ["Plugins.Feed.GoogleShopping.Generate"] = "Generate feed",
            ["Plugins.Feed.GoogleShopping.Override"] = "Override product settings",
            ["Plugins.Feed.GoogleShopping.OverrideInstructions"] = "<p>You can download the list of allowed Google product category attributes <a href=\"http://www.google.com/support/merchants/bin/answer.py?answer=160081\" target=\"_blank\">here</a></p>",
            ["Plugins.Feed.GoogleShopping.PassShippingInfoWeight"] = "Pass shipping info (weight)",
            ["Plugins.Feed.GoogleShopping.PassShippingInfoWeight.Hint"] = "Check if you want to include shipping information (weight) in generated XML file.",
            ["Plugins.Feed.GoogleShopping.PassShippingInfoDimensions"] = "Pass shipping info (dimensions)",
            ["Plugins.Feed.GoogleShopping.PassShippingInfoDimensions.Hint"] = "Check if you want to include shipping information (dimensions) in generated XML file.",
            ["Plugins.Feed.GoogleShopping.PricesConsiderPromotions"] = "Prices consider promotions",
            ["Plugins.Feed.GoogleShopping.PricesConsiderPromotions.Hint"] = "Check if you want prices to be calculated with promotions (tier prices] = discounts] = special prices] = tax] = etc). But please note that it can significantly reduce time required to generate the feed file.",
            ["Plugins.Feed.GoogleShopping.ProductPictureSize"] = "Product thumbnail image size",
            ["Plugins.Feed.GoogleShopping.ProductPictureSize.Hint"] = "The default size (pixels) for product thumbnail images.",
            ["Plugins.Feed.GoogleShopping.Products.ProductName"] = "Product",
            ["Plugins.Feed.GoogleShopping.Products.ProductName.Hint"] = "Product Name",
            ["Plugins.Feed.GoogleShopping.Products.GoogleCategory"] = "Google Category",
            ["Plugins.Feed.GoogleShopping.Products.GoogleCategory.Hint"] = "Product category according to the Google product taxonomy.",
            ["Plugins.Feed.GoogleShopping.Products.Gender"] = "Gender",
            ["Plugins.Feed.GoogleShopping.Products.Gender.Hint"] = "Gender of the people for whom the product is intended.",
            ["Plugins.Feed.GoogleShopping.Products.AgeGroup"] = "Age group",
            ["Plugins.Feed.GoogleShopping.Products.AgeGroup.Hint"] = "Age category of people for whom the goods are intended.",
            ["Plugins.Feed.GoogleShopping.Products.Color"] = "Color",
            ["Plugins.Feed.GoogleShopping.Products.Color.Hint"] = "Product color.",
            ["Plugins.Feed.GoogleShopping.Products.Size"] = "Size",
            ["Plugins.Feed.GoogleShopping.Products.Size.Hint"] = "Product size.",
            ["Plugins.Feed.GoogleShopping.Products.CustomGoods"] = "Custom goods",
            ["Plugins.Feed.GoogleShopping.Products.CustomGoods.Hint"] = "Custom goods (no identifier exists).",
            ["Plugins.Feed.GoogleShopping.SuccessResult"] = "Google Shopping feed has been successfully generated.",
            ["Plugins.Feed.GoogleShopping.StaticFilePath"] = "Generated file path (static)",
            ["Plugins.Feed.GoogleShopping.StaticFilePath.Hint"] = "A file path of the generated file. It's static for your store and can be shared with the Google Shopping service.",
            ["Plugins.Feed.GoogleShopping.PageSizeForGoogleXmlProducts"] = "Products per Feed",
            ["Plugins.Feed.GoogleShopping.PageSizeForGoogleXmlProducts.Hint"] = "Enter how many products you want to write per feed max can be 5000",
            ["Plugins.Feed.GoogleShopping.WarningResult"] = "Seems like same file already exist in the Directory OR No New Products found to write!",
        });

        await base.InstallAsync();
    }

    /// <summary>
    /// Uninstall the plugin
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    public override async Task UninstallAsync()
    {
        string[] files = Directory.GetFiles(_nopFileProvider.Combine(_webHostEnvironment.WebRootPath, "files", "exportimport", "googleFeedXmls"));

        if (files.Length > 0)
        {
            foreach (var file in files)
                if (_nopFileProvider.FileExists(file))
                    File.Delete(file);
        }

        //settings
        await _settingService.DeleteSettingAsync<GoogleShoppingSettings>();

        //locales
        await _localizationService.DeleteLocaleResourcesAsync("Plugins.Feed.GoogleShopping");

        //schedule task
        var task = await _scheduleTaskService.GetTaskByTypeAsync("Plugins.Feed.GoogleShopping.ScheduleTask.GoogleFeedGenerate");
        if (task != null)
            await _scheduleTaskService.DeleteTaskAsync(task);

        await base.UninstallAsync();
    }

    /// <summary>
    /// Generate a static feed file
    /// </summary>
    /// <param name="store">Store</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public virtual async Task<bool> GenerateStaticFileAsync(Store store)
    {
        if (store == null)
            return false;

        var result = await GenerateFeedAsync(store);
        return result;
    }

    #endregion
}
