using Nop.Core.Domain.Orders;
using Nop.Core;
using Nop.Services.Catalog;
using Nop.Services.Directory;
using Nop.Services.Html;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Tax;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Stores;
using System.Net;
using System.Text;

namespace Nop.Plugin.Misc.InvoicePDF.Services
{
    /// <summary>
    /// Override Product attribute formatter
    /// </summary>
    public class OverrideProductAttributeFormatter : ProductAttributeFormatter
    {
        #region Ctor

        public OverrideProductAttributeFormatter(ICurrencyService currencyService,
            IDownloadService downloadService,
            IHtmlFormatter htmlFormatter,
            ILocalizationService localizationService,
            IPriceCalculationService priceCalculationService,
            IPriceFormatter priceFormatter,
            IProductAttributeParser productAttributeParser,
            IProductAttributeService productAttributeService,
            ITaxService taxService,
            IWebHelper webHelper,
            IWorkContext workContext,
            IStoreContext storeContext,
            ShoppingCartSettings shoppingCartSettings) : base (currencyService,
                downloadService,
                htmlFormatter,
                localizationService,
                priceCalculationService,
                priceFormatter,
                productAttributeParser,
                productAttributeService,
                taxService,
                webHelper,
                workContext,
                storeContext,
                shoppingCartSettings)
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Formats attributes
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <param name="customer">Customer</param>
        /// <param name="store">Store</param>
        /// <param name="separator">Separator</param>
        /// <param name="htmlEncode">A value indicating whether to encode (HTML) values</param>
        /// <param name="renderPrices">A value indicating whether to render prices</param>
        /// <param name="renderProductAttributes">A value indicating whether to render product attributes</param>
        /// <param name="renderGiftCardAttributes">A value indicating whether to render gift card attributes</param>
        /// <param name="allowHyperlinks">A value indicating whether to HTML hyperink tags could be rendered (if required)</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the attributes
        /// </returns>
        public override async Task<string> FormatAttributesAsync(Product product, string attributesXml,
            Customer customer, Store store, string separator = "<br />", bool htmlEncode = true, bool renderPrices = true,
            bool renderProductAttributes = true, bool renderGiftCardAttributes = true,
            bool allowHyperlinks = true)
        {
            var result = new StringBuilder();
            var currentLanguage = await _workContext.GetWorkingLanguageAsync();
            //attributes
            if (renderProductAttributes)
            {
                foreach (var attribute in await _productAttributeParser.ParseProductAttributeMappingsAsync(attributesXml))
                {
                    var productAttribute = await _productAttributeService.GetProductAttributeByIdAsync(attribute.ProductAttributeId);
                    var attributeName = await _localizationService.GetLocalizedAsync(productAttribute, a => a.Name, currentLanguage.Id);

                    //attributes without values
                    if (!attribute.ShouldHaveValues())
                    {
                        foreach (var value in _productAttributeParser.ParseValues(attributesXml, attribute.Id))
                        {
                            var formattedAttribute = string.Empty;
                            if (attribute.AttributeControlType == AttributeControlType.MultilineTextbox)
                            {
                                //encode (if required)
                                if (htmlEncode)
                                    attributeName = WebUtility.HtmlEncode(attributeName);

                                //we never encode multiline textbox input
                                formattedAttribute = $"{attributeName}: {_htmlFormatter.FormatText(value, false, true, false, false, false, false)}";
                            }
                            else if (attribute.AttributeControlType == AttributeControlType.FileUpload)
                            {
                                //file upload
                                _ = Guid.TryParse(value, out var downloadGuid);
                                var download = await _downloadService.GetDownloadByGuidAsync(downloadGuid);
                                if (download != null)
                                {
                                    var fileName = $"{download.Filename ?? download.DownloadGuid.ToString()}{download.Extension}";

                                    //encode (if required)
                                    if (htmlEncode)
                                        fileName = WebUtility.HtmlEncode(fileName);

                                    var attributeText = allowHyperlinks ? $"<a href=\"{_webHelper.GetStoreLocation()}download/getfileupload/?downloadId={download.DownloadGuid}\" class=\"fileuploadattribute\">{fileName}</a>"
                                        : fileName;

                                    //encode (if required)
                                    if (htmlEncode)
                                        attributeName = WebUtility.HtmlEncode(attributeName);

                                    formattedAttribute = $"{attributeName}: {attributeText}";
                                }
                            }
                            else
                            {
                                //other attributes (textbox, datepicker)
                                formattedAttribute = $"{attributeName}: {value}";

                                //encode (if required)
                                if (htmlEncode)
                                    formattedAttribute = WebUtility.HtmlEncode(formattedAttribute);
                            }

                            if (string.IsNullOrEmpty(formattedAttribute))
                                continue;

                            if (result.Length > 0)
                                result.Append(separator);
                            result.Append(formattedAttribute);
                        }
                    }
                    //product attribute values
                    else
                    {
                        foreach (var attributeValue in await _productAttributeParser.ParseProductAttributeValuesAsync(attributesXml, attribute.Id))
                        {
                            var formattedAttribute = $"{attributeName}: {await _localizationService.GetLocalizedAsync(attributeValue, a => a.Name, currentLanguage.Id)} ";
                            
                            //display quantity for customer enter qty
                            if (!_shoppingCartSettings.RenderAssociatedAttributeValueQuantity && attributeValue.AttributeValueType == AttributeValueType.AssociatedToProduct && attributeValue.CustomerEntersQty)
                            {
                                //render only when more than 1
                                if (attributeValue.Quantity > 1)
                                    formattedAttribute += string.Format("x {0} ", attributeValue.Quantity);
                            }

                            if (renderPrices)
                            {
                                if (attributeValue.PriceAdjustmentUsePercentage)
                                {
                                    if (attributeValue.PriceAdjustment > decimal.Zero)
                                    {
                                        formattedAttribute += string.Format(
                                                await _localizationService.GetResourceAsync("FormattedAttributes.PriceAdjustment"),
                                                "+", attributeValue.PriceAdjustment.ToString("G29"), "%");
                                    }
                                    else if (attributeValue.PriceAdjustment < decimal.Zero)
                                    {
                                        formattedAttribute += string.Format(
                                                await _localizationService.GetResourceAsync("FormattedAttributes.PriceAdjustment"),
                                                string.Empty, attributeValue.PriceAdjustment.ToString("G29"), "%");
                                    }
                                }
                                else
                                {
                                    var attributeValuePriceAdjustment = await _priceCalculationService.GetProductAttributeValuePriceAdjustmentAsync(product, attributeValue, customer, store);
                                    var (priceAdjustmentBase, _) = await _taxService.GetProductPriceAsync(product, attributeValuePriceAdjustment, customer);
                                    var priceAdjustment = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(priceAdjustmentBase, await _workContext.GetWorkingCurrencyAsync());

                                    if (priceAdjustmentBase > decimal.Zero)
                                    {
                                        formattedAttribute += string.Format(
                                                await _localizationService.GetResourceAsync("FormattedAttributes.PriceAdjustment"),
                                                "+", await _priceFormatter.FormatPriceAsync(priceAdjustment, false, false), string.Empty);
                                    }
                                    else if (priceAdjustmentBase < decimal.Zero)
                                    {
                                        formattedAttribute += string.Format(
                                                await _localizationService.GetResourceAsync("FormattedAttributes.PriceAdjustment"),
                                                "-", await _priceFormatter.FormatPriceAsync(-priceAdjustment, false, false), string.Empty);
                                    }
                                }
                            }

                            //display quantity
                            if (_shoppingCartSettings.RenderAssociatedAttributeValueQuantity && attributeValue.AttributeValueType == AttributeValueType.AssociatedToProduct)
                            {
                                //render only when more than 1
                                if (attributeValue.Quantity > 1)
                                    formattedAttribute += string.Format(await _localizationService.GetResourceAsync("ProductAttributes.Quantity"), attributeValue.Quantity);
                            }

                            //encode (if required)
                            if (htmlEncode)
                                formattedAttribute = WebUtility.HtmlEncode(formattedAttribute);

                            if (string.IsNullOrEmpty(formattedAttribute))
                                continue;

                            if (result.Length > 0)
                                result.Append(separator);
                            result.Append(formattedAttribute);
                        }
                    }
                }
            }

            //gift cards
            if (!renderGiftCardAttributes)
                return result.ToString();

            if (!product.IsGiftCard)
                return result.ToString();

            _productAttributeParser.GetGiftCardAttribute(attributesXml, out var giftCardRecipientName, out var giftCardRecipientEmail, out var giftCardSenderName, out var giftCardSenderEmail, out var _);

            //sender
            var giftCardFrom = product.GiftCardType == GiftCardType.Virtual ?
                string.Format(await _localizationService.GetResourceAsync("GiftCardAttribute.From.Virtual"), giftCardSenderName, giftCardSenderEmail) :
                string.Format(await _localizationService.GetResourceAsync("GiftCardAttribute.From.Physical"), giftCardSenderName);
            //recipient
            var giftCardFor = product.GiftCardType == GiftCardType.Virtual ?
                string.Format(await _localizationService.GetResourceAsync("GiftCardAttribute.For.Virtual"), giftCardRecipientName, giftCardRecipientEmail) :
                string.Format(await _localizationService.GetResourceAsync("GiftCardAttribute.For.Physical"), giftCardRecipientName);

            //encode (if required)
            if (htmlEncode)
            {
                giftCardFrom = WebUtility.HtmlEncode(giftCardFrom);
                giftCardFor = WebUtility.HtmlEncode(giftCardFor);
            }

            if (!string.IsNullOrEmpty(result.ToString()))
            {
                result.Append(separator);
            }

            result.Append(giftCardFrom);
            result.Append(separator);
            result.Append(giftCardFor);

            return result.ToString();
        }

        #endregion
    }
}
