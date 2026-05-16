using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Vendors;
using Nop.Core.Infrastructure;
using Nop.Services.Catalog;
using Nop.Services.Common.Pdf;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Html;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Tax;
using Nop.Services.Vendors;
using PdfRpt.Core.Contracts;
using System.Globalization;

namespace Nop.Plugin.Widgets.ProductExtension.Services
{
    /// <summary>
    /// Quote service
    /// </summary>
    public partial class QuoteService : IQuoteService
    {
        #region Fields

        protected readonly CatalogSettings _catalogSettings;
        protected readonly ICurrencyService _currencyService;
        protected readonly ILocalizationService _localizationService;
        protected readonly INopFileProvider _fileProvider;
        protected readonly IPictureService _pictureService;
        protected readonly IPriceFormatter _priceFormatter;
        protected readonly IProductService _productService;
        protected readonly IStoreContext _storeContext;
        protected readonly IWorkContext _workContext;
        protected readonly VendorSettings _vendorSettings;
        protected readonly ISettingService _settingService;
        protected readonly IProductAttributeFormatter _productAttributeFormatter;
        protected readonly IVendorService _vendorService;
        protected readonly IHtmlFormatter _htmlFormatter;
        protected readonly OrderSettings _orderSettings;
        protected readonly ITaxService _taxService;
        protected readonly IShoppingCartService _shoppingCartService;
        protected readonly IOrderTotalCalculationService _orderTotalCalculationService;

        #endregion

        #region Ctor

        public QuoteService(CatalogSettings catalogSettings,
            ICurrencyService currencyService,
            ILocalizationService localizationService,
            INopFileProvider fileProvider,
            IPictureService pictureService,
            IPriceFormatter priceFormatter,
            IProductService productService,
            IStoreContext storeContext,
            IWorkContext workContext,
            VendorSettings vendorSettings,
            ISettingService settingService,
            IProductAttributeFormatter productAttributeFormatter,
            IVendorService vendorService,
            IHtmlFormatter htmlFormatter,
            OrderSettings orderSettings,
            ITaxService taxService,
            IShoppingCartService shoppingCartService,
            IOrderTotalCalculationService orderTotalCalculationService)
        {
            _catalogSettings = catalogSettings;
            _currencyService = currencyService;
            _localizationService = localizationService;
            _fileProvider = fileProvider;
            _pictureService = pictureService;
            _priceFormatter = priceFormatter;
            _productService = productService;
            _storeContext = storeContext;
            _workContext = workContext;
            _vendorSettings = vendorSettings;
            _settingService = settingService;
            _productAttributeFormatter = productAttributeFormatter;
            _vendorService = vendorService;
            _htmlFormatter = htmlFormatter;
            _orderSettings = orderSettings;
            _taxService = taxService;
            _shoppingCartService = shoppingCartService;
            _orderTotalCalculationService = orderTotalCalculationService;
        }


        #endregion

        #region Utilities

        /// <summary>
        /// Get product entries for document data source
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="cartItems">Collection of shopping cart items</param>
        /// <param name="language">Language</param>
        /// <returns>A task that contains collection of product entries</returns>
        protected virtual async Task<List<ProductItem>> GetCartProductItemsAsync(Customer customer, IList<ShoppingCartItem> cartItems, Language language)
        {
            var vendors = _vendorSettings.ShowVendorOnOrderDetailsPage ? await _vendorService.GetVendorsByProductIdsAsync(cartItems.Select(item => item.ProductId).ToArray()) : new List<Vendor>();

            var result = new List<ProductItem>();

            foreach (var sci in cartItems)
            {
                var productItem = new ProductItem();
                var product = await _productService.GetProductByIdAsync(sci.ProductId);
                var store = await _storeContext.GetCurrentStoreAsync();

                //product name
                productItem.Name = await _localizationService.GetLocalizedAsync(product, x => x.Name, language.Id);

                //attributes
                if (!string.IsNullOrEmpty(sci.AttributesXml))
                {
                    var attributeDescription = await _productAttributeFormatter.FormatAttributesAsync(product, sci.AttributesXml, customer, store);
                    var attributes = _htmlFormatter.ConvertHtmlToPlainText(attributeDescription, true, true);
                    productItem.ProductAttributes = attributes.Split('\n').ToList();
                }

                //SKU
                if (_catalogSettings.ShowSkuOnProductDetailsPage)
                    productItem.Sku = await _productService.FormatSkuAsync(product, sci.AttributesXml);

                //Vendor name
                if (_vendorSettings.ShowVendorOnOrderDetailsPage)
                    productItem.VendorName = vendors.FirstOrDefault(v => v.Id == product.VendorId)?.Name ?? string.Empty;

                //unit prices
                var currentCurrency = await _workContext.GetWorkingCurrencyAsync();
                if (product.CallForPrice &&
                    //also check whether the current user is impersonated
                    (!_orderSettings.AllowAdminsToBuyCallForPriceProducts || _workContext.OriginalCustomerIfImpersonated == null))
                {
                    productItem.Price = await _localizationService.GetResourceAsync("Products.CallForPrice");
                }
                else
                {
                    var (shoppingCartUnitPriceWithDiscountBase, _) = await _taxService.GetProductPriceAsync(product, (await _shoppingCartService.GetUnitPriceAsync(sci, true)).unitPrice);
                    var shoppingCartUnitPriceWithDiscount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(shoppingCartUnitPriceWithDiscountBase, currentCurrency);
                    productItem.Price = await _priceFormatter.FormatPriceAsync(shoppingCartUnitPriceWithDiscount);
                }
                
                //qty
                productItem.Quantity = sci.Quantity.ToString();

                //subtotal, discount
                if (product.CallForPrice &&
                    //also check whether the current user is impersonated
                    (!_orderSettings.AllowAdminsToBuyCallForPriceProducts || _workContext.OriginalCustomerIfImpersonated == null))
                {
                    productItem.Total = await _localizationService.GetResourceAsync("Products.CallForPrice");
                }
                else
                {
                    //sub total
                    var (subTotal, shoppingCartItemDiscountBase, _, maximumDiscountQty) = await _shoppingCartService.GetSubTotalAsync(sci, true);
                    var (shoppingCartItemSubTotalWithDiscountBase, _) = await _taxService.GetProductPriceAsync(product, subTotal);
                    var shoppingCartItemSubTotalWithDiscount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(shoppingCartItemSubTotalWithDiscountBase, currentCurrency);
                    productItem.Total = await _priceFormatter.FormatPriceAsync(shoppingCartItemSubTotalWithDiscount);
                }

                result.Add(productItem);
            }

            return result;
        }

        /// <summary>
        /// Get cart sub totals
        /// </summary>
        /// <param name="lang">Language</param>
        /// <param name="customer">Customer</param>
        /// <param name="cartItems">Collection of shopping cart items</param>
        /// <returns>A task that contains invoice totals</returns>
        protected virtual async Task<InvoiceTotals> GetSubTotalsAsync(Language lang, Customer customer, IList<ShoppingCartItem> cart)
        {
            var result = new InvoiceTotals();
            var languageId = lang.Id;

            //subtotal
            var (_, _, subTotalWithoutDiscountBase, _, _) = await _orderTotalCalculationService.GetShoppingCartSubTotalAsync(cart, false);
            var subtotalBase = subTotalWithoutDiscountBase;
            var currentCurrency = await _workContext.GetWorkingCurrencyAsync();
            var subtotal = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(subtotalBase, currentCurrency);
            var currentLanguage = await _workContext.GetWorkingLanguageAsync();
            result.SubTotal = await _priceFormatter.FormatPriceAsync(subtotal, true, currentCurrency, currentLanguage.Id, false);

            return result;
        }

        #endregion

        #region Methods

        public virtual async Task PrintCartToPdfAsync(Stream stream, IList<ShoppingCartItem> shoppingCartItems)
        {
            ArgumentNullException.ThrowIfNull(shoppingCartItems);

            var store = await _storeContext.GetCurrentStoreAsync();
            var language = await _workContext.GetWorkingLanguageAsync();
            var pdfSettings = await _settingService.LoadSettingAsync<PdfSettings>(store.Id);

            byte[] logo = null;
            var logoPicture = await _pictureService.GetPictureByIdAsync(pdfSettings.LogoPictureId);
            if (logoPicture != null)
            {
                var binary = await _pictureService.LoadPictureBinaryAsync(logoPicture);
                logo = logoPicture.MimeType == MimeTypes.ImageSvg
                    ? await _pictureService.ConvertSvgToPngAsync(new MemoryStream(binary))
                    : binary;
            }

            var customer = await _workContext.GetCurrentCustomerAsync();

            var document = new CartQuoteDocument
            {
                CustomerGuid = string.Format(await _localizationService.GetResourceAsync("Plugins.Widgets.ProductExtension.PDFQuote.Cart#", language.Id),customer.CustomerGuid),

                StoreUrl = store.Url?.Trim('/'),
                OrderDateUser = DateTime.UtcNow.ToString("D", CultureInfo.GetCultureInfo(language.LanguageCulture)),
                LogoData = logo,

                Language = language,
                PageSize = pdfSettings.LetterPageSizeEnabled ? PdfPageSize.Letter : PdfPageSize.A4,
                Font = PdfDocumentHelper.GetFont(
                    language.Rtl ? pdfSettings.RtlFontName : pdfSettings.LtrFontName,
                    pdfSettings.BaseFontSize > 0 ? pdfSettings.BaseFontSize : 10),
                ImageTargetSize = 80,
                GetResourceAsync = (key, id) => _localizationService.GetResourceAsync(key, id),

                Products = await GetCartProductItemsAsync(customer, shoppingCartItems, language),
                ShowSkuInProductList = _catalogSettings.ShowSkuOnProductDetailsPage,
                ShowVendorInProductList = _vendorSettings.ShowVendorOnOrderDetailsPage,
                Totals = await GetSubTotalsAsync(language, customer, shoppingCartItems)
            };

            await using var pdfStream = new MemoryStream();
            document.Generate(pdfStream);
            pdfStream.Position = 0;
            await pdfStream.CopyToAsync(stream);
        }

        #endregion

    }
}